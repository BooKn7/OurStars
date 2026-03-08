using UnityEngine;
using System.Collections;

public class Player2Move : MonoBehaviour
{
    [Header("移動・ジャンプ")]
    public float normalSpeed = 5f;
    public float gravityModeSpeed = 0.5f;
    public float jumpForce = 5f;
    public float circleMoveSpeed = 2f;
    public int maxJumps = 2;

    [Header("地面判定")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("側面判定")]
    [SerializeField] private Transform leftCheck;
    [SerializeField] private Transform rightCheck;
    [SerializeField] private float sideCheckRadius = 0.15f;

    [Header("サウンド(P2)")]
    public AudioSource audioSource;
    public AudioClip[] runClips;
    public AudioClip jumpClip;
    public AudioClip landClip;
    public float footstepInterval = 0.5f;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private CircularPathP1 p1Script;

    private bool isGrounded = false;
    private bool isOnGravityMode = false;
    private float angle = 0f;
    private int jumpCount = 0;
    private bool isLeftBlocked = false;
    private bool isRightBlocked = false;
    private bool wasGroundedLastFrame = false;
    private bool isFootstepLooping = false;
    private Coroutine footstepCoroutine = null;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        p1Script = FindObjectOfType<CircularPathP1>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        wasGroundedLastFrame = false;
        isFootstepLooping = false;
    }

    private void Update()
    {
        CheckGrounded();
        CheckSides();

        // P1側の拘束状態を確認
        isOnGravityMode = (p1Script != null && p1Script.IsCtrlPressed);

        if (isOnGravityMode)
        {
            MoveOnCircle(gravityModeSpeed);
        }
        else
        {
            NormalMovement(normalSpeed);
        }

        // ジャンプ
        if (Input.GetButtonDown("JoystickA_P2"))
        {
            if (jumpCount < maxJumps)
            {
                Jump();
                jumpCount++;
            }
        }

        UpdateAnimator();
        HandleFootstepLoop();

        // 着地音
        if (isGrounded && !wasGroundedLastFrame)
        {
            if (audioSource != null && landClip != null)
            {
                audioSource.PlayOneShot(landClip);
            }
        }

        wasGroundedLastFrame = isGrounded;
    }

    // 通常の左右移動
    private void NormalMovement(float speed)
    {
        float horizontalInput = Input.GetAxis("JoystickLeftHorizontal_P2");

        if (horizontalInput > 0 && isRightBlocked) horizontalInput = 0f;
        else if (horizontalInput < 0 && isLeftBlocked) horizontalInput = 0f;

        transform.Translate(Vector2.right * horizontalInput * speed * Time.deltaTime);

        if (horizontalInput > 0.01f) spriteRenderer.flipX = false;
        else if (horizontalInput < -0.01f) spriteRenderer.flipX = true;
    }

    // 拘束中の円周移動
    private void MoveOnCircle(float speed)
    {
        float horizontal = Input.GetAxis("JoystickLeftHorizontal_P2");
        float vertical = Input.GetAxis("JoystickLeftVertical_P2");
        Vector2 inputDir = new Vector2(horizontal, vertical);

        if (inputDir.magnitude > 0.1f)
        {
            float targetAngle = Mathf.Atan2(vertical, horizontal);
            float deltaAngle = Mathf.DeltaAngle(angle * Mathf.Rad2Deg, targetAngle * Mathf.Rad2Deg);
            angle += Mathf.Sign(deltaAngle)
                   * Mathf.Min(Mathf.Abs(deltaAngle), circleMoveSpeed * Time.deltaTime)
                   * Mathf.Deg2Rad;
        }

        float fixedRadius = p1Script.FixedRadius;
        Vector3 centerPos = p1Script.transform.position;

        Vector2 desiredPos = new Vector2(
            centerPos.x + Mathf.Cos(angle) * fixedRadius,
            centerPos.y + Mathf.Sin(angle) * fixedRadius
        );

        Vector2 currentPos = rb.position;
        Vector2 moveDir = desiredPos - currentPos;
        float moveDistance = moveDir.magnitude;
        if (moveDistance < 0.001f) return;

        // 進行方向にキャストして移動可否を判断
        RaycastHit2D[] hitResults = new RaycastHit2D[10];
        int hitCount = rb.Cast(moveDir.normalized, hitResults, moveDistance);
        bool allowMovement = true;

        if (hitCount > 0)
        {
            allowMovement = false;
            for (int i = 0; i < hitCount; i++)
            {
                if (Vector2.Dot(moveDir.normalized, hitResults[i].normal) < -0.1f)
                {
                    allowMovement = true;
                    break;
                }
            }
        }

        if (allowMovement)
        {
            rb.MovePosition(desiredPos);
        }
    }

    private void Jump()
    {
        if (audioSource != null && jumpClip != null)
        {
            audioSource.PlayOneShot(jumpClip);
        }

        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    // 接地中に足音を再生
    private void HandleFootstepLoop()
    {
        bool isMovingHorizontally = Mathf.Abs(Input.GetAxis("JoystickLeftHorizontal_P2")) > 0.1f;
        bool shouldPlayFootsteps = isMovingHorizontally && isGrounded;

        if (shouldPlayFootsteps && !isFootstepLooping)
        {
            footstepCoroutine = StartCoroutine(FootstepLoop());
        }
        else if (!shouldPlayFootsteps && isFootstepLooping)
        {
            if (footstepCoroutine != null) StopCoroutine(footstepCoroutine);
            footstepCoroutine = null;
            isFootstepLooping = false;
        }
    }

    private IEnumerator FootstepLoop()
    {
        isFootstepLooping = true;
        while (true)
        {
            if (audioSource != null && runClips != null && runClips.Length > 0)
            {
                int randIndex = Random.Range(0, runClips.Length);
                audioSource.PlayOneShot(runClips[randIndex]);
            }
            yield return new WaitForSeconds(footstepInterval);
        }
    }

    private void CheckGrounded()
    {
        Collider2D col = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        bool wasGnd = isGrounded;
        isGrounded = (col != null);

        if (!wasGnd && isGrounded)
        {
            jumpCount = 0;
        }
    }

    private void CheckSides()
    {
        RaycastHit2D leftHit = Physics2D.Raycast(leftCheck.position, Vector2.left, sideCheckRadius, groundLayer);
        RaycastHit2D rightHit = Physics2D.Raycast(rightCheck.position, Vector2.right, sideCheckRadius, groundLayer);

        isLeftBlocked = (leftHit.collider != null);
        isRightBlocked = (rightHit.collider != null);

        Debug.DrawRay(leftCheck.position, Vector2.left * sideCheckRadius, Color.red);
        Debug.DrawRay(rightCheck.position, Vector2.right * sideCheckRadius, Color.red);
    }

    private void UpdateAnimator()
    {
        float horizontal = Mathf.Abs(Input.GetAxis("JoystickLeftHorizontal_P2"));
        animator.SetBool("run", horizontal > 0.1f);
        animator.SetBool("jump", !isGrounded);
    }

    public void SetInitialAngle(float initialAngle)
    {
        angle = initialAngle;
    }
}
