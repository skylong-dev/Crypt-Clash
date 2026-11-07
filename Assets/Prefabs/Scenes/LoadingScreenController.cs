
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class LoadingScreenController : MonoBehaviour
{
    public Slider loadingBar;
    public float loadingDuration = 2.5f;

    private void Start()
    {
        if (loadingBar == null)
        {
            Debug.LogError("Loading Bar is not assigned!");
            return;
        }

        loadingBar.minValue = 0f;
        loadingBar.maxValue = 1f;
        loadingBar.value = 0f;

        StartCoroutine(LoadSceneAsync("CharacterSelect"));
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        Debug.Log("Starting to load: " + sceneName);

        AsyncOperation operation =
            SceneManager.LoadSceneAsync(sceneName);

        if (operation == null)
        {
            Debug.LogError("Could not load scene: " + sceneName);
            yield break;
        }

        operation.allowSceneActivation = false;

        float elapsed = 0f;

        // Fill the bar over 2.5 seconds
        while (elapsed < loadingDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            loadingBar.value = Mathf.Clamp01(
                elapsed / loadingDuration
            );

            yield return null;
        }

        // Wait until Unity finishes loading the scene
        while (operation.progress < 0.9f)
        {
            yield return null;
        }

        loadingBar.value = 1f;

        Debug.Log("Loading complete. Switching scenes...");

        // Allow Unity to switch to MapSelection
        operation.allowSceneActivation = true;
    }
}
