using UnityEngine;
using TMPro;

public class HeaderController : MonoBehaviour
{
    [Header("Username")]
    [SerializeField] private TextMeshProUGUI usernameTMP;

    [Header("Currency")]
    [SerializeField] private TextMeshProUGUI goldTMP;
    [SerializeField] private TextMeshProUGUI gemTMP;

    // Update UI with profile values (TextMeshPro only)
    public void SetProfile(string username, int gold, int gems)
    {
        if (usernameTMP != null) usernameTMP.text = username;
        if (goldTMP != null) goldTMP.text = gold.ToString();
        if (gemTMP != null) gemTMP.text = gems.ToString();
    }

    // Called when the visible content panel changes - useful for small header tweaks
    public void OnPanelChanged(int panelIndex)
    {
        // Optional: animate, show context hints, etc.
    }
}
