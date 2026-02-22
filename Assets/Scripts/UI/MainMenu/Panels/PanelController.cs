using UnityEngine;

public class PanelController : MonoBehaviour
{
    // Called by MenuController when this panel becomes visible
    public virtual void OnShow()
    {
        gameObject.SetActive(true);
    }

    // Called by MenuController when this panel is hidden
    public virtual void OnHide()
    {
        gameObject.SetActive(false);
    }
}
