using UnityEngine;
using System;

// コイン取得時の処理とCoinManagerへの通知
public class Coin2D : MonoBehaviour
{
    [Header("サウンド設定")]
    public AudioClip collectSoundEffect;
    [Range(0f, 1f)] public float collectSoundVolume = 1f;

    public AudioClip deathSoundEffect;
    [Range(0f, 1f)] public float deathSoundVolume = 1f;

    private AudioSource audioSource;
    private bool isCollected = false;

    public static event Action<Coin2D> OnCoinCollected;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError($"{name}: AudioSourceが見つかりません");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player1") || other.CompareTag("Player2"))
        {
            CollectCoin();
        }
    }

    private void CollectCoin()
    {
        if (isCollected) return;

        isCollected = true;
        OnCoinCollected?.Invoke(this);

        if (collectSoundEffect != null)
        {
            PlayTempSound(collectSoundEffect, collectSoundVolume, "Collect");
        }

        if (deathSoundEffect != null)
        {
            PlayTempSound(deathSoundEffect, deathSoundVolume, "Death");
        }

        GetComponent<Collider2D>().enabled = false;
        Destroy(gameObject);
    }

    private void PlayTempSound(AudioClip clip, float volume, string nameSuffix)
    {
        GameObject tempAudio = new GameObject($"TempAudio_{nameSuffix}");
        tempAudio.transform.position = transform.position;

        AudioSource tempSource = tempAudio.AddComponent<AudioSource>();
        tempSource.clip = clip;
        tempSource.volume = volume;
        tempSource.Play();

        Destroy(tempAudio, clip.length);
    }
}
