using UnityEngine;
using UnityEngine.UI;

public class HomePanel : PanelController
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform featuredArea;
    [SerializeField] private RectTransform gamesListContainer;
    [SerializeField] private GameObject gameItemPrefab;

    // Called when panel becomes visible
    public override void OnShow()
    {
        base.OnShow();
        RefreshHome();
    }

    public override void OnHide()
    {
        base.OnHide();
    }

    public void RefreshHome()
    {
        // Clear existing list items and add placeholders if a prefab is assigned
        ClearChildren(gamesListContainer);

        if (gameItemPrefab != null && gamesListContainer != null)
        {
            // Example: instantiate a couple of placeholder items
            Instantiate(gameItemPrefab, gamesListContainer);
            Instantiate(gameItemPrefab, gamesListContainer);
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
