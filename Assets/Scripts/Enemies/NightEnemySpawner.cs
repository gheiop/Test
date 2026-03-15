using System.Collections.Generic;
using UnityEngine;
using Islebound.World;

namespace Islebound.Enemies
{
    public class NightEnemySpawner : MonoBehaviour
    {
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private EnemySpawnPoint[] spawnPoints;
        [SerializeField] private int maxAliveEnemies = 4;
        [SerializeField] private float spawnCheckInterval = 5f;

        private float timer;
        private readonly List<GameObject> aliveEnemies = new();

        private void Update()
        {
            CleanupDeadEnemies();

            timer += Time.deltaTime;
            if (timer < spawnCheckInterval)
                return;

            timer = 0f;

            if (WorldTimeManager.Instance == null || !WorldTimeManager.Instance.IsNight)
                return;

            if (enemyPrefab == null || spawnPoints == null || spawnPoints.Length == 0)
                return;

            if (aliveEnemies.Count >= maxAliveEnemies)
                return;

            SpawnEnemy();
        }

        private void SpawnEnemy()
        {
            int index = Random.Range(0, spawnPoints.Length);
            Transform spawnTransform = spawnPoints[index].transform;

            GameObject enemy = Instantiate(enemyPrefab, spawnTransform.position, spawnTransform.rotation);
            aliveEnemies.Add(enemy);
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