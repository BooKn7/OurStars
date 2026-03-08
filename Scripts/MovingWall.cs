using UnityEngine;

// 一定速度で上方向へ移動し続ける壁
public class MovingWall : MonoBehaviour
{
    [Header("移動速度")]
    public float speed = 5f;

    private void Update()
    {
        transform.Translate(Vector3.up * speed * Time.deltaTime);
    }
}
