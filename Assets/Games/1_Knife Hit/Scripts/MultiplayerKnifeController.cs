using UnityEngine;

/// <summary>
/// Knife controller untuk mode Multiplayer.
/// Tahu milik player siapa (playerID).
/// P1 terbang ke atas (Vector3.up), P2 terbang ke bawah (Vector3.down).
/// </summary>
public class MultiplayerKnifeController : MonoBehaviour
{
    public float speed = 40f;

    [HideInInspector] public int playerID = 1; // 1 = bawah (up), 2 = atas (down)

    private bool isFlying = false;
    private bool hasHit = false;

    private Vector3 flyDirection;

    void Start()
    {
        // P1 dari bawah → terbang ke atas
        // P2 dari atas → terbang ke bawah
        flyDirection = (playerID == 1) ? Vector3.up : Vector3.down;
    }

    void Update()
    {
        if (isFlying)
            transform.Translate(flyDirection * speed * Time.deltaTime, Space.World);
    }

    public void Shoot() => isFlying = true;

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        if (other.CompareTag("Log"))
        {
            hasHit = true;
            isFlying = false;
            transform.SetParent(other.transform);

            MultiplayerManager.instance.RegisterStuckKnife(gameObject);
            MultiplayerManager.instance.AddScore(playerID, MultiplayerManager.instance.scorePerHit);

            // Damage log
            MultiplayerLogController mpLog = other.GetComponent<MultiplayerLogController>();
            if (mpLog != null)
            {
                mpLog.TakeDamage(1, playerID);
            }

            gameObject.tag = "Knife";
        }
        else if (other.CompareTag("Knife"))
        {
            if (!isFlying) return;

            hasHit = true;
            isFlying = false;

            // ===== FIX: Disable collider =====
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;

            // ===== FIX: Pastikan Rigidbody ADA =====
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }

            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero; // Reset velocity lama
            rb.AddForce(Vector3.down * 8f, ForceMode.Impulse);
            rb.AddTorque(new Vector3(0f, 0f, Random.Range(-15f, 15f)), ForceMode.Impulse);
            // =======================================

            Destroy(gameObject, 2f);
        }
    }
}