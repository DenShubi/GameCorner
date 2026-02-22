using UnityEngine;
using UnityEngine.UI;

public class FooterController : MonoBehaviour
{
    [Tooltip("Footer button for Home (index 0)")]
    [SerializeField] private Button homeButton;

    [Tooltip("Footer button for Daily (index 1)")]
    [SerializeField] private Button dailyButton;

    [Tooltip("Footer button for Leaderboard (index 2)")]
    [SerializeField] private Button leaderboardButton;

    [Tooltip("Footer button for Profile (index 3)")]
    [SerializeField] private Button profileButton;

    [Tooltip("Optional MenuController to call when footer buttons are clicked")]
    [SerializeField] private MenuController menuController;

    [Tooltip("Color for selected button background")]
    [SerializeField] private Color selectedColor = new Color(0.15f, 0.6f, 1f, 1f);
    [SerializeField] private Color normalColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);
    
    // internal delegates so we can add/remove our listeners without touching user-set listeners
    private UnityEngine.Events.UnityAction[] clickActions;

    // Call to update footer visuals when a panel is selected
    public void SetSelected(int index)
    {
        Button[] buttons = new Button[] { homeButton, dailyButton, leaderboardButton, profileButton };
        if (buttons == null) return;

        index = Mathf.Clamp(index, 0, buttons.Length - 1);

        for (int i = 0; i < buttons.Length; i++)
        {
            var b = buttons[i];
            if (b == null) continue;
            var img = b.GetComponent<Image>();
            if (img != null)
                img.color = (i == index) ? selectedColor : normalColor;

            b.interactable = (i != index);
        }
    }

    void OnEnable()
    {
        AddListeners();
    }

    void OnDisable()
    {
        RemoveListeners();
    }

    void AddListeners()
    {
        Button[] buttons = new Button[] { homeButton, dailyButton, leaderboardButton, profileButton };
        if (menuController == null) return;

        // initialize action array
        clickActions = new UnityEngine.Events.UnityAction[buttons.Length];

        for (int i = 0; i < buttons.Length; i++)
        {
            var b = buttons[i];
            if (b == null) continue;
            int idx = i; // capture
            UnityEngine.Events.UnityAction action = () => menuController.ShowPanel(idx);
            clickActions[i] = action;
            b.onClick.AddListener(action);
        }
    }

    void RemoveListeners()
    {
        Button[] buttons = new Button[] { homeButton, dailyButton, leaderboardButton, profileButton };
        if (clickActions == null) return;

        for (int i = 0; i < buttons.Length && i < clickActions.Length; i++)
        {
            var b = buttons[i];
            if (b == null) continue;
            var action = clickActions[i];
            if (action != null)
                b.onClick.RemoveListener(action);
        }

        clickActions = null;
    }
}
