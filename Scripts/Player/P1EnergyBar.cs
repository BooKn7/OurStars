using UnityEngine;
using UnityEngine.UI;

// P1のエネルギーUIを更新する
public class P1EnergyBar : MonoBehaviour
{
    [Header("設定")]
    [Tooltip("P1のコントローラー")]
    public CircularPathP1 p1Script;

    [Tooltip("円形ゲージ表示用のImage")]
    public Image energyImage;

    [Header("表示オフセット")]
    public Vector3 offset = new Vector3(1.2f, 1.8f, 0f);

    private void Update()
    {
        if (p1Script == null || energyImage == null) return;

        // エネルギーの割合を計算して反映
        float fillValue = Mathf.Clamp01(p1Script.currentEnergy / p1Script.maxEnergy);
        energyImage.fillAmount = fillValue;

        // P1に追従させる
        transform.position = p1Script.transform.position + offset;
    }
}
