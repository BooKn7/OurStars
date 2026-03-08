using UnityEngine;
using UnityEngine.UI;

// P2のエネルギーUIを更新する
public class P2EnergyBar : MonoBehaviour
{
    [Header("設定")]
    [Tooltip("P2のコントローラー")]
    public CircularPathP2 p2Script;

    [Tooltip("円形ゲージ表示用のImage")]
    public Image energyImage;

    [Header("表示オフセット")]
    public Vector3 offset = new Vector3(0f, 2f, 0f);

    private void Update()
    {
        if (p2Script == null || energyImage == null) return;

        // エネルギーの割合を計算して反映
        float fillValue = Mathf.Clamp01(p2Script.currentEnergy / p2Script.maxEnergy);
        energyImage.fillAmount = fillValue;

        // P2に追従させる
        transform.position = p2Script.transform.position + offset;
    }
}
