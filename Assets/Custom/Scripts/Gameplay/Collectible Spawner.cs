using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CollectibleSpawner : NetworkBehaviour
{
    public static CollectibleSpawner Instance;

    [System.Serializable]
    public class CollectibleVariant
    {
        public Collectible prefab;
        public int weight = 1;
    }

    [Header("Spawn Settings")]
    [SerializeField] private Transform locationsParent;
    [SerializeField] private int startingCollectibles = 8;
    [SerializeField] private float respawnDelay = 5f;

    [Header("Collectible Variants")]
    [SerializeField] private List<CollectibleVariant> variants = new();

    private readonly List<Transform> spawnPoints = new();
    private readonly HashSet<Transform> occupiedSpawnPoints = new();
    private readonly List<Collectible> activeCollectibles = new();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        CacheSpawnPoints();

        for (int i = 0; i < startingCollectibles; i++)
        {
            SpawnCollectible();
        }
    }

    private void CacheSpawnPoints()
    {
        spawnPoints.Clear();

        foreach (Transform building in locationsParent)
        {
            foreach (Transform point in building)
            {
                spawnPoints.Add(point);
            }
        }

        Debug.Log($"Loaded {spawnPoints.Count} spawn points.");
    }

    public void CollectibleCollected(Collectible collectible)
    {
        activeCollectibles.Remove(collectible);

        if (collectible.SpawnPoint != null)
        {
            occupiedSpawnPoints.Remove(collectible.SpawnPoint);
        }

        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);

        SpawnCollectible();
    }

    private void SpawnCollectible()
    {
        Transform spawnPoint = GetRandomFreeSpawnPoint();

        if (spawnPoint == null)
        {
            Debug.LogWarning("No free spawn points available.");
            return;
        }

        Collectible prefab = GetRandomVariant();

        Debug.Log($"Spawning prefab: {prefab.name}");
        Collectible collectible = Instantiate(
            prefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        collectible.SetSpawnPoint(spawnPoint);

        collectible.NetworkObject.Spawn();

        activeCollectibles.Add(collectible);
        occupiedSpawnPoints.Add(spawnPoint);
    }

    public void ResetCollectibles()
    {
        if (!IsServer)
            return;

        StopAllCoroutines();

        foreach (Collectible collectible in activeCollectibles)
        {
            if (collectible != null && collectible.NetworkObject != null && collectible.NetworkObject.IsSpawned)
            {
                collectible.NetworkObject.Despawn(true);
            }
        }

        activeCollectibles.Clear();
        occupiedSpawnPoints.Clear();

        for (int i = 0; i < startingCollectibles; i++)
        {
            SpawnCollectible();
        }
    }

    private Transform GetRandomFreeSpawnPoint()
    {
        List<Transform> available = new();

        foreach (Transform point in spawnPoints)
        {
            if (!occupiedSpawnPoints.Contains(point))
            {
                available.Add(point);
            }
        }

        if (available.Count == 0)
            return null;

        return available[Random.Range(0, available.Count)];
    }

    private Collectible GetRandomVariant()
    {
        int totalWeight = 0;

        foreach (CollectibleVariant variant in variants)
        {
            totalWeight += variant.weight;
        }

        int random = Random.Range(0, totalWeight);

        foreach (CollectibleVariant variant in variants)
        {
            random -= variant.weight;

            if (random < 0)
            {
                return variant.prefab;
            }
        }

        return variants[0].prefab;
    }
}