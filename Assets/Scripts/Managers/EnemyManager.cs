using UnityEngine;
using System.Collections.Generic;
using Attrition.Controllers;

namespace Attrition.Managers
{
    public class EnemyManager : MonoBehaviour
    {
        public static EnemyManager Instance;

        // Danh sách quản lý quái theo ID để đồng bộ mạng
        private Dictionary<string, EnemyController> activeEnemies = new Dictionary<string, EnemyController>();

        void Awake()
        {
            Instance = this;
        }

        public void RegisterEnemy(string id, EnemyController enemy)
        {
            if (!activeEnemies.ContainsKey(id))
                activeEnemies.Add(id, enemy);
        }

        // Hàm này được gọi khi có gói tin từ Network báo quái bị trúng đòn
        public void SyncEnemyDamage(string id, int damage)
        {
            if (activeEnemies.ContainsKey(id))
            {
                activeEnemies[id].TakeDamage(damage);
            }
        }
    }
}