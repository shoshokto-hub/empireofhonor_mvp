using UnityEngine;
using UnityEngine.AI;

namespace EmpireOfHonor.Gameplay
{
    /// <summary>
    /// Controls friendly units that can receive orders for movement, holding, or attacking targets.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Weapon))]
    [RequireComponent(typeof(Health))]
    public class UnitController : MonoBehaviour
    {
        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private Weapon weapon;
        [SerializeField] private Health health;
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private float attackChaseBuffer = 0.25f;

        private Vector3 holdPosition;
        private bool hasHoldPosition;
        private Health attackTarget;

        private enum UnitState
        {
            Idle,
            Moving,
            Hold,
            Attacking
        }

        private UnitState state = UnitState.Idle;

        private void Awake()
        {
            agent ??= GetComponent<NavMeshAgent>();
            weapon ??= GetComponent<Weapon>();
            health ??= GetComponent<Health>();
            holdPosition = transform.position;
            hasHoldPosition = true;
        }

        private void Update()
        {
            if (!gameObject.activeInHierarchy || health.CurrentHealth <= 0f)
            {
                return;
            }

            switch (state)
            {
                case UnitState.Moving:
                    TickMove();
                    break;
                case UnitState.Hold:
                    TickHold();
                    break;
                case UnitState.Attacking:
                    TickAttack();
                    break;
            }
        }

        /// <summary>
        /// Issues a move order towards the specified destination.
        /// </summary>
        public void OrderMove(Vector3 destination)
        {
            if (!agent.isOnNavMesh)
            {
                return;
            }

            attackTarget = null;
            hasHoldPosition = true;
            holdPosition = destination;
            state = UnitState.Moving;
            agent.isStopped = false;
            agent.SetDestination(destination);
        }

        /// <summary>
        /// Makes the unit hold its current position.
        /// </summary>
        public void OrderHold()
        {
            if (!agent.isOnNavMesh)
            {
                return;
            }

            attackTarget = null;
            hasHoldPosition = true;
            holdPosition = transform.position;
            state = UnitState.Hold;
            agent.isStopped = true;
        }

        /// <summary>
        /// Orders the unit to pursue and attack the provided target.
        /// </summary>
        public void OrderAttack(Health target)
        {
            if (target == null)
            {
                return;
            }

            attackTarget = target;
            state = UnitState.Attacking;
            if (agent.isOnNavMesh)
            {
                agent.isStopped = false;
                agent.SetDestination(target.transform.position);
            }
        }

        private void TickMove()
        {
            if (!agent.isOnNavMesh)
            {
                return;
            }

            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                agent.isStopped = true;
                state = UnitState.Idle;
            }
        }

        private void TickHold()
        {
            if (!agent.isOnNavMesh || !hasHoldPosition)
            {
                return;
            }

            var flatPosition = new Vector3(holdPosition.x, transform.position.y, holdPosition.z);
            transform.position = Vector3.Lerp(transform.position, flatPosition, Time.deltaTime * 2f);
        }

        private void TickAttack()
        {
            if (attackTarget == null || !attackTarget.gameObject.activeInHierarchy || attackTarget.CurrentHealth <= 0f)
            {
                state = UnitState.Idle;
                return;
            }

            var targetPosition = attackTarget.transform.position;
            var distance = Vector3.Distance(transform.position, targetPosition);
            var requiredDistance = weapon.Range + attackChaseBuffer;

            if (agent.isOnNavMesh)
            {
                if (distance > requiredDistance)
                {
                    agent.isStopped = false;
                    agent.SetDestination(targetPosition);
                }
                else
                {
                    agent.isStopped = true;
                }
            }

            RotateTowards(targetPosition);
            if (distance <= weapon.Range + 0.05f)
            {
                weapon.TryAttack();
            }
        }

        private void RotateTowards(Vector3 targetPosition)
        {
            var direction = targetPosition - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.001f)
            {
                return;
            }

            var desiredRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
