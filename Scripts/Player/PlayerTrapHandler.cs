using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// トラップ接触時のリスポーンと無敵時間の処理
public class PlayerTrapHandler : MonoBehaviour
{
    [Header("演出")]
    public GameObject particleEffectPrefab;
    public AudioClip deathSoundEffect;
    [Range(0f, 1f)] public float deathSoundVolume = 1f;
    public Renderer circleRenderer;

    [Header("リスポーン設定")]
    public Transform respawnPoint;
    public float upwardForce = 10f;
    public float inputDisableDuration = 1f;

    [Header("設定")]
    public int groundLayer = 8;
    public MonoBehaviour[] dontDisableOnDeath;

    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private Renderer[] renderers;
    private AudioSource audioSource;
    private bool isRespawning = false;

    private readonly Dictionary<MonoBehaviour, bool> disabledComponents = new Dictionary<MonoBehaviour, bool>();

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        renderers = GetComponentsInChildren<Renderer>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        if (deathSoundEffect)
        {
            audioSource.clip = deathSoundEffect;
            audioSource.playOnAwake = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Trap") && !isRespawning)
        {
            StartCoroutine(HandleTrapCollision());
        }
    }

    private IEnumerator HandleTrapCollision()
    {
        isRespawning = true;

        // 引力を強制解除
        StopGravityEffects();
        DisableAllInputComponents();

        // 死亡演出
        if (particleEffectPrefab != null)
        {
            Instantiate(particleEffectPrefab, transform.position, Quaternion.identity);
        }

        if (deathSoundEffect != null)
        {
            audioSource.PlayOneShot(deathSoundEffect, deathSoundVolume);
        }

        // 姿を隠す
        if (renderers != null)
        {
            foreach (var rend in renderers) rend.enabled = false;
        }

        if (circleRenderer != null) circleRenderer.enabled = false;
        if (playerCollider != null) playerCollider.enabled = false;

        float waitTime = deathSoundEffect != null ? Mathf.Max(1f, deathSoundEffect.length) : 1f;
        yield return new WaitForSeconds(waitTime);

        // リスポーン位置へ
        if (respawnPoint != null) transform.position = respawnPoint.position;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        if (playerCollider != null) playerCollider.enabled = true;

        Physics2D.IgnoreLayerCollision(gameObject.layer, groundLayer, true);

        if (rb != null) rb.AddForce(Vector2.up * upwardForce, ForceMode2D.Impulse);

        if (renderers != null)
        {
            foreach (var rend in renderers) rend.enabled = true;
        }

        if (circleRenderer != null) circleRenderer.enabled = true;

        // 一定時間入力を無効化
        yield return new WaitForSeconds(inputDisableDuration);

        RestoreInputComponents();
        StartCoroutine(WaitToRestoreCollision());

        isRespawning = false;
    }

    private IEnumerator WaitToRestoreCollision()
    {
        while (IsTouchingGround()) yield return null;
        Physics2D.IgnoreLayerCollision(gameObject.layer, groundLayer, false);
    }

    private bool IsTouchingGround()
    {
        var box = playerCollider as BoxCollider2D;
        if (box != null)
        {
            Vector2 size = box.size + Vector2.one * 0.1f;
            Vector2 pos = (Vector2)transform.position + box.offset;
            return Physics2D.OverlapBoxAll(pos, size, 0f, 1 << groundLayer).Length > 0;
        }

        var circle = playerCollider as CircleCollider2D;
        if (circle != null)
        {
            Vector2 pos = (Vector2)transform.position + circle.offset;
            return Physics2D.OverlapCircleAll(pos, circle.radius + 0.1f, 1 << groundLayer).Length > 0;
        }

        return false;
    }

    private void DisableAllInputComponents()
    {
        MonoBehaviour[] allComponents = GetComponentsInChildren<MonoBehaviour>();
        foreach (MonoBehaviour comp in allComponents)
        {
            if (comp == this) continue;

            bool skip = false;
            if (dontDisableOnDeath != null)
            {
                foreach (MonoBehaviour keep in dontDisableOnDeath)
                {
                    if (comp == keep)
                    {
                        skip = true;
                        break;
                    }
                }
            }

            if (!skip)
            {
                disabledComponents[comp] = comp.enabled;
                comp.enabled = false;
            }
        }
    }

    private void RestoreInputComponents()
    {
        foreach (var kvp in disabledComponents)
        {
            if (kvp.Value) kvp.Key.enabled = true;
        }
        disabledComponents.Clear();
    }

    private void StopGravityEffects()
    {
        var cp1 = GetComponent<CircularPathP1>();
        if (cp1 != null && cp1.IsCtrlPressed) cp1.StopGravityMode();

        var cp2 = GetComponent<CircularPathP2>();
        if (cp2 != null && cp2.IsCtrlPressed) cp2.StopGravityMode();

        CircularPathP1[] allCP1 = FindObjectsOfType<CircularPathP1>();
        foreach (var cp in allCP1)
        {
            if (cp.targetPTwo == transform && cp.IsCtrlPressed) cp.StopGravityMode();
        }

        CircularPathP2[] allCP2 = FindObjectsOfType<CircularPathP2>();
        foreach (var cp in allCP2)
        {
            if (cp.targetPOne == transform && cp.IsCtrlPressed) cp.StopGravityMode();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (playerCollider == null) return;
        Gizmos.color = Color.red;

        var box = playerCollider as BoxCollider2D;
        if (box != null)
        {
            Vector2 size = box.size + Vector2.one * 0.1f;
            Vector2 pos = (Vector2)transform.position + box.offset;
            Gizmos.DrawWireCube(pos, size);
            return;
        }

        var circle = playerCollider as CircleCollider2D;
        if (circle != null)
        {
            Vector2 pos = (Vector2)transform.position + circle.offset;
            Gizmos.DrawWireSphere(pos, circle.radius + 0.1f);
        }
    }
}
