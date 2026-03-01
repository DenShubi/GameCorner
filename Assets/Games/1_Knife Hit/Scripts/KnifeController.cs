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

            // ===== DOUBLE HIT: ambil damage dari GameManager =====
            int damage = GameManager.instance.GetKnifeDamage();
            other.GetComponent<LogController>().TakeDamage(damage);
            // =====================================================

            gameObject.tag = "Knife";
        }
        else if (other.CompareTag("Knife"))
        {
            if (!isFlying) return;

            hasHit = true;
            isFlying = false;

            GameManager.instance.LoseHeart();

            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;

            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
            }

            Destroy(gameObject, 2f);
        }
    }
}