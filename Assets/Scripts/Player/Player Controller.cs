using UnityEngine;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 7f;
    public float jumpForce = 10f;

    [Header("Combat")]
    public Transform shootPoint;
    public GameObject bulletPrefab;

    private Rigidbody2D rb;
    private Vector3 baseScale;

    private bool grounded;

    private NetworkVariable<bool> facingRight =
        new NetworkVariable<bool>(true);

    private float moveInput;
    private bool jumpQueued;

    // RPC optimization
    private float lastSentMoveInput = 999f;
    private bool lastSentFacing;
    private float movementSendTimer;

    private const float MovementSendRate = 0.05f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        baseScale = transform.localScale;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Transform spawn =
                GameManager.Instance.GetSpawnPoint(OwnerClientId);

            transform.position = spawn.position;

            facingRight.Value = OwnerClientId == 0;
        }

        if (IsOwner && Camera.main != null)
        {
            Camera.main.GetComponent<CameraFollow>().target =
                transform;
        }
    }

    void Update()
    {
        if (!IsOwner) 
            return;

        if (GameManager.Instance.IsGameOver())
            return;

        moveInput = Input.GetAxisRaw("Horizontal");

        if (moveInput != 0)
        {
            bool newFacing = moveInput > 0;

            if (newFacing != lastSentFacing)
            {
                lastSentFacing = newFacing;
                SetFacingServerRpc(newFacing);
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpQueued = true;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    void FixedUpdate()
    {
        if (!IsOwner) 
            return;

        if (GameManager.Instance.IsGameOver())
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        movementSendTimer += Time.fixedDeltaTime;

        bool inputChanged =
            moveInput != lastSentMoveInput;

        bool sendHeartbeat =
            movementSendTimer >= MovementSendRate;

        if (inputChanged || sendHeartbeat)
        {
            movementSendTimer = 0f;
            lastSentMoveInput = moveInput;

            MoveServerRpc(moveInput);
        }

        if (jumpQueued)
        {
            jumpQueued = false;
            JumpServerRpc();
        }
    }

    [ServerRpc]
    void MoveServerRpc(float x)
    {
        rb.linearVelocity =
            new Vector2(
                x * moveSpeed,
                rb.linearVelocity.y
            );
    }

    [ServerRpc]
    void SetFacingServerRpc(bool facing)
    {
        facingRight.Value = facing;
    }

    [ServerRpc]
    void JumpServerRpc()
    {
        if (!grounded) 
            return;

        rb.linearVelocity =
            new Vector2(
                rb.linearVelocity.x,
                jumpForce
            );
    }

    void Shoot()
    {
        if (!IsOwner) 
            return;

        float dir =
            facingRight.Value ? 1f : -1f;

        ShootServerRpc(
            shootPoint.position,
            dir
        );
    }

    [ServerRpc]
    void ShootServerRpc(Vector3 pos, float dir)
    {
        Vector3 spawnPos =
            pos + new Vector3(
                dir * 0.5f,
                0f,
                0f
            );

        GameObject bullet =
            Instantiate(
                bulletPrefab,
                spawnPos,
                Quaternion.identity
            );

        bullet.GetComponent<NetworkObject>()
            .Spawn();

        bullet.GetComponent<Bullet>()
            .Setup(dir);
    }

    void LateUpdate()
    {
        transform.localScale =
            new Vector3(
                facingRight.Value
                    ? baseScale.x
                    : -baseScale.x,
                baseScale.y,
                baseScale.z
            );
    }

    void OnCollisionStay2D(Collision2D col)
    {
        grounded = true;
    }

    void OnCollisionExit2D(Collision2D col)
    {
        grounded = false;
    }
}