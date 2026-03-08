using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PongBall : MonoBehaviour
{
    [Header("Speed Settings")]
    public float initialSpeed = 5f;
    public float maxSpeed = 12f;
    public float speedIncreasePerHit = 0.3f;

    [Header("Spin Settings")]
    [Range(0f, 2f)]
    public float hitOffsetInfluence = 1f;

    [Range(0f, 1f)]
    public float batRotationInfluence = 0.3f;

    [Range(5f, 45f)]
    public float minVerticalAngle = 20f;

    [Header("Wall Bounds")]
    public float wallXMin = -2.5f;
    public float wallXMax = 2.5f;
    public float wallBounceOffset = 0.05f;

    [Header("References")]
    public PongGameManager gameManager;

    // Internal
    private Rigidbody rb;
    private float currentSpeed;
    private Vector3 spawnPosition;
    private bool isActive = false;

    private GameObject lastHitBat = null;
    private float hitCooldownTimer = 0f;
    private const float HIT_COOLDOWN = 0.3f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity  = false;
        rb.constraints = RigidbodyConstraints.FreezePositionY
                       | RigidbodyConstraints.FreezeRotationX
                       | RigidbodyConstraints.FreezeRotationY
                       | RigidbodyConstraints.FreezeRotationZ;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation          = RigidbodyInterpolation.Interpolate;
    }

    void Start()
    {
        spawnPosition = transform.position;
        // ← Tidak ada LaunchBall() di sini, countdown yang handle
    }

    // ── Update ────────────────────────────────────────────────────────────

    void Update()
    {
        if (hitCooldownTimer > 0f)
            hitCooldownTimer -= Time.deltaTime;
        else
            lastHitBat = null;
    }

    void FixedUpdate()
    {
        if (!isActive) return;
        CheckWallBounce();
        EnforceConstantSpeed();
    }

    // ── Wall Bounce ───────────────────────────────────────────────────────

    void CheckWallBounce()
    {
        Vector3 pos = transform.position;
        Vector3 vel = rb.linearVelocity;
        bool bounced = false;

        if (pos.x <= wallXMin && vel.x < 0f)
        {
            vel.x = Mathf.Abs(vel.x);
            transform.position = new Vector3(wallXMin + wallBounceOffset, pos.y, pos.z);
            bounced = true;
        }
        else if (pos.x >= wallXMax && vel.x > 0f)
        {
            vel.x = -Mathf.Abs(vel.x);
            transform.position = new Vector3(wallXMax - wallBounceOffset, pos.y, pos.z);
            bounced = true;
        }

        if (bounced)
        {
            vel.y             = 0f;
            rb.linearVelocity = vel.normalized * currentSpeed;
        }
    }

    // ── Launch & Stop ─────────────────────────────────────────────────────

    /// <summary>
    /// Dipanggil HANYA oleh PongCountdown setelah countdown selesai.
    /// </summary>
    public void LaunchBall()
    {
        isActive         = true;
        currentSpeed     = initialSpeed;
        lastHitBat       = null;
        hitCooldownTimer = 0f;

        float randomX = Random.Range(-0.4f, 0.4f);
        float randomZ = Random.value > 0.5f ? 1f : -1f;

        Vector3 dir = ClampVerticalAngle(
            new Vector3(randomX, 0f, randomZ).normalized
        );
        rb.linearVelocity = dir * currentSpeed;
    }

    /// <summary>
    /// Stop bola total — dipanggil saat gol atau game over.
    /// </summary>
    public void StopBall()
    {
        isActive          = false;
        lastHitBat        = null;
        hitCooldownTimer  = 0f;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    /// <summary>
    /// Reset posisi bola ke spawn — TIDAK launch.
    /// Launch dilakukan countdown setelah selesai.
    /// </summary>
    public void ResetPosition()
    {
        StopBall();
        transform.position = spawnPosition;
    }

    /// <summary>
    /// Dipanggil dari luar jika butuh force reset total.
    /// </summary>
    public void ForceReset()
    {
        CancelInvoke();
        ResetPosition();
    }

    // ── Bat Collision ─────────────────────────────────────────────────────

    void OnCollisionEnter(Collision collision)
    {
        if (!isActive) return;
        if (!collision.gameObject.CompareTag("Bat")) return;

        if (collision.gameObject == lastHitBat && hitCooldownTimer > 0f) return;

        lastHitBat       = collision.gameObject;
        hitCooldownTimer = HIT_COOLDOWN;

        currentSpeed = Mathf.Min(currentSpeed + speedIncreasePerHit, maxSpeed);
        ApplyBatDeflection(collision);
    }

    void ApplyBatDeflection(Collision collision)
    {
        Transform batTransform = collision.transform;
        ContactPoint contact   = collision.GetContact(0);
        Vector3 hitPoint       = contact.point;

        PongBatController batCtrl = collision.gameObject
                                    .GetComponentInParent<PongBatController>();
        float dirZ = (batCtrl != null)
            ? (batCtrl.playerSide == PongBatController.PlayerSide.Bottom ? 1f : -1f)
            : (batTransform.position.z < 0f ? 1f : -1f);

        Vector3 batRight = batTransform.right;
        batRight.y       = 0f;
        if (batRight.sqrMagnitude < 0.01f) batRight = Vector3.right;
        batRight.Normalize();

        float offsetDot     = Vector3.Dot(hitPoint - batTransform.position, batRight);
        float halfWidth     = collision.collider.bounds.extents.x;
        float normalizedOff = halfWidth > 0.01f
                              ? Mathf.Clamp(offsetDot / halfWidth, -1f, 1f)
                              : 0f;
        float deflectX      = normalizedOff * hitOffsetInfluence;

        Vector3 batFwd = batTransform.forward;
        batFwd.y       = 0f;
        if (batFwd.sqrMagnitude < 0.01f) batFwd = Vector3.forward;
        batFwd.Normalize();
        float rotSpinX = batFwd.x * batRotationInfluence;

        Vector3 finalDir = ClampVerticalAngle(
            new Vector3(deflectX + rotSpinX, 0f, dirZ).normalized
        );

        rb.linearVelocity = finalDir * currentSpeed;

        Debug.DrawRay(hitPoint, finalDir * 2f, Color.green, 1f);
    }

    // ── Goal ──────────────────────────────────────────────────────────────

    void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;

        if (other.CompareTag("GoalP1"))      OnGoal(2);
        else if (other.CompareTag("GoalP2")) OnGoal(1);
    }

    void OnGoal(int scorer)
    {
        // Stop bola langsung
        StopBall();

        // Beritahu GameManager — GameManager yang trigger countdown
        gameManager?.OnGoal(scorer);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    Vector3 ClampVerticalAngle(Vector3 dir)
    {
        float minZ = Mathf.Sin(minVerticalAngle * Mathf.Deg2Rad);
        if (Mathf.Abs(dir.z) < minZ)
        {
            dir.z = minZ * Mathf.Sign(dir.z == 0f ? 1f : dir.z);
            dir.Normalize();
        }
        return dir;
    }

    void EnforceConstantSpeed()
    {
        if (rb.linearVelocity.sqrMagnitude < 0.01f) return;
        Vector3 vel = rb.linearVelocity;
        vel.y       = 0f;
        rb.linearVelocity = vel.normalized * currentSpeed;
    }
}