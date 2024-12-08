using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snake : CombatBase
{
    public GameObject projectilePrefab; // 투사체 프리팹
    public GameObject tailPrefab; // 꼬리 프리팹
    public Transform[] tailSpawnPoints; // 꼬리 스폰 위치
    public CompositeCollider2D mapBoundary; 
    public float attackIntervalPhase1 = 3.5f; // 1페이즈 공격 간격
    public float attackIntervalPhase2 = 1.5f; // 2페이즈 공격 간격
    public GameObject corpsePrefab; 
    private float nextAttackTime = 0.0f;
    private int attackCount = 0;
    private bool isIdle = true;
    private bool isCoolDown = false;
    private float coolDownDamageTaken = 0f; // CoolDown 상태에서 받은 데미지
    private Transform player; 
    private float HALF_HP;

    protected override void Start()
    {
        base.Start();
        HALF_HP = maxHealth/2;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        StartCoroutine(BossAttackRoutine());
    }

    void Update(){
        if(GameManager.instance.isGameOver){
            ResetAll();
        }
    }

    void ResetAll(){
        currentHealth = maxHealth;
        nextAttackTime = 0.0f;
        attackCount = 0;
        isIdle = true;
        isCoolDown = false;
        coolDownDamageTaken = 0;

        anim.ResetTrigger("Attack");
        anim.ResetTrigger("CoolDown");
        anim.SetBool("isIdle",isIdle);
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
                    yield return new WaitForSeconds(1f);//플레이어가 입장하거나, 그루기 상태가 풀리자마자 공격하는걸 막기위함
                    PerformAttack();
                    nextAttackTime = Time.time + (currentHealth > HALF_HP ? attackIntervalPhase1 : attackIntervalPhase2);
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
            int projectileCnt = (currentHealth > HALF_HP) ? 2 : 3;
            FireProjectile(projectileCnt);
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

   void FireProjectile(int projectileCount)
    {
        Vector3 firePosition = transform.position + transform.right / 2;
        Vector3 playerPosition = player.position; // 플레이어의 위치

        for (int i = 0; i < projectileCount; i++)
        {
            GameObject projectile = Instantiate(projectilePrefab, firePosition, Quaternion.identity);
            GameManager.instance.AddProjectile(projectile);
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();

            // 플레이어 방향으로 발사 방향 설정
            Vector2 direction = (playerPosition - firePosition).normalized;

            // 각 투사체 간의 각도 차이 추가
            float angleStep = 10f; // 각 투사체 간의 각도 차이
            float angle = (i - (projectileCount - 1) / 2.0f) * angleStep; // 중심에서 양쪽으로 퍼지도록 계산
            direction = Quaternion.Euler(0, 0, angle) * direction;

            rb.linearVelocity = direction * 4.5f; // 투사체 속도 설정

            // 충돌 처리
            StartCoroutine(CheckCollision(projectile));
        }
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
        Vector3 tailPosition = tailSpawnPoints[spawnPointIndex].position;

        // 경고 표시 나중에 구현
        //ShowWarning(tailPosition);

        // 경고 표시 후 꼬리 활성화
        yield return new WaitForSeconds(1.0f); // 경고 시간 (1초)

        GameObject tail = Instantiate(tailPrefab, tailPosition, Quaternion.identity);
        GameManager.instance.AddProjectile(tail);
        StartCoroutine(CheckCollision(tail));
        StartCoroutine(RaiseAndLowerTail(tail));
    }

    IEnumerator RaiseAndLowerTail(GameObject tail)
    {
        Vector3 originalPosition = tail.transform.position;
        Vector3 targetPosition = new Vector3(tail.transform.position.x, tail.transform.position.y + 5.0f, tail.transform.position.z); // +5까지 상승

        float timeSpeed = currentHealth > HALF_HP ? 4.0f : 6.0f;

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
        if (coolDownDamageTaken >= 50)
        {
            isCoolDown = false;
            EndCoolDown();
        }

        base.TakeDamage(amount);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected override void Die()
    {
        Debug.Log("snake die");
        GameManager.instance.isBossDie = true;
        StopAllCoroutines();
        this.gameObject.SetActive(false);
        Vector3 temp = transform.position;
        temp.y -= 0.5f;
        if (corpsePrefab != null)
        {
            corpsePrefab = Instantiate(corpsePrefab, temp, Quaternion.identity);
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
