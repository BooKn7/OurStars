using UnityEngine;

/// <summary>
/// コインの回転アニメーション制御。
/// Y軸方向に 0〜-90 度の範囲で往復回転させる。
/// </summary>
public class CoinRotation : MonoBehaviour
{
    [Header("回転設定")]
    [Tooltip("回転速度（度/秒）")]
    public float rotationSpeed = 50f;

    private void Update()
    {
        // 時間経過に応じて0〜90度間で往復回転
        float angle = Mathf.PingPong(Time.time * rotationSpeed, 90);
        transform.rotation = Quaternion.Euler(0f, -angle, 0f);
    }

    // 触れたら即削除（衝突時）
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Destroy(gameObject);
    }
}
