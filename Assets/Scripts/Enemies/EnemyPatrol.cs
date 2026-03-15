using UnityEngine;
using UnityEngine.AI;

namespace Islebound.Enemies
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyPatrol : MonoBehaviour
    {
        [SerializeField] private Transform[] patrolPoints;

        protected NavMeshAgent agent;
        protected int patrolIndex;

        protected virtual void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        protected virtual void Start()
        {
            MoveToNextPatrolPoint();
        }

        protected void Patrol()
        {
            if (patrolPoints == null || patrolPoints.Length == 0)
                return;

            if (!agent.pathPending && agent.remainingDistance <= 0.5f)
            {
                MoveToNextPatrolPoint();
            }
        }

        protected void MoveToNextPatrolPoint()
        {
            if (patrolPoints == null || patrolPoints.Length == 0)
                return;

            agent.SetDestination(patrolPoints[patrolIndex].position);
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
        }
    }
}