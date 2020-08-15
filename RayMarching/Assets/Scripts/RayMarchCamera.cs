using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public Transform directionalLight;
    
    public float maxDistance;
    public Color mainColor;

    public Vector4 sphere1;
    
    private void Awake() {
        _camera = GetComponent<Camera>();
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination) {
        if (!_rayMarchMaterial) {
            Graphics.Blit(source, destination);
            return;
        }
        
        _rayMarchMaterial.SetMatrix("_CamFrustum", camFrustum(_camera));
        _rayMarchMaterial.SetMatrix("_CamToWorld", _camera.cameraToWorldMatrix);
        _rayMarchMaterial.SetFloat("_MaxDistance", maxDistance);
        _rayMarchMaterial.SetVector("_sphere1", sphere1);
        _rayMarchMaterial.SetVector("_LightDir", directionalLight ? directionalLight.forward : Vector3.down);
        _rayMarchMaterial.SetColor("_MainColor", mainColor);
        

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

}
