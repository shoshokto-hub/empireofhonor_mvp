using System.Collections.Generic;
using UnityEngine;

namespace EmpireOfHonor.Gameplay
{
    /// <summary>
    /// Simple team-based health container with automatic registry management.
    /// </summary>
    public class Health : MonoBehaviour
    {
        public enum Team
        {
            Player,
            Ally,
            Enemy
        }

        private static readonly Dictionary<Team, List<Health>> Registry = new();

        [SerializeField] private Team team = Team.Enemy;
        [SerializeField] private float maxHealth = 100f;

        private float currentHealth;

        /// <summary>
        /// Gets the team this entity belongs to.
        /// </summary>
        public Team TeamId => team;

        /// <summary>
        /// Gets the current health value.
        /// </summary>
        public float CurrentHealth => currentHealth;

        private void OnEnable()
        {
            currentHealth = Mathf.Clamp(currentHealth <= 0f ? maxHealth : currentHealth, 0f, maxHealth);
            if (!Registry.TryGetValue(team, out var list))
            {
                list = new List<Health>();
                Registry.Add(team, list);
            }

            if (!list.Contains(this))
            {
                list.Add(this);
            }
        }

        private void OnDisable()
        {
            if (Registry.TryGetValue(team, out var list))
            {
                list.Remove(this);
            }
        }

        /// <summary>
        /// Applies damage and disables the GameObject when health reaches zero.
        /// </summary>
        /// <param name="amount">Amount of health to remove.</param>
        public void ApplyDamage(float amount)
        {
            if (currentHealth <= 0f)
            {
                return;
            }

            currentHealth = Mathf.Clamp(currentHealth - Mathf.Abs(amount), 0f, maxHealth);
            if (currentHealth <= 0f)
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Returns a copy of the list of health components registered for the given team.
        /// </summary>
        /// <param name="team">Team to query.</param>
        public static List<Health> GetAllies(Team team)
        {
            if (!Registry.TryGetValue(team, out var list))
            {
                return new List<Health>();
            }

            return new List<Health>(list);
        }
    }
}
