using System;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class RayMarchCamera : SceneViewFilter {
    
    [SerializeField]
    private Shader _shader;

    private Material _rayMarchMat;
    public Material _rayMarchMaterial {
        get {
            if (!_rayMarchMat && _shader) {
                _rayMarchMat = new Material(_shader);
                _rayMarchMat.hideFlags = HideFlags.HideAndDontSave;
            }
            return _rayMarchMat;
        }
    }

    private Camera _camera;


    
    [Header("Setup")]
    public float maxDistance;
    [Range(1, 1000)]
    public int maxIterations;
    [Range(0.1f, 0.001f)]
    public float accuracy;

    [Header("Directional Light")]
    public Transform directionalLight;
    public Color lightColor;
    public float lightIntensity;

    [Header("Shadow")]
    [Range(0, 4)] 
    public float shadowIntensity;
    public Vector2 shadowDistance;
    [Range(1, 128)] 
    public float shadowPenumbra;

    [Header("Shadow")]
    [Range(0.01f, 10.0f)]
    public float aoStepSize;
    [Range(1, 5)]
    public int aoIterations;
    [Range(0, 1)]
    public float aoIntensity;
    
    [Header("Reflection")]
    [Range(0, 2)]
    public int reflectionCount;
    [Range(0, 1)]
    public float reflectionIntensity;
    [Range(0, 1)]
    public float envReflIntensity;
    public Cubemap reflectionCube;
    
    [Header("Signed Distance Field")]
    public Vector4 sphere;
    public float sphereSmooth;
    public float degreeRotate;
    
    private Vector4[] spheres = new Vector4[100];
    private Vector3[] sphereMovement = new Vector3[100];
    private Color[] sphereColors = new Color[100];
    
    [Range(1, 99)]
    public int nbSpheres;
    
    [Range(1, 10)]
    public float sphereSize;
    
    public Color groundColor;
    
    [Range(0, 4)]
    public float colorIntensity;
    
    [Range(0, 10)]
    public float  transparencyIntensity;
    
    private void Awake() {
        _camera = GetComponent<Camera>();
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination) {
        if (!_rayMarchMaterial) {
            Graphics.Blit(source, destination);
            return;
        }
        
        _rayMarchMaterial.SetColor("_GroundColor", groundColor);
        _rayMarchMaterial.SetColorArray("_SphereColors", sphereColors);
        _rayMarchMaterial.SetFloat("_ColorIntensity", colorIntensity);

        
        _rayMarchMaterial.SetMatrix("_CamFrustum", camFrustum(_camera));
        _rayMarchMaterial.SetMatrix("_CamToWorld", _camera.cameraToWorldMatrix);
        _rayMarchMaterial.SetFloat("_MaxDistance", maxDistance);
        _rayMarchMaterial.SetVector("_LightDir", directionalLight ? directionalLight.forward : Vector3.down);
        _rayMarchMaterial.SetColor("_LightCol", lightColor);
        _rayMarchMaterial.SetFloat("_LightIntensity", lightIntensity);
        _rayMarchMaterial.SetFloat("_ShadowIntensity", shadowIntensity);
        _rayMarchMaterial.SetVector("_ShadowDistance", shadowDistance);
        _rayMarchMaterial.SetFloat("_ShadowPenumbra", shadowPenumbra);

        _rayMarchMaterial.SetInt("_MaxIterations", maxIterations);
        _rayMarchMaterial.SetFloat("_Accuracy", accuracy);

        _rayMarchMaterial.SetInt("_AOIterations", aoIterations);
        _rayMarchMaterial.SetFloat("_AOStepSize", aoStepSize);
        _rayMarchMaterial.SetFloat("_AOIntensity", aoIntensity);

        _rayMarchMaterial.SetFloat("_SphereSmooth", sphereSmooth);
        
        _rayMarchMaterial.SetVectorArray("_Spheres", spheres);
        _rayMarchMaterial.SetInt("_NbSpheres", nbSpheres);

        // Reflection
        _rayMarchMaterial.SetInt("_ReflectionCount", reflectionCount);
        _rayMarchMaterial.SetFloat("_ReflectionIntensity", reflectionIntensity);
        _rayMarchMaterial.SetFloat("_EnvReflIntensity", envReflIntensity);
        _rayMarchMaterial.SetTexture("_ReflectionCube", reflectionCube);
        
        _rayMarchMaterial.SetFloat("_TransparencyIntensity", transparencyIntensity);
        
        RenderTexture.active = destination;
        _rayMarchMaterial.SetTexture("_MainTex", source);
        GL.PushMatrix();
        GL.LoadOrtho();
        _rayMarchMaterial.SetPass(0);
        GL.Begin(GL.QUADS);
        
        //BotLeft
        GL.MultiTexCoord2(0, 0.0f, 0.0f);
        GL.Vertex3(0.0f, 0.0f, 3.0f);
        
        //BotRight
        GL.MultiTexCoord2(0, 1.0f, 0.0f);
        GL.Vertex3(1.0f, 0.0f, 2.0f);
        
        //TopRight
        GL.MultiTexCoord2(0, 1.0f, 1.0f);
        GL.Vertex3(1.0f, 1.0f, 1.0f);

        //TopLeft
        GL.MultiTexCoord2(0, 0.0f, 1.0f);
        GL.Vertex3(0.0f, 1.0f, 0.0f);
        

        GL.End();
        GL.PopMatrix();
    }

    private Matrix4x4 camFrustum(Camera cam) {
        Matrix4x4 frustum = Matrix4x4.identity;
        float fov = Mathf.Tan((cam.fieldOfView * 0.5f) * Mathf.Deg2Rad);
        
        Vector3 upVector = Vector3.up * fov;
        Vector3 rightVector = Vector3.right * fov * cam.aspect;

        Vector3 topLeft = -Vector3.forward - rightVector + upVector;
        Vector3 topRight = -Vector3.forward + rightVector + upVector;
        Vector3 botLeft = -Vector3.forward - rightVector - upVector;
        Vector3 botRight = -Vector3.forward + rightVector - upVector;
        
        frustum.SetRow(0, topLeft);
        frustum.SetRow(1, topRight);
        frustum.SetRow(2, botRight);
        frustum.SetRow(3, botLeft);
        
        
        return frustum;
    }


    private void Start() {
        for (int i = 0; i < sphereMovement.Length; i++) {
            sphereMovement[i].x = (Random.value - 0.5f) * 2f;
            sphereMovement[i].y = (Random.value - 0.5f) * 2f;
            sphereMovement[i].z = (Random.value - 0.5f) * 2f;
            sphereMovement[i].Normalize();
            sphereMovement[i] *= 0.05f;
        }
        for (int i = 0; i < spheres.Length; i++) {
            spheres[i].x = (Random.value - 0.5f) * 10f;
            spheres[i].y = (Random.value - 0.5f) * 10f;
            spheres[i].z = (Random.value - 0.5f) * 10f;
            spheres[i].w = sphereSize;
            
        }
        
        for (int i = 0; i < sphereColors.Length; i++) {
//            sphereColors[i].r = Random.value;
//            sphereColors[i].g = Random.value;
//            sphereColors[i].b = Random.value;
//            sphereColors[i].a = 1;
            
            sphereColors[i].r = 1;
            sphereColors[i].g = 1;
            sphereColors[i].b = 1;
            sphereColors[i].a = 1;
        }

    }

    private int direction = 1;
    public void FixedUpdate() {
        for (int i = 0; i < spheres.Length && i < sphereMovement.Length; i++) {
            spheres[i].x += sphereMovement[i].x;
            spheres[i].y += sphereMovement[i].y;
            spheres[i].z += sphereMovement[i].z;
            spheres[i].w = sphereSize;
            
            if (spheres[i].x > 10 || spheres[i].x < -10) {
                sphereMovement[i].x *= -1;
                spheres[i].x = spheres[i].x < 0 ? -10 : 10;
            }

            if (spheres[i].y > 15 || spheres[i].y < 5) {
                sphereMovement[i].y *= -1;
                spheres[i].y = spheres[i].y < 10 ? 5 : 15;
            }

            if (spheres[i].z > 10 || spheres[i].z < -10) {
                sphereMovement[i].z *= -1;
                spheres[i].z = spheres[i].z < 0 ? -10 : 10;
            }
        }
    }

}
