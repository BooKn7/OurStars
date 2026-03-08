using UnityEngine;

// プレイヤーのエネルギーを回復させるアイテム
public class EnergyPickup : MonoBehaviour
{
    [Header("回復設定")]
    public float energyToRestore = 100f;

    [Header("表示・演出")]
    public bool destroyOnPickup = true;
    public GameObject pickupEffect;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player1"))
        {
            CircularPathP1 p1 = other.GetComponent<CircularPathP1>();
            if (p1 != null)
            {
                p1.currentEnergy = Mathf.Min(p1.currentEnergy + energyToRestore, p1.maxEnergy);
                Debug.Log("P1のエネルギーを回復しました");
                HandlePickup();
            }
        }
        else if (other.CompareTag("Player2"))
        {
            CircularPathP2 p2 = other.GetComponent<CircularPathP2>();
            if (p2 != null)
            {
                p2.currentEnergy = Mathf.Min(p2.currentEnergy + energyToRestore, p2.maxEnergy);
                Debug.Log("P2のエネルギーを回復しました");
                HandlePickup();
            }
        }
    }

    private void HandlePickup()
    {
        if (pickupEffect != null)
        {
            Instantiate(pickupEffect, transform.position, Quaternion.identity);
        }

        if (destroyOnPickup)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
