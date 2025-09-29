using UnityEngine;
using UnityEngine.AI;

namespace EmpireOfHonor.Gameplay
{
    /// <summary>
    /// Snaps a NavMeshAgent to the closest position on the baked NavMesh on start.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class NavMeshAgentSnap : MonoBehaviour
    {
        [SerializeField] private float maxDistance = 5f;

        private void Start()
        {
            var agent = GetComponent<NavMeshAgent>();
            if (agent == null)
            {
                return;
            }

            if (NavMesh.SamplePosition(transform.position, out var hit, maxDistance, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
            }
        }
    }
}
