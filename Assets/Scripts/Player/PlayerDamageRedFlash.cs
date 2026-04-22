using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDamageRedFlash : MonoBehaviour
{
    private sealed class MaterialColorBinding
    {
        public Material Material;
        public int ColorPropertyId;
        public Color OriginalColor;
    }

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    [SerializeField] private Color flashColor = new Color(1f, 0.2f, 0.2f, 1f);
    [SerializeField] private float flashDuration = 0.2f;
    [SerializeField] private bool includeInactiveRenderers = true;

    private readonly List<MaterialColorBinding> bindings = new List<MaterialColorBinding>();
    private Coroutine flashRoutine;

    private void Awake()
    {
        CacheBindings();
    }

    private void OnDisable()
    {
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }

        RestoreOriginalColors();
    }

    public void PlayFlash()
    {
        if (bindings.Count == 0)
        {
            CacheBindings();
        }

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            RestoreOriginalColors();
        }

        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private void CacheBindings()
    {
        bindings.Clear();
        HashSet<Material> seenMaterials = new HashSet<Material>();
        Renderer[] renderers = GetComponentsInChildren<Renderer>(includeInactiveRenderers);

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null) continue;
            if (renderer is ParticleSystemRenderer || renderer is TrailRenderer || renderer is LineRenderer) continue;

            Material[] materials = renderer.materials;
            foreach (Material material in materials)
            {
                if (material == null || !seenMaterials.Add(material)) continue;

                int propertyId = GetColorPropertyId(material);
                if (propertyId < 0) continue;

                bindings.Add(new MaterialColorBinding
                {
                    Material = material,
                    ColorPropertyId = propertyId,
                    OriginalColor = material.GetColor(propertyId)
                });
            }
        }
    }

    private IEnumerator FlashRoutine()
    {
        ApplyFlashColor(0f);

        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(flashDuration, 0.0001f));
            ApplyFlashColor(t);
            yield return null;
        }

        RestoreOriginalColors();
        flashRoutine = null;
    }

    private void ApplyFlashColor(float t)
    {
        for (int i = 0; i < bindings.Count; i++)
        {
            MaterialColorBinding binding = bindings[i];
            if (binding.Material == null) continue;

            Color color = Color.Lerp(flashColor, binding.OriginalColor, t);
            color.a = binding.OriginalColor.a;
            binding.Material.SetColor(binding.ColorPropertyId, color);
        }
    }

    private void RestoreOriginalColors()
    {
        for (int i = 0; i < bindings.Count; i++)
        {
            MaterialColorBinding binding = bindings[i];
            if (binding.Material == null) continue;
            binding.Material.SetColor(binding.ColorPropertyId, binding.OriginalColor);
        }
    }

    private static int GetColorPropertyId(Material material)
    {
        if (material.HasProperty(BaseColorId)) return BaseColorId;
        if (material.HasProperty(ColorId)) return ColorId;
        return -1;
    }
}
