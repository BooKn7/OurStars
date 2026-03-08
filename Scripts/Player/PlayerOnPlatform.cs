using UnityEngine;

// プラットフォーム移動時にプレイヤーを追従させる
public class PlayerOnPlatform2D : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 platformPreviousPosition;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            platformPreviousPosition = collision.transform.position;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            Vector2 platformCurrentPosition = (Vector2)collision.transform.position;
            Vector2 platformMovement = platformCurrentPosition - platformPreviousPosition;

            rb.position += platformMovement;
            platformPreviousPosition = platformCurrentPosition;
        }
    }
}
