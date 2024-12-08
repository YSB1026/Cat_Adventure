using UnityEngine;
using System.Collections;

public abstract class CombatBase : MonoBehaviour
{
    [Header("Status")]
    public float maxHealth;
    public float currentHealth;
    public float damage = 10f;

    protected SpriteRenderer spriteRenderer;
    protected Animator anim;
    protected bool isFlashing = false;

    // 초기화
    protected virtual void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    // 데미지 입기
    public virtual void TakeDamage(float amount)
    {
        if(currentHealth <= 0) return;//공격할 수 없는경우 return

        currentHealth -= amount;
        if(!isFlashing)
            StartCoroutine(FlashDamage());
        Debug.Log($"{gameObject.name} took {amount} damage. Current Health: {currentHealth}");
    }

    protected virtual void Die()
    {
        Debug.Log($"{gameObject.name} has died.");
    }

    public virtual void Attack(CombatBase target)
    {
        if (target != null)
        {
            target.TakeDamage(damage);
            Debug.Log($"{gameObject.name} attacked {target.gameObject.name} for {damage} damage.");
        }
    }

    protected IEnumerator FlashDamage() {
        if (isFlashing) yield break;

        isFlashing = true;
        Color originalColor = spriteRenderer.color; 
        
        spriteRenderer.color = new Color(1f, 0f, 0f); 

        yield return new WaitForSeconds(0.2f);
        
        spriteRenderer.color = originalColor; 
        
        isFlashing = false;
    }
}
