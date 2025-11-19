using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class CleanVHSEffect : MonoBehaviour
{
    public Shader cleanVhsShader;
    [Range(0.5f, 2f)] public float warmth = 1.1f;
    [Range(0f, 0.5f)] public float vignette = 0.3f;
    [Range(0f, 0.3f)] public float scanLines = 0.1f;
    [Range(0f, 0.02f)] public float chromaShift = 0.005f;

    private Material _material;

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (_material == null)
        {
            if (cleanVhsShader == null) return;
            _material = new Material(cleanVhsShader);
        }

        _material.SetFloat("_Warmth", warmth);
        _material.SetFloat("_Vignette", vignette);
        _material.SetFloat("_ScanLines", scanLines);
        _material.SetFloat("_ChromaShift", chromaShift);

        Graphics.Blit(src, dest, _material);
    }
}