using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class LoadingSceneController : MonoBehaviour
{
    [Tooltip("Name of the scene to load (set to your menu scene name)")]
    public string sceneToLoad = "Menu";

    [Tooltip("Minimum time the loading/title screen stays visible")]
    public float minDisplayTime = 1.0f;

    [Tooltip("Optional UI slider to show load progress")]
    public Slider progressBar;

    IEnumerator Start()
    {
        float startTime = Time.time;
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneToLoad);
        op.allowSceneActivation = false;

        while (!op.isDone)
        {
            float progress = Mathf.Clamp01(op.progress / 0.9f);
            if (progressBar != null)
                progressBar.value = progress;

            if (op.progress >= 0.9f && Time.time - startTime >= minDisplayTime)
            {
                op.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
