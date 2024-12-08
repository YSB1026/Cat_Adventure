using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region Variables
    #region public
    [Header("플레이어 속성")]
    public float walkSpeed = 1.5f, runSpeed = 2.5f, specialIdleInterval;  // 걷기 속도, 달리기 속도,특수 idle상태 진입에 걸리는 시간

    [Header("상호작용 핸들러")]
    public InteractManager  interactManager;

    [Header("전투 핸들러")]
    public PlayerCombat playerCombat;

    #endregion

    #region private
    private float currentSpeed, h, v,specialIdleTimer;//현재 이동속도, Horizontal, Vertical 입력 값, 특수 Idle 상태로 진입하기 위해 timer 측정 시간
    private float lastMoveX = 0f, lastMoveY = -1f; // 마지막 입력 방향 (Idle 전환 시 유지)
    private bool isMoving,isRunning,isInSpecialIdle;//,isAttacking;// 캐릭터 이동 여부, 달리기 여부,특수 Idle 상태 여부
    private Rigidbody2D rigid;   // Rigidbody2D 컴포넌트
    private Animator anim;       // Animator 컴포넌트
    private Vector2 movement, viewDir;    // 이동 방향 벡터, 보는 방향 벡터
    private GameObject scanObj, prevScanObj;//현재 ray에 의해 detection된 object, 마지막 대화 상호작용때 detection된 obejct
    private System.Random random;
    #endregion

    #endregion

    #region Unity Methods
    void Awake()
    {
        int seed = gameObject.GetInstanceID() ^ System.DateTime.Now.Ticks.GetHashCode();
        random = new System.Random(seed); 

        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        HandleInput();
        UpdateMovement();
        InteractObject();
        UpdateAnimator();
    }

    // 물리 업데이트: Rigidbody2D 이동 처리
    void FixedUpdate()
    {
        MoveCharacter();
        ScanObject();
    }
    #endregion

    #region Custom Methods

    #region Update
    void HandleInput()// 키 입력 처리
    {
        //if(Input.GetMouseButtonDown(0)) isAttacking = true;
        h = interactManager.isTalk || playerCombat.isAttacking? 0 : Input.GetAxisRaw("Horizontal"); // 좌우 입력
        v = interactManager.isTalk || playerCombat.isAttacking ? 0 :Input.GetAxisRaw("Vertical");   // 상하 입력\
    }

    void UpdateMovement()
    {
        isMoving = h != 0 || v != 0;        // 이동 여부
        // 달리기 처리: Left Shift 누른 상태.
        isRunning = Input.GetKey(KeyCode.LeftShift) && isMoving; //달리기 여부

        if (isMoving)//키를 땠을때 어느 방향을 보고있는지 저장하기 위함.
        {
            lastMoveX = h; 
            lastMoveY = v;
        }

        currentSpeed = isRunning ? runSpeed : walkSpeed; //달릴때는 runSpeed, 아닐때는 walkSpeed
        viewDir = new Vector2(lastMoveX,lastMoveY).normalized;
        movement = new Vector2(h, v).normalized;
    }

    void UpdateAnimator()
    {
        anim.SetFloat("moveX", isMoving ? h : lastMoveX);
        anim.SetFloat("moveY", isMoving ? v : lastMoveY);
        anim.SetBool("isMoving", isMoving);
        anim.SetBool("isRunning", isRunning);
        if(playerCombat.isAttacking) anim.SetTrigger("Attack");
        CheckSpecialIdle(); 
    }
    void CheckSpecialIdle()    // 특수 Idle 상태(sleep, grooming)을 위한 로직
    {
        bool isActing = isMoving || interactManager.isTalk || playerCombat.isAttacking;

        if(isActing){
            EndSpecialIdle();
        }
        else if (!isActing && !isInSpecialIdle) // 특수 idle 상태
        {
            specialIdleTimer += Time.deltaTime;
            if (specialIdleTimer >= specialIdleInterval) 
            {
                TriggerSpecialIdle(); 
                specialIdleTimer = 0f; 
            }
        }
        else if (isActing && isInSpecialIdle) //움직인 경우 idle 상태 해제
        {
            EndSpecialIdle();
        }
    }
    void TriggerSpecialIdle()// 특수 Idle 상태
    {
        isInSpecialIdle = true;

        int randomIdle = random.Next(0, 2); // 0: Sleep, 1: Grooming
        anim.SetInteger("specialIdleType", randomIdle); 
        anim.SetTrigger("specialIdleTrigger");        

        //DebugMessage($"Entered Special Idle: Type = {randomIdle}");
    }

    void EndSpecialIdle()
    {
        if (!isInSpecialIdle) return;

        specialIdleTimer = 0f;
        isInSpecialIdle = false;

        //DebugMessage("Exited Special Idle");
    }
    void InteractObject(){
        if(Input.GetButtonDown("Jump")&&scanObj!=null){//스페이스바 눌렀을 때 scan된 object가 있으면 대화 시작\
            prevScanObj = scanObj;//대각선 이동중에 대화를 걸면 대화가 안넘어가는 버그가 있어서 만든 로직
            interactManager.HandleInteraction(scanObj);//interact manager에서 talk시작.
    
            if(interactManager.isTalk){//대각선 이동중에 대화를 걸면 대화가 안넘어가는 버그가 있어서 만든 로직
                scanObj = prevScanObj;
                anim.SetBool("isInteracting", true);
            }
            else{
                anim.SetBool("isInteracting", false);
            }
        }
        else if(Input.GetMouseButtonDown(0) && !playerCombat.isAttacking && !interactManager.isTalk){
            playerCombat.PerformAttack(attackDirection:viewDir);
            Invoke(nameof(EndAttack), 0.5f); // 공격 애니메이션 시간(1초)뒤 isAttacking false;
        }
    }
    void EndAttack()
    {
        anim.ResetTrigger("Attack");
    }
    #endregion

    #region FixedUpdate
    void MoveCharacter()
    {
        rigid.linearVelocity = movement * currentSpeed;
    }

    void ScanObject(){//ray 사용
        Debug.DrawRay(rigid.position, (Vector3)viewDir *1f, new Color(0,1,0));
        RaycastHit2D rayHit = Physics2D.Raycast(rigid.position, (Vector3)viewDir, 
                                1f,LayerMask.GetMask("Object"));
        Collider2D rayCollider = rayHit.collider;
        scanObj = rayCollider != null ? rayCollider.gameObject : null;
    }

    #endregion

    void DebugMessage(string message)
    { 
        #if UNITY_EDITOR
        Debug.Log(message);
        #endif
    }
    #endregion
}
