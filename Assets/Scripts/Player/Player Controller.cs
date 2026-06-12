using UnityEngine;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    public float moveSpeed = 7f;
    public float jumpForce = 10f;

    public Transform shootPoint;
    public GameObject bulletPrefab;

    Rigidbody2D rb;

    bool grounded;

    private NetworkVariable<bool> facingRight =
        new NetworkVariable<bool>(true);

    float moveInput;
    bool jumpQueued;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Transform spawn =
                GameManager.Instance.GetSpawnPoint(OwnerClientId);

            transform.SetPositionAndRotation(spawn.position, spawn.rotation);
        }

        if (IsOwner)
        {
            Camera.main.GetComponent<CameraFollow>().target = transform;
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        moveInput = Input.GetAxisRaw("Horizontal");

        if (moveInput != 0)
            facingRight.Value = moveInput > 0;

        if (Input.GetKeyDown(KeyCode.Space))
            jumpQueued = true;

        if (Input.GetMouseButtonDown(0))
            Shoot();
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        MoveServerRpc(moveInput);

        if (jumpQueued)
        {
            JumpServerRpc();
            jumpQueued = false;
        }
    }

    [ServerRpc]
    void MoveServerRpc(float x)
    {
        rb.linearVelocity =
            new Vector2(x * moveSpeed, rb.linearVelocity.y);
    }

    [ServerRpc]
    void JumpServerRpc()
    {
        if (!grounded) return;

        rb.linearVelocity =
            new Vector2(rb.linearVelocity.x, jumpForce);
    }

    void Shoot()
    {
        if (!IsOwner) return;

        float dir = facingRight.Value ? 1f : -1f;

        ShootServerRpc(shootPoint.position, dir);
    }

    [ServerRpc]
    void ShootServerRpc(Vector3 pos, float dir)
    {
        Vector3 spawnPos = pos + new Vector3(dir * 0.5f, 0, 0);

        GameObject bullet =
            Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

        bullet.GetComponent<NetworkObject>().Spawn();

        bullet.GetComponent<Bullet>().Setup(dir);
    }

    void LateUpdate()
    {
        float dir = facingRight.Value ? 1f : -1f;

        Vector3 baseScale = new Vector3(0.5f, 0.5f, 1f);

        transform.localScale = new Vector3(
            baseScale.x * dir,
            baseScale.y,
            baseScale.z
        );
    }

    void OnCollisionStay2D(Collision2D col) => grounded = true;
    void OnCollisionExit2D(Collision2D col) => grounded = false;
}