using UnityEngine;
using Islebound.World;

namespace Islebound.Enemies
{
    [CreateAssetMenu(fileName = "EnemyData_", menuName = "Islebound/Enemies/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [Header("Identity")]
        public string displayName;

        [Header("Stats")]
        public float maxHealth = 40f;
        public int contactDamage = 10;
        public float moveSpeed = 3.5f;

        [Header("Detection")]
        public float detectionRange = 12f;
        public float attackRange = 2f;
        public float attackCooldown = 1.2f;
        public float loseTargetDistance = 25f;

        [Header("Biome & Time")]
        public BiomeType[] allowedBiomes;
        public EnemyTimeRule activeTimeRule = EnemyTimeRule.NightOnly;

        [Header("Roam")]
        public float roamPointReachedDistance = 1.2f;
        public float roamWaitTime = 1.5f;
        public float navMeshSampleRadius = 6f;
        public int randomBiomePickAttempts = 12;
        public int randomPointSampleAttempts = 10;
        public bool returnToNearestAllowedBiomeWhenIdle = true;
        public bool roamAcrossAllAllowedBiomes = true;

        [Header("Despawn")]
        public bool canDespawnWhenNotAggro = true;
        public float despawnDelayAfterLosingAggro = 20f;
        public float minDistanceFromPlayerToDespawn = 22f;

        [Header("Ranged")]
        public float projectileSpeed = 12f;
        public float projectileLifetime = 4f;
        public float preferredDistance = 8f;
        public float projectileHitRadius = 0.25f;
        public float targetHeightOffset = 0.75f;
    }
}