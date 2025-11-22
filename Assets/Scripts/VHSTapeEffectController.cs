using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class CleanVHSEffect : MonoBehaviour
{
    public Shader cleanVhsShader;
    [Range(0.5f, 2f)] public float warmth = 1.1f;
    [Range(0f, 0.5f)] public float vignette = 0.3f;
    [Range(0f, 0.3f)] public float scanLines = 0.1f;
    [Range(0f, 0.02f)] public float chromaShift = 0.005f;
    
    [Header("Animation Settings")]
    public bool enableAnimation = true;
    [Range(0f, 0.1f)] public float chromaShiftNoise = 0.003f;
    [Range(0f, 0.2f)] public float scanLineJitter = 0.05f;
    [Range(0f, 0.1f)] public float staticNoise = 0.02f;
    public float animationSpeed = 1f;

    private Material _material;
    private float _timeOffset;
    private float _currentChromaShift;
    private float _currentScanLines;

    void Start()
    {
        _timeOffset = Random.Range(0f, 100f);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (_material == null)
        {
            if (cleanVhsShader == null) return;
            _material = new Material(cleanVhsShader);
        }

        if (enableAnimation && Application.isPlaying)
        {
            AnimateParameters();
        }

        _material.SetFloat("_Warmth", warmth);
        _material.SetFloat("_Vignette", vignette);
        _material.SetFloat("_ScanLines", _currentScanLines);
        _material.SetFloat("_ChromaShift", _currentChromaShift);
        _material.SetFloat("_TimeOffset", _timeOffset);
        _material.SetFloat("_StaticNoise", staticNoise);
        _material.SetFloat("_ScanLineJitter", scanLineJitter);

        Graphics.Blit(src, dest, _material);
    }

    void AnimateParameters()
    {
        float time = Time.time * animationSpeed + _timeOffset;
        
        _currentChromaShift = chromaShift + Mathf.PerlinNoise(time * 2f, 0) * chromaShiftNoise;
        
        _currentScanLines = scanLines + Mathf.Sin(time * 3f) * scanLines * 0.3f;
        
        if (Random.value < 0.005f)
        {
            _currentChromaShift *= 3f;
            _currentScanLines *= 2f;
        }
    }

    public void TriggerGlitch(float intensity = 1f, float duration = 0.2f)
    {
        StartCoroutine(GlitchCoroutine(intensity, duration));
    }

    private IEnumerator GlitchCoroutine(float intensity, float duration)
    {
        float originalChroma = chromaShiftNoise;
        float originalStatic = staticNoise;
        
        chromaShiftNoise *= intensity * 3f;
        staticNoise *= intensity * 2f;
        
        yield return new WaitForSeconds(duration);
        
        chromaShiftNoise = originalChroma;
        staticNoise = originalStatic;
    }
}