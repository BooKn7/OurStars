using UnityEngine;

// プレイヤー接触時にトラップを生成・射出する
public class TrapSpawner : MonoBehaviour
{
    [Header("設定")]
    public GameObject movingWallPrefab;
    public Transform trapSpawnPoint;

    private bool hasSpawned = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!hasSpawned && (collision.CompareTag("Player1") || collision.CompareTag("Player2")))
        {
            Debug.Log("トラップ起動");
            if (movingWallPrefab != null && trapSpawnPoint != null)
            {
                Instantiate(movingWallPrefab, trapSpawnPoint.position, trapSpawnPoint.rotation);
            }
            hasSpawned = true; // 多重生成を防止
        }
    }
}
