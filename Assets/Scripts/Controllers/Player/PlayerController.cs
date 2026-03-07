using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour, IDamageable
{
    [Header("---- NETWORK CONFIG ----")]
    [Tooltip("Đánh dấu true nếu nhân vật này do máy hiện tại điều khiển.")]
    public bool isLocalPlayer = true;

    [Header("---- MOVEMENT SETTINGS ----")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float jumpForce = 15f;

    [Tooltip("Hệ số cắt lực nhảy (Hollow Knight Mechanic). Càng nhỏ thì buông phím rơi càng nhanh.")]
    [SerializeField][Range(0f, 1f)] private float jumpCutMultiplier = 0.4f; // MỚI THÊM

    [Header("---- PHYSICS SETTINGS ----")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.2f;

    [Header("---- HEALTH & STATS ----")]
    public int maxHP = 100;
    public int currentHP;
    public float invincibleTime = 0.8f;

    private float horizontalInput;
    public bool isGrounded;
    private bool isKnockedBack = false;
    private bool isInvincible = false;
    private bool isDead = false;

    private SpriteRenderer sr;
    private int playerLayer;
    private int enemyLayer;
    private int bossLayer;

    public bool IsFacingRight { get; private set; } = true;

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();

        currentHP = maxHP;
        playerLayer = gameObject.layer;
        bossLayer = LayerMask.NameToLayer("Boss");
        enemyLayer = LayerMask.NameToLayer("Enemy");

        if (!isLocalPlayer)
        {
            rb.isKinematic = true;
        }
    }

    void Update()
    {
        if (!isLocalPlayer || isKnockedBack || isDead) return;

        horizontalInput = Input.GetAxisRaw("Horizontal");

        CheckGround();
        HandleJump();
        HandleFlip();
    }

    void FixedUpdate()
    {
        if (!isLocalPlayer || isKnockedBack || isDead) return;

        Move();
    }

    void CheckGround()
    {
        if (groundCheck != null)
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    // --- CƠ CHẾ NHẢY HOLLOW KNIGHT ---
    void HandleJump()
    {
        // 1. Nếu nhấn phím (Bấm W hoặc Space) và đang đứng trên đất -> Nhảy tối đa lực
        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space)) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // 2. Nếu nhả phím ra GIỮA CHỪNG lúc đang bay lên -> Cắt đứt lực bay, ép rơi xuống
        if ((Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.Space)) && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }
    }

    void Move()
    {
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    void HandleFlip()
    {
        if (horizontalInput > 0 && !IsFacingRight) Flip();
        else if (horizontalInput < 0 && IsFacingRight) Flip();
    }

    void Flip()
    {
        IsFacingRight = !IsFacingRight;
        Vector3 currentScale = transform.localScale;
        transform.localScale = new Vector3(currentScale.x * -1, currentScale.y, currentScale.z);
    }

    // --- IMPLEMENT IDAMAGEABLE ---
    public void TakeDamage(int damage)
    {
        if (isInvincible || isDead) return;

        currentHP -= damage;
        currentHP = Mathf.Max(currentHP, 0);
        Debug.Log("Player HP: " + currentHP);

        // Mở khóa combat nếu đang chém mà bị quái đập trúng (chống kẹt nút chém)
        PlayerCombat combat = GetComponent<PlayerCombat>();
        if (combat != null) combat.FinishAttack();

        if (currentHP <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvincibleCoroutine());
        }
    }

    public void TakeKnockback(Vector2 direction, float force)
    {
        if (isDead) return;

        isKnockedBack = true;

        if (isLocalPlayer)
        {
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(direction * force, ForceMode2D.Impulse);
        }

        Invoke("StopKnockback", 0.2f);
    }

    public bool IsDead() => isDead;

    void StopKnockback() => isKnockedBack = false;

    // --- LOGIC BẤT TỬ & CHẾT ---
    IEnumerator InvincibleCoroutine()
    {
        isInvincible = true;

        if (bossLayer != -1) Physics2D.IgnoreLayerCollision(playerLayer, bossLayer, true);
        if (enemyLayer != -1) Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);

        float timer = 0f;
        while (timer < invincibleTime)
        {
            if (sr != null) sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(0.1f);
            timer += 0.1f;
        }

        if (sr != null) sr.enabled = true;

        if (bossLayer != -1) Physics2D.IgnoreLayerCollision(playerLayer, bossLayer, false);
        if (enemyLayer != -1) Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);

        isInvincible = false;
    }

    void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        Debug.Log("Player Dead");
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}