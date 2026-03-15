using UnityEngine;
using UnityEngine.AI;
using Islebound.Player;
using Islebound.World;

namespace Islebound.Factions
{
    public class FactionAgent : MonoBehaviour
    {
        private enum AIState
        {
            Roaming,
            Chasing,
            Attacking,
            Returning
        }

        [SerializeField] private FactionData factionData;
        [SerializeField] private BiomeType[] allowedBiomes;

        [Header("Combat")]
        [SerializeField] private float detectionRange = 12f;
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float attackCooldown = 1.5f;
        [SerializeField] private int attackDamage = 10;
        [SerializeField] private float loseTargetDistance = 28f;

        [Header("Roam")]
        [SerializeField] private float roamPointReachedDistance = 1.5f;
        [SerializeField] private float roamWaitTime = 2f;
        [SerializeField] private float navMeshSampleRadius = 6f;
        [SerializeField] private int randomBiomePickAttempts = 12;
        [SerializeField] private int randomPointSampleAttempts = 10;

        [Header("Leash")]
        [SerializeField] private float maxChaseDistanceOutsideAllowedBiomes = 18f;
        [SerializeField] private bool returnToAllowedBiomeIfPlayerTooFar = true;

        [Header("Debug")]
        [SerializeField] private bool debugLogs = false;
        [SerializeField] private bool debugDrawRoamTarget = true;

        private NavMeshAgent agent;
        private Transform player;
        private DamageablePlayer damageablePlayer;

        private float attackTimer;
        private float roamWaitTimer;
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
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                damageablePlayer = playerObj.GetComponent<DamageablePlayer>();
            }
        }

        private void Update()
        {
            attackTimer -= Time.deltaTime;

            if (player == null || factionData == null || FactionManager.Instance == null)
            {
                RoamInsideAllowedBiomes();
                return;
            }

            bool hostile = FactionManager.Instance.IsHostile(factionData.factionType);

            if (!hostile)
            {
                HandleIdleOrReturn();
                return;
            }

            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer > loseTargetDistance)
            {
                HandleIdleOrReturn();
                return;
            }

            if (!BiomeUtility.IsPointInsideAnyAllowedBiome(player.position, allowedBiomes) &&
                returnToAllowedBiomeIfPlayerTooFar)
            {
                BiomeZone nearestAllowedBiome = BiomeUtility.GetNearestAllowedBiome(player.position, allowedBiomes);
                if (nearestAllowedBiome != null)
                {
                    Bounds bounds = nearestAllowedBiome.GetBounds();
                    Vector3 biomeCenter = bounds.center;
                    biomeCenter.y = player.position.y;

                    float distanceOutside = Vector3.Distance(player.position, biomeCenter);
                    if (distanceOutside > maxChaseDistanceOutsideAllowedBiomes)
                    {
                        currentState = AIState.Returning;
                        ReturnToNearestAllowedBiome();
                        return;
                    }
                }
            }

            if (distanceToPlayer <= attackRange)
            {
                currentState = AIState.Attacking;
                agent.SetDestination(transform.position);
                transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

                if (attackTimer <= 0f)
                {
                    attackTimer = attackCooldown;
                    damageablePlayer?.TakeDamage(attackDamage);

                    if (debugLogs)
                        Debug.Log($"{name} attacked player");
                }

                return;
            }

            if (distanceToPlayer <= detectionRange)
            {
                currentState = AIState.Chasing;
                agent.SetDestination(player.position);
                return;
            }

            HandleIdleOrReturn();
        }

        private void HandleIdleOrReturn()
        {
            bool insideAllowedBiome = BiomeUtility.IsPointInsideAnyAllowedBiome(transform.position, allowedBiomes);

            if (!insideAllowedBiome)
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
            BiomeZone nearestBiome = BiomeUtility.GetNearestAllowedBiome(transform.position, allowedBiomes);
            if (nearestBiome == null)
                return;

            if (BiomeUtility.TryGetRandomNavMeshPointInBiome(
                nearestBiome,
                navMeshSampleRadius,
                randomPointSampleAttempts,
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
                    allowedBiomes,
                    navMeshSampleRadius,
                    randomBiomePickAttempts,
                    randomPointSampleAttempts,
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

            if (!agent.pathPending && agent.remainingDistance <= roamPointReachedDistance)
            {
                roamWaitTimer += Time.deltaTime;
                if (roamWaitTimer >= roamWaitTime)
                {
                    roamWaitTimer = 0f;
                    hasRoamTarget = false;
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            if (debugDrawRoamTarget && hasRoamTarget)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(currentRoamTarget, 0.25f);
            }
        }
    }
}