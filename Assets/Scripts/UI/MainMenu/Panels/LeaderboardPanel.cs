using UnityEngine;
using UnityEngine.UI;

public class LeaderboardPanel : PanelController
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform scoresContainer;
    [SerializeField] private GameObject scoreItemPrefab; // prefab showing game name + score

    public override void OnShow()
    {
        base.OnShow();
        RefreshLeaderboard();
    }

    public override void OnHide()
    {
        base.OnHide();
    }

    public void RefreshLeaderboard()
    {
        ClearChildren(scoresContainer);

        if (scoreItemPrefab != null && scoresContainer != null)
        {
            // Example: create a couple of placeholder entries
            Instantiate(scoreItemPrefab, scoresContainer);
            Instantiate(scoreItemPrefab, scoresContainer);
        }

        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 1f;
    }

    private void ClearChildren(Transform parent)
    {
        if (parent == null) return;
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            var go = parent.GetChild(i).gameObject;
#if UNITY_EDITOR
            if (!Application.isPlaying) DestroyImmediate(go); else Destroy(go);
#else
            Destroy(go);
#endif
        }
    }
}
