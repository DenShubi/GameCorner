using UnityEngine;

namespace Game2048
{
    [CreateAssetMenu(menuName = "2048/Tile State")]
    public class TileState : ScriptableObject
{
    public int number;
    public Color backgroundColor;
    public Color textColor;
}
}
