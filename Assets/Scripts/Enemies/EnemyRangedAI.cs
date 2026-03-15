using UnityEngine;
using UnityEngine.AI;
using Islebound.Combat;
using Islebound.World;

namespace Islebound.Enemies
{
    public class EnemyRangedAI : MonoBehaviour
    {
        private enum AIState
        {
            Roaming,
            Chasing,
            Attacking,
            Returning
        }

        [SerializeField] private EnemyData enemyData;
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private Transform firePoint;

        [Header("Debug")]
        [SerializeField] private bool debugLogs = false;
        [SerializeField] private bool debugDrawShotLine = true;
        [SerializeField] private bool debugDrawRoamTarget = true;

        private NavMeshAgent agent;
        private Transform player;

        private float attackTimer;
        private float roamWaitTimer;
        private float notAggroTimer;
        private Vector3 currentRoamTarget;
        private bool hasRoamTarget;
        private AIState currentState = AIState.Roaming;
        private BiomeZone currentRoamBiome;

        public string DebugState => currentState.ToString();

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        private void Start()
        {
            if (enemyData != null)
                agent.speed = enemyData.moveSpeed;

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;

            if (projectilePrefab != null && projectilePrefab.gameObject.scene.IsValid())
            {
                Debug.LogWarning($"{name}: Projectile Prefab points to a scene object. Assign prefab asset from Assets/Prefabs/Combat instead.");
            }
        }

        private void Update()
        {
            if (enemyData == null)
                return;

            attackTimer -= Time.deltaTime;

            bool isAggro = false;

            if (EnemyTimeRuleUtility.IsAllowed(enemyData.activeTimeRule) && player != null)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, player.position);

                if (distanceToPlayer <= enemyData.detectionRange)
                {
                    isAggro = true;

                    Vector3 flatPlayerPos = new Vector3(player.position.x, transform.position.y, player.position.z);
                    transform.LookAt(flatPlayerPos);

                    if (distanceToPlayer > enemyData.preferredDistance)
                    {
                        currentState = AIState.Chasing;
                        agent.SetDestination(player.position);
                    }
                    else
                    {
                        currentState = AIState.Attacking;
                        agent.SetDestination(transform.position);

                        if (attackTimer <= 0f)
                        {
                            attackTimer = enemyData.attackCooldown;
                            FireProjectile();
                        }
                    }
                }
                else
                {
                    HandleIdleOrReturn();
                }
            }
            else
            {
                HandleIdleOrReturn();
            }

            HandleDespawnWhenNotAggro(isAggro);
        }

        private void HandleDespawnWhenNotAggro(bool isAggro)
        {
            if (!enemyData.canDespawnWhenNotAggro || player == null)
                return;

            if (isAggro)
            {
                notAggroTimer = 0f;
                return;
            }

            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer < enemyData.minDistanceFromPlayerToDespawn)
            {
                notAggroTimer = 0f;
                return;
            }

            notAggroTimer += Time.deltaTime;
            if (notAggroTimer >= enemyData.despawnDelayAfterLosingAggro)
            {
                if (debugLogs)
                    Debug.Log($"{name} despawned after being non-aggro for {notAggroTimer:F1}s");

                Destroy(gameObject);
            }
        }

        private void HandleIdleOrReturn()
        {
            bool insideAllowedBiome = BiomeUtility.IsPointInsideAnyAllowedBiome(transform.position, enemyData.allowedBiomes);

            if (!insideAllowedBiome && enemyData.returnToNearestAllowedBiomeWhenIdle)
            {
                currentState = AIState.Returning;
                ReturnToNearestAllowedBiome();
                return;
            }

            currentState = AIState.Roaming;
            RoamInsideAllowedBiomes();
        }

        private void ReturnToNearestAllowedBiome()
        {
            BiomeZone nearestBiome = BiomeUtility.GetNearestAllowedBiome(transform.position, enemyData.allowedBiomes);
            if (nearestBiome == null)
                return;

            if (BiomeUtility.TryGetRandomNavMeshPointInBiome(
                nearestBiome,
                enemyData.navMeshSampleRadius,
                enemyData.randomPointSampleAttempts,
                out Vector3 point))
            {
                agent.SetDestination(point);

                if (debugLogs)
                    Debug.Log($"{name} returning to nearest allowed biome: {nearestBiome.BiomeType}");
            }
        }

        private void RoamInsideAllowedBiomes()
        {
            if (!hasRoamTarget)
            {
                if (BiomeUtility.TryGetRandomNavMeshPointInAllowedBiomes(
                    enemyData.allowedBiomes,
                    enemyData.navMeshSampleRadius,
                    enemyData.randomBiomePickAttempts,
                    enemyData.randomPointSampleAttempts,
                    out Vector3 point,
                    out BiomeZone biome))
                {
                    currentRoamTarget = point;
                    currentRoamBiome = biome;
                    hasRoamTarget = true;
                    agent.SetDestination(currentRoamTarget);

                    if (debugLogs)
                        Debug.Log($"{name} roaming to biome {currentRoamBiome.BiomeType}");
                }

                return;
            }

            if (!agent.pathPending && agent.remainingDistance <= enemyData.roamPointReachedDistance)
            {
                roamWaitTimer += Time.deltaTime;
                if (roamWaitTimer >= enemyData.roamWaitTime)
                {
                    roamWaitTimer = 0f;
                    hasRoamTarget = false;
                }
            }
        }

        private void FireProjectile()
        {
         if (projectilePrefab == null || firePoint == null || player == null)
            {
            if (debugLogs)
                  Debug.LogWarning($"{name}: missing projectile prefab / fire point / player");
                return;
            }

            Vector3 targetPoint = player.position + Vector3.up * enemyData.targetHeightOffset;
            Vector3 direction = (targetPoint - firePoint.position).normalized;

            Projectile projectile = Instantiate(
                projectilePrefab,
                firePoint.position,
                Quaternion.LookRotation(direction));

            projectile.Initialize(
                direction,
                enemyData.projectileSpeed,
                enemyData.projectileLifetime,
                enemyData.contactDamage,
                gameObject,
                enemyData.projectileHitRadius);

            if (debugLogs)
                Debug.Log($"{name} fired projectile at body");

            if (debugDrawShotLine)
                Debug.DrawLine(firePoint.position, targetPoint, Color.red, 0.5f);
        }

        private void OnDrawGizmosSelected()
        {
            if (enemyData == null)
                return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, enemyData.detectionRange);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, enemyData.preferredDistance);

            if (debugDrawRoamTarget && hasRoamTarget)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(currentRoamTarget, 0.25f);
            }
        }
    }
}