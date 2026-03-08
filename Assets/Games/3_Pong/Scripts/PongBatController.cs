using UnityEngine;

/// <summary>
/// Kontrol Pong Bat 3D untuk mobile PvP.
/// - Bat bergerak kiri-kanan (sumbu X) mengikuti posisi jari.
/// - Bat otomatis rotate menghadap tengah layar berdasarkan posisi X-nya.
/// - Pivot bat ada di handle (bawah bat).
/// </summary>
public class PongBatController : MonoBehaviour
{
    public enum PlayerSide { Bottom, Top }

    [Header("Player Settings")]
    [Tooltip("Bottom = P1 (area bawah layar), Top = P2 (area atas layar)")]
    public PlayerSide playerSide = PlayerSide.Bottom;

    [Header("Movement Settings")]
    [Tooltip("Kecepatan smoothing pergerakan horizontal")]
    public float moveSpeed = 12f;

    [Tooltip("Batas gerak horizontal bat di sumbu X")]
    public float xMin = -3f;
    public float xMax = 3f;

    [Tooltip("Seberapa banyak bat ikut geser horizontal sesuai jari (0 = tidak geser, 1 = ikut penuh)")]
    [Range(0f, 1f)]
    public float horizontalFollowStrength = 1f;

    [Header("Rotation Settings")]
    [Tooltip("Kecepatan smoothing rotasi")]
    public float rotationSpeed = 12f;

    [Tooltip("Sudut maksimum rotasi bat saat di ujung layar (derajat)")]
    public float maxRotationAngle = 45f;

    // Internal
    private Camera mainCamera;
    private float targetX;
    private float currentX;
    private Vector3 basePosition;
    private float screenCenterX; // posisi X tengah layar dalam world space
    private float currentAngleY = 0f;

    void Start()
    {
        mainCamera = Camera.main;
        basePosition = transform.position;
        targetX = basePosition.x;
        currentX = basePosition.x;

        // Hitung posisi X tengah layar di world space
        screenCenterX = mainCamera.ScreenToWorldPoint(
            new Vector3(Screen.width * 0.5f, Screen.height * 0.5f,
                        Mathf.Abs(mainCamera.transform.position.z))
        ).x;
    }

    void Update()
    {
        ProcessInput();
        ApplyHorizontalMovement();
        ApplyAutoRotation();
    }

    void ProcessInput()
    {
        // === Touch Input (Device) ===
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            if (!IsInMyZone(touch.position)) continue;

            if (touch.phase == TouchPhase.Began   ||
                touch.phase == TouchPhase.Moved    ||
                touch.phase == TouchPhase.Stationary)
            {
                ComputeTargetX(touch.position);
            }
        }

        // === Mouse Input (Editor/Testing) ===
#if UNITY_EDITOR
        if (Input.GetMouseButton(0) && IsInMyZone(Input.mousePosition))
        {
            ComputeTargetX(Input.mousePosition);
        }
#endif
    }

    void ComputeTargetX(Vector2 screenPos)
    {
        // Posisi sentuhan di world space
        Vector3 touchWorld = mainCamera.ScreenToWorldPoint(
            new Vector3(screenPos.x, screenPos.y, Mathf.Abs(mainCamera.transform.position.z))
        );

        // Hitung target posisi X berdasarkan touch
        float newX = Mathf.Lerp(basePosition.x, touchWorld.x, horizontalFollowStrength);
        targetX = Mathf.Clamp(newX, xMin, xMax);
    }

    void ApplyHorizontalMovement()
    {
        currentX = Mathf.Lerp(currentX, targetX, Time.deltaTime * moveSpeed);
        transform.position = new Vector3(currentX, basePosition.y, basePosition.z);
    }

    /// <summary>
    /// Bat otomatis rotate menghadap tengah layar berdasarkan posisi X-nya.
    /// - Bat di kanan tengah → rotate ke kiri (menghadap tengah)
    /// - Bat di kiri tengah  → rotate ke kanan (menghadap tengah)
    /// - Bat di tengah       → tidak rotate (lurus)
    /// </summary>
    void ApplyAutoRotation()
    {
        // Hitung offset dari tengah layar
        float offsetFromCenter = currentX - screenCenterX;

        // Normalisasi offset: -1 (paling kiri) sampai +1 (paling kanan)
        float halfRange = (xMax - xMin) * 0.5f;
        float normalizedOffset = Mathf.Clamp(offsetFromCenter / halfRange, -1f, 1f);

        // Target sudut: negatif saat di kanan (menghadap ke tengah), positif saat di kiri
        float targetAngleY = -normalizedOffset * maxRotationAngle;

        // P2 (atas) orientasi bat sudah terbalik, kompensasi arah
        if (playerSide == PlayerSide.Top)
            targetAngleY = -targetAngleY;

        // Smooth rotation
        currentAngleY = Mathf.LerpAngle(currentAngleY, targetAngleY, Time.deltaTime * rotationSpeed);

        // Pertahankan rotation X dan Z asli dari model
        Vector3 euler = transform.eulerAngles;
        transform.rotation = Quaternion.Euler(euler.x, currentAngleY, euler.z);
    }

    bool IsInMyZone(Vector2 screenPos)
    {
        float midY = Screen.height * 0.5f;
        return playerSide == PlayerSide.Bottom
            ? screenPos.y < midY
            : screenPos.y >= midY;
    }

    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || mainCamera == null) return;

        // Gambar garis pivot di tengah layar
        Vector3 pivotWorld = mainCamera.ScreenToWorldPoint(
            new Vector3(Screen.width * 0.5f, Screen.height * 0.5f,
                        Mathf.Abs(mainCamera.transform.position.z))
        );

        Gizmos.color = playerSide == PlayerSide.Bottom ? Color.cyan : Color.red;
        Gizmos.DrawLine(pivotWorld + Vector3.left * 5f, pivotWorld + Vector3.right * 5f);
        Gizmos.DrawSphere(pivotWorld, 0.08f);

        // Gambar batas gerak X
        Gizmos.color = Color.yellow;
        Vector3 batPos = transform.position;
        Gizmos.DrawLine(new Vector3(xMin, batPos.y, batPos.z), new Vector3(xMax, batPos.y, batPos.z));
    }
}