using System.Collections.Generic;
using UnityEngine;
using Islebound.World;

namespace Islebound.Enemies
{
    public class BiomeEnemySpawner : MonoBehaviour
    {
        [Header("Spawn Setup")]
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private EnemyData enemyData;
        [SerializeField] private BiomeType[] allowedBiomes;

        [Header("Distance From Player")]
        [SerializeField] private float minSpawnDistanceFromPlayer = 18f;
        [SerializeField] private float maxSpawnDistanceFromPlayer = 35f;

        [Header("Spawn Timing")]
        [SerializeField] private EnemyTimeRule spawnTimeRule = EnemyTimeRule.NightOnly;
        [SerializeField] private float minSpawnInterval = 3f;
        [SerializeField] private float maxSpawnInterval = 6f;
        [SerializeField] private int maxAliveEnemies = 4;

        [Header("Sampling")]
        [SerializeField] private int attemptsPerCheck = 20;
        [SerializeField] private float navMeshSampleRadius = 6f;

        [Header("Debug")]
        [SerializeField] private bool debugLogs = false;

        private Transform player;
        private float spawnTimer;
        private float currentSpawnInterval;
        private readonly List<GameObject> aliveEnemies = new();

        private void Start()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;

            RollNextSpawnInterval();
        }

        private void Update()
        {
            CleanupDeadEnemies();

            spawnTimer += Time.deltaTime;
            if (spawnTimer < currentSpawnInterval)
                return;

            spawnTimer = 0f;
            RollNextSpawnInterval();

            if (player == null || enemyPrefab == null)
                return;

            if (!EnemyTimeRuleUtility.IsAllowed(spawnTimeRule))
                return;

            if (aliveEnemies.Count >= maxAliveEnemies)
                return;

            TrySpawn();
        }

        private void RollNextSpawnInterval()
        {
            currentSpawnInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
        }

        private void TrySpawn()
        {
            List<BiomeZone> possibleBiomes = BiomeUtility.GetBiomeZonesByTypes(allowedBiomes);
            if (possibleBiomes.Count == 0)
                return;

            for (int i = 0; i < attemptsPerCheck; i++)
            {
                BiomeZone biome = possibleBiomes[Random.Range(0, possibleBiomes.Count)];

                if (!BiomeUtility.TryGetRandomNavMeshPointInBiome(biome, navMeshSampleRadius, 10, out Vector3 spawnPoint))
                    continue;

                float distance = Vector3.Distance(player.position, spawnPoint);
                if (distance < minSpawnDistanceFromPlayer || distance > maxSpawnDistanceFromPlayer)
                    continue;

                GameObject enemy = Instantiate(enemyPrefab, spawnPoint, Quaternion.identity);
                aliveEnemies.Add(enemy);

                if (debugLogs)
                    Debug.Log($"Spawned {enemyPrefab.name} in biome {biome.BiomeType} at distance {distance:F1}");
                return;
            }

            if (debugLogs)
                Debug.Log($"Failed to spawn {enemyPrefab.name}: no valid point found.");
        }

        private void CleanupDeadEnemies()
        {
            for (int i = aliveEnemies.Count - 1; i >= 0; i--)
            {
                if (aliveEnemies[i] == null)
                    aliveEnemies.RemoveAt(i);
            }
        }
    }
}