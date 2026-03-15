using UnityEngine;

namespace Islebound.Player
{
    [RequireComponent(typeof(PlayerStats))]
    public class DamageablePlayer : MonoBehaviour
    {
        private PlayerStats playerStats;

        private void Awake()
        {
            playerStats = GetComponent<PlayerStats>();
        }

        public void TakeDamage(float amount)
        {
            if (playerStats == null)
                return;

            playerStats.SetHealth(playerStats.CurrentHealth - amount);
        }
    }
}