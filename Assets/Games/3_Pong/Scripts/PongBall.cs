using UnityEngine;

/// <summary>
/// Bola Pong dengan kecepatan konstan.
/// Pasang pada GameObject bola beserta:
///   - Rigidbody (Is Kinematic = OFF, Use Gravity = OFF, Constraints: Freeze Y position & rotation XZ)
///   - SphereCollider / CapsuleCollider
/// Material Physics: Bounciness = 1, Friction = 0
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PongBall : MonoBehaviour
{
    [Header("Speed Settings")]
    [Tooltip("Kecepatan awal bola")]
    public float initialSpeed = 5f;

    [Tooltip("Kecepatan maksimum bola (agar tidak terlalu cepat)")]
    public float maxSpeed = 12f;

    [Tooltip("Penambahan kecepatan tiap kali bola kena bet")]
    public float speedIncreasePerHit = 0.3f;

    [Header("Reset Settings")]
    [Tooltip("Delay sebelum bola di-reset setelah gol (detik)")]
    public float resetDelay = 1.5f;

    [Header("References")]
    [Tooltip("Referensi ke GameManager untuk notifikasi gol")]
    public PongGameManager gameManager;

    // Internal
    private Rigidbody rb;
    private float currentSpeed;
    private Vector3 spawnPosition;
    private bool isActive = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Setup Rigidbody untuk top-down 2.5D
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezePositionY
                       | RigidbodyConstraints.FreezeRotationX
                       | RigidbodyConstraints.FreezeRotationZ;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void Start()
    {
        spawnPosition = transform.position;
        LaunchBall();
    }

    // ── Launch ────────────────────────────────────────────────────────────

    /// <summary>
    /// Lempar bola ke arah random (ke atas atau ke bawah).
    /// </summary>
    public void LaunchBall()
    {
        isActive = true;
        currentSpeed = initialSpeed;

        // Arah awal: diagonal random ke salah satu sisi
        // X antara -0.7 dan 0.7, Z ke atas atau bawah
        float randomX = Random.Range(-0.7f, 0.7f);
        float randomZ = Random.value > 0.5f ? 1f : -1f; // ke P1 atau P2

        Vector3 dir = new Vector3(randomX, 0f, randomZ).normalized;
        rb.linearVelocity = dir * currentSpeed;
    }

    // ── Collision ─────────────────────────────────────────────────────────

    void OnCollisionEnter(Collision collision)
    {
        if (!isActive) return;

        // Kena Bet → tambah kecepatan
        if (collision.gameObject.CompareTag("Bat"))
        {
            currentSpeed = Mathf.Min(currentSpeed + speedIncreasePerHit, maxSpeed);
        }

        // Setelah collision, normalisasi velocity agar kecepatan tetap konstan
        EnforceConstantSpeed();
    }

    // ── Trigger (Goal Zone) ───────────────────────────────────────────────

    void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;

        if (other.CompareTag("GoalP1"))
        {
            // Bola masuk gawang P1 → P2 dapat poin
            isActive = false;
            rb.linearVelocity = Vector3.zero;
            gameManager?.OnGoal(2);
            Invoke(nameof(ResetBall), resetDelay);
        }
        else if (other.CompareTag("GoalP2"))
        {
            // Bola masuk gawang P2 → P1 dapat poin
            isActive = false;
            rb.linearVelocity = Vector3.zero;
            gameManager?.OnGoal(1);
            Invoke(nameof(ResetBall), resetDelay);
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    /// <summary>
    /// Paksa kecepatan bola tetap konstan (tidak melambat karena physics).
    /// Dipanggil setiap frame dan setelah collision.
    /// </summary>
    void EnforceConstantSpeed()
    {
        if (rb.linearVelocity.magnitude < 0.1f) return;
        rb.linearVelocity = rb.linearVelocity.normalized * currentSpeed;
    }

    void FixedUpdate()
    {
        if (!isActive) return;
        EnforceConstantSpeed();
    }

    void ResetBall()
    {
        transform.position = spawnPosition;
        rb.linearVelocity = Vector3.zero;
        LaunchBall();
    }

    /// <summary>
    /// Bisa dipanggil dari luar (misal tombol pause/restart).
    /// </summary>
    public void ForceReset()
    {
        CancelInvoke(nameof(ResetBall));
        ResetBall();
    }
}