using UnityEngine;

/// <summary>
/// Kontrol Pong Bat 3D untuk mobile PvP (Top-Down Camera).
/// Camera setup: Position(0, Y, 0), Rotation(90, 0, 0) - melihat ke bawah.
/// Bat bergerak kiri-kanan (sumbu X) dan auto-rotate Y menghadap tengah layar.
/// Pivot bat ada di handle.
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
        mainCamera = Camera.main;
        basePosition = transform.position;
        targetX = basePosition.x;
        currentX = basePosition.x;

        // Simpan rotasi awal bat dari Scene (termasuk jika P2 sudah di-rotate di Inspector)
        initialRotation = transform.rotation;

        // Hitung posisi X tengah layar di world space
        // Untuk top-down camera, kita gunakan plane Y=0 (atau Y bat)
        screenCenterWorldX = ScreenToWorldX(Screen.width * 0.5f, Screen.height * 0.5f);
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
        float worldX = ScreenToWorldX(screenPos.x, screenPos.y);
        float newX = Mathf.Lerp(basePosition.x, worldX, horizontalFollowStrength);
        targetX = Mathf.Clamp(newX, xMin, xMax);
    }

    void ApplyHorizontalMovement()
    {
        currentX = Mathf.Lerp(currentX, targetX, Time.deltaTime * moveSpeed);
        transform.position = new Vector3(currentX, basePosition.y, basePosition.z);
    }

    /// <summary>
    /// Bat otomatis rotate menghadap tengah layar berdasarkan posisi X-nya.
    /// Menggunakan initialRotation sebagai base, sehingga P1 dan P2
    /// tidak perlu special case — rotasi relatif terhadap orientasi awal masing-masing.
    /// </summary>
    void ApplyAutoRotation()
    {
        float offsetFromCenter = currentX - screenCenterWorldX;

        float halfRange = (xMax - xMin) * 0.5f;
        float normalizedOffset = Mathf.Clamp(offsetFromCenter / halfRange, -1f, 1f);

        // Bat di kanan → rotate menghadap tengah (negatif Y)
        // Bat di kiri  → rotate menghadap tengah (positif Y)
        float targetAngleY = -normalizedOffset * maxRotationAngle;

        // Smooth rotation
        currentAngleY = Mathf.LerpAngle(currentAngleY, targetAngleY, Time.deltaTime * rotationSpeed);

        // Terapkan rotasi RELATIF terhadap rotasi awal bat
        // Ini otomatis handle P2 yang sudah di-rotate 180° di Scene
        transform.rotation = initialRotation * Quaternion.Euler(0f, currentAngleY, 0f);
    }

    /// <summary>
    /// Konversi screen position ke world X menggunakan Raycast ke plane bat.
    /// Bekerja dengan kamera jenis apapun (top-down, perspective, ortho).
    /// </summary>
    float ScreenToWorldX(float screenX, float screenY)
    {
        Ray ray = mainCamera.ScreenPointToRay(new Vector3(screenX, screenY, 0f));

        // Buat plane horizontal di ketinggian bat
        Plane batPlane = new Plane(Vector3.up, new Vector3(0f, basePosition.y, 0f));

        if (batPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance).x;
        }

        // Fallback jika ray tidak hit plane
        return mainCamera.ScreenToWorldPoint(
            new Vector3(screenX, screenY, Mathf.Abs(mainCamera.transform.position.y))
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

        // Gambar garis tengah layar
        Vector3 center = transform.position;
        center.x = Application.isPlaying ? screenCenterWorldX : 0f;

        Gizmos.color = playerSide == PlayerSide.Bottom ? Color.cyan : Color.red;
        Gizmos.DrawLine(center + Vector3.left * 5f, center + Vector3.right * 5f);
        Gizmos.DrawSphere(center, 0.08f);

        // Gambar batas gerak X
        Gizmos.color = Color.yellow;
        Vector3 batPos = transform.position;
        Gizmos.DrawLine(new Vector3(xMin, batPos.y, batPos.z), new Vector3(xMax, batPos.y, batPos.z));
    }
}