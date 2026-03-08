using UnityEngine;
using System.Collections;

// プレイヤーのリスポーン管理を行う
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("リスポーン設定")]
    [Tooltip("プレイヤーのプレハブ (0=P1, 1=P2)")]
    public GameObject[] playerPrefabs;

    [Tooltip("リスポーン地点")]
    public Transform[] respawnPoints;

    [Tooltip("リスポーンまでの待機時間")]
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
