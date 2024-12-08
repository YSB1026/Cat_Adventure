using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;
    public CompositeCollider2D[] mapBoundaries; // Composite Collider 2D 맵 경계 
    
    [Header("현재 맵 경계")]
    private CompositeCollider2D currentBoundary; // 현재 맵 경계

    private Camera cam;
    private float cameraHalfWidth;
    private float cameraHalfHeight;

    void Start()
    {
        cam = Camera.main;

        // 카메라의 절반 크기
        cameraHalfHeight = cam.orthographicSize;
        cameraHalfWidth = cam.aspect * cameraHalfHeight;

        if (mapBoundaries.Length > 0)
        {
            currentBoundary = mapBoundaries[0];
        }
    }

    void LateUpdate()
    {
        if (player == null || currentBoundary == null) return;

        float clampedX = Mathf.Clamp(
            player.position.x,
            currentBoundary.bounds.min.x + cameraHalfWidth,
            currentBoundary.bounds.max.x - cameraHalfWidth
        );
        float clampedY = Mathf.Clamp(
            player.position.y,
            currentBoundary.bounds.min.y + cameraHalfHeight,
            currentBoundary.bounds.max.y - cameraHalfHeight
        );

        transform.position = new Vector3(clampedX, clampedY, transform.position.z);
    }

    public void SwitchBoundary(int mapIndex)
    {
        if (mapIndex < 0 || mapIndex >= mapBoundaries.Length)
        {
            Debug.LogError("잘못된 맵 인덱스입니다.");
            return;
        }

        currentBoundary = mapBoundaries[mapIndex];
    }
}
