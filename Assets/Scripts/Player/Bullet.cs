using UnityEngine;
using Unity.Netcode;

public class Bullet : NetworkBehaviour
{
    public float speed = 15f;
    public float destroy = 25f;

    private Rigidbody2D rb;
    private float direction;

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody2D>();

        if (IsServer)
        {
            
        }
    }

    public void Setup(float dir)
    {
        direction = dir;

        if (!IsServer) 
            return;

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        rb.linearVelocity =
            new Vector2(direction * speed, 0f);
    }

    void Update()
    {
        if (!IsServer) 
            return;

        if (Mathf.Abs(transform.position.x) >= destroy)
        {
            NetworkObject.Despawn();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer) 
            return;

        if (other.TryGetComponent(out Health health))
        {
            health.ApplyDamage(25);
        }

        NetworkObject.Despawn();
    }
}