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
            RepositioningForShot
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
        private float lineOfSightTimer;
        private float repositionTimer;

        private Vector3 currentRoamTarget;
        private bool hasRoamTarget;
        private BiomeZone currentRoamBiome;

        private Vector3 currentShotPosition;
        private bool hasShotPosition;

        private bool cachedHasClearShot;

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
            lineOfSightTimer -= Time.deltaTime;
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

                    bool hasClearShot = GetHasClearShot();

                    if (ShouldReposition(distanceToPlayer, hasClearShot))
                    {
                        HandleRepositionOrChase(distanceToPlayer);
                    }
                    else
                    {
                        currentState = AIState.Attacking;
                        hasShotPosition = false;
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

        private bool GetHasClearShot()
        {
            if (!enemyData.requireClearShotToAttack)
                return true;

            if (lineOfSightTimer > 0f)
                return cachedHasClearShot;

            lineOfSightTimer = enemyData.lineOfSightCheckInterval;
            cachedHasClearShot = HasClearShotFromPosition(firePoint != null ? firePoint.position : transform.position);
            return cachedHasClearShot;
        }

        private bool ShouldReposition(float distanceToPlayer, bool hasClearShot)
        {
            if (distanceToPlayer > enemyData.preferredDistance)
                return true;

            if (enemyData.requireClearShotToAttack && !hasClearShot)
                return true;

            return false;
        }

        private void HandleRepositionOrChase(float distanceToPlayer)
        {
            bool needsCloserDistance = distanceToPlayer > enemyData.preferredDistance;

            if (needsCloserDistance)
            {
                currentState = AIState.Chasing;
                hasShotPosition = false;
                agent.SetDestination(player.position);
                return;
            }

            currentState = AIState.RepositioningForShot;

            if (!hasShotPosition || repositionTimer <= 0f)
            {
                repositionTimer = enemyData.repositionRepathInterval;
                hasShotPosition = TryFindBestShotPosition(out currentShotPosition);

                if (hasShotPosition)
                {
                    agent.SetDestination(currentShotPosition);

                    if (debugLogs)
                    {
                        Debug.Log($"{name} repositioning to clear shot point.");
                    }
                }
                else
                {
                    agent.SetDestination(player.position);

                    if (debugLogs)
                    {
                        Debug.Log($"{name} could not find clear shot point, chasing player.");
                    }
                }
            }
            else
            {
                agent.SetDestination(currentShotPosition);
            }
        }

        private bool TryFindBestShotPosition(out Vector3 bestPoint)
        {
            bestPoint = Vector3.zero;

            if (player == null)
                return false;

            float bestScore = float.MaxValue;
            bool found = false;

            Vector3 playerPos = player.position;

            for (int i = 0; i < enemyData.repositionSampleCount; i++)
            {
                Vector2 randomCircle = Random.insideUnitCircle * enemyData.repositionSearchRadius;
                Vector3 candidate = transform.position + new Vector3(randomCircle.x, 0f, randomCircle.y);

                if (!NavMesh.SamplePosition(candidate, out NavMeshHit navHit, enemyData.navMeshSampleRadius, NavMesh.AllAreas))
                    continue;

                Vector3 point = navHit.position;

                float distanceToPlayer = Vector3.Distance(point, playerPos);
                if (distanceToPlayer < enemyData.repositionMinDistanceFromPlayer)
                    continue;

                if (distanceToPlayer > enemyData.repositionMaxDistanceFromPlayer)
                    continue;

                if (enemyData.keepRepositionInsideAllowedBiomes &&
                    !BiomeUtility.IsPointInsideAnyAllowedBiome(point, enemyData.allowedBiomes))
                    continue;

                if (!HasClearShotFromPosition(GetShotOriginAt(point)))
                    continue;

                NavMeshPath path = new NavMeshPath();
                if (!NavMesh.CalculatePath(transform.position, point, NavMesh.AllAreas, path))
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

        private bool HasClearShotFromPosition(Vector3 shotOrigin)
        {
            if (player == null)
                return false;

            Vector3 targetPoint = player.position + Vector3.up * enemyData.targetHeightOffset;
            Vector3 toTarget = targetPoint - shotOrigin;
            float distance = toTarget.magnitude;

            if (distance <= 0.01f)
                return true;

            Vector3 direction = toTarget / distance;

            if (Physics.SphereCast(
                shotOrigin,
                enemyData.projectileHitRadius,
                direction,
                out RaycastHit hit,
                distance,
                enemyData.lineOfSightBlockMask,
                QueryTriggerInteraction.Ignore))
            {
                if (hit.transform == player || hit.transform.IsChildOf(player))
                    return true;

                return false;
            }

            return true;
        }

        private Vector3 GetShotOriginAt(Vector3 worldPosition)
        {
            float fireHeight = 1.25f;

            if (firePoint != null)
            {
                fireHeight = firePoint.position.y - transform.position.y;
            }

            return worldPosition + Vector3.up * fireHeight;
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
            hasShotPosition = false;

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

            if (debugDrawRepositionTarget && hasShotPosition)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawSphere(currentShotPosition, 0.3f);
            }
        }
    }
}