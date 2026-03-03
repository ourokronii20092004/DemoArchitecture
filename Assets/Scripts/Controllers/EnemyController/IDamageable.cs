using UnityEngine;

namespace Attrition.Controllers
{
    /// <summary>
    /// Bất kỳ đối tượng nào có thể bị nhận sát thương (Player, Quái, Vật phẩm) 
    /// đều phải kế thừa Interface này.
    /// </summary>
    public interface IDamageable
    {
        // Hàm nhận sát thương cơ bản
        void TakeDamage(int damage);

        // Bạn có thể thêm các hàm bổ trợ nếu cần trong tương lai
        // void TakeKnockback(Vector2 direction, float force);
        // bool IsDead();
    }
}