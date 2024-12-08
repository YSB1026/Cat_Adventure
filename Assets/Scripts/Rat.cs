using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class Rat : CombatBase
{
    public float moveSpeed = 2f;
    public float slowSpeed = 1f;
    public float slowDuration = 1f; 
    public float changeDirectionInterval = 1f; 
    [Tooltip("피격시 플레이어 쫓아갈 수 있는 거리")]
    public float chaseRange = 5f; 

    [Tooltip("플레이어 놓치는 거리")]
    public float loseSightRange = 7f;

    [Header("쥐 시체 프리팹")]
    public GameObject corpsePrefab; 
    private Vector2 movementDirection; 
    private Rigidbody2D rb; 


    [Header("쥐 상태")]
    private bool isChasing = false; 
    private Transform playerTransform; 
    private bool isSlowed = false; //피격시 슬로우 효과


    protected override void Start()
    {
        base.Start(); // CombatBase 초기화
        rb = GetComponent<Rigidbody2D>();

        StartCoroutine(ChangeDirectionRoutine());
    }

    private void Update()
    {
        if (isChasing && playerTransform != null)
        {
            ChasePlayer();
        }
        else
        {
            Move();
        }

        CheckPlayerDistance(); // 플레이어 거리 체크
    }

    private void Move()
    {
        rb.linearVelocity = movementDirection * (isSlowed ? slowSpeed : moveSpeed); 
    }

    private IEnumerator ChangeDirectionRoutine()
    {
        while (!isChasing)
        {
            // 상하좌우로만 이동
            int randomDirection = Random.Range(0, 4);
            switch (randomDirection)
            {
                case 0: movementDirection = Vector2.up; break;    // 위로 이동
                case 1: movementDirection = Vector2.down; break;  // 아래로 이동
                case 2: movementDirection = Vector2.left; break;  // 왼쪽으로 이동
                case 3: movementDirection = Vector2.right; break; // 오른쪽으로 이동
            }

            anim.SetFloat("moveX",movementDirection.x);
            anim.SetFloat("moveY",movementDirection.y);

            // 방향 변경 간격
            yield return new WaitForSeconds(changeDirectionInterval);
        }
    }

    private void ChasePlayer()
    {
        Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
        rb.linearVelocity = directionToPlayer * (isSlowed ? slowSpeed : moveSpeed);
    }

    private void CheckPlayerDistance()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (isChasing)
        {
            if (distanceToPlayer > loseSightRange)
            {
                isChasing = false;
                StartCoroutine(ChangeDirectionRoutine());
            }
        }
        else
        {
            if (distanceToPlayer <= chaseRange)
            {
                isChasing = true;
                StopCoroutine(ChangeDirectionRoutine()); // 랜덤 이동 중단
            }
        }
    }

    public override void Attack(CombatBase target)
    {
        if (target != null)
        {
            base.Attack(target);
        }
    }

    public override void TakeDamage(float amount)
    {
        base.TakeDamage(amount);
        if (currentHealth <= 0)
        {
            Die();
        }

        Vector2 reverseDirection = -movementDirection; 
        transform.position += (Vector3)reverseDirection * moveSpeed * Time.deltaTime;
        
        StartCoroutine(ApplySlowEffect());

        // 공격받으면 추적 상태로 전환
        if (!isChasing)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                isChasing = true;
                StopAllCoroutines(); // 랜덤 이동 중단
            }
        }
    }

    private IEnumerator ApplySlowEffect()
    {
        if (isSlowed) yield break;

        isSlowed = true;
        yield return new WaitForSeconds(slowDuration); 
        isSlowed = false; 
    }

    protected override void Die()
    {
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
    
    private void OnCollisionEnter2D(Collision2D collision) { 
        if (collision.gameObject.CompareTag("Player")) { 
            PlayerCombat player = collision.gameObject.GetComponentInChildren<PlayerCombat>(); 
            if (player == null) return; 
            Attack(player); 
        } 
    }
}
