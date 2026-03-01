using UnityEngine;

public class KnifeController : MonoBehaviour
{
    public float speed = 40f;
    private bool isFlying = false;
    private bool hasHit = false;

    void Update()
    {
        if (isFlying)
            transform.Translate(Vector3.up * speed * Time.deltaTime, Space.World);
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

            GameManager.instance.RegisterStuckKnife(gameObject);
            GameManager.instance.AddScore(10);

            other.GetComponent<LogController>().TakeDamage(1);
            gameObject.tag = "Knife";
        }
        else if (other.CompareTag("Knife"))
        {
            // Hanya knife yang sedang TERBANG yang boleh trigger LoseHeart
            // Knife yang sudah menancap (isFlying=false) TIDAK boleh trigger
            if (!isFlying) return;

            hasHit = true;
            isFlying = false;

            // Kurangi heart
            GameManager.instance.LoseHeart();

            // Matikan collider LANGSUNG agar tidak double trigger
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;

            // Knife jatuh ke bawah
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
            }

            // Hapus knife yang gagal
            Destroy(gameObject, 2f);
        }
    }
}