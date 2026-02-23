using UnityEngine;
using UnityEngine.UI;

public class ProfilePanel : PanelController
{
    [SerializeField] private Button editProfileButton;
    [SerializeField] private Button viewProfileButton;
    [SerializeField] private Button shareButton;

    public override void OnShow()
    {
        base.OnShow();
        // Load current profile state here (stub)
    }

    public override void OnHide()
    {
        base.OnHide();
    }

    private void Awake()
    {
        if (editProfileButton != null) editProfileButton.onClick.AddListener(OnEditProfile);
        if (viewProfileButton != null) viewProfileButton.onClick.AddListener(OnViewProfile);
        if (shareButton != null) shareButton.onClick.AddListener(OnShare);
    }

    private void OnDestroy()
    {
        if (editProfileButton != null) editProfileButton.onClick.RemoveListener(OnEditProfile);
        if (viewProfileButton != null) viewProfileButton.onClick.RemoveListener(OnViewProfile);
        if (shareButton != null) shareButton.onClick.RemoveListener(OnShare);
    }

    private void OnEditProfile()
    {
        // TODO: open edit profile flow
        Debug.Log("Edit Profile clicked");
    }

    private void OnViewProfile()
    {
        // TODO: open view profile/details
        Debug.Log("View Profile clicked");
    }

    private void OnShare()
    {
        // TODO: share profile or invite friends
        Debug.Log("Share clicked");
    }
}
