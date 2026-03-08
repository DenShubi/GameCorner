using UnityEngine;
using System;

namespace Game2048
{
    public class TileRow : MonoBehaviour
{
    public TileCell[] cells { get; private set; }

    private void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        cells = GetComponentsInChildren<TileCell>(true);

        Array.Sort(cells, (a, b) =>
        {
            float ax = a.transform.localPosition.x;
            float bx = b.transform.localPosition.x;
            return ax.CompareTo(bx);
        });
    }

}
}
