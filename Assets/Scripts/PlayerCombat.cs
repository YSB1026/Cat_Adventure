using UnityEngine;
using System.Collections;
using UnityEngine.Analytics;

public class PlayerCombat : CombatBase
{
    public float attackRange;//사거리
    public float effectRadius;//이펙트 반지름
    public float effetDuration = 0.5f; // 애니메이션 지속 시간

    [Header("이펙트 프리팹")]
    public GameObject attackEffectPrefab; // 이펙트 프리팹
    
    [HideInInspector]
    public bool isAttacking { get; set; }
    private Vector3 initEffectScale, lastEffectScale;//초기 이펙트크기, 사라지기전 이펙트 크기

    protected override void Start()
    {
        base.Start();
        spriteRenderer = GetComponentInParent<SpriteRenderer>(); // 부모의 SpriteRenderer
        initEffectScale = new Vector3(effectRadius * 2f, effectRadius * 2f, 1f);
        lastEffectScale = new Vector3(effectRadius, effectRadius, 1f);
    }


    public void PerformAttack(Vector3 attackDirection)
    {
        if (isAttacking)
            return;

        isAttacking = true;

        Vector3 normalizedDirection = attackDirection.normalized;
        Vector3 effectPosition = transform.position + normalizedDirection * attackRange;//이펙트 position 계산

        GameObject attackEffect = Instantiate(attackEffectPrefab, effectPosition, Quaternion.identity);//이펙트 생성
        attackEffect.transform.localScale = initEffectScale;//초기 크기 설정
        attackEffect.transform.up = normalizedDirection; //이펙트 방향

        // Coroutine을 통해 점진적으로 Scale 조정, gpt 도움받음.
        //sprite가 하나라 기존에 알고있던 방식으로 애니매이션으로 크기 조정하거나 위치조정하는식으로하니까
        //animator의 크기 or 위치가 우선되어서 내가 계산한 위치에 이펙트가 생기지 않았음.
        //gpt방법은 material만들어서 이펙트에 적용시키고 StartCoroutine 비동기 방식으로 크기조절.
        StartCoroutine(AnimateEffectScale(attackEffect));

        ApplyEffect(effectPosition);

        // 공격 상태 초기화
        Invoke(nameof(ResetAttack), 0.5f);
    }

    
    //이펙트가 초기 크기에서 1/2크기로 줄어들면서 점점 사라지는 애니메이션
    private IEnumerator AnimateEffectScale(GameObject effect)
    {
        float elapsed = 0f;

        if (effect == null)
        {
            Debug.LogError("Effect object is null!");
            yield break;
        }

        SpriteRenderer spriteRenderer = effect.GetComponent<SpriteRenderer>();

        Color initialColor = spriteRenderer.color;
        Color targetColor = new Color(initialColor.r, initialColor.g, initialColor.b, 0f); // Alpha 0

        while (elapsed < effetDuration)
        {
            if (effect == null)
            {
                Debug.LogWarning("Effect object destroyed during animation!");
                yield break;
            }

            elapsed += Time.deltaTime;
            float t = elapsed / effetDuration;

            // Scale과 Alpha 조정
            effect.transform.localScale = Vector3.Lerp(initEffectScale, lastEffectScale, t);
            spriteRenderer.color = Color.Lerp(initialColor, targetColor, t);

            yield return null;
        }

        if (effect != null)
        {
            effect.transform.localScale = lastEffectScale;
            spriteRenderer.color = targetColor;

            Destroy(effect);//이펙트 삭제
        }
    }

    private void ApplyEffect(Vector3 position)
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(position, effectRadius);

        foreach (Collider2D collider in hitEnemies)
        {
            CombatBase target = collider.GetComponent<CombatBase>();
            if (target != null && target != this) // 자신은 제외하고 공격해야함.
            {
                base.Attack(target);
            }
        }
    }

    private void ResetAttack()
    {
        isAttacking = false;
    }

    public override void TakeDamage(float amount)
    {
        base.TakeDamage(amount);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected override void Die()
    {
        base.Die();
        GameManager.instance.GameOver();
        // 추가 효과나 사운드 나중에 구현
    }
}
