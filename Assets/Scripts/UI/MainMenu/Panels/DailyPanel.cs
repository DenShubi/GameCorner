using UnityEngine;
using UnityEngine.UI;

public class DailyPanel : PanelController
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform mainChallengesContainer;
    [SerializeField] private RectTransform otherChallengesContainer;
    [SerializeField] private GameObject challengeItemPrefab;

    public override void OnShow()
    {
        base.OnShow();
        RefreshDaily();
    }

    public override void OnHide()
    {
        base.OnHide();
    }

    public void RefreshDaily()
    {
        ClearChildren(mainChallengesContainer);
        ClearChildren(otherChallengesContainer);

        if (challengeItemPrefab != null)
        {
            // Example placeholders
            Instantiate(challengeItemPrefab, mainChallengesContainer);
            Instantiate(challengeItemPrefab, otherChallengesContainer);
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
