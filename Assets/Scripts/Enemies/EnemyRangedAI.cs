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
            Returning,
            Repositioning
        }

        [SerializeField] private EnemyData enemyData;
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private Transform firePoint;

        [Header("Debug")]
        [SerializeField] private bool debugLogs = false;
        [SerializeField] private bool debugDrawShotLine = true;
        [SerializeField] private bool debugDrawRoamTarget = true;
        [SerializeField] private bool debugDrawRepositionTarget = true;

        private NavMeshAgent agent;
        private Transform player;

        private float attackTimer;
        private float roamWaitTimer;
        private float notAggroTimer;
        private float losCheckTimer;
        private float repositionTimer;

        private Vector3 currentRoamTarget;
        private bool hasRoamTarget;
        private BiomeZone currentRoamBiome;

        private Vector3 currentRepositionTarget;
        private bool hasRepositionTarget;

        private AIState currentState = AIState.Roaming;

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
            losCheckTimer -= Time.deltaTime;
            repositionTimer -= Time.deltaTime;

            bool isAggro = false;

            if (EnemyTimeRuleUtility.IsAllowed(enemyData.activeTimeRule) && player != null)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, player.position);

                if (distanceToPlayer <= enemyData.detectionRange)
                {
                    isAggro = true;

                    Vector3 flatPlayerPos = new Vector3(player.position.x, transform.position.y, player.position.z);
                    transform.LookAt(flatPlayerPos);

                    bool clearShot = HasClearShot();

                    if (distanceToPlayer > enemyData.preferredDistance)
                    {
                        currentState = AIState.Chasing;
                        hasRepositionTarget = false;
                        agent.SetDestination(player.position);
                    }
                    else if (enemyData.requireClearShot && !clearShot)
                    {
                        HandleRepositionForShot();
                    }
                    else
                    {
                        currentState = AIState.Attacking;
                        hasRepositionTarget = false;
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

        private bool HasClearShot()
        {
            if (!enemyData.requireClearShot)
                return true;

            if (losCheckTimer > 0f)
                return CheckLineOfSightOnce(false);

            losCheckTimer = enemyData.lineOfSightCheckInterval;
            return CheckLineOfSightOnce(true);
        }

        private bool CheckLineOfSightOnce(bool drawDebug)
        {
            if (player == null)
                return false;

            Vector3 origin = firePoint != null ? firePoint.position : transform.position + Vector3.up * 1.2f;
            Vector3 target = player.position + Vector3.up * enemyData.targetHeightOffset;
            Vector3 direction = target - origin;
            float distance = direction.magnitude;

            if (distance <= 0.01f)
                return true;

            direction /= distance;

            if (Physics.SphereCast(
                origin,
                enemyData.projectileHitRadius,
                direction,
                out RaycastHit hit,
                distance,
                enemyData.lineOfSightBlockMask,
                QueryTriggerInteraction.Ignore))
            {
                if (drawDebug && debugDrawShotLine)
                {
                    Debug.DrawLine(origin, hit.point, Color.yellow, enemyData.lineOfSightCheckInterval);
                }

                if (hit.transform == player || hit.transform.IsChildOf(player))
                    return true;

                if (enemyData.ignoreEnemiesAsShotBlockers &&
                    hit.collider.GetComponentInParent<EnemyHealth>() != null)
                {
                    return true;
                }

                return false;
            }

            if (drawDebug && debugDrawShotLine)
            {
                Debug.DrawLine(origin, target, Color.green, enemyData.lineOfSightCheckInterval);
            }

            return true;
        }

        private void HandleRepositionForShot()
        {
            currentState = AIState.Repositioning;

            if (!hasRepositionTarget || repositionTimer <= 0f)
            {
                repositionTimer = enemyData.repositionRepathInterval;

                if (TryFindRepositionTarget(out Vector3 bestPoint))
                {
                    currentRepositionTarget = bestPoint;
                    hasRepositionTarget = true;
                    agent.SetDestination(currentRepositionTarget);

                    if (debugLogs)
                    {
                        Debug.Log($"{name} repositioning for clear shot.");
                    }
                }
                else
                {
                    hasRepositionTarget = false;
                    agent.SetDestination(player.position);
                }
            }
            else
            {
                agent.SetDestination(currentRepositionTarget);

                if (!agent.pathPending && agent.remainingDistance <= enemyData.repositionPointReachedDistance)
                {
                    hasRepositionTarget = false;
                }
            }
        }

        private bool TryFindRepositionTarget(out Vector3 bestPoint)
        {
            bestPoint = Vector3.zero;

            if (player == null)
                return false;

            float bestScore = float.MaxValue;
            bool found = false;

            for (int i = 0; i < enemyData.repositionSampleCount; i++)
            {
                Vector2 randomCircle = Random.insideUnitCircle * enemyData.repositionSearchRadius;
                Vector3 candidate = transform.position + new Vector3(randomCircle.x, 0f, randomCircle.y);

                if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, enemyData.navMeshSampleRadius, NavMesh.AllAreas))
                    continue;

                Vector3 point = hit.position;
                float distanceToPlayer = Vector3.Distance(point, player.position);

                if (distanceToPlayer < enemyData.repositionMinDistanceToPlayer)
                    continue;

                if (distanceToPlayer > enemyData.repositionMaxDistanceToPlayer)
                    continue;

                if (!BiomeUtility.IsPointInsideAnyAllowedBiome(point, enemyData.allowedBiomes))
                    continue;

                if (!HasClearShotFromPoint(point))
                    continue;

                NavMeshPath path = new NavMeshPath();
                if (!agent.CalculatePath(point, path))
                    continue;

                if (path.status != NavMeshPathStatus.PathComplete)
                    continue;

                float score = Vector3.Distance(transform.position, point);

                if (score < bestScore)
                {
                    bestScore = score;
                    bestPoint = point;
                    found = true;
                }
            }

            return found;
        }

        private bool HasClearShotFromPoint(Vector3 point)
        {
            if (player == null)
                return false;

            float heightOffset = 1.2f;
            if (firePoint != null)
            {
                heightOffset = firePoint.position.y - transform.position.y;
            }

            Vector3 origin = point + Vector3.up * heightOffset;
            Vector3 target = player.position + Vector3.up * enemyData.targetHeightOffset;
            Vector3 direction = target - origin;
            float distance = direction.magnitude;

            if (distance <= 0.01f)
                return true;

            direction /= distance;

            if (Physics.SphereCast(
                origin,
                enemyData.projectileHitRadius,
                direction,
                out RaycastHit hit,
                distance,
                enemyData.lineOfSightBlockMask,
                QueryTriggerInteraction.Ignore))
            {
                if (hit.transform == player || hit.transform.IsChildOf(player))
                    return true;

                if (enemyData.ignoreEnemiesAsShotBlockers &&
                    hit.collider.GetComponentInParent<EnemyHealth>() != null)
                {
                    return true;
                }

                return false;
            }

            return true;
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
            hasRepositionTarget = false;

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

            if (debugDrawRepositionTarget && hasRepositionTarget)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawSphere(currentRepositionTarget, 0.3f);
            }
        }
    }
}