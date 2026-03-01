using UnityEngine;

/// <summary>
/// Memecah sprite log menjadi keping-keping kecil saat hancur.
/// Dipasang pada Log prefab bersama LogController.
/// </summary>
public class LogShatter : MonoBehaviour
{
    [Header("Shatter Settings")]
    [Tooltip("Jumlah keping horizontal")]
    public int piecesX = 3;

    [Tooltip("Jumlah keping vertikal")]
    public int piecesY = 3;

    [Tooltip("Kekuatan ledakan yang mendorong pecahan")]
    public float explosionForce = 5f;

    [Tooltip("Torque (putaran) acak pada pecahan")]
    public float torqueForce = 200f;

    [Tooltip("Berapa detik pecahan bertahan")]
    public float pieceLifetime = 3f;

    /// <summary>
    /// Panggil method ini untuk memecah log menjadi keping-keping.
    /// Akan membuat sprite pieces dari sprite renderer log.
    /// </summary>
    public void Shatter()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null)
        {
            Debug.LogWarning("[LogShatter] SpriteRenderer atau Sprite tidak ditemukan!");
            return;
        }

        Sprite originalSprite = sr.sprite;
        Texture2D texture = originalSprite.texture;

        // Pastikan texture bisa dibaca
        // (Di Unity, set Read/Write Enabled = true pada import settings texture)
        if (!texture.isReadable)
        {
            Debug.LogError("[LogShatter] Texture harus Read/Write Enabled! " +
                           "Cek Import Settings pada sprite texture.");
            return;
        }

        // Hitung ukuran setiap keping dalam pixel
        Rect spriteRect = originalSprite.rect;
        int pieceWidth = Mathf.FloorToInt(spriteRect.width / piecesX);
        int pieceHeight = Mathf.FloorToInt(spriteRect.height / piecesY);

        float ppu = originalSprite.pixelsPerUnit;

        // Hitung offset agar keping muncul di posisi yang benar
        // relatif terhadap pivot sprite asli
        Vector2 spriteSize = new Vector2(spriteRect.width / ppu, spriteRect.height / ppu);
        Vector2 startOffset = -spriteSize / 2f;

        for (int x = 0; x < piecesX; x++)
        {
            for (int y = 0; y < piecesY; y++)
            {
                // Buat texture keping dari potongan texture asli
                int startX = Mathf.FloorToInt(spriteRect.x) + (x * pieceWidth);
                int startY = Mathf.FloorToInt(spriteRect.y) + (y * pieceHeight);

                // Clamp agar tidak keluar dari batas texture
                int actualWidth = Mathf.Min(pieceWidth, texture.width - startX);
                int actualHeight = Mathf.Min(pieceHeight, texture.height - startY);

                if (actualWidth <= 0 || actualHeight <= 0) continue;

                // Ambil pixel dari area yang sesuai
                Color[] pixels = texture.GetPixels(startX, startY, actualWidth, actualHeight);

                // Buat texture baru untuk keping
                Texture2D pieceTex = new Texture2D(actualWidth, actualHeight);
                pieceTex.filterMode = texture.filterMode;
                pieceTex.SetPixels(pixels);
                pieceTex.Apply();

                // Buat sprite dari texture keping
                Sprite pieceSprite = Sprite.Create(
                    pieceTex,
                    new Rect(0, 0, actualWidth, actualHeight),
                    new Vector2(0.5f, 0.5f),
                    ppu
                );

                // Buat GameObject keping
                GameObject piece = new GameObject($"LogPiece_{x}_{y}");

                // Hitung posisi keping di world space
                // Sesuaikan dengan rotasi dan skala log saat ini
                Vector2 localPos = startOffset + new Vector2(
                    (x + 0.5f) * (pieceWidth / ppu),
                    (y + 0.5f) * (pieceHeight / ppu)
                );
                piece.transform.position = transform.TransformPoint(localPos);
                piece.transform.rotation = transform.rotation;
                piece.transform.localScale = transform.localScale;

                // Tambahkan SpriteRenderer
                SpriteRenderer pieceSR = piece.AddComponent<SpriteRenderer>();
                pieceSR.sprite = pieceSprite;
                pieceSR.sortingLayerID = sr.sortingLayerID;
                pieceSR.sortingOrder = sr.sortingOrder;

                // Tambahkan Rigidbody2D untuk fisika
                Rigidbody2D rb = piece.AddComponent<Rigidbody2D>();
                rb.mass = 0.3f;
                rb.gravityScale = 2f;

                // Hitung arah ledakan dari tengah log ke keping
                Vector2 direction = ((Vector2)piece.transform.position - (Vector2)transform.position).normalized;
                // Tambah sedikit random agar tidak terlalu simetris
                direction += Random.insideUnitCircle * 0.3f;

                // Beri gaya ledakan
                rb.AddForce(direction * explosionForce, ForceMode2D.Impulse);

                // Beri putaran acak
                rb.AddTorque(Random.Range(-torqueForce, torqueForce));

                // Tambahkan collider kecil (opsional, untuk interaksi dengan lantai)
                BoxCollider2D col = piece.AddComponent<BoxCollider2D>();
                col.size = new Vector2(actualWidth / ppu, actualHeight / ppu) * 0.8f;

                // Auto-destroy setelah beberapa detik
                Destroy(piece, pieceLifetime);
            }
        }
    }
}