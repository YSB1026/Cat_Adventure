using UnityEngine;
using System.Collections;
//깃허브 테스트입니다.
public class Butterfly : CombatBase
{
    public float moveSpeed = 2f; // 이동 속도
    public float changeDirectionInterval = 1f; // 방향 변경 간격
    
    [Header("나비 시체 프리팹")]
    public GameObject corpsePrefab; // 시체 프리팹 참조
    private Vector2 movementDirection; // 현재 이동 방향
    private Rigidbody2D rb; // Rigidbody2D 컴포넌트
    private SpriteRenderer spriteRenderer;

    protected override void Start()
    {
        base.Start(); // CombatBase 초기화
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        StartCoroutine(ChangeDirectionRoutine());
    }

    private void Update()
    {
        Move();
    }

    private void Move()
    {
        rb.linearVelocity = movementDirection * moveSpeed; // Rigidbody2D를 사용한 이동
    }
    private IEnumerator ChangeDirectionRoutine()
    {
        while (true)
        {
            // 랜덤한 방향 생성
            movementDirection = Random.insideUnitCircle.normalized;

            // 지정된 시간 간격 동안 대기
            yield return new WaitForSeconds(changeDirectionInterval);
        }
    }

    public override void Attack(CombatBase target)
    {
        /*
        Butterfly is neutral mob.
        Butterfly does not attack.
        */
        return;
    }

    public override void TakeDamage(float amount)
    {
        base.TakeDamage(amount);
        StartCoroutine(FlashDamage());
    }

    private IEnumerator FlashDamage()//데미지를 입었을때 피격 효과
    {
        Color originalColor = spriteRenderer.color;

        spriteRenderer.color = new Color(1f, 0.5f, 0f); 

        yield return new WaitForSeconds(0.2f);

        spriteRenderer.color = originalColor;
    }

    protected override void Die()
    {
        StopAllCoroutines();
        this.gameObject.SetActive(false);

        if (corpsePrefab != null)
        {
            corpsePrefab = Instantiate(corpsePrefab, transform.position, Quaternion.identity);
        }
    }

    public void Remove()
    {
        if (corpsePrefab != null)
        {
            Destroy(this);
            Destroy(corpsePrefab);
            corpsePrefab = null;
        }
    }
}
