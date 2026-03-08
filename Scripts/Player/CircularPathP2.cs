using UnityEngine;
using System.Collections;

// P2がRT入力でP1を円軌道に拘束する制御
public class CircularPathP2 : MonoBehaviour
{
    [Header("ターゲット")]
    public Transform targetPOne;

    [Header("基本設定")]
    public float speed = 1f;
    public float radiusDecreaseSpeed = 0.5f;
    public float minRadius = 2.5f;
    public float orbitSpeed = 2f;

    private float fixedRadius = 0f;
    private bool isCtrlPressed = false;
    private float angle = 0f;
    private LineRenderer lineRenderer;
    private Rigidbody2D p1Rigidbody;
    private Renderer p2Renderer;
    private Color originalColor;
    private bool isShaking = false;
    private float prevCtrlValue = 0f;

    [Header("エネルギー")]
    public float currentEnergy = 100f;
    public float maxEnergy = 100f;
    public float energyCostOnPress = 30f;
    public float energyCostPerSecond = 10f;
    public float energyRecoveryPerSecond = 20f;
    public GameObject p2EnergyBarUI;
    public float circleMoveSpeed = 2f;
    public float radiusExpandSpeed = 0.5f;

    private bool barVisible = false;
    private bool hideBarScheduled = false;

    [Header("サウンド")]
    public AudioSource gravityAudioSource;
    public AudioClip gravityLoopClip;
    public AudioClip gravityFailClip;
    public float gravityLoopInterval = 1f;

    private bool isGravityLooping = false;
    private Coroutine gravityLoopCo = null;

    public bool IsCtrlPressed => isCtrlPressed;

    private void Start()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.widthMultiplier = 0.1f;
        lineRenderer.positionCount = 2;

        Color lineColor = new Color(0.27f, 0.76f, 0.95f, 1f);
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        lineRenderer.enabled = false;

        p1Rigidbody = targetPOne.GetComponent<Rigidbody2D>();
        p2Renderer = GetComponent<Renderer>();
        originalColor = p2Renderer.material.color;

        if (p2EnergyBarUI != null) p2EnergyBarUI.SetActive(false);
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
        }
        else
        {
            currentEnergy += energyRecoveryPerSecond * Time.deltaTime;
            if (currentEnergy > maxEnergy) currentEnergy = maxEnergy;
        }

        CheckCtrlInput();

        // 拘束中の更新
        if (isCtrlPressed)
        {
            UpdateLine();
            UpdateRadius();
            UpdateP1Position();
        }

        if (currentEnergy >= maxEnergy && barVisible && !hideBarScheduled)
        {
            StartCoroutine(HideBarAfterDelay(1f));
        }
    }

    // RT入力と拘束状態の管理
    private void CheckCtrlInput()
    {
        float currentCtrl = Input.GetAxis("JoystickRT_P2");
        bool currentPressed = (currentCtrl > 0.5f);
        bool justPressed = (currentPressed && !(prevCtrlValue > 0.5f));
        bool justReleased = (!currentPressed && (prevCtrlValue > 0.5f));

        if (justPressed)
        {
            float distance = Vector2.Distance(transform.position, targetPOne.position);

            if (currentEnergy < energyCostOnPress || distance <= minRadius)
            {
                StartCoroutine(ShakeEffect());
                PlayFailSoundOnce();
                prevCtrlValue = currentCtrl;
                return;
            }

            if (ControlLock.TryActivateP2())
            {
                currentEnergy = Mathf.Max(0f, currentEnergy - energyCostOnPress);
                isCtrlPressed = true;
                lineRenderer.enabled = true;

                fixedRadius = distance;
                Vector2 toTarget = targetPOne.position - transform.position;
                angle = Mathf.Atan2(toTarget.y, toTarget.x);

                var playerMove = targetPOne.GetComponent<PlayerMove>();
                if (playerMove != null) playerMove.enabled = false;

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

        prevCtrlValue = currentCtrl;
    }

    // 半径の変更（P1のAボタンで拡大、その他は縮小）
    private void UpdateRadius()
    {
        if (Input.GetButton("JoystickA_P1"))
        {
            fixedRadius += radiusExpandSpeed * Time.deltaTime;
        }
        else
        {
            if (fixedRadius > minRadius)
            {
                fixedRadius -= radiusDecreaseSpeed * Time.deltaTime;
                if (fixedRadius < minRadius) fixedRadius = minRadius;
            }
        }
    }

    // P1をP2中心の円周上に移動させる
    private void UpdateP1Position()
    {
        float h = Input.GetAxis("JoystickLeftHorizontal_P1");
        float v = Input.GetAxis("JoystickLeftVertical_P1");

        float targetAngle = angle;
        if (Mathf.Abs(h) > Mathf.Abs(v))
        {
            if (h > 0.5f) targetAngle = 0f;
            else if (h < -0.5f) targetAngle = Mathf.PI;
        }
        else
        {
            if (v > 0.5f) targetAngle = Mathf.PI / 2f;
            else if (v < -0.5f) targetAngle = -Mathf.PI / 2f;
        }

        float deltaAngle = Mathf.DeltaAngle(angle * Mathf.Rad2Deg, targetAngle * Mathf.Rad2Deg);
        angle += Mathf.Sign(deltaAngle)
                 * Mathf.Min(Mathf.Abs(deltaAngle), circleMoveSpeed * Time.deltaTime)
                 * Mathf.Deg2Rad;

        float newX = transform.position.x + Mathf.Cos(angle) * fixedRadius;
        float newY = transform.position.y + Mathf.Sin(angle) * fixedRadius;

        p1Rigidbody.MovePosition(new Vector2(newX, newY));
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
        ControlLock.ReleaseP2();
        lineRenderer.enabled = false;

        StopGravityLoop();

        var playerMove = targetPOne.GetComponent<PlayerMove>();
        if (playerMove != null) playerMove.enabled = true;
    }

    // ラインの描画更新
    private void UpdateLine()
    {
        Vector3 p1Center = targetPOne.position + new Vector3(0f, 0.5f, 0f);
        Vector3 p2Center = transform.position + new Vector3(0f, 0.5f, 0f);

        Vector3 direction = (p2Center - p1Center).normalized;
        float offsetP1 = 0.3f;
        float offsetP2 = 0.5f;

        Vector3 startPoint = p1Center + direction * offsetP1;
        Vector3 endPoint = p2Center - direction * offsetP2;

        if (Vector3.Distance(startPoint, endPoint) < 0.1f)
        {
            endPoint = startPoint + direction * 0.1f;
        }

        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, endPoint);
    }

    // エラー時のシェイク演出
    private IEnumerator ShakeEffect()
    {
        if (isShaking) yield break;

        isShaking = true;
        Color original = p2Renderer.material.color;

        float elapsed = 0f;
        Vector3 startPosition = transform.position;

        while (elapsed < 0.2f)
        {
            transform.position = startPosition + (Vector3)(Random.insideUnitCircle * 0.05f);
            p2Renderer.material.color = Color.red;
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = startPosition;
        p2Renderer.material.color = original;
        isShaking = false;
    }

    private IEnumerator HideBarAfterDelay(float delay)
    {
        hideBarScheduled = true;
        yield return new WaitForSeconds(delay);

        if (currentEnergy >= maxEnergy && barVisible)
        {
            p2EnergyBarUI.SetActive(false);
            barVisible = false;
        }
        hideBarScheduled = false;
    }

    private void ShowEnergyBarUI()
    {
        if (p2EnergyBarUI != null && !barVisible)
        {
            p2EnergyBarUI.SetActive(true);
            barVisible = true;
            hideBarScheduled = false;
        }
    }
}
