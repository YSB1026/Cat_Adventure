using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GameObject gameOverPanel; // 게임 오버 UI 패널
    private Transform savePoint; // 현재 세이브 포인트 위치
    public bool isGameOver {get; set;} // 게임 오버 상태
    public bool isBossDie {get; set;}

    // 보스 몬스터의 모든 이펙트 관리하는 HashSet
    private HashSet<GameObject> activeProjectiles = new HashSet<GameObject>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        isGameOver = false;
        isBossDie = false;
    }

    private void Start()
    {
        gameOverPanel.SetActive(false); // 시작 시 게임 오버 패널 비활성화
        Time.timeScale = 1f; // 게임 시간 정상 속도로 설정

        if(savePoint == null)
        {
            GameObject defaultSavePoint = new GameObject("DefaultSavePoint");
            defaultSavePoint.transform.position = new Vector3(-2.5f, -1.5f, 0);
            savePoint = defaultSavePoint.transform;
        }
    }

    private void Update()
    {
        if (isGameOver && Input.GetKeyDown(KeyCode.R))
        {
            Restart();
        }
        if(isBossDie){
            DestroyAllProjectiles();
        }

        CleanUpProjectiles();
    }

    // 동적으로 세이브 포인트를 추가하는 메서드
    public void AddSavePoint(Transform newSavePoint)
    {
        savePoint = newSavePoint;
        Debug.Log("New save point added at: " + newSavePoint.position);
    }

    public void GameOver()
    {
        gameOverPanel.SetActive(true); // 게임 오버 패널 활성화
        isGameOver = true; // 게임 오버 상태 설정
        Time.timeScale = 0f; // 게임 시간 일시 정지
        DestroyAllProjectiles();
    }

    public void Restart()
    {
        RespawnPlayer(); // 플레이어를 세이브 포인트에서 재시작
        gameOverPanel.SetActive(false); // 게임 오버 패널 비활성화
        isGameOver = false; // 게임 오버 상태 해제
        Time.timeScale = 1f; // 게임 시간 정상 속도로 설정
    }
    private void RespawnPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (savePoint == null)
        {
            Debug.LogError("세이브 포인트 없음.");
            return;
        }

        player.transform.position = savePoint.position;
        PlayerCombat playerCombat = player.GetComponentInChildren<PlayerCombat>();
        if(playerCombat==null){
            Debug.Log("PlayerCombat null임");
            return;
        }
        playerCombat.currentHealth = playerCombat.maxHealth;
        // 추가적인 초기화 작업이 있으면 추가할 것.
    }

    public void AddProjectile(GameObject obj)
    {
        activeProjectiles.Add(obj);
    }

    private void DestroyAllProjectiles()
    {
        foreach (GameObject projectile in activeProjectiles)
        {
            if (projectile != null)
            {
                Destroy(projectile);
            }
        }
        activeProjectiles.Clear();
    }

    

    // null 참조를 HashSet에서 제거하는 메서드
    private void CleanUpProjectiles()
    {
        activeProjectiles.RemoveWhere(projectile => projectile == null);
    }
}
