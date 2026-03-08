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
    [Tooltip("Batas X kiri meja (world space). Samakan dengan posisi X Wall_Left)")]
    public float wallXMin = -2.5f;

    [Tooltip("Batas X kanan meja (world space). Samakan dengan posisi X Wall_Right)")]
    public float wallXMax = 2.5f;

    [Tooltip("Offset kecil agar bola tidak stuck di wall")]
    public float wallBounceOffset = 0.05f;

    [Header("Reset Settings")]
    public float resetDelay = 1.5f;

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
        //LaunchBall();
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

        // ── WALL BOUNCE via posisi (tidak bergantung OnCollision) ──────
        CheckWallBounce();

        // Paksa kecepatan konstan
        EnforceConstantSpeed();
    }

    // ── Wall Bounce ───────────────────────────────────────────────────────

    void CheckWallBounce()
    {
        Vector3 pos = transform.position;
        Vector3 vel = rb.linearVelocity;
        bool bounced = false;

        // Kena wall KIRI
        if (pos.x <= wallXMin && vel.x < 0f)
        {
            vel.x = Mathf.Abs(vel.x);   // balik ke kanan
            transform.position = new Vector3(wallXMin + wallBounceOffset, pos.y, pos.z);
            bounced = true;
        }
        // Kena wall KANAN
        else if (pos.x >= wallXMax && vel.x > 0f)
        {
            vel.x = -Mathf.Abs(vel.x);  // balik ke kiri
            transform.position = new Vector3(wallXMax - wallBounceOffset, pos.y, pos.z);
            bounced = true;
        }

        if (bounced)
        {
            vel.y             = 0f;
            rb.linearVelocity = vel.normalized * currentSpeed;
        }
    }

    // ── Launch ────────��───────────────────────────────────────────────────

    public void LaunchBall()
    {
        isActive         = true;
        currentSpeed     = initialSpeed;
        lastHitBat       = null;
        hitCooldownTimer = 0f;

        float randomX = Random.Range(-0.4f, 0.4f);
        float randomZ = Random.value > 0.5f ? 1f : -1f;

        Vector3 dir = ClampVerticalAngle(new Vector3(randomX, 0f, randomZ).normalized);
        rb.linearVelocity = dir * currentSpeed;
    }

    // ── Bat Collision ─────────────────────────────────────────────────────

    void OnCollisionEnter(Collision collision)
    {
        if (!isActive) return;
        if (!collision.gameObject.CompareTag("Bat")) return;

        // Anti double-hit
        if (collision.gameObject == lastHitBat && hitCooldownTimer > 0f) return;

        lastHitBat       = collision.gameObject;
        hitCooldownTimer = HIT_COOLDOWN;

        currentSpeed = Mathf.Min(currentSpeed + speedIncreasePerHit, maxSpeed);
        ApplyBatDeflection(collision);
    }

    // ── Bat Deflection ──────��─────────────────────────────────────────────

    void ApplyBatDeflection(Collision collision)
    {
        Transform batTransform = collision.transform;
        ContactPoint contact   = collision.GetContact(0);
        Vector3 hitPoint       = contact.point;

        // Tentukan arah Z dari playerSide
        PongBatController batCtrl = collision.gameObject
                                    .GetComponentInParent<PongBatController>();
        float dirZ = 1f;
        if (batCtrl != null)
            dirZ = (batCtrl.playerSide == PongBatController.PlayerSide.Bottom) ? 1f : -1f;
        else
            dirZ = batTransform.position.z < 0f ? 1f : -1f;

        // Offset titik kena (kiri/kanan bat)
        Vector3 batRight = batTransform.right;
        batRight.y       = 0f;
        if (batRight.sqrMagnitude < 0.01f) batRight = Vector3.right;
        batRight.Normalize();

        float offsetDot      = Vector3.Dot(hitPoint - batTransform.position, batRight);
        float halfWidth      = collision.collider.bounds.extents.x;
        float normalizedOff  = halfWidth > 0.01f
                               ? Mathf.Clamp(offsetDot / halfWidth, -1f, 1f)
                               : 0f;
        float deflectX       = normalizedOff * hitOffsetInfluence;

        // Rotasi bat
        Vector3 batFwd = batTransform.forward;
        batFwd.y       = 0f;
        if (batFwd.sqrMagnitude < 0.01f) batFwd = Vector3.forward;
        batFwd.Normalize();
        float rotSpinX = batFwd.x * batRotationInfluence;

        // Final direction
        Vector3 finalDir = ClampVerticalAngle(
            new Vector3(deflectX + rotSpinX, 0f, dirZ).normalized
        );

        rb.linearVelocity = finalDir * currentSpeed;

        Debug.DrawRay(hitPoint, finalDir * 2f, Color.green, 1f);
        Debug.Log($"[Ball] Kena {collision.gameObject.name} | " +
                  $"offset={normalizedOff:F2} | dirZ={dirZ} | final={finalDir}");
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
        isActive          = false;
        rb.linearVelocity = Vector3.zero;
        gameManager?.OnGoal(scorer);
        Invoke(nameof(ResetBall), resetDelay);
    }

    void ResetBall()
    {
        transform.position = spawnPosition;
        rb.linearVelocity  = Vector3.zero;
        lastHitBat         = null;
        hitCooldownTimer   = 0f;
        LaunchBall();
    }

    public void ForceReset()
    {
        CancelInvoke(nameof(ResetBall));
        ResetBall();
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