using UnityEngine;

public abstract class CombatBase : MonoBehaviour
{
    [Header("Status")]
    public float maxHealth;
    public float currentHealth;
    public float damage = 10f;

    // 초기화
    protected virtual void Start()
    {
        currentHealth = maxHealth;
    }

    // 데미지 입기
    public virtual void TakeDamage(float amount)
    {
        if(currentHealth <= 0) return;//공격할 수 없는경우 return

        currentHealth -= amount;
        Debug.Log($"{gameObject.name} took {amount} damage. Current Health: {currentHealth}");
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // 죽음 처리
    protected virtual void Die()
    {
        Debug.Log($"{gameObject.name} has died.");
    }

    // 공격
    public virtual void Attack(CombatBase target)
    {
        if (target != null)
        {
            target.TakeDamage(damage);
            Debug.Log($"{gameObject.name} attacked {target.gameObject.name} for {damage} damage.");
        }
    }
}
