using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;

    public AudioClip scene00Music;
    public AudioClip scene01To05Music;
    public AudioClip scene06To07Music;

    [Range(0f, 1f)]
    public float musicVolume = 1f;

    private AudioSource audioSource;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.loop = true;
        audioSource.volume = musicVolume;
        audioSource.playOnAwake = false;

        SetMusicForScene(SceneManager.GetActiveScene().name);
        if (audioSource.clip != null) audioSource.Play();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetMusicForScene(scene.name);
    }

    private void SetMusicForScene(string sceneName)
    {
        AudioClip newMusic;
        if (sceneName == "00") newMusic = scene00Music;
        else if (sceneName == "06" || sceneName == "07") newMusic = scene06To07Music;
        else newMusic = scene01To05Music;

        // TODO: 切背景音乐的时候最好加个淡入淡出，现在太硬了
        ChangeMusic(newMusic);
    }

    public void ChangeMusic(AudioClip newMusic)
    {
        if (newMusic == null) return;

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.loop = true;
            audioSource.volume = musicVolume;
            audioSource.playOnAwake = false;
        }

        if (audioSource.clip != newMusic)
        {
            audioSource.Stop();
            audioSource.clip = newMusic;
            audioSource.Play();
        }
    }
}
