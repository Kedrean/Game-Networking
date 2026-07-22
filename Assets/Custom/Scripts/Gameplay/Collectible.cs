using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class Collectible : NetworkBehaviour
{
    [Header("Collectible")]
    [SerializeField] private int points = 1;

    [Header("Visual Effects")]
    [SerializeField] private float rotationSpeed = 60f;
    [SerializeField] private float floatHeight = 0.25f;
    [SerializeField] private float floatSpeed = 2f;

    public int Points => points;

    public Transform SpawnPoint { get; private set; }

    private bool collected = false;

    private Vector3 startPosition;

    public void SetSpawnPoint(Transform spawnPoint)
    {
        SpawnPoint = spawnPoint;
    }

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        // Cosmetic animation only.
        // Every client runs this locally.
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

        Vector3 pos = startPosition;
        pos.y += Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = pos;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (GameManager.Instance.IsGameEnded)
            return;

        if (!IsServer)
            return;

        if (collected)
            return;

        NetworkObject player = other.GetComponent<NetworkObject>();

        if (player == null)
            return;

        collected = true;

        ScoreManager.Instance.AddPoint(player.OwnerClientId, points);

        CollectibleSpawner.Instance.CollectibleCollected(this);

        NetworkObject.Despawn(true);
    }
}