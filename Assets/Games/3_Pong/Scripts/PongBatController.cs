using UnityEngine;

/// <summary>
/// Kontrol Pong Bat 3D untuk mobile PvP.
/// Kamera: Orthographic, menghadap ke +Z (default Unity).
/// Bat bergerak horizontal (X) dan berotasi menghadap tengah layar.
/// P1 = Bottom, P2 = Top — tidak perlu rotate Y manual di Scene.
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
    private float screenCenterWorldX;
    private float currentAngleY = 0f;
    private Quaternion initialRotation;

    void Start()
    {
        mainCamera   = Camera.main;
        basePosition = transform.position;
        targetX      = basePosition.x;
        currentX     = basePosition.x;

        // Simpan rotasi awal bat persis seperti di scene (tidak diubah)
        initialRotation = transform.rotation;

        screenCenterWorldX = ScreenToWorldX(Screen.width * 0.5f, Screen.height * 0.5f);
    }

    void Update()
    {
        ProcessInput();
        ApplyHorizontalMovement();
        ApplyAutoRotation();
    }

    // ── Input ─────────────────────────────────────────────────────────────

    void ProcessInput()
    {
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            if (!IsInMyZone(touch.position)) continue;

            if (touch.phase == TouchPhase.Began    ||
                touch.phase == TouchPhase.Moved     ||
                touch.phase == TouchPhase.Stationary)
            {
                ComputeTargetX(touch.position);
            }
        }

#if UNITY_EDITOR
        if (Input.GetMouseButton(0) && IsInMyZone(Input.mousePosition))
            ComputeTargetX(Input.mousePosition);
#endif
    }

    void ComputeTargetX(Vector2 screenPos)
    {
        float worldX = ScreenToWorldX(screenPos.x, screenPos.y);
        float newX   = Mathf.Lerp(basePosition.x, worldX, horizontalFollowStrength);
        targetX      = Mathf.Clamp(newX, xMin, xMax);
    }

    // ── Movement ──────────────────────────────────────────────────────────

    void ApplyHorizontalMovement()
    {
        currentX = Mathf.Lerp(currentX, targetX, Time.deltaTime * moveSpeed);
        transform.position = new Vector3(currentX, basePosition.y, basePosition.z);
    }

    // ── Rotation ──────────────────────────────────────────────────────────

    void ApplyAutoRotation()
    {
        float offsetFromCenter = currentX - screenCenterWorldX;
        float halfRange        = (xMax - xMin) * 0.5f;
        float normalizedOffset = Mathf.Clamp(offsetFromCenter / halfRange, -1f, 1f);

        // ── FIX ──────────────────────────────────��────────────────────────
        // P1 (Bottom): kanan → tilt kiri  → multiplier = -1
        // P2 (Top)   : kanan → tilt kanan → multiplier = +1
        // Tidak perlu rotate Y manual di scene untuk P2
        float directionMultiplier = (playerSide == PlayerSide.Bottom) ? -1f : 1f;
        float targetAngleY        = directionMultiplier * normalizedOffset * maxRotationAngle;
        // ──────────────────────────────────────────────────────────────────

        currentAngleY = Mathf.LerpAngle(currentAngleY, targetAngleY, Time.deltaTime * rotationSpeed);

        // Rotasi relatif terhadap orientasi awal bat di scene
        transform.rotation = initialRotation * Quaternion.Euler(0f, currentAngleY, 0f);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    float ScreenToWorldX(float screenX, float screenY)
    {
        Ray ray      = mainCamera.ScreenPointToRay(new Vector3(screenX, screenY, 0f));
        Plane plane  = new Plane(Vector3.up, new Vector3(0f, basePosition.y, 0f));

        if (plane.Raycast(ray, out float dist))
            return ray.GetPoint(dist).x;

        // Fallback untuk kamera orthographic
        return mainCamera.ScreenToWorldPoint(
            new Vector3(screenX, screenY, Mathf.Abs(mainCamera.transform.position.z))
        ).x;
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
        Camera cam = Application.isPlaying ? mainCamera : Camera.main;
        if (cam == null) return;

        Vector3 center = transform.position;
        center.x = Application.isPlaying ? screenCenterWorldX : 0f;

        Gizmos.color = playerSide == PlayerSide.Bottom ? Color.cyan : Color.red;
        Gizmos.DrawLine(center + Vector3.left * 5f, center + Vector3.right * 5f);
        Gizmos.DrawSphere(center, 0.08f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(
            new Vector3(xMin, transform.position.y, transform.position.z),
            new Vector3(xMax, transform.position.y, transform.position.z)
        );
    }
}