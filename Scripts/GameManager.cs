using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject[] playerPrefabs;
    public Transform[] respawnPoints;
    public float respawnDelay = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RespawnPlayer(int playerID)
    {
        // 这里的逻辑有点暴力，后面如果加了复杂的转场可能得重写
        StartCoroutine(RespawnCoroutine(playerID));
    }

    private IEnumerator RespawnCoroutine(int playerID)
    {
        yield return new WaitForSeconds(respawnDelay);

        if (playerPrefabs == null || respawnPoints == null ||
            playerID < 0 || playerID >= playerPrefabs.Length || playerID >= respawnPoints.Length)
        {
            Debug.LogError("GameManager: プレハブかリスポーン地点の設定が不足しています");
            yield break;
        }

        Instantiate(playerPrefabs[playerID], respawnPoints[playerID].position, Quaternion.identity);
    }
}
