using UnityEngine;

public class LogController : MonoBehaviour
{
    public float rotationSpeed = 100f;
    public int toughness = 10;

    void Start()
    {
        Debug.Log("Log HP: " + toughness);
    }

    void Update()
    {
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }

    public void TakeDamage(int damage)
    {
        Debug.Log("Log HP: " + toughness);
        toughness -= damage;
        if (toughness <= 0)
        {
            // Matikan collider agar tidak bisa dipukul lagi saat proses hancur
            if (GetComponent<Collider>()) GetComponent<Collider>().enabled = false;

            // ======= EFEK HANCUR BERKEPING =======
            LogShatter shatter = GetComponent<LogShatter>();
            if (shatter != null)
            {
                shatter.Shatter(); // Pecahkan sprite jadi keping-keping!
            }
            // =====================================

            GameManager.instance.LogDestroyed();
            Destroy(gameObject);
        }
    }
}