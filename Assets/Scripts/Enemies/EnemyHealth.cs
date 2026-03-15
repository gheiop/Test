using UnityEngine;

namespace Islebound.Enemies
{
    public class EnemyHealth : MonoBehaviour
    {
        [SerializeField] private EnemyData enemyData;
        [SerializeField] private float currentHealth;

        public float CurrentHealth => currentHealth;

        private void Start()
        {
            if (enemyData != null)
                currentHealth = enemyData.maxHealth;
        }

        public void TakeDamage(float amount)
        {
            currentHealth -= amount;

            if (currentHealth <= 0f)
                Die();
        }

        private void Die()
        {
            Destroy(gameObject);
        }
    }
}