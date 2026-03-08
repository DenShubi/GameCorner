using UnityEngine;

/// <summary>
/// Kontrol rotasi Pong Bat 3D untuk mobile PvP.
/// Kamera: Orthographic, menghadap ke +Z (default Unity).
/// Bat berotasi pada sumbu Y berdasarkan posisi geser jari.
/// Pivot referensi = tengah layar (batas P1 dan P2).
/// </summary>
public class PongBatController : MonoBehaviour
{
    public enum PlayerSide { Bottom, Top }

    [Header("Player Settings")]
    [Tooltip("Bottom = P1 (area bawah layar), Top = P2 (area atas layar)")]
    public PlayerSide playerSide = PlayerSide.Bottom;

    [Header("Rotation Settings")]
    [Tooltip("Kecepatan smoothing rotasi")]
    public float rotationSpeed = 12f;

    [Tooltip("Batas sudut kiri/kanan (derajat)")]
    public float minAngle = -75f;
    public float maxAngle = 75f;

    [Header("Position Constraint")]
    [Tooltip("Bat juga bergerak horizontal di sumbu X mengikuti jari")]
    public bool lockXMovement = false;

    [Tooltip("Batas gerak horizontal bat di sumbu X")]
    public float xMin = -3f;
    public float xMax = 3f;

    [Tooltip("Seberapa banyak bat ikut geser horizontal sesuai jari (0 = tidak geser, 1 = ikut penuh)")]
    [Range(0f, 1f)]
    public float horizontalFollowStrength = 0.6f;

    // Internal
    private Camera mainCamera;
    private float targetAngleY = 0f;
    private float currentAngleY = 0f;
    private float targetX;
    private float currentX;
    private Vector3 basePosition;

    void Start()
    {
        mainCamera = Camera.main;
        basePosition = transform.position;
        targetX = basePosition.x;
        currentX = basePosition.x;
    }

    void Update()
    {
        ProcessInput();
        ApplyRotation();
        if (lockXMovement) ApplyHorizontalMovement();
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
                ComputeAngleFromScreen(touch.position);
            }
        }

        // === Mouse Input (Editor/Testing) ===
#if UNITY_EDITOR
        if (Input.GetMouseButton(0) && IsInMyZone(Input.mousePosition))
        {
            ComputeAngleFromScreen(Input.mousePosition);
        }
#endif
    }

    void ComputeAngleFromScreen(Vector2 screenPos)
    {
        // Pivot = tengah layar di world space
        Vector3 pivotScreen = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
        Vector3 pivotWorld  = mainCamera.ScreenToWorldPoint(
            new Vector3(pivotScreen.x, pivotScreen.y, Mathf.Abs(mainCamera.transform.position.z))
        );

        // Posisi sentuhan di world space
        Vector3 touchWorld = mainCamera.ScreenToWorldPoint(
            new Vector3(screenPos.x, screenPos.y, Mathf.Abs(mainCamera.transform.position.z))
        );

        // Arah dari pivot ke sentuhan (sumbu X = horizontal, sumbu Y = vertikal)
        float dx = touchWorld.x - pivotWorld.x;
        float dy = touchWorld.y - pivotWorld.y;

        // Hitung sudut rotasi Y:
        // - Geser ke kanan  → bat miring kanan  (Y positif)
        // - Geser ke kiri   → bat miring kiri   (Y negatif)
        float rawAngle = Mathf.Atan2(dx, Mathf.Abs(dy) + 0.01f) * Mathf.Rad2Deg;

        // P2 (atas) orientasi bat sudah terbalik, kompensasi arah
        if (playerSide == PlayerSide.Top)
            rawAngle = -rawAngle;

        targetAngleY = Mathf.Clamp(rawAngle, minAngle, maxAngle);

        // Hitung target posisi X horizontal (opsional)
        if (lockXMovement)
        {
            float newX = Mathf.Lerp(basePosition.x, touchWorld.x, horizontalFollowStrength);
            targetX = Mathf.Clamp(newX, xMin, xMax);
        }
    }

    void ApplyRotation()
    {
        currentAngleY = Mathf.LerpAngle(currentAngleY, targetAngleY, Time.deltaTime * rotationSpeed);

        // Pertahankan rotation X dan Z asli dari model (tidak diubah)
        Vector3 euler = transform.eulerAngles;
        transform.rotation = Quaternion.Euler(euler.x, currentAngleY, euler.z);
    }

    void ApplyHorizontalMovement()
    {
        currentX = Mathf.Lerp(currentX, targetX, Time.deltaTime * rotationSpeed);
        transform.position = new Vector3(currentX, basePosition.y, basePosition.z);
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
    }
}
