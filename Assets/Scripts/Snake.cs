using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snake : CombatBase
{
    public GameObject projectilePrefab; // 투사체 프리팹
    public GameObject tailPrefab; // 꼬리 프리팹
    public Transform[] tailSpawnPoints; // 꼬리 스폰 위치
    public CompositeCollider2D mapBoundary; 
    public float attackIntervalPhase1 = 4.0f; // 1페이즈 공격 간격
    public float attackIntervalPhase2 = 2.0f; // 2페이즈 공격 간격
    public GameObject corpsePrefab; 
    private Animator anim; // 애니메이터
    private float nextAttackTime = 0.0f;
    private int attackCount = 0;
    private bool isIdle = true;
    private bool isCoolDown = false;
    private float coolDownDamageTaken = 0f; // CoolDown 상태에서 받은 데미지
    private Transform player; 
    private SpriteRenderer spriteRenderer;

    protected override void Start()
    {
        base.Start();
        player = GameObject.FindGameObjectWithTag("Player").transform; // 플레이어를 태그로 찾기
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>(); // 애니메이터 컴포넌트 찾기
        StartCoroutine(BossAttackRoutine());
    }

    IEnumerator BossAttackRoutine()
    {
        while (true)
        {
            if (isCoolDown)
            {
                yield return null; // CoolDown 상태에서는 대기
            }
            else if (Time.time >= nextAttackTime)
            {
                if (mapBoundary.bounds.Contains(player.position))
                {
                    PerformAttack();
                    nextAttackTime = Time.time + (currentHealth > 50 ? attackIntervalPhase1 : attackIntervalPhase2);
                }
            }
            yield return null;
        }
    }

    void PerformAttack()
    {
        isIdle = false;
        anim.SetBool("isIdle", isIdle);

        int attackType = Random.Range(0, 2); // 0: 투사체, 1: 꼬리
        if (attackType == 0)
        {
            anim.SetTrigger("Attack");
            FireProjectile();
        }
        else if (attackType == 1)
        {
            anim.SetTrigger("Attack");
            StartCoroutine(ShowTailWarningAndActivate());
        }

        StartCoroutine(AttackRoutine());
        attackCount++;
        if (attackCount >= 3)
        {
            isCoolDown = true;
            StartCoroutine(CoolDownRoutine());
        }
    }

    IEnumerator AttackRoutine()
    {
        // 공격 애니메이션이 끝날 때까지 대기
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);

        anim.ResetTrigger("Attack");
        if (!isCoolDown) isIdle = true;
        anim.SetBool("isIdle", isIdle);
    }

    IEnumerator CoolDownRoutine()
    {
        anim.SetTrigger("CoolDown");
        coolDownDamageTaken = 0f;

        for (float timer = 0; timer < 7f; timer += Time.deltaTime)
        {
            if (!isCoolDown) yield break; // TakeDamage에서 종료될 경우 즉시 중단
            yield return null;
        }

        EndCoolDown();
    }

    void EndCoolDown()
    {
        isCoolDown = false;
        attackCount = 0;
        isIdle = true;
        anim.ResetTrigger("Attack");
        anim.SetBool("isIdle", isIdle);
    }

    void FireProjectile()
    {
        Vector3 firePosition = transform.position + transform.right; // 보스 앞에서 투사체 발사
        GameObject projectile = Instantiate(projectilePrefab, firePosition, Quaternion.identity);
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        Vector2 direction = (player.position - firePosition).normalized;
        rb.linearVelocity = direction * 5.0f; // 투사체 속도 설정

        // 충돌 처리
        StartCoroutine(CheckCollision(projectile));
    }

    IEnumerator CheckCollision(GameObject obj)
    {
        Collider2D objCollider = obj.GetComponent<Collider2D>();
        if (objCollider == null) yield break;

        while (obj != null)
        {
            Collider2D[] hits = Physics2D.OverlapBoxAll(obj.transform.position, objCollider.bounds.size, 0f);
            foreach (Collider2D hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    PlayerCombat playerCombat = hit.GetComponentInChildren<PlayerCombat>();
                    playerCombat.TakeDamage(damage);
                    if (obj.CompareTag("SnakeFire"))
                    {
                        Destroy(obj);
                    }
                    yield break;
                }

                if (obj.CompareTag("SnakeFire") && !mapBoundary.bounds.Contains(obj.transform.position))
                {
                    Destroy(obj); // 맵 경계를 벗어나면 투사체 삭제
                    yield break;
                }
            }
            yield return null;
        }
    }

    IEnumerator ShowTailWarningAndActivate()
    {
        int spawnPointIndex = Random.Range(0, tailSpawnPoints.Length);
        Vector3 tailPosition = tailSpawnPoints[spawnPointIndex].position; // 스폰 포인트 위치 사용

        // 경고 표시 나중에 구현
        //ShowWarning(tailPosition);

        // 경고 표시 후 꼬리 활성화
        yield return new WaitForSeconds(1.0f); // 경고 시간 (1초)

        GameObject tail = Instantiate(tailPrefab, tailPosition, Quaternion.identity);
        StartCoroutine(CheckCollision(tail));
        StartCoroutine(RaiseAndLowerTail(tail));
    }

    IEnumerator RaiseAndLowerTail(GameObject tail)
    {
        Vector3 originalPosition = tail.transform.position;
        Vector3 targetPosition = new Vector3(tail.transform.position.x, tail.transform.position.y + 5.0f, tail.transform.position.z); // +5까지 상승

        float timeSpeed = currentHealth > 50 ? 4.0f : 6.0f;
        // 상승
        while (tail.transform.position.y < targetPosition.y)
        {
            tail.transform.position = Vector3.MoveTowards(tail.transform.position, targetPosition, Time.deltaTime * timeSpeed); // 상승
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        while (tail.transform.position.y > originalPosition.y)
        {
            tail.transform.position = Vector3.MoveTowards(tail.transform.position, originalPosition, Time.deltaTime * 6.0f); // 하강
            yield return null;
        }

        // 하강 후 꼬리 삭제
        Destroy(tail);
    }

    public override void TakeDamage(float amount)
    {
        if (!isCoolDown) return;

        coolDownDamageTaken += amount;
        if (coolDownDamageTaken >= 50f)
        {
            isCoolDown = false;
            EndCoolDown();
        }

        base.TakeDamage(amount);
        StartCoroutine(FlashDamage());
    }


    private IEnumerator FlashDamage()
    {
        Color originalColor = spriteRenderer.color;

        spriteRenderer.color = new Color(1f, 0f, 0f); 

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
