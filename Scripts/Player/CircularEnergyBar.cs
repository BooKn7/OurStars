using UnityEngine;
using UnityEngine.UI;

// 汎用の円形エネルギーバーUI
public class CircularEnergyBar : MonoBehaviour
{
    [Header("設定")]
    public Image energyImage;

    [Header("エネルギー状態")]
    public float currentEnergy;
    public float maxEnergy = 100f;

    private void Update()
    {
        currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);
        
        float fillValue = (maxEnergy > 0f) ? currentEnergy / maxEnergy : 0f;
        if (energyImage != null)
        {
            energyImage.fillAmount = fillValue;
        }
    }
}
