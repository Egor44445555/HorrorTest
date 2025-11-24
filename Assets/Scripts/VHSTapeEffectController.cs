using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class CleanVHSEffect : MonoBehaviour
{
    public Shader cleanVhsShader;
    [Range(0.5f, 2f)] public float warmth = 1.35f;
    [Range(0f, 2f)] public float vignette = 2f;
    [Range(0f, 10f)] public float scanLines = 0.003f;
    [Range(0f, 0.02f)] public float chromaShift = 0.003f;
    
    [Header("Animation Settings")]
    public bool enableAnimation = true;
    [Range(0f, 1f)] public float scanLineJitter = 0.05f;
    [Range(0f, 0.1f)] public float staticNoise = 0.02f;
    public float animationSpeed = 1f;

    [Header("Tape Damage Effects")]
    [Range(0f, 0.1f)] public float headWobble = 0.0345f;
    [Range(1f, 10f)] public float headWobbleWidth = 5.13f;
    [Range(0f, 0.1f)] public float trackingError = 0f;
    [Range(0f, 0.1f)] public float tapeNoise = 0.01f;

    [Header("Color Effects")]
    [Range(0f, 2f)] public float saturation = 1.2f;
    [Range(0.5f, 1.5f)] public float contrast = 1f;

    Material _material;
    float _timeOffset;
    float _currentChromaShift;
    float _currentScanLines;
    float _currentHeadWobble;
    float _currentTrackingError;
    float _currentStaticNoise;

    bool _isGlitchActive = false;
    float _originalStaticNoise;

    void Start()
    {
        _timeOffset = Random.Range(0f, 100f);
        _currentStaticNoise = staticNoise;
        _originalStaticNoise = staticNoise;
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
        _material.SetFloat("_StaticNoise", _currentStaticNoise);
        _material.SetFloat("_ScanLineJitter", scanLineJitter);
        
        _material.SetFloat("_HeadWobble", _currentHeadWobble);
        _material.SetFloat("_HeadWobbleWidth", headWobbleWidth);
        _material.SetFloat("_TrackingError", _currentTrackingError);
        _material.SetFloat("_TapeNoise", tapeNoise);
        
        _material.SetFloat("_Saturation", saturation);
        _material.SetFloat("_Contrast", contrast);

        Graphics.Blit(src, dest, _material);
    }

    void AnimateParameters()
    {
        float time = Time.time * animationSpeed + _timeOffset;
        
        _currentChromaShift = chromaShift + Mathf.PerlinNoise(time * 2f, 0) * chromaShift * 0.3f;
        _currentScanLines = scanLines + Mathf.Sin(time * 3f) * scanLines * 0.2f;
        _currentHeadWobble = headWobble + Mathf.PerlinNoise(time * 1.5f, 10f) * headWobble * 0.5f;
        _currentTrackingError = trackingError + Mathf.Sin(time * 2f) * trackingError * 0.3f;

        if (!_isGlitchActive)
        {
            _currentStaticNoise = staticNoise + Mathf.PerlinNoise(time * 5f, 15f) * staticNoise * 0.5f;
        }
        
        if (!_isGlitchActive && Random.value < 0.003f)
        {
            TriggerRandomGlitch();
        }
    }

    void TriggerRandomGlitch()
    {
        float glitchIntensity = Random.Range(1.5f, 3f);
        float glitchDuration = Random.Range(0.1f, 0.3f);
        
        StartCoroutine(GlitchCoroutine(glitchIntensity, glitchDuration));
    }

    public void TriggerGlitch(float intensity = 1f, float duration = 0.2f)
    {
        if (!_isGlitchActive)
        {
            StartCoroutine(GlitchCoroutine(intensity, duration));
        }
    }

    IEnumerator GlitchCoroutine(float intensity, float duration)
    {
        _isGlitchActive = true;
        
        float originalChroma = _currentChromaShift;
        float originalStatic = _currentStaticNoise;
        float originalHeadWobble = _currentHeadWobble;
        float originalTracking = _currentTrackingError;
        float originalJitter = scanLineJitter;
        
        _currentChromaShift = originalChroma * intensity * 2f;
        _currentStaticNoise = originalStatic * intensity * 3f;
        _currentHeadWobble = originalHeadWobble * intensity * 4f;
        _currentTrackingError = originalTracking * intensity * 3f;
        scanLineJitter = originalJitter * intensity * 2f;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            float progress = elapsedTime / duration;
            float glitchIntensity = intensity;
            
            if (progress > 0.7f)
            {
                glitchIntensity = Mathf.Lerp(intensity, 1f, (progress - 0.7f) / 0.3f);
                
                _currentChromaShift = Mathf.Lerp(originalChroma * intensity * 2f, originalChroma, (progress - 0.7f) / 0.3f);
                _currentStaticNoise = Mathf.Lerp(originalStatic * intensity * 3f, originalStatic, (progress - 0.7f) / 0.3f);
                _currentHeadWobble = Mathf.Lerp(originalHeadWobble * intensity * 4f, originalHeadWobble, (progress - 0.7f) / 0.3f);
                _currentTrackingError = Mathf.Lerp(originalTracking * intensity * 3f, originalTracking, (progress - 0.7f) / 0.3f);
                scanLineJitter = Mathf.Lerp(originalJitter * intensity * 2f, originalJitter, (progress - 0.7f) / 0.3f);
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        _currentChromaShift = originalChroma;
        _currentStaticNoise = originalStatic;
        _currentHeadWobble = originalHeadWobble;
        _currentTrackingError = originalTracking;
        scanLineJitter = originalJitter;
        
        _isGlitchActive = false;
    }

    public void ResetAnimatedParameters()
    {
        _currentChromaShift = chromaShift;
        _currentScanLines = scanLines;
        _currentHeadWobble = headWobble;
        _currentTrackingError = trackingError;
        _currentStaticNoise = staticNoise;
        
        if (_isGlitchActive)
        {
            StopAllCoroutines();
            _isGlitchActive = false;
        }
    }

    public void SetTapeDamage(float wobble, float width, float tracking, float noise)
    {
        headWobble = Mathf.Clamp(wobble, 0f, 0.1f);
        headWobbleWidth = Mathf.Clamp(width, 1f, 10f);
        trackingError = Mathf.Clamp(tracking, 0f, 0.1f);
        tapeNoise = Mathf.Clamp(noise, 0f, 0.1f);
    }

    public void SetColorEffects(float sat, float cont, float warm)
    {
        saturation = Mathf.Clamp(sat, 0f, 2f);
        contrast = Mathf.Clamp(cont, 0.5f, 1.5f);
        warmth = Mathf.Clamp(warm, 0.5f, 2f);
    }

    public void ResetToDefaults()
    {
        warmth = 1.1f;
        vignette = 1f;
        scanLines = 0.3f;
        chromaShift = 0.005f;
        scanLineJitter = 0.05f;
        staticNoise = 0.02f;
        headWobble = 0.02f;
        headWobbleWidth = 3f;
        trackingError = 0.01f;
        tapeNoise = 0.05f;
        saturation = 1.0f;
        contrast = 1.0f;
        
        ResetAnimatedParameters();
    }

    void OnDestroy()
    {
        if (_material != null)
        {
            DestroyImmediate(_material);
        }
    }
}