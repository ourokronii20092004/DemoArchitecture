using UnityEngine;
using System.Collections.Generic;

namespace Attrition.Controllers
{
    public class EnemyController : MonoBehaviour, IDamageable
    {
        [Header("--- Multiplayer Sync ---")]
        public string monsterUniqueId; // ID để đồng bộ giữa các máy

        [Header("--- Components ---")]
        public Animator anim;
        public Rigidbody2D rb;
        private AxeDemonAI aiLogic; // Giữ logic AI tách biệt

        [Header("--- Stats ---")]
        public int maxHealth = 10;
        public int currentHealth;
        public float attackRange = 1.5f;
        public int damage = 1;

        private bool isDead = false;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            anim = GetComponent<Animator>();
            aiLogic = GetComponent<AxeDemonAI>();
            currentHealth = maxHealth;
        }

        // Hàm này sẽ được gọi bởi AttritionNetworkInterface khi Server báo quái mất máu
        public void TakeDamage(int damageAmount)
        {
            if (isDead) return;

            currentHealth -= damageAmount;

            if (currentHealth <= 0)
            {
                Die();
            }
            else
            {
                anim.SetTrigger("Hit");
                // Khi bị đánh, bắt AI phải quay lại nhìn kẻ thù gần nhất
                if (aiLogic) aiLogic.AlertAndFacePlayer();
            }
        }

        private void Die()
        {
            isDead = true;
            anim.SetBool("IsDead", true);
            rb.linearVelocity = Vector2.zero;
            GetComponent<Collider2D>().enabled = false;

            // Thông báo cho Manager xóa quái này khỏi danh sách quản lý
            // EnemyManager.Instance.UnregisterEnemy(monsterUniqueId);

            Destroy(gameObject, 1.5f);
        }

        public bool IsDead() => isDead;

        // Event gọi từ Animation để gây sát thương
        public void TriggerAttackDamage()
        {
            // Tìm người chơi trong tầm đánh
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    // Gửi lệnh gây sát thương lên mạng qua Network Interface
                    // AttritionNetworkInterface.Instance.SendDamage(hit.GetComponent<PlayerController>().playerId, damage);
                }
            }
        }
    }
}