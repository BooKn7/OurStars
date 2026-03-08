using UnityEngine;
using UnityEngine.SceneManagement;

// 2人のプレイヤーが同時に到達した際に次のシーンを読み込む
public class LoadSceneTrigger : MonoBehaviour
{
    private bool isLoading = false;
    private bool isPlayerOneInside = false;
    private bool isPlayerTwoInside = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isLoading) return;

        if (other.CompareTag("Player1")) isPlayerOneInside = true;
        else if (other.CompareTag("Player2")) isPlayerTwoInside = true;

        CheckAndLoadScene();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player1")) isPlayerOneInside = false;
        else if (other.CompareTag("Player2")) isPlayerTwoInside = false;
    }

    private void CheckAndLoadScene()
    {
        if (isPlayerOneInside && isPlayerTwoInside)
        {
            LoadNextScene();
        }
    }

    private void LoadNextScene()
    {
        if (isLoading) return;

        isLoading = true;
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int total = SceneManager.sceneCountInBuildSettings;
        int nextIndex = (currentIndex + 1) % total;

        SceneManager.LoadScene(nextIndex);
    }
}
