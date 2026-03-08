using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game2048
{
    public class Tile : MonoBehaviour
    {
        public enum SpecialType
        {
            None,
            DoubleMerge,
            DoubleValue,
            MultiplierTurn,
            UndoShield,
            ClearSingleTile,
            LockedTile,
            SplitTile,
            FogBoard,
            ReverseSwipe
        }

        public TileState state { get; private set; }
        public TileCell cell { get; private set; }
        public bool locked { get; set; }

        public SpecialType specialType { get; private set; } = SpecialType.None;
        public int lockTurnsRemaining { get; private set; }
        public int fogTurnsRemaining { get; private set; }

        public bool IsLocked => lockTurnsRemaining > 0;
        public bool IsFogged => fogTurnsRemaining > 0;

        private Image background;
        private TextMeshProUGUI text;

        private void Awake()
        {
            EnsureVisualReferences();
        }

        private void EnsureVisualReferences()
        {
            if (background == null) {
                background = GetComponent<Image>();
            }

            if (background == null) {
                background = gameObject.AddComponent<Image>();
            }

            if (text == null) {
                text = GetComponentInChildren<TextMeshProUGUI>(true);
            }

            if (text == null)
            {
                GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                textObject.transform.SetParent(transform, false);

                RectTransform textRect = textObject.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;

                text = textObject.GetComponent<TextMeshProUGUI>();
                text.alignment = TextAlignmentOptions.Center;
                text.enableAutoSizing = true;
                text.fontSizeMin = 18;
                text.fontSizeMax = 42;
            }
        }

        public void SetState(TileState state)
        {
            EnsureVisualReferences();

            if (state == null)
            {
                Debug.LogError("TileState is null in Tile.SetState().", this);
                return;
            }

            this.state = state;

            if (background != null) {
                background.color = state.backgroundColor;
            }

            UpdateLabel();
        }

        public void SetSpecialType(SpecialType type)
        {
            specialType = type;
            UpdateLabel();
        }

        public void ClearSpecialType()
        {
            specialType = SpecialType.None;
            UpdateLabel();
        }

        public void SetLockTurns(int turns)
        {
            lockTurnsRemaining = Mathf.Max(lockTurnsRemaining, turns);
            UpdateLabel();
        }

        public void ReduceLockTurn()
        {
            if (lockTurnsRemaining > 0) {
                lockTurnsRemaining--;
            }

            UpdateLabel();
        }

        public void SetFogTurns(int turns)
        {
            fogTurnsRemaining = Mathf.Max(fogTurnsRemaining, turns);
            UpdateLabel();
        }

        public void ReduceFogTurn()
        {
            if (fogTurnsRemaining > 0) {
                fogTurnsRemaining--;
            }

            UpdateLabel();
        }

        private void UpdateLabel()
        {
            if (text == null || state == null) {
                return;
            }

            string baseLabel = IsFogged ? "?" : state.number.ToString();
            string tag = GetSpecialTag(specialType);
            string status = IsLocked ? "LOCKED" : string.Empty;

            text.color = state.textColor;
            text.text = string.IsNullOrEmpty(tag) && string.IsNullOrEmpty(status)
                ? baseLabel
                : $"{baseLabel}\n{tag} {status}".Trim();

            if (background != null)
            {
                Color tileColor = state.backgroundColor;

                if (specialType != SpecialType.None) {
                    tileColor = Color.Lerp(tileColor, Color.white, 0.15f);
                }

                if (IsLocked) {
                    tileColor = Color.Lerp(tileColor, Color.gray, 0.35f);
                }

                if (IsFogged) {
                    tileColor = Color.Lerp(tileColor, Color.black, 0.4f);
                }

                background.color = tileColor;
            }
        }

        private string GetSpecialTag(SpecialType type)
        {
            switch (type)
            {
                case SpecialType.DoubleMerge: return "DOUBLE MERGE";
                case SpecialType.DoubleValue: return "DOUBLE VALUE";
                case SpecialType.MultiplierTurn: return "SCORE MULTIPLIER";
                case SpecialType.UndoShield: return "UNDO SHIELD";
                case SpecialType.ClearSingleTile: return "CLEAR TILE";
                case SpecialType.LockedTile: return "LOCKED TILE";
                case SpecialType.SplitTile: return "SPLIT TILE";
                case SpecialType.FogBoard: return "FOG";
                case SpecialType.ReverseSwipe: return "REVERSE SWIPE";
                default: return string.Empty;
            }
        }

        public void Spawn(TileCell cell)
        {
            if (this.cell != null) {
                this.cell.tile = null;
            }

            this.cell = cell;
            this.cell.tile = this;

            transform.position = cell.transform.position;
        }

        public void MoveTo(TileCell cell)
        {
            if (this.cell != null) {
                this.cell.tile = null;
            }

            this.cell = cell;
            this.cell.tile = this;

            StartCoroutine(Animate(cell.transform.position, false));
        }

        public void Merge(TileCell cell)
        {
            if (this.cell != null) {
                this.cell.tile = null;
            }

            this.cell = null;
            cell.tile.locked = true;

            StartCoroutine(Animate(cell.transform.position, true));
        }

        private IEnumerator Animate(Vector3 to, bool merging)
        {
            float elapsed = 0f;
            float duration = 0.1f;

            Vector3 from = transform.position;

            while (elapsed < duration)
            {
                transform.position = Vector3.Lerp(from, to, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.position = to;

            if (merging) {
                Destroy(gameObject);
            }
        }
    }
}
