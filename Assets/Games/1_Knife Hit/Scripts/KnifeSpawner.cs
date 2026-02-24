using UnityEngine;

public class KnifeSpawner : MonoBehaviour
{
    public GameObject knifePrefab;
    public Transform spawnPoint;
    private KnifeController currentKnife;

    void Start() => SpawnNewKnife();

    void Update()
    {
        if (GameManager.instance.isGameOver) return;

        if (Input.GetMouseButtonDown(0) && currentKnife != null)
        {
            currentKnife.Shoot();
            currentKnife = null;
            Invoke(nameof(SpawnNewKnife), 0.15f);
        }
    }

    void SpawnNewKnife()
    {
        if (knifePrefab == null || spawnPoint == null) return;

        GameObject newKnife = Instantiate(knifePrefab, spawnPoint.position, Quaternion.identity);
        currentKnife = newKnife.GetComponent<KnifeController>();
    }
}
