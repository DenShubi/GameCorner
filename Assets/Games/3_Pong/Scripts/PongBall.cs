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
    private PongBallVisuals visuals;
    private float currentSpeed;
    private Vector3 spawnPosition;
    private bool isActive = false;

    private GameObject lastHitBat = null;
    private float hitCooldownTimer = 0f;
    private const float HIT_COOLDOWN = 0.3f;

    void Awake()
    {
        rb      = GetComponent<Rigidbody>();
        visuals = GetComponent<PongBallVisuals>();

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
    }

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

    // ── Wall ──────────────────────────────────────────────────────────────

    void CheckWallBounce()
    {
        Vector3 pos     = transform.position;
        Vector3 vel     = rb.linearVelocity;
        bool    bounced = false;

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

    public void StopBall()
    {
        isActive           = false;
        lastHitBat         = null;
        hitCooldownTimer   = 0f;
        rb.linearVelocity  = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public void ResetPosition()
    {
        StopBall();
        transform.position = spawnPosition;
        visuals?.ClearTrail();
    }

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

        ApplyBatDeflection(collision); // ← hanya 1 method sekarang
    }

    // ── ApplyBatDeflection (GABUNGAN — hanya ada 1) ───────────────────────

    void ApplyBatDeflection(Collision collision)
    {
        Transform    batTransform = collision.transform;
        ContactPoint contact      = collision.GetContact(0);
        Vector3      hitPoint     = contact.point;

        // ── Cek Smash ─────────────────────────────────────────────────
        PongPowerCharge powerCharge = collision.gameObject
                                      .GetComponentInParent<PongPowerCharge>();
        bool isSmash = powerCharge != null && powerCharge.ConsumeSmash();

        // ── Tentukan arah Z (ke sisi lawan) ───────────────────────────
        PongBatController batCtrl = collision.gameObject
                                    .GetComponentInParent<PongBatController>();
        float dirZ = (batCtrl != null)
            ? (batCtrl.playerSide == PongBatController.PlayerSide.Bottom ? 1f : -1f)
            : (batTransform.position.z < 0f ? 1f : -1f);

        // ── Hitung arah X ─────────────────────────────────────────────
        float finalX;

        if (isSmash)
        {
            // SMASH: bola lurus ke depan, tanpa spin
            finalX       = 0f;
            currentSpeed = Mathf.Min(
                currentSpeed * powerCharge.SmashSpeedMultiplier, maxSpeed
            );
            Debug.Log($"[Ball] SMASH! Speed={currentSpeed}");
        }
        else
        {
            // Normal: spin berdasarkan offset & rotasi bat
            Vector3 batRight = batTransform.right;
            batRight.y       = 0f;
            if (batRight.sqrMagnitude < 0.01f) batRight = Vector3.right;
            batRight.Normalize();

            float offsetDot     = Vector3.Dot(hitPoint - batTransform.position, batRight);
            float halfWidth     = collision.collider.bounds.extents.x;
            float normalizedOff = halfWidth > 0.01f
                                  ? Mathf.Clamp(offsetDot / halfWidth, -1f, 1f)
                                  : 0f;

            Vector3 batFwd = batTransform.forward;
            batFwd.y       = 0f;
            if (batFwd.sqrMagnitude < 0.01f) batFwd = Vector3.forward;
            batFwd.Normalize();

            finalX = normalizedOff * hitOffsetInfluence
                   + batFwd.x    * batRotationInfluence;
        }

        // ── Terapkan velocity ─────────────────────────────────────────
        Vector3 finalDir = ClampVerticalAngle(
            new Vector3(finalX, 0f, dirZ).normalized
        );

        rb.linearVelocity = finalDir * currentSpeed;

        // ── Visual feedback ───────────────────────────────────────────
        visuals?.PlayHitPulse();
        if (isSmash) visuals?.PlaySmashEffect();

        Debug.DrawRay(hitPoint, finalDir * 2f,
                      isSmash ? Color.red : Color.green, 1f);
    }

    // ── Goal ─────────────────────────���────────────────────────────────────

    void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;

        if (other.CompareTag("GoalP1"))      OnGoal(2);
        else if (other.CompareTag("GoalP2")) OnGoal(1);
    }

    void OnGoal(int scorer)
    {
        StopBall();
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