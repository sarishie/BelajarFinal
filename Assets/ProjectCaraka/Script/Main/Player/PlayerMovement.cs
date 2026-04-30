using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    private Rigidbody2D rb;

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

    [Tooltip("Kalau true, input horizontal dikunci setelah release grapple sampai player menyentuh tanah.")]
    public bool lockHorizontalInputUntilGroundedAfterGrapple = true;

    private bool isGrappling;
    private bool isGrappleMomentum;
    private float grappleReleaseTimer;

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
    }

    void Update()
    {
        CheckMove();
        CheckJump();
        CheckGrappleInput();
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
        UpdatePlayerFacingVisual();

        if (isGrappling) return;

        if (isGrappleMomentum && lockHorizontalInputUntilGroundedAfterGrapple)
        {
            return;
        }

        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
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
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
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

        isJumping = false;
        currentCoyoteTime = 0;

        rb.linearVelocity = Vector2.zero;
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

        rb.linearVelocity = Vector2.zero;
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

        currentSnapPoint = null;
        currentCenterPos = null;

        rb.gravityScale = normalGravityScale;

        rb.linearVelocity = new Vector2(directionX * horizontalForce, grappleUpForce);

        Debug.Log("[PlayerMovement] Release Grapple");
        Debug.Log("[PlayerMovement] Distance X: " + distanceX);
        Debug.Log("[PlayerMovement] Horizontal Force: " + horizontalForce);
        Debug.Log("[PlayerMovement] Up Force: " + grappleUpForce);
        Debug.Log("[PlayerMovement] Velocity setelah grapple: " + rb.linearVelocity);
        Debug.Log("[PlayerMovement] Input horizontal dikunci sampai grounded: " + lockHorizontalInputUntilGroundedAfterGrapple);
    }

    private void UpdateGrappleMomentum()
    {
        if (!isGrappleMomentum) return;

        bool groundedNow = IsGrounded();

        if (groundedNow)
        {
            isGrappleMomentum = false;

            Debug.Log("[PlayerMovement] Player sudah menyentuh tanah. Movement normal aktif lagi.");
        }
    }

    private void StopGrappleWithoutJump()
    {
        isGrappling = false;
        isGrappleMomentum = false;

        currentSnapPoint = null;
        currentCenterPos = null;

        rb.gravityScale = normalGravityScale;

        Debug.Log("[PlayerMovement] Grapple berhenti tanpa lompatan.");
    }

    #endregion

    #region Collision Detections

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheckObj.transform.position, groundCheckRadius, groundCheckLayer);
    }

    #endregion
}