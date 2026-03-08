using UnityEngine;
using System.Collections;

// オブジェクトを複数のポイント間で巡回移動させる
public class MoveToPoints : MonoBehaviour
{
    [Header("移動ポイント")]
    public Transform[] targetPoints;

    [Header("移動速度")]
    public float speed = 2.0f;

    private int currentPointIndex = 0;

    private void Start()
    {
        if (targetPoints == null || targetPoints.Length == 0)
        {
            Debug.LogError("ターゲットポイントを1つ以上設定してください");
            enabled = false;
            return;
        }

        StartCoroutine(MoveToNextPoint());
    }

    private IEnumerator MoveToNextPoint()
    {
        while (true)
        {
            Vector3 targetPos = targetPoints[currentPointIndex].position;

            // ターゲット位置まで移動
            while (Vector3.Distance(transform.position, targetPos) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
                yield return null;
            }

            // 到達位置を補正
            transform.position = targetPos;

            // 次のポイントへ
            yield return new WaitForSeconds(0f);

            currentPointIndex = (currentPointIndex + 1) % targetPoints.Length;
        }
    }
}
