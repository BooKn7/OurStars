using UnityEngine;
using System.Collections;

public class PlayerMove : MonoBehaviour
{
    [Header("移動・ジャンプ")]
    public float speed = 5f;
    public float jumpForce = 5f;
    public int maxJumps = 2;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private Transform leftCheck;
    [SerializeField] private Transform rightCheck;
    [SerializeField] private float sideCheckRadius = 0.15f;

    [Header("サウンド(P1)")]
    public AudioSource audioSource;
    public AudioClip[] runClips;
    public AudioClip jumpClip;
    public AudioClip landClip;
    public float footstepInterval = 0.5f;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private bool isGrounded = false;
    private int jumpCount = 0;
    private bool isLeftBlocked = false;
    private bool isRightBlocked = false;
    private bool wasGroundedLastFrame = false;
    private bool isFootstepLooping = false;
    private Coroutine footstepCoroutine = null;
    private float currentAngle = 0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        wasGroundedLastFrame = false;
        isFootstepLooping = false;
    }

    private void Update()
    {
        CheckGrounded();
        CheckSides();

        // 左スティック入力
        float horizontalInput = Input.GetAxis("JoystickLeftHorizontal_P1");

        // 水平移動
        if (horizontalInput > 0f)
        {
            if (!isRightBlocked) transform.Translate(Vector2.right * horizontalInput * speed * Time.deltaTime);
            spriteRenderer.flipX = false;
        }
        else if (horizontalInput < 0f)
        {
            if (!isLeftBlocked) transform.Translate(Vector2.right * horizontalInput * speed * Time.deltaTime);
            spriteRenderer.flipX = true;
        }

        bool isRunning = Mathf.Abs(horizontalInput) > 0.1f;
        animator.SetBool("run", isRunning);

        // ジャンプ
        if (Input.GetButtonDown("JoystickA_P1"))
        {
            // TODO: 考虑把跳跃手感再调得顺滑一点，现在还是有点僵硬
            if (jumpCount < maxJumps)
            {
                Jump();
                jumpCount++;
            }
        }

        animator.SetBool("jump", !isGrounded);
        HandleFootstepLoop(isRunning);

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

    private void Jump()
    {
        if (audioSource != null && jumpClip != null)
        {
            audioSource.PlayOneShot(jumpClip);
        }

        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    // 接地+走行時に一定間隔で足音を再生
    private void HandleFootstepLoop(bool isRunningNow)
    {
        bool shouldPlayFootsteps = (isRunningNow && isGrounded);

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

    // 拘束開始時の基準角度を受け取る
    public void SetInitialAngle(float initialAngle)
    {
        currentAngle = initialAngle;
    }
}
