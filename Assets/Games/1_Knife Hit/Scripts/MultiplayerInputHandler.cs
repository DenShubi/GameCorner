using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Menangani input 2 player di 1 layar.
/// Layar dibagi 2 secara horizontal:
/// - Bagian bawah (Y < 50% layar) = Player 1
/// - Bagian atas (Y >= 50% layar) = Player 2
/// Spawn knife untuk masing-masing player.
/// </summary>
public class MultiplayerInputHandler : MonoBehaviour
{
    [Header("Knife Settings")]
    public GameObject knifePrefab;

    [Header("Spawn Points")]
    [Tooltip("Posisi spawn knife Player 1 (bawah layar)")]
    public Transform p1SpawnPoint;

    [Tooltip("Posisi spawn knife Player 2 (atas layar)")]
    public Transform p2SpawnPoint;

    [Header("Cooldown")]
    [Tooltip("Cooldown antar throw per player (detik)")]
    public float throwCooldown = 0.4f;

    // Internal — masing-masing player punya knife sendiri
    private List<MultiplayerKnifeController> p1Knives = new List<MultiplayerKnifeController>();
    private List<MultiplayerKnifeController> p2Knives = new List<MultiplayerKnifeController>();

    private bool p1CanThrow = true;
    private bool p2CanThrow = true;

    void Start()
    {
        SpawnKnifeForPlayer(1);
        SpawnKnifeForPlayer(2);
    }

    void Update()
    {
        if (MultiplayerManager.instance.isGameOver) return;

        // ===== HANDLE TOUCH INPUT (mobile) =====
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            if (touch.phase == TouchPhase.Began)
            {
                HandleInput(touch.position);
            }
        }

        // ===== HANDLE MOUSE INPUT (editor/PC) =====
        if (Input.GetMouseButtonDown(0))
        {
            HandleInput(Input.mousePosition);
        }
    }

    private void HandleInput(Vector2 screenPosition)
    {
        float halfScreen = Screen.height / 2f;

        if (screenPosition.y < halfScreen)
        {
            // ===== BAGIAN BAWAH = PLAYER 1 =====
            ShootPlayer(1);
        }
        else
        {
            // ===== BAGIAN ATAS = PLAYER 2 =====
            ShootPlayer(2);
        }
    }

    private void ShootPlayer(int playerID)
    {
        if (playerID == 1 && p1CanThrow && p1Knives.Count > 0)
        {
            foreach (var knife in p1Knives)
            {
                if (knife != null) knife.Shoot();
            }
            p1Knives.Clear();

            p1CanThrow = false;
            Invoke(nameof(RespawnP1), throwCooldown);
        }
        else if (playerID == 2 && p2CanThrow && p2Knives.Count > 0)
        {
            foreach (var knife in p2Knives)
            {
                if (knife != null) knife.Shoot();
            }
            p2Knives.Clear();

            p2CanThrow = false;
            Invoke(nameof(RespawnP2), throwCooldown);
        }
    }

    private void SpawnKnifeForPlayer(int playerID)
    {
        if (MultiplayerManager.instance.isGameOver) return;
        if (knifePrefab == null) return;

        Transform spawnPoint = (playerID == 1) ? p1SpawnPoint : p2SpawnPoint;
        if (spawnPoint == null) return;

        GameObject knifeObj = Instantiate(knifePrefab, spawnPoint.position, Quaternion.identity);

        // Setup MultiplayerKnifeController
        // Hapus KnifeController biasa jika ada (dari prefab singleplayer)
        KnifeController oldKC = knifeObj.GetComponent<KnifeController>();
        if (oldKC != null) Destroy(oldKC);

        MultiplayerKnifeController mpKnife = knifeObj.GetComponent<MultiplayerKnifeController>();
        if (mpKnife == null) mpKnife = knifeObj.AddComponent<MultiplayerKnifeController>();

        mpKnife.playerID = playerID;

        // P2 knife diputar 180° (menghadap ke bawah)
        if (playerID == 2)
        {
            knifeObj.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
        }

        if (playerID == 1)
            p1Knives.Add(mpKnife);
        else
            p2Knives.Add(mpKnife);

        Debug.Log($"[Multi] Knife spawned for P{playerID}");
    }

    // ===== RESPAWN CALLBACKS =====
    private void RespawnP1()
    {
        p1CanThrow = true;
        SpawnKnifeForPlayer(1);
    }

    private void RespawnP2()
    {
        p2CanThrow = true;
        SpawnKnifeForPlayer(2);
    }
}