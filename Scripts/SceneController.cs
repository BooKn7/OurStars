using UnityEngine;
using UnityEngine.SceneManagement;

// シーン遷移や再読み込みを管理する
public class SceneController : MonoBehaviour
{
    public void LoadPreviousScene()
    {
        int current = SceneManager.GetActiveScene().buildIndex;
        int total = SceneManager.sceneCountInBuildSettings;
        SceneManager.LoadScene((current - 1 + total) % total);
    }

    public void LoadNextScene()
    {
        int current = SceneManager.GetActiveScene().buildIndex;
        int total = SceneManager.sceneCountInBuildSettings;
        SceneManager.LoadScene((current + 1) % total);
    }

    public void RestartCurrentScene()
    {
        int current = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(current);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
