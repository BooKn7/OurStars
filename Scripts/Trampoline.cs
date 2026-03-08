using UnityEngine;

// プレイヤーが触れると上へ跳ね返すトランポリン
public class Trampoline : MonoBehaviour
{
    [Header("跳ね返す力")]
    public float jumpForce = 20f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player1") || collision.gameObject.CompareTag("Player2"))
        {
            Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
            rb.AddForce(Vector3.up * jumpForce, ForceMode2D.Impulse);
        }
    }
}
