using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game2048
{
    public class TileBoard : MonoBehaviour
    {
        [SerializeField] private Tile tilePrefab;
        [SerializeField] private TileState[] tileStates;

        private TileGrid grid;
        private List<Tile> tiles;
        private bool waiting;

        private bool doubleMergeNext;
        private bool doubleValueNext;
        private int scoreMultiplierTurns;
        private bool undoShieldNext;
        private bool reverseSwipeNext;
        private int challengeTurnsRemaining;
        private int lastChallengeMilestone;

        private bool lastMoveHadMerge;

        private void Awake()
        {
            EnsureInitialized();
        }

        private void EnsureInitialized()
        {
            if (grid == null) {
                grid = GetComponentInChildren<TileGrid>(true);
            }

            if (grid == null) {
                grid = CreateRuntimeGrid();
            }

            if (grid != null) {
                grid.Initialize();
            }

            if (tileStates == null || tileStates.Length == 0) {
                tileStates = CreateDefaultTileStates();
            }

            if (tiles == null) {
                tiles = new List<Tile>(16);
            }
        }

        private TileGrid CreateRuntimeGrid()
        {
            GameObject gridObject = new GameObject("TileGrid", typeof(RectTransform), typeof(TileGrid));
            gridObject.transform.SetParent(transform, false);

            TileGrid runtimeGrid = gridObject.GetComponent<TileGrid>();

            for (int y = 0; y < 4; y++)
            {
                GameObject rowObject = new GameObject($"Row_{y}", typeof(RectTransform), typeof(TileRow));
                rowObject.transform.SetParent(gridObject.transform, false);

                for (int x = 0; x < 4; x++)
                {
                    GameObject cellObject = new GameObject($"Cell_{x}_{y}", typeof(RectTransform), typeof(TileCell));
                    cellObject.transform.SetParent(rowObject.transform, false);

                    RectTransform cellRect = cellObject.GetComponent<RectTransform>();
                    cellRect.anchoredPosition = new Vector2((x - 1.5f) * 130f, (1.5f - y) * 130f);
                    cellRect.sizeDelta = new Vector2(120f, 120f);
                }
            }

            return runtimeGrid;
        }

        private TileState[] CreateDefaultTileStates()
        {
            int[] values = { 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048 };
            TileState[] defaults = new TileState[values.Length];

            Color[] colors = new Color[]
            {
                new Color(0.227f, 0.745f, 1f),
                new Color(1f, 0.851f, 0.239f),
                new Color(1f, 0.420f, 0.420f),
                new Color(0.541f, 0.361f, 1f)
            };

            for (int i = 0; i < values.Length; i++)
            {
                TileState state = ScriptableObject.CreateInstance<TileState>();
                state.number = values[i];

                float t = i / (float)(values.Length - 1);
                t = t * (colors.Length - 1);
                int colorIndex = Mathf.FloorToInt(t);
                float colorT = t - colorIndex;

                if (colorIndex >= colors.Length - 1)
                {
                    state.backgroundColor = colors[colors.Length - 1];
                }
                else
                {
                    state.backgroundColor = Color.Lerp(colors[colorIndex], colors[colorIndex + 1], colorT);
                }

                state.textColor = Color.white;
                defaults[i] = state;
            }

            return defaults;
        }

        private Tile CreateRuntimeTile()
        {
            GameObject tileObject = new GameObject("Tile", typeof(RectTransform), typeof(Image), typeof(Tile));
            tileObject.transform.SetParent(grid.transform, false);

            RectTransform tileRect = tileObject.GetComponent<RectTransform>();
            tileRect.sizeDelta = new Vector2(120f, 120f);

            GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(tileObject.transform, false);

            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            TextMeshProUGUI label = textObject.GetComponent<TextMeshProUGUI>();
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = 42;
            label.enableAutoSizing = true;
            label.fontSizeMin = 18;
            label.fontSizeMax = 42;

            return tileObject.GetComponent<Tile>();
        }

        public void ClearBoard()
        {
            EnsureInitialized();

            if (grid == null || grid.cells == null) {
                Debug.LogError("TileBoard is missing TileGrid/TileCell setup.", this);
                return;
            }

            foreach (var cell in grid.cells) {
                cell.tile = null;
            }

            foreach (var tile in tiles) {
                Destroy(tile.gameObject);
            }

            tiles.Clear();
            ResetEffectState();
        }

        private void ResetEffectState()
        {
            doubleMergeNext = false;
            doubleValueNext = false;
            scoreMultiplierTurns = 0;
            undoShieldNext = false;
            reverseSwipeNext = false;
            challengeTurnsRemaining = 0;
            lastChallengeMilestone = 0;
            lastMoveHadMerge = false;
        }

        public void CreateTile()
        {
            EnsureInitialized();

            if (grid == null || tileStates == null || tileStates.Length == 0) {
                Debug.LogError("TileBoard is not configured correctly (Grid/TileStates).", this);
                return;
            }

            TileCell emptyCell = grid.GetRandomEmptyCell();
            if (emptyCell == null) {
                return;
            }

            Tile tile = tilePrefab != null
                ? Instantiate(tilePrefab, grid.transform)
                : CreateRuntimeTile();

            int spawnNumber = GetSpawnNumberByProgression();
            TileState initialState = FindTileStateByNumber(spawnNumber);

            if (initialState == null)
            {
                Debug.LogError($"TileState for spawn number {spawnNumber} was not found.", this);
                return;
            }

            tile.SetState(initialState);
            tile.SetSpecialType(GetRandomSpecialType());
            tile.Spawn(emptyCell);
            tiles.Add(tile);
        }

        private Tile.SpecialType GetRandomSpecialType()
        {
            int score = GameManager.Instance != null ? GameManager.Instance.score : 0;
            bool inChallenge = challengeTurnsRemaining > 0;

            float spawnChance = inChallenge ? 0.5f : 0.2f;
            if (Random.value > spawnChance) {
                return Tile.SpecialType.None;
            }

            float nerfWeight;
            if (inChallenge) {
                nerfWeight = 0.7f;
            } else if (score < 1000) {
                nerfWeight = 0.1f;
            } else if (score < 3000) {
                nerfWeight = 0.35f;
            } else {
                nerfWeight = 0.5f;
            }

            if (Random.value < nerfWeight)
            {
                Tile.SpecialType[] nerfs =
                {
                    Tile.SpecialType.LockedTile,
                    Tile.SpecialType.SplitTile,
                    Tile.SpecialType.FogBoard,
                    Tile.SpecialType.ReverseSwipe
                };

                return nerfs[Random.Range(0, nerfs.Length)];
            }

            Tile.SpecialType[] powers =
            {
                Tile.SpecialType.DoubleMerge,
                Tile.SpecialType.DoubleValue,
                Tile.SpecialType.MultiplierTurn,
                Tile.SpecialType.UndoShield,
                Tile.SpecialType.ClearSingleTile
            };

            return powers[Random.Range(0, powers.Length)];
        }

        private void Update()
        {
            if (waiting) return;

            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) {
                HandleMoveInput(Vector2Int.up);
            } else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) {
                HandleMoveInput(Vector2Int.left);
            } else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) {
                HandleMoveInput(Vector2Int.down);
            } else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) {
                HandleMoveInput(Vector2Int.right);
            }
        }

        private void HandleMoveInput(Vector2Int direction)
        {
            if (reverseSwipeNext)
            {
                direction *= -1;
                reverseSwipeNext = false;
            }

            if (direction == Vector2Int.up) {
                Move(direction, 0, 1, 1, 1);
            } else if (direction == Vector2Int.left) {
                Move(direction, 1, 1, 0, 1);
            } else if (direction == Vector2Int.down) {
                Move(direction, 0, 1, grid.Height - 2, -1);
            } else if (direction == Vector2Int.right) {
                Move(direction, grid.Width - 2, -1, 0, 1);
            }
        }

        private void Move(Vector2Int direction, int startX, int incrementX, int startY, int incrementY)
        {
            bool changed = false;
            bool mergedThisMove = false;

            for (int x = startX; x >= 0 && x < grid.Width; x += incrementX)
            {
                for (int y = startY; y >= 0 && y < grid.Height; y += incrementY)
                {
                    TileCell cell = grid.GetCell(x, y);

                    if (cell.Occupied && !cell.tile.IsLocked)
                    {
                        bool moved = MoveTile(cell.tile, direction, out bool merged);
                        changed |= moved;
                        mergedThisMove |= merged;
                    }
                }
            }

            if (changed)
            {
                lastMoveHadMerge = mergedThisMove;
                StartCoroutine(WaitForChanges());
            }
        }

        private bool MoveTile(Tile tile, Vector2Int direction, out bool merged)
        {
            merged = false;
            TileCell newCell = null;
            TileCell adjacent = grid.GetAdjacentCell(tile.cell, direction);

            while (adjacent != null)
            {
                if (adjacent.Occupied)
                {
                    if (CanMerge(tile, adjacent.tile))
                    {
                        MergeTiles(tile, adjacent.tile);
                        merged = true;
                        return true;
                    }

                    break;
                }

                newCell = adjacent;
                adjacent = grid.GetAdjacentCell(adjacent, direction);
            }

            if (newCell != null)
            {
                tile.MoveTo(newCell);
                return true;
            }

            return false;
        }

        private bool CanMerge(Tile a, Tile b)
        {
            return a.state == b.state && !b.locked;
        }

        private void MergeTiles(Tile a, Tile b)
        {
            Tile.SpecialType aSpecial = a.specialType;
            Tile.SpecialType bSpecial = b.specialType;

            tiles.Remove(a);
            a.Merge(b.cell);

            int index = Mathf.Clamp(IndexOf(b.state) + 1, 0, tileStates.Length - 1);
            if (doubleValueNext)
            {
                index = Mathf.Clamp(index + 1, 0, tileStates.Length - 1);
                doubleValueNext = false;
            }

            TileState newState = tileStates[index];
            b.SetState(newState);
            b.ClearSpecialType();

            int points = newState.number * 2;
            if (doubleMergeNext)
            {
                points *= 2;
                doubleMergeNext = false;
            }

            if (scoreMultiplierTurns > 0) {
                points *= 2;
            }

            GameManager.Instance.IncreaseScore(points);

            ActivateSpecial(aSpecial);
            ActivateSpecial(bSpecial);
        }

        private void ActivateSpecial(Tile.SpecialType special)
        {
            switch (special)
            {
                case Tile.SpecialType.DoubleMerge:
                    doubleMergeNext = true;
                    break;
                case Tile.SpecialType.DoubleValue:
                    doubleValueNext = true;
                    break;
                case Tile.SpecialType.MultiplierTurn:
                    scoreMultiplierTurns = 3;
                    break;
                case Tile.SpecialType.UndoShield:
                    undoShieldNext = true;
                    break;
                case Tile.SpecialType.ClearSingleTile:
                    ClearSmallTile();
                    break;
                case Tile.SpecialType.LockedTile:
                    LockRandomTile(3);
                    break;
                case Tile.SpecialType.SplitTile:
                    SplitBigTile();
                    break;
                case Tile.SpecialType.FogBoard:
                    ApplyFog(2);
                    break;
                case Tile.SpecialType.ReverseSwipe:
                    reverseSwipeNext = true;
                    break;
            }
        }

        private void ClearSmallTile()
        {
            List<Tile> smallTiles = new List<Tile>();
            foreach (var tile in tiles)
            {
                if (tile != null && tile.state != null && tile.state.number <= 8) {
                    smallTiles.Add(tile);
                }
            }

            if (smallTiles.Count == 0) {
                return;
            }

            Tile target = smallTiles[Random.Range(0, smallTiles.Count)];
            if (target.cell != null) {
                target.cell.tile = null;
            }

            tiles.Remove(target);
            Destroy(target.gameObject);
        }

        private void LockRandomTile(int turns)
        {
            if (tiles.Count == 0) {
                return;
            }

            Tile target = tiles[Random.Range(0, tiles.Count)];
            target.SetLockTurns(turns);
        }

        private void SplitBigTile()
        {
            Tile bigTile = null;
            foreach (var tile in tiles)
            {
                if (tile.state.number >= 16 && (bigTile == null || tile.state.number > bigTile.state.number)) {
                    bigTile = tile;
                }
            }

            if (bigTile == null) {
                return;
            }

            int half = bigTile.state.number / 2;
            TileState halfState = FindTileStateByNumber(half);
            if (halfState == null) {
                return;
            }

            TileCell empty = grid.GetRandomEmptyCell();
            if (empty == null) {
                return;
            }

            bigTile.SetState(halfState);

            Tile newTile = tilePrefab != null
                ? Instantiate(tilePrefab, grid.transform)
                : CreateRuntimeTile();

            newTile.SetState(halfState);
            newTile.Spawn(empty);
            tiles.Add(newTile);
        }

        private void ApplyFog(int turns)
        {
            int count = Mathf.Min(4, tiles.Count);
            for (int i = 0; i < count; i++)
            {
                Tile target = tiles[Random.Range(0, tiles.Count)];
                target.SetFogTurns(turns);
            }
        }

        private int IndexOf(TileState state)
        {
            for (int i = 0; i < tileStates.Length; i++)
            {
                if (state == tileStates[i]) {
                    return i;
                }
            }

            return -1;
        }

        private IEnumerator WaitForChanges()
        {
            waiting = true;

            yield return new WaitForSeconds(0.1f);

            waiting = false;

            foreach (var tile in tiles) {
                tile.locked = false;
                tile.ReduceLockTurn();
                tile.ReduceFogTurn();
            }

            if (scoreMultiplierTurns > 0) {
                scoreMultiplierTurns--;
            }

            if (challengeTurnsRemaining > 0) {
                challengeTurnsRemaining--;
            }

            CheckChallengePhase();

            if (tiles.Count != grid.Size)
            {
                if (undoShieldNext)
                {
                    if (!lastMoveHadMerge) {
                        // skip spawn
                    } else {
                        CreateTile();
                    }

                    undoShieldNext = false;
                }
                else
                {
                    CreateTile();
                }
            }

            GameManager.Instance.SaveCurrentGame();

            if (CheckForGameOver()) {
                GameManager.Instance.GameOver();
            }
        }

        private void CheckChallengePhase()
        {
            int score = GameManager.Instance != null ? GameManager.Instance.score : 0;
            int milestone = score / 2048;

            if (milestone > lastChallengeMilestone)
            {
                lastChallengeMilestone = milestone;
                challengeTurnsRemaining = 5;
            }
        }

        public bool CheckForGameOver()
        {
            if (tiles.Count != grid.Size) {
                return false;
            }

            foreach (var tile in tiles)
            {
                TileCell up = grid.GetAdjacentCell(tile.cell, Vector2Int.up);
                TileCell down = grid.GetAdjacentCell(tile.cell, Vector2Int.down);
                TileCell left = grid.GetAdjacentCell(tile.cell, Vector2Int.left);
                TileCell right = grid.GetAdjacentCell(tile.cell, Vector2Int.right);

                if (up != null && CanMerge(tile, up.tile)) {
                    return false;
                }

                if (down != null && CanMerge(tile, down.tile)) {
                    return false;
                }

                if (left != null && CanMerge(tile, left.tile)) {
                    return false;
                }

                if (right != null && CanMerge(tile, right.tile)) {
                    return false;
                }
            }

            return true;
        }

        public int[] GetBoardState()
        {
            int[] state = new int[grid.Size];

            for (int i = 0; i < state.Length; i++) {
                state[i] = grid.cells[i].Occupied ? grid.cells[i].tile.state.number : 0;
            }

            return state;
        }

        public void RestoreFromState(int[] boardState)
        {
            EnsureInitialized();

            if (grid == null || boardState == null || boardState.Length != grid.Size) {
                Debug.LogError("Cannot restore board state.", this);
                return;
            }

            for (int i = 0; i < boardState.Length; i++)
            {
                if (boardState[i] > 0)
                {
                    TileState tileState = FindTileStateByNumber(boardState[i]);
                    if (tileState != null)
                    {
                        Tile tile = tilePrefab != null
                            ? Instantiate(tilePrefab, grid.transform)
                            : CreateRuntimeTile();

                        tile.SetState(tileState);
                        tile.Spawn(grid.cells[i]);
                        tiles.Add(tile);
                    }
                }
            }
        }

        private int GetSpawnNumberByProgression()
        {
            int score = GameManager.Instance != null ? GameManager.Instance.score : 0;
            float roll = Random.value;

            // Early game: mostly 2, small chance 4
            if (score < 1000)
            {
                if (roll < 0.85f) return 2;
                return 4;
            }

            // Mid game: 2/4 balanced, small chance 8
            if (score < 3000)
            {
                if (roll < 0.65f) return 2;
                if (roll < 0.92f) return 4;
                return 8;
            }

            // Late game: more 4 and occasional 8
            if (roll < 0.45f) return 2;
            if (roll < 0.82f) return 4;
            return 8;
        }

        private TileState FindTileStateByNumber(int number)
        {
            foreach (var state in tileStates)
            {
                if (state != null && state.number == number) {
                    return state;
                }
            }

            return null;
        }
    }
}
