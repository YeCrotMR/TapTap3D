using UnityEngine;

[ExecuteAlways]
public class WaterController : MonoBehaviour
{
    public Material waterMaterial;

    [Header("Wave")]
    public float amplitude = 0.12f;
    public float frequency = 1.0f;
    public float speed = 0.4f;

    [Header("Look")]
    public Color waterColor = new Color(0f, 0.45f, 0.6f, 0.5f);
    [Range(0,1)] public float transparency = 0.5f;
    [Range(0,1)] public float reflectionStrength = 0.6f;

    void Update()
    {
        if (waterMaterial == null) return;

        waterMaterial.SetFloat("_WaveAmplitude", amplitude);
        waterMaterial.SetFloat("_WaveFrequency", frequency);
        waterMaterial.SetFloat("_WaveSpeed", speed);
        waterMaterial.SetColor("_Color", waterColor);
        waterMaterial.SetFloat("_Transparency", transparency);
        waterMaterial.SetFloat("_ReflectionStrength", reflectionStrength);
    }
}