using UnityEngine;
using System.Collections.Generic;

// すべてのコインが取得された際に特定オブジェクトを置き換える
public class CoinManager : MonoBehaviour
{
    [Header("置き換え設定")]
    public GameObject objectToReplace;
    public GameObject replacementObject;

    private List<Coin2D> allCoins = new List<Coin2D>();

    private void OnEnable()
    {
        Coin2D.OnCoinCollected += HandleCoinCollected;
    }

    private void OnDisable()
    {
        Coin2D.OnCoinCollected -= HandleCoinCollected;
    }

    private void Start()
    {
        Coin2D[] coins = FindObjectsOfType<Coin2D>();
        allCoins.AddRange(coins);
        Debug.Log($"シーン内のコイン数: {allCoins.Count}");
    }

    private void HandleCoinCollected(Coin2D collectedCoin)
    {
        if (allCoins.Contains(collectedCoin))
        {
            allCoins.Remove(collectedCoin);
            Debug.Log($"残りコイン数: {allCoins.Count}");

            if (allCoins.Count == 0)
            {
                AllCoinsCollected();
            }
        }
    }

    private void AllCoinsCollected()
    {
        Debug.Log("すべてのコインを取得しました");

        if (objectToReplace != null && replacementObject != null)
        {
            Vector3 spawnPosition = objectToReplace.transform.position;
            replacementObject.transform.position = spawnPosition;

            if (!replacementObject.activeInHierarchy)
            {
                replacementObject.SetActive(true);
            }

            Destroy(objectToReplace);
            Debug.Log("オブジェクトを置き換えました");
        }
        else
        {
            if (objectToReplace == null) Debug.LogError("ObjectToReplaceが未設定です");
            if (replacementObject == null) Debug.LogError("ReplacementObjectが未設定です");
        }
    }
}
