using UnityEngine;

public class KnifeController : MonoBehaviour
{
    public float speed = 40f;
    private bool isFlying = false;

    void Update()
    {
        if (isFlying)
            transform.Translate(Vector3.up * speed * Time.deltaTime, Space.World);
    }

    public void Shoot() => isFlying = true;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Pisau menyentuh: " + other.name + " dengan Tag: " + other.tag);
        if (other.CompareTag("Log"))
        {
            isFlying = false;
            transform.SetParent(other.transform);

            GameManager.instance.RegisterStuckKnife(gameObject);
            GameManager.instance.AddScore(10);

            other.GetComponent<LogController>().TakeDamage(1);
            gameObject.tag = "Knife"; // Berubah jadi penghalang
        }
        else if (other.CompareTag("Knife"))
        {
            isFlying = false;
            GameManager.instance.TriggerGameOver();

            Rigidbody rb = GetComponent<Rigidbody>();
            if(rb != null) {
                rb.isKinematic = false;
                rb.AddForce(Vector3.down * 10f, ForceMode.Impulse);
            }
        }
    }
}
