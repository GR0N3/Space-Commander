using UnityEngine;

/// <summary>
/// Componente que permite a un objeto ser gestionado por ScreenCullingManager
/// para desactivar sus Renderers y optimizar sus Animators cuando no aparece en pantalla.
/// </summary>
[DisallowMultipleComponent]
public class CullableObject : MonoBehaviour
{
    [Tooltip("Radio base de bounding box si no hay renderers activos para calcular bounds")]
    public float fallbackRadius = 2.5f;

    private Renderer[] cachedRenderers;
    private Animator[] cachedAnimators;
    private bool isCurrentlyCulled = false;

    public bool IsCulled => isCurrentlyCulled;

    void Awake()
    {
        RefreshCache();
    }

    void OnEnable()
    {
        RefreshCache();
        isCurrentlyCulled = false;
        ApplyVisibility(true);

        if (ScreenCullingManager.Instance != null)
        {
            ScreenCullingManager.Instance.Register(this);
        }
    }

    void OnDisable()
    {
        // Restaurar visibilidad al desactivarse para que los pools funcionen limpiamente
        ApplyVisibility(true);
        isCurrentlyCulled = false;
    }

    void OnDestroy()
    {
        if (ScreenCullingManager.Instance != null)
        {
            ScreenCullingManager.Instance.Unregister(this);
        }
    }

    public void RefreshCache()
    {
        cachedRenderers = GetComponentsInChildren<Renderer>(true);
        cachedAnimators = GetComponentsInChildren<Animator>(true);
    }

    public Bounds GetCullingBounds(float extraMargin)
    {
        float size = (fallbackRadius + extraMargin) * 2f;
        Bounds b = new Bounds(transform.position, new Vector3(size, size, size));

        if (cachedRenderers != null)
        {
            for (int i = 0; i < cachedRenderers.Length; i++)
            {
                Renderer r = cachedRenderers[i];
                if (r != null && r.enabled)
                {
                    b.Encapsulate(r.bounds);
                }
            }
        }

        b.Expand(extraMargin * 2f);
        return b;
    }

    public void SetCulledState(bool culled)
    {
        if (isCurrentlyCulled == culled) return;
        isCurrentlyCulled = culled;
        ApplyVisibility(!culled);
    }

    private void ApplyVisibility(bool visible)
    {
        if (cachedRenderers != null)
        {
            for (int i = 0; i < cachedRenderers.Length; i++)
            {
                if (cachedRenderers[i] != null)
                {
                    cachedRenderers[i].enabled = visible;
                }
            }
        }

        if (cachedAnimators != null)
        {
            for (int i = 0; i < cachedAnimators.Length; i++)
            {
                if (cachedAnimators[i] != null)
                {
                    cachedAnimators[i].cullingMode = visible
                        ? AnimatorCullingMode.AlwaysAnimate
                        : AnimatorCullingMode.CullUpdateTransforms;
                }
            }
        }
    }
}
