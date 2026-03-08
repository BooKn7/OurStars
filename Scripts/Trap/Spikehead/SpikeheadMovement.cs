using UnityEngine;

// トゲ付き壁（Spikehead）をX軸マイナス方向へ直進させる
public class SpikeheadMovement : MonoBehaviour
{
    [Header("移動速度")]
    public float speed = 2f;

    private void Update()
    {
        transform.position += new Vector3(-speed * Time.deltaTime, 0f, 0f);
    }
}
