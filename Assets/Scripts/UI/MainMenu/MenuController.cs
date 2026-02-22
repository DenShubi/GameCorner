using UnityEngine;

public class MenuController : MonoBehaviour
{
    [Tooltip("Content panels for Home, Daily, Leaderboard, Profile (in that order)")]
    [SerializeField] private GameObject[] contentPanels;

    [Tooltip("Footer controller that manages footer buttons/visuals")]
    [SerializeField] private FooterController footerController;

    [Tooltip("Optional header controller to update profile/gold/gems")]
    [SerializeField] private HeaderController header;

    [Tooltip("Optional PlayerProfile data source to populate header on start")]
    [SerializeField] private PlayerProfile playerProfile;

    [Tooltip("Which panel to show on start (0 = Home)")]
    [SerializeField] private int startingPanel = 0;

    private int currentIndex = -1;

    void Start()
    {
        // populate header from profile if present
        if (header != null && playerProfile != null)
            playerProfile.ApplyTo(header);

        // disable all panels initially
        if (contentPanels != null)
        {
            for (int i = 0; i < contentPanels.Length; i++)
            {
                var go = contentPanels[i];
                if (go == null) continue;
                var panel = go.GetComponent<PanelController>();
                if (panel != null) panel.OnHide(); else go.SetActive(false);
            }
        }

        ShowPanel(startingPanel);
    }

    // Called by footer buttons (set in the inspector) or other UI handlers
    public void ShowPanel(int index)
    {
        if (contentPanels == null || contentPanels.Length == 0)
            return;

        index = Mathf.Clamp(index, 0, contentPanels.Length - 1);

        if (index == currentIndex)
            return; // already showing

        // hide current
        if (currentIndex >= 0 && currentIndex < contentPanels.Length)
        {
            var cur = contentPanels[currentIndex];
            if (cur != null)
            {
                var panel = cur.GetComponent<PanelController>();
                if (panel != null) panel.OnHide(); else cur.SetActive(false);
            }
        }

        // show new
        var next = contentPanels[index];
        if (next != null)
        {
            var panel = next.GetComponent<PanelController>();
            if (panel != null) panel.OnShow(); else next.SetActive(true);
        }

        currentIndex = index;

        if (footerController != null)
            footerController.SetSelected(index);

        if (header != null)
            header.OnPanelChanged(index);
    }
}
