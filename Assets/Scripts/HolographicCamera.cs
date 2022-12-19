using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class HolographicCamera : MonoBehaviour
{
    [Header("[?] The point from which the observer observes:")]
    [Tooltip("Which attributes of the object to log.")]
    public Transform targetObserver;
    public Transform trueObserver;
    public GameObject screen;
    public GameObject ownScreen;
    public GameObject leftEdge, rightEdge, topEdge, bottomEdge, center;
    public Shader unlit;
    float left, right, top, bottom;
    float screenDistance, screenWidth, screenHeight;

    public bool disableTextureRendering;

    // The Dynamic Effect will distort asymtotically at close distances
    // We dynamically upscale the near-distance virtual resolution to combat this.
    [Tooltip("Virtual Display Resolution Scaling when Near")]
    public float nearResolutionScaling = 1f;

    [Tooltip("Virtual Display Resolution Scaling when Far")]
    public float farResolutionScaling = 2f;

    Material material;
    RenderTexture renderTexture_far;
    RenderTexture renderTexture_near;
    Renderer cam_renderer;

    public Camera cam;
    public float upscaleDistance = 1f;

    void Awake()
    {
        Time.fixedDeltaTime = 1 / 120f;
        cam = GetComponent<Camera>();
        screenWidth = screen.transform.localScale.x;
        screenHeight = screen.transform.localScale.y;
        
        leftEdge.transform.localPosition = screen.transform.localPosition + new Vector3(-screenWidth / 2, 0, 0);
        rightEdge.transform.localPosition = screen.transform.localPosition + new Vector3(screenWidth / 2, 0, 0);
        topEdge.transform.localPosition = screen.transform.localPosition + new Vector3(0, screenHeight / 2, 0);
        bottomEdge.transform.localPosition = screen.transform.localPosition + new Vector3(0, -screenHeight / 2, 0);
        
        renderTexture_far = new RenderTexture((int)(1920 * farResolutionScaling), (int)(1080 * farResolutionScaling), 16, RenderTextureFormat.ARGB32);
        renderTexture_far.name = "far";
        renderTexture_near = new RenderTexture((int)(1920 * nearResolutionScaling), (int)(1080 * nearResolutionScaling), 16, RenderTextureFormat.ARGB32);
        renderTexture_near.name = "near";
        cam_renderer = screen.GetComponent<Renderer>();
        material = new Material(unlit);
        cam.targetTexture = renderTexture_far;
    }

    void FixedUpdate()
    {
        cam_renderer.material = material;
        transform.position = targetObserver.position;
        screenDistance = Mathf.Max(0.1f, screen.transform.localPosition.z - transform.localPosition.z);
        cam.nearClipPlane = screenDistance;

        if (disableTextureRendering)
        {
            cam.targetTexture = null;
            return;
        }

        if (Vector3.Distance(trueObserver.transform.position, ownScreen.transform.position) < upscaleDistance)
        {
            material.SetTexture("_MainTex", renderTexture_near);
            cam.targetTexture = renderTexture_near;
        }
        else
        {
            material.SetTexture("_MainTex", renderTexture_far);
            cam.targetTexture = renderTexture_far;
        }
    }

    private void LateUpdate()
    {
        CalculateFrustum();
    }

    void CalculateFrustum()
    {
        left = leftEdge.transform.localPosition.x - transform.localPosition.x;
        right = rightEdge.transform.localPosition.x - transform.localPosition.x;
        top = topEdge.transform.localPosition.y - transform.localPosition.y;
        bottom = bottomEdge.transform.localPosition.y - transform.localPosition.y;

        Matrix4x4 m = PerspectiveOffCenter(left, right, bottom, top, cam.nearClipPlane, cam.farClipPlane);
        cam.projectionMatrix = m;
    }

    //This is the Projection Matrix responsible for anamorphic distortion and for changing field of view
    //Sets an oblique viewing frustrum 
    static Matrix4x4 PerspectiveOffCenter(float left, float right, float bottom, float top, float near, float far)
    {
        float x = 2.0F * near / (right - left);
        float y = 2.0F * near / (top - bottom);
        float a = (right + left) / (right - left);
        float b = (top + bottom) / (top - bottom);
        float c = -(far + near) / (far - near);
        float d = -(2.0F * far * near) / (far - near);
        float e = -1.0F;
        Matrix4x4 m = new Matrix4x4();
        m[0, 0] = x;
        m[0, 1] = 0;
        m[0, 2] = a;
        m[0, 3] = 0;
        m[1, 0] = 0;
        m[1, 1] = y;
        m[1, 2] = b;
        m[1, 3] = 0;
        m[2, 0] = 0;
        m[2, 1] = 0;
        m[2, 2] = c;
        m[2, 3] = d;
        m[3, 0] = 0;
        m[3, 1] = 0;
        m[3, 2] = e;
        m[3, 3] = 0;
        return m;
    }
}