using UnityEngine;

/// <summary>
/// Power-up Double Knife. Menempel di log, ikut berputar.
/// Saat knife player mengenai power-up ini, player akan meluncurkan
/// 2 knife berdampingan untuk beberapa throw berikutnya.
/// </summary>
public class PowerUpDoubleKnife : MonoBehaviour
{
    [Header("Double Knife Settings")]
    [Tooltip("Berapa kali throw yang mendapat double knife")]
    public int throwCount = 3;

    private void OnTriggerEnter(Collider other)
    {
        KnifeController knife = other.GetComponent<KnifeController>();
        if (knife == null) return;

        GameManager.instance.ActivateDoubleKnife(throwCount);

        Debug.Log($"[PowerUp] Double Knife aktif! {throwCount} throw berikutnya spawn 2 knife");

        Destroy(gameObject);
    }
}