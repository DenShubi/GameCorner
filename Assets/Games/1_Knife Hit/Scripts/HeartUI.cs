using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Mengelola tampilan heart icons di UI.
/// Pasang pada Canvas, assign heart Image objects ke array.
/// </summary>
public class HeartUI : MonoBehaviour
{
    [Header("Heart Icons")]
    [Tooltip("Drag heart Image objects ke sini (urut dari kiri ke kanan)")]
    public Image[] heartImages;

    [Header("Sprites")]
    [Tooltip("Sprite heart penuh (merah/aktif)")]
    public Sprite heartFull;

    [Tooltip("Sprite heart kosong (abu/mati)")]
    public Sprite heartEmpty;

    /// <summary>
    /// Update tampilan heart berdasarkan jumlah heart saat ini.
    /// </summary>
    public void UpdateHearts(int currentHearts)
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] == null) continue;

            if (i < currentHearts)
            {
                // Heart masih ada
                heartImages[i].sprite = heartFull;
                heartImages[i].color = Color.white;
            }
            else
            {
                // Heart sudah hilang
                if (heartEmpty != null)
                {
                    heartImages[i].sprite = heartEmpty;
                    heartImages[i].color = Color.white;
                }
                else
                {
                    // Jika tidak ada sprite kosong, buat semi-transparan
                    heartImages[i].sprite = heartFull;
                    heartImages[i].color = new Color(1f, 1f, 1f, 0.25f);
                }
            }
        }
    }
}