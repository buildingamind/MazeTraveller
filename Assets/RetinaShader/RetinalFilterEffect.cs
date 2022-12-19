using UnityEngine;

//[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class RetinalFilterEffect : MonoBehaviour
{
    [Header("[?] Select the Color Shader Material")]
    public Material colorShaderMat;
    [Header("[?] Select the Grayscale Shader Material")]
    public Material grayscaleShaderMat;
    [Header("[?] Should the Color Channels be aggregated to Grayscale?")]
    public bool grayscale = false;

    private Material currentShaderMaterial;
    private RenderTexture previousSource;
    private RenderTexture currentSource;

    private Material colorShaderMatCopy;
    private Material grayscaleShaderMatCopy;

    private void Start()
    {
        colorShaderMatCopy = new Material(colorShaderMat);
        grayscaleShaderMatCopy = new Material(grayscaleShaderMat);
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        int width = source.width;
        int height = source.height;

        currentShaderMaterial = grayscale ? grayscaleShaderMatCopy : colorShaderMatCopy;
        currentSource = RenderTexture.GetTemporary(width, height);
        Graphics.Blit(source, currentSource);

        currentShaderMaterial.SetTexture("_MainTex", currentSource);
        currentShaderMaterial.SetTexture("_PreviousTex", previousSource);

        if (previousSource)
        {
            RenderTexture.ReleaseTemporary(previousSource);
        }
        previousSource = currentSource;
        Graphics.Blit(currentSource, destination, currentShaderMaterial);
    }
}
