using UnityEngine;

public class KnifeController : MonoBehaviour
{
    public float speed = 40f;
    private bool isFlying = false;
    private bool hasHit = false; // Mencegah multiple trigger

    void Update()
    {
        if (isFlying)
            transform.Translate(Vector3.up * speed * Time.deltaTime, Space.World);
    }

    public void Shoot() => isFlying = true;

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return; // Sudah pernah hit, abaikan

        Debug.Log("Pisau menyentuh: " + other.name + " dengan Tag: " + other.tag);

        if (other.CompareTag("Log"))
        {
            hasHit = true;
            isFlying = false;
            transform.SetParent(other.transform);

            GameManager.instance.RegisterStuckKnife(gameObject);
            GameManager.instance.AddScore(10);

            other.GetComponent<LogController>().TakeDamage(1);
            gameObject.tag = "Knife"; // Berubah jadi penghalang
        }
        else if (other.CompareTag("Knife"))
        {
            hasHit = true;
            isFlying = false;

            // ===== HEART SYSTEM: Kurangi heart, bukan langsung game over =====
            GameManager.instance.LoseHeart();

            // Knife yang gagal jatuh ke bawah
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
            }

            // Hapus knife yang gagal setelah 2 detik
            Destroy(gameObject, 2f);
        }
    }
}