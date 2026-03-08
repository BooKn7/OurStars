using UnityEngine;

// 一定間隔でSpikeheadを生成するスポナー
public class SpikeheadSpawner : MonoBehaviour
{
    [Header("スポーン設定")]
    public GameObject spikeheadPrefab;
    public float spawnInterval = 2f;
    public Transform[] spawnPoints;

    private float timer = 0f;
    private int currentSpawnIndex = 0;

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnSpikehead();
            timer = 0f;
        }
    }

    private void SpawnSpikehead()
    {
        if (spikeheadPrefab != null && spawnPoints != null && spawnPoints.Length > 0)
        {
            Transform point = spawnPoints[currentSpawnIndex];
            Instantiate(spikeheadPrefab, point.position, point.rotation);

            currentSpawnIndex = (currentSpawnIndex + 1) % spawnPoints.Length;
        }
    }
}
