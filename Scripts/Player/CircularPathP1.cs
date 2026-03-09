using System.Collections;
using UnityEngine;

public class CircularPathP1 : MonoBehaviour
{
    public Transform targetPTwo;

    public float speed = 1f;
    public float activationDistance = 2.5f; // 拘束開始の最小距離
    public float maxGrabDistance = 5f;    // 拘束維持の最大距離

    public float shakeMagnitude = 0.1f;
    public float shakeDuration = 0.2f;

    private float fixedRadius = 0f;
    private bool isCtrlPressed = false;
    private bool isShaking = false;
    private float prevRTValue = 0f;

    private LineRenderer lineRenderer;
    private LineRenderer maxGrabCircle;
    private Renderer objectRenderer;

    [Header("エネルギー")]
    public float currentEnergy = 100f;
    public float maxEnergy = 100f;
    public float energyCostOnPress = 30f;
    public float energyCostPerSecond = 10f;
    public float energyRecoveryPerSecond = 20f;
    public GameObject p1EnergyBarUI;
    public float circleMoveSpeed = 2f;

    private bool barVisible = false;
    private bool hideBarScheduled = false;

    [Header("サウンド")]
    public AudioSource gravityAudioSource;
    public AudioClip gravityLoopClip;
    public AudioClip gravityFailClip;
    public float gravityLoopInterval = 1f;

    private bool isGravityLooping = false;
    private Coroutine gravityLoopCo = null;

    [Header("P2発射")]
    public float launchForce = 10f;

    public float FixedRadius => fixedRadius;
    public bool IsCtrlPressed => isCtrlPressed;

    private void Start()
    {
        lineRenderer = CreateCircleRenderer("#FF814C", 0.5f, 0f);
        maxGrabCircle = CreateCircleRenderer("#FF814C", 0.35f, maxGrabDistance);
        objectRenderer = GetComponent<Renderer>();
        prevRTValue = 0f;

        if (p1EnergyBarUI != null) p1EnergyBarUI.SetActive(false);
    }

    private void Update()
    {
        // エネルギーの消費と回復
        if (isCtrlPressed)
        {
            currentEnergy -= energyCostPerSecond * Time.deltaTime;
            if (currentEnergy <= 0f)
            {
                currentEnergy = 0f;
                StopGravityMode();
            }

            // 拘束中のP2発射
            if (Input.GetButtonDown("JoystickA_P2"))
            {
                LaunchP2();
            }
        }
        else
        {
            currentEnergy += energyRecoveryPerSecond * Time.deltaTime;
            if (currentEnergy > maxEnergy) currentEnergy = maxEnergy;
        }

        CheckCtrlInput();

        // 拘束円の描画制御
        if (isCtrlPressed)
        {
            lineRenderer.enabled = true;
            DrawCircle(lineRenderer, fixedRadius);
            maxGrabCircle.enabled = false;
        }
        else
        {
            lineRenderer.enabled = false;
            maxGrabCircle.enabled = true;
            DrawCircle(maxGrabCircle, maxGrabDistance - 0.5f);
        }

        if (currentEnergy >= maxEnergy && barVisible && !hideBarScheduled)
        {
            StartCoroutine(HideBarAfterDelay(1f));
        }
    }

    // P1からP2の方向へ発射
    private void LaunchP2()
    {
        Vector2 dir = (targetPTwo.position - transform.position).normalized;
        Rigidbody2D rbP2 = targetPTwo.GetComponent<Rigidbody2D>();
        
        if (rbP2 != null)
        {
            rbP2.velocity = Vector2.zero;
            rbP2.AddForce(dir * launchForce, ForceMode2D.Impulse);
        }
        StopGravityMode();
    }

    // RT入力と拘束状態の管理
    private void CheckCtrlInput()
    {
        float currentRT = Input.GetAxis("JoystickRT_P1");
        bool justPressed = (currentRT > 0.5f && prevRTValue <= 0.5f);
        bool justReleased = (currentRT < 0.1f && prevRTValue >= 0.1f);

        if (justPressed)
        {
            float distance = Vector2.Distance(transform.position, targetPTwo.position);

            if (currentEnergy < energyCostOnPress || distance < activationDistance || distance > maxGrabDistance)
            {
                StartCoroutine(ShakeEffect());
                PlayFailSoundOnce();
                prevRTValue = currentRT;
                return;
            }

            if (!isCtrlPressed && ControlLock.TryActivateP1())
            {
                currentEnergy = Mathf.Max(0f, currentEnergy - energyCostOnPress);
                isCtrlPressed = true;
                lineRenderer.enabled = true;
                fixedRadius = distance;

                var p2Move = targetPTwo.GetComponent<Player2Move>();
                if (p2Move != null)
                {
                    p2Move.SetInitialAngle(CalculateAngle(transform.position, targetPTwo.position));
                }

                ShowEnergyBarUI();
                StartGravityLoop();
            }
            else
            {
                StartCoroutine(ShakeEffect());
                PlayFailSoundOnce();
            }
        }
        else if (justReleased)
        {
            if (currentEnergy >= maxEnergy && !hideBarScheduled)
                StartCoroutine(HideBarAfterDelay(1f));
                
            StopGravityMode();
        }

        prevRTValue = currentRT;
    }

    private void StartGravityLoop()
    {
        if (!isGravityLooping && gravityAudioSource != null && gravityLoopClip != null)
        {
            gravityLoopCo = StartCoroutine(GravityLoopCoroutine());
            isGravityLooping = true;
        }
    }

    private IEnumerator GravityLoopCoroutine()
    {
        isGravityLooping = true;
        while (true)
        {
            gravityAudioSource.PlayOneShot(gravityLoopClip);
            yield return new WaitForSeconds(gravityLoopInterval);
        }
    }

    private void StopGravityLoop()
    {
        if (isGravityLooping && gravityLoopCo != null)
        {
            StopCoroutine(gravityLoopCo);
            gravityLoopCo = null;
            isGravityLooping = false;
        }
    }

    private void PlayFailSoundOnce()
    {
        if (gravityAudioSource != null && gravityFailClip != null)
        {
            gravityAudioSource.PlayOneShot(gravityFailClip);
        }
    }

    public void StopGravityMode()
    {
        isCtrlPressed = false;
        ControlLock.ReleaseP1();
        lineRenderer.enabled = false;
        StopGravityLoop();
    }

    private float CalculateAngle(Vector3 origin, Vector3 target)
    {
        Vector2 toTarget = target - origin;
        return Mathf.Atan2(toTarget.y, toTarget.x);
    }

    private LineRenderer CreateCircleRenderer(string htmlHex, float alpha, float radius)
    {
        var circleRenderer = new GameObject("Circle").AddComponent<LineRenderer>();
        circleRenderer.transform.parent = transform;
        circleRenderer.material = new Material(Shader.Find("Sprites/Default"));
        circleRenderer.widthMultiplier = 0.05f;
        circleRenderer.positionCount = 360;
        circleRenderer.loop = true;
        circleRenderer.enabled = false;

        if (ColorUtility.TryParseHtmlString(htmlHex, out Color color))
        {
            color.a = alpha;
            circleRenderer.startColor = color;
            circleRenderer.endColor = color;
        }
        return circleRenderer;
    }

    private void DrawCircle(LineRenderer circleRenderer, float rad)
    {
        for (int i = 0; i < 360; i++)
        {
            float angleRad = Mathf.Deg2Rad * i;
            Vector3 pos = new Vector3(
                transform.position.x + Mathf.Cos(angleRad) * rad,
                transform.position.y + Mathf.Sin(angleRad) * rad,
                0f
            );
            circleRenderer.SetPosition(i, pos);
        }
    }

    // エラー時のシェイク演出
    private IEnumerator ShakeEffect()
    {
        // TODO: 震动效果有点太夸张了，暂时先调小点，等测试反馈再说
        if (isShaking) yield break;

        isShaking = true;
        Color originalColor = objectRenderer.material.color;

        float elapsed = 0f;
        Vector3 startPosition = transform.position;

        while (elapsed < shakeDuration)
        {
            transform.position = startPosition + (Vector3)(Random.insideUnitCircle * shakeMagnitude);
            objectRenderer.material.color = Color.red;
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = startPosition;
        objectRenderer.material.color = originalColor;
        isShaking = false;
    }

    private void ShowEnergyBarUI()
    {
        if (p1EnergyBarUI != null && !barVisible)
        {
            p1EnergyBarUI.SetActive(true);
            barVisible = true;
            hideBarScheduled = false;
        }
    }

    private IEnumerator HideBarAfterDelay(float delay)
    {
        hideBarScheduled = true;
        yield return new WaitForSeconds(delay);

        if (currentEnergy >= maxEnergy && barVisible)
        {
            p1EnergyBarUI.SetActive(false);
            barVisible = false;
        }
        hideBarScheduled = false;
    }
}
