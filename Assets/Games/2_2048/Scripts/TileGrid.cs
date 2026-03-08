using UnityEngine;
using System;
using System.Collections.Generic;

namespace Game2048
{
    public class TileGrid : MonoBehaviour
{
    public TileRow[] rows { get; private set; }
    public TileCell[] cells { get; private set; }

    public int Size => cells != null ? cells.Length : 0;
    public int Height => rows != null ? rows.Length : 0;
    public int Width => Height > 0 ? Size / Height : 0;

    private void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        rows = GetComponentsInChildren<TileRow>(true);

        Array.Sort(rows, (a, b) =>
        {
            float ay = a.transform.localPosition.y;
            float by = b.transform.localPosition.y;
            return by.CompareTo(ay);
        });

        for (int r = 0; r < rows.Length; r++) {
            rows[r].Initialize();
        }

        List<TileCell> orderedCells = new List<TileCell>();
        for (int r = 0; r < rows.Length; r++) {
            orderedCells.AddRange(rows[r].cells);
        }

        cells = orderedCells.ToArray();

        if (rows.Length == 0 || cells.Length == 0) {
            return;
        }

        for (int i = 0; i < cells.Length; i++) {
            cells[i].coordinates = new Vector2Int(i % Width, i / Width);
        }
    }

    public TileCell GetCell(Vector2Int coordinates)
    {
        return GetCell(coordinates.x, coordinates.y);
    }

    public TileCell GetCell(int x, int y)
    {
        if (rows == null || rows.Length == 0) {
            return null;
        }

        if (x >= 0 && x < Width && y >= 0 && y < Height) {
            return rows[y].cells[x];
        } else {
            return null;
        }
    }

    public TileCell GetAdjacentCell(TileCell cell, Vector2Int direction)
    {
        Vector2Int coordinates = cell.coordinates;
        coordinates.x += direction.x;
        coordinates.y -= direction.y;

        return GetCell(coordinates);
    }

    public TileCell GetRandomEmptyCell()
    {
        if (cells == null || cells.Length == 0) {
            return null;
        }

        int index = UnityEngine.Random.Range(0, cells.Length);
        int startingIndex = index;

        while (cells[index].Occupied)
        {
            index++;

            if (index >= cells.Length) {
                index = 0;
            }

            // all cells are occupied
            if (index == startingIndex) {
                return null;
            }
        }

        return cells[index];
 
}   }

}
