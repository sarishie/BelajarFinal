using System.Collections;
using UnityEngine;

public enum GrappleMomentumLockMode
{
    UntilGrounded,
    ByTime
}

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    private Rigidbody2D rb;
    public Animator animator;

    [Header("Animation Names")]
    public string idleAnimationName = "Player_Idle";
    public string runAnimationName = "Player_Run";
    public string jumpAnimationName = "Player_Jump";
    public string fallAnimationName = "Player_Fall";
    public string grappleAnimationName = "Player_Grapple";

    private string currentAnimationName;

    [Header("Move")]
    public float moveSpeed = 5f;
    private bool isFacingRight;
    private float horizontalInput;

    [Header("Jump")]
    public float jumpForce = 5f;
    public float maxCoyoteTime = 0.5f;
    private float currentCoyoteTime = 0;
    private bool isCoyoteRunning;
    private bool isJumping;
    private bool isGrounded;

    [Header("Simple Grappling Hook")]
    public float grappleHorizontalMultiplier = 10f;
    public float grappleMinHorizontalForce = 4f;
    public float grappleMaxHorizontalForce = 18f;
    public float grappleUpForce = 8f;

    [Tooltip("Delay kecil supaya player tidak langsung release grapple karena tombol Space dari jump sebelumnya.")]
    public float grappleReleaseDelay = 0.15f;

    [Header("Grapple Momentum Lock")]
    public GrappleMomentumLockMode grappleMomentumLockMode = GrappleMomentumLockMode.UntilGrounded;

    [Tooltip("Dipakai kalau Grapple Momentum Lock Mode = ByTime.")]
    public float grappleMomentumLockTime = 1f;

    private bool isGrappling;
    private bool isGrappleMomentum;
    private float grappleReleaseTimer;
    private float grappleMomentumTimer;

    private Transform currentSnapPoint;
    private Transform currentCenterPos;

    [Header("Gravity")]
    public float normalGravityScale = 3f;

    [Header("Collision Detector")]
    public GameObject groundCheckObj;
    public float groundCheckRadius = 0.3f;
    public LayerMask groundCheckLayer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        isFacingRight = true;

        normalGravityScale = rb.gravityScale;

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    void Update()
    {
        CheckMove();
        CheckJump();
        CheckGrappleInput();
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        UpdateGrapple();
        UpdateGrappleMomentum();

        Move();
        Jump();
    }

    #region Movement

    private void CheckMove()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        if (horizontalInput > 0)
        {
            isFacingRight = true;
        }
        else if (horizontalInput < 0)
        {
            isFacingRight = false;
        }
    }

    private void Move()
    {
        // Visual tetap boleh berubah, walaupun movement sedang dikunci.
        UpdatePlayerFacingVisual();

        if (isGrappling) return;

        // Saat momentum grapple aktif, input horizontal tidak boleh menimpa velocity grapple.
        if (IsGrappleMomentumActive())
        {
            return;
        }

        rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
    }

    private bool IsGrappleMomentumActive()
    {
        return isGrappleMomentum;
    }

    private void UpdatePlayerFacingVisual()
    {
        if (isFacingRight)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
    }

    #endregion

    #region Jumping

    private void CheckJump()
    {
        if (isGrappling) return;

        isGrounded = IsGrounded();

        if (!isGrounded && !isCoyoteRunning)
        {
            StartCoroutine(IsCoyoteRunning());
        }
        else if (isGrounded)
        {
            currentCoyoteTime = maxCoyoteTime;
        }

        if (Input.GetKeyDown(KeyCode.Space) && (isGrounded || currentCoyoteTime > 0))
        {
            isJumping = true;
        }
    }

    private void Jump()
    {
        if (isGrappling) return;

        if (isJumping)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            isJumping = false;
        }
    }

    private IEnumerator IsCoyoteRunning()
    {
        isCoyoteRunning = true;

        while (currentCoyoteTime > 0)
        {
            currentCoyoteTime -= Time.deltaTime;
            yield return null;
        }

        isCoyoteRunning = false;
    }

    #endregion

    #region Simple Grappling Hook

    private void CheckGrappleInput()
    {
        if (!isGrappling) return;

        if (grappleReleaseTimer > 0)
        {
            grappleReleaseTimer -= Time.deltaTime;
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ReleaseGrapple();
        }
    }

    public void StartGrapple(Transform snapPoint, Transform centerPos)
    {
        Debug.Log("[PlayerMovement] StartGrapple terpanggil. Player snap ke: " + snapPoint.name);

        currentSnapPoint = snapPoint;
        currentCenterPos = centerPos;

        isGrappling = true;
        isGrappleMomentum = false;

        grappleReleaseTimer = grappleReleaseDelay;
        grappleMomentumTimer = 0;

        isJumping = false;
        currentCoyoteTime = 0;

        rb.velocity = Vector2.zero;
        rb.gravityScale = 0;

        transform.position = currentSnapPoint.position;
    }

    private void UpdateGrapple()
    {
        if (!isGrappling) return;

        if (currentSnapPoint == null || currentCenterPos == null)
        {
            StopGrappleWithoutJump();
            return;
        }

        rb.velocity = Vector2.zero;
        transform.position = currentSnapPoint.position;
    }

    private void ReleaseGrapple()
    {
        if (currentCenterPos == null)
        {
            StopGrappleWithoutJump();
            return;
        }

        float distanceX = Mathf.Abs(transform.position.x - currentCenterPos.position.x);
        float directionX = Mathf.Sign(transform.position.x - currentCenterPos.position.x);

        if (directionX == 0)
        {
            directionX = isFacingRight ? 1 : -1;
        }

        float horizontalForce = distanceX * grappleHorizontalMultiplier;
        horizontalForce = Mathf.Clamp(horizontalForce, grappleMinHorizontalForce, grappleMaxHorizontalForce);

        isGrappling = false;
        isGrappleMomentum = true;
        grappleMomentumTimer = grappleMomentumLockTime;

        currentSnapPoint = null;
        currentCenterPos = null;

        rb.gravityScale = normalGravityScale;

        rb.velocity = new Vector2(directionX * horizontalForce, grappleUpForce);

        Debug.Log("[PlayerMovement] Release Grapple");
        Debug.Log("[PlayerMovement] Distance X: " + distanceX);
        Debug.Log("[PlayerMovement] Horizontal Force: " + horizontalForce);
        Debug.Log("[PlayerMovement] Up Force: " + grappleUpForce);
        Debug.Log("[PlayerMovement] Lock Mode: " + grappleMomentumLockMode);
        Debug.Log("[PlayerMovement] Velocity setelah grapple: " + rb.velocity);
    }

    private void UpdateGrappleMomentum()
    {
        if (!isGrappleMomentum) return;

        if (grappleMomentumLockMode == GrappleMomentumLockMode.UntilGrounded)
        {
            if (IsGrounded())
            {
                isGrappleMomentum = false;
                Debug.Log("[PlayerMovement] Grapple momentum selesai karena player sudah menyentuh tanah.");
            }
        }
        else if (grappleMomentumLockMode == GrappleMomentumLockMode.ByTime)
        {
            grappleMomentumTimer -= Time.fixedDeltaTime;

            if (grappleMomentumTimer <= 0)
            {
                isGrappleMomentum = false;
                Debug.Log("[PlayerMovement] Grapple momentum selesai karena timer habis.");
            }
        }
    }

    private void StopGrappleWithoutJump()
    {
        isGrappling = false;
        isGrappleMomentum = false;

        currentSnapPoint = null;
        currentCenterPos = null;

        grappleReleaseTimer = 0;
        grappleMomentumTimer = 0;

        rb.gravityScale = normalGravityScale;

        Debug.Log("[PlayerMovement] Grapple berhenti tanpa lompatan.");
    }

    #endregion

    #region Animation

    private void UpdateAnimation()
    {
        if (animator == null) return;

        bool groundedNow = IsGrounded();

        if (isGrappling)
        {
            ChangeAnimationState(grappleAnimationName);
        }
        else if (!groundedNow && rb.velocity.y > 0.1f)
        {
            ChangeAnimationState(jumpAnimationName);
        }
        else if (!groundedNow && rb.velocity.y < -0.1f)
        {
            ChangeAnimationState(fallAnimationName);
        }
        else if (Mathf.Abs(horizontalInput) > 0)
        {
            ChangeAnimationState(runAnimationName);
        }
        else
        {
            ChangeAnimationState(idleAnimationName);
        }
    }

    private void ChangeAnimationState(string newAnimationName)
    {
        if (string.IsNullOrEmpty(newAnimationName)) return;

        if (currentAnimationName == newAnimationName) return;

        animator.Play(newAnimationName);
        currentAnimationName = newAnimationName;
    }

    #endregion

    #region Collision Detections

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheckObj.transform.position, groundCheckRadius, groundCheckLayer);
    }

    #endregion
}