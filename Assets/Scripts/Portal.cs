using UnityEngine;
using System.Collections;

public class Portal : MonoBehaviour
{
    [Header("포탈 설정")]
    public GameObject targetPortal;    // 이동할 타겟 포탈
    public Vector2 exitOffset = Vector2.zero; // 이동 후 추가 Offset=

    [Header("맵 설정")]
    public GameObject targetMap;       // 이동 후 활성화할 맵
    public GameObject currentMap;      // 이동 후 비활성화할 맵
    
    [Header("카메라 limit 설정(0:main, 1:forst1, 2:forest2, 3:forest3, 4:Boss)")]
    public int targetMapIndex;         //맵 bounds 설정
    private CameraController cam;

    private bool isFirst = true;
    [System.Obsolete]
    void Awake()
    {
        // CameraBoundaryManager 찾기
        cam = FindObjectOfType<CameraController>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(isFirst && targetMap==null || currentMap == null || targetPortal ==null){//보스 포탈에서 save..
            if(GameManager.instance == null){
                Debug.Log("Game Manager is null");
                return;
            }
            isFirst = false;
            GameManager.instance.AddSavePoint(transform);
        }
        else if (collision.CompareTag("Player"))
        {
            // 플레이어 위치를 타겟 포탈로 이동
            collision.transform.position = targetPortal.transform.position + (Vector3)exitOffset;

            // 현재 맵 비활성화
            if (currentMap != null)
                currentMap.SetActive(false);

            // 타겟 맵 활성화
            if (targetMap != null)
                targetMap.SetActive(true);

            // 카메라 경계 변경
            if (cam != null)
            {
                cam.SwitchBoundary(targetMapIndex);
            }
        }
    }
}
