using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace EmpireOfHonor.Gameplay
{
    /// <summary>
    /// Minimal AI that chases the closest non-allied target and performs melee attacks.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Weapon))]
    [RequireComponent(typeof(Health))]
    public class SimpleEnemyAI : MonoBehaviour
    {
        [SerializeField] private float repathInterval = 0.5f;
        [SerializeField] private float rotationSpeed = 10f;

        private NavMeshAgent agent;
        private Weapon weapon;
        private Health health;
        private Health currentTarget;
        private float repathTimer;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            weapon = GetComponent<Weapon>();
            health = GetComponent<Health>();
        }

        private void Update()
        {
            if (!gameObject.activeInHierarchy || health.CurrentHealth <= 0f)
            {
                return;
            }

            repathTimer -= Time.deltaTime;
            if (currentTarget == null || !currentTarget.gameObject.activeInHierarchy || currentTarget.CurrentHealth <= 0f || repathTimer <= 0f)
            {
                currentTarget = FindClosestTarget();
                repathTimer = repathInterval;
                if (currentTarget != null && agent.isOnNavMesh)
                {
                    agent.isStopped = false;
                    agent.SetDestination(currentTarget.transform.position);
                }
            }

            if (currentTarget == null)
            {
                if (agent.isOnNavMesh)
                {
                    agent.isStopped = true;
                }

                return;
            }

            var targetPosition = currentTarget.transform.position;
            var distance = Vector3.Distance(transform.position, targetPosition);
            if (agent.isOnNavMesh)
            {
                agent.SetDestination(targetPosition);
                if (distance <= weapon.Range + 0.05f)
                {
                    agent.isStopped = true;
                }
                else
                {
                    agent.isStopped = false;
                }
            }

            RotateTowards(targetPosition);
            if (distance <= weapon.Range + 0.05f)
            {
                weapon.TryAttack();
            }
        }

        private Health FindClosestTarget()
        {
            var allTargets = new List<Health>();
            foreach (Health.Team team in Enum.GetValues(typeof(Health.Team)))
            {
                if (team == health.TeamId)
                {
                    continue;
                }

                allTargets.AddRange(Health.GetAllies(team));
            }

            var shortestDistance = float.MaxValue;
            Health bestTarget = null;
            foreach (var candidate in allTargets)
            {
                if (candidate == null || !candidate.gameObject.activeInHierarchy || candidate.CurrentHealth <= 0f)
                {
                    continue;
                }

                var distance = Vector3.Distance(transform.position, candidate.transform.position);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    bestTarget = candidate;
                }
            }

            return bestTarget;
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
