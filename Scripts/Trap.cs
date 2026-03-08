using UnityEngine;
using UnityEngine.SceneManagement;

// プレイヤー接触時に現在のシーンを再読み込みする基本的なトラップ
public class Trap : MonoBehaviour
{
    private int sceneIndex;

    private void Start()
    {
        sceneIndex = SceneManager.GetActiveScene().buildIndex;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player1") || collision.gameObject.CompareTag("Player2"))
        {
            SceneManager.LoadScene(sceneIndex);
        }
    }
}
