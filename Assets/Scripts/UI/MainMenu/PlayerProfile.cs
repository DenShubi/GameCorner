using UnityEngine;

[CreateAssetMenu(fileName = "PlayerProfile", menuName = "GameCorner/PlayerProfile", order = 100)]
public class PlayerProfile : ScriptableObject
{
    [SerializeField] private string username = "Player";
    [SerializeField] private int gold = 0;
    [SerializeField] private int gems = 0;

    public void ApplyTo(HeaderController header)
    {
        if (header != null)
            header.SetProfile(username, gold, gems);
    }
}
