using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransition : MonoBehaviour
{
    private static SceneTransition instance;
    private CanvasGroup overlay;

    public static SceneTransition Instance
    {
        get
        {
            if (instance == null)
            {
                var go = new GameObject("SceneTransition");
                instance = go.AddComponent<SceneTransition>();
                DontDestroyOnLoad(go);
                instance.EnsureOverlay();
            }
            return instance;
        }
    }

    private void EnsureOverlay()
    {
        if (overlay != null) return;
        var canvasGO = new GameObject("SceneTransitionCanvas");
        canvasGO.transform.SetParent(transform);
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32767;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        var imageGO = new GameObject("Overlay");
        imageGO.transform.SetParent(canvasGO.transform);
        var image = imageGO.AddComponent<Image>();
        image.color = Color.black;
        image.raycastTarget = true;
        var rect = image.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        overlay = imageGO.AddComponent<CanvasGroup>();
        overlay.alpha = 0f;
        overlay.blocksRaycasts = false;
        overlay.interactable = false;
    }

    public static void LoadScene(string sceneName, float duration = 0.5f)
    {
        Instance.StartCoroutine(Instance.FadeAndLoad(() => SceneManager.LoadScene(sceneName), duration));
    }

    public static void LoadScene(int sceneIndex, float duration = 0.5f)
    {
        Instance.StartCoroutine(Instance.FadeAndLoad(() => SceneManager.LoadScene(sceneIndex), duration));
    }

    public static void FadeInFromBlack(float duration = 0.5f)
    {
        Instance.StartCoroutine(Instance.FadeInCoroutine(duration));
    }

    private IEnumerator FadeAndLoad(System.Action loadAction, float duration)
    {
        EnsureOverlay();
        overlay.blocksRaycasts = true;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            overlay.alpha = Mathf.Clamp01(t / duration);
            yield return null;
        }

        loadAction();
        yield return null;

        t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            overlay.alpha = 1f - Mathf.Clamp01(t / duration);
            yield return null;
        }
        overlay.alpha = 0f;
        overlay.blocksRaycasts = false;
    }

    private IEnumerator FadeInCoroutine(float duration)
    {
        EnsureOverlay();
        overlay.alpha = 1f;
        overlay.blocksRaycasts = true;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            overlay.alpha = 1f - Mathf.Clamp01(t / duration);
            yield return null;
        }
        overlay.alpha = 0f;
        overlay.blocksRaycasts = false;
    }
}
