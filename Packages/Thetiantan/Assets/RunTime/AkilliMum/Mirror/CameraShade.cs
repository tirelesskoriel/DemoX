﻿//#define DEBUG_RENDER
//#define DEBUG_LOG

//#define STEAMVR_ENABLED //open (uncomment) this for steamvr!

using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
#if STEAMVR_ENABLED
using Valve.VR;
#endif
using UnityEngine.XR;

namespace AkilliMum.Standard.Mirror
{
    [ImageEffectAllowedInSceneView]
    [ExecuteInEditMode]
    public class CameraShade : EffectBase
    {
        [Tooltip("Please use this to enable/disable the script. DO NOT USE script's enable/disable check box!")]
        public bool IsEnabled = true;
        [Tooltip("Please use this to enable/disable the mirror in mirror effect. Use careful for performance reasons!")]
        public bool IsMirrorInMirror = false;
        [Tooltip("Please use this to give unique id's to mirrors which will be drawn together. So if you want to see a mirror inside another mirror, their id must be same!")]
        public string MirrorInMirrorId;
        [Tooltip("Check this if you suffer reflection glitches. Because camera may occlude some objects according to unity settings!")]
        public bool TurnOffOcclusion = false;

        [Header("Device Type (AR, VR, XR)")]
        [Tooltip("Please select the correct Device Type (Normal for stand alone, AR for augmented reality, Correct VR device for virtual reality device etc.!")]
        public DeviceType DeviceType = DeviceType.Normal;

        //[Header("AR")]
        //[Tooltip("Check this only for AR mode (AR foundation, ARKit, ARCore etc.)!")]
        //public bool EnableAR = false;

        [Header("Common")]
        [Tooltip("'Reflect' (mirror, reflective surface, transparent glass etc.) and 'Transparent' (transparent AR surface) is supported only right now.")]
        public WorkType WorkType = WorkType.Reflect;
        [Tooltip("'Work Type->Transparent->Clear Color' clear color for AR mode transparency")]
        public Color TClearColor = new Color(0,0,0,0);
        [Tooltip("The mirror object's normal direction. Most of the time default 'GreenY' works perfectly. But try others if you get strange behavior.")]
        public FollowVector UpVector = FollowVector.GreenY;
        [Range(0, 1)]
        [Tooltip("Reflection intensity. 0 none to 1 perfect.")]
        public float Intensity = 0.5f;
        [Tooltip("Disables the GI, if you want perfect reflections check this (like mirrors)")]
        public bool DisableGBuffer = false;
        [Tooltip("Disables the reflection probes. Specially for deferred rendering, because probes may blend strange with refletion texture!")]
        public bool DisableRProbes = true;
        [Range(1, 20)]
        [Tooltip("Runs the script for every Xth frame; you may gain the fps, but you will lose the reality (realtime) of reflection!")]
        public int RunForEveryXthFrame = 1;
        [Range(0, 10)]
        [Tooltip("Starts to drawing at this level of LOD. So if you do not creating perfect mirrors, you can use lower lods for surface and gain fps.")]
        public int CameraLODLevel = 0;
        [Range(0, 10)]
        [Tooltip("Creates the mipmaps of the texture and uses the Xth value, you can use it specially for blur effects.")]
        public int TextureLODLevel = 0;
        private int _oldTextureLODLevel;
        //[Range(0, 1)]
        //[Tooltip("Adds a simple wetness (darkens) to the reflection.")]
        //public float Wetness = 0;

        [Header("Camera")]
        [Tooltip("Enables the HDR, so post effects will be visible (like bloom) on the reflection.")]
        public bool HDR = false;
        private bool _oldHDR;
        [Tooltip("Enables the anti aliasing (if only enabled in the project settings) for reflection.")]
        public bool MSAA = false;
        [Tooltip("Disables the point and spot lights for the reflection. You may gain the fps, but you will lose the reality of reflection.")]
        public bool DisablePixelLights = false;
        [Tooltip("By default (false) reflection renders the shadows. If you want to disable them, check this box (true) and put 0 to 'Shadow Distance'.")]
        public bool Shadow = false;
        [Tooltip("Max shadow distance to draw. You may gain fps for lower values but you will lose the reality of reflection.")]
        public float ShadowDistance = 0;
        //[Range(0, float.MaxValue)]
        [Tooltip("Enables the culling distance calculations.")]
        public bool Cull = false;
        [Tooltip("Cull circle distance, so it just draws the objects in the distance. You may gain the fps, but you will lose the reality of reflection.")]
        public float CullDistance = 0;
        [Tooltip("Easy selection for reflection quality. Select 'Full' for perfect reflections! VR can render half; so select x2 etc. and try to find the best visual!")]
        public TextureSizeType TextureSize = TextureSizeType.Manual;
        [Tooltip("The size (quality) of the reflection if manual is selected above! It should be set to width of the screen for perfect reflection! But try lower values to gain fps.")]
        public int ManualSize = 256;
        private int _oldTextureSize;
        //public bool UseCameraClipPlane = false; //todo:
        [Tooltip("Does not works right now, will be used on next releases.")]
        public bool UseCameraClipPlane = false;
        [Tooltip("Clipping distance to draw the reflection X units from the surface.")]
        public float ClipPlaneOffset = 0f;
        [Tooltip("Only these layers will be reflected by the reflection. So you can select what to be reflected with the reflection by putting them on certain layers.")]
        public LayerMask ReflectLayers = -1;
        [Tooltip("Use this option if you use CULL-ing or Reflect-Layers != Everything. So, not reflected surfaces blend with texture nicely.")]
        public bool MixBlackColor = false;
        //private LayerMask refractLayers = -1; //todo: use later for water bottom

        [Header("Depth Blur")]
        [Tooltip("Enables the advanced depth blur calculations.")]
        public bool EnableDepthBlur = false;
        private bool _oldEnableDepthBlur;
        [Tooltip("Depth blur shader to run on reflection texture.")]
        public Shader DepthBlurShader;
        private Material _depthBlurMaterial = null; //dynamic material to create and shade for above shader
        [Range(0, 30)]
        [Tooltip("Changes the depth distance, so the objects near to the reflective surface becomes more visible or not.")]
        public float DepthBlurCutoff = 0.8f;
        [Range(1, 20)]
        [Tooltip("Runs the depth blur shader on reflection texture X times. Larger iterations make more blurry images (but may decrease the fps)!")]
        public int DepthBlurIterations = 3;
        [Range(1, 20)]
        [Tooltip("This option makes the less blur on the pixels which are closer to surface. So you can make the far away pixels (on depth) more blurry!")]
        public float DepthBlurSurfacePower = 1;
        [Range(1, 50)]
        [Tooltip("Normally depth blur makes a circle blur. You can change this value if you want more horizontal blur!")]
        public int DepthBlurHorizontalMultiplier = 1;
        [Range(1, 50)]
        [Tooltip("Normally depth blur makes a circle blur. You can change this value if you want more vertical blur!")]
        public int DepthBlurVerticalMultiplier = 1;

        [Header("Simple Depth")]
        [Tooltip("Enables the simple depth calculations.")]
        public bool EnableSimpleDepth = false;
        private bool _oldEnableSimpleDepth;
        [Range(0, 10)]
        [Tooltip("Changes the depth distance, so the objects near to the reflective surface becomes more visible or not.")]
        public float SimpleDepthCutoff = 0.8f;
        //[Tooltip("Changes the depth blur value. It calculates the blur according to distance. Very big values may be needed for large scenes!")]
        //public float DepthBlur = 1f;

        [Header("Effected Objects and Materials")]
        [Tooltip("Reflective surfaces (gameObjects) must be put here with their's shader! Script calculates the reflection according to their position etc.")]
        public Shade[] Shades;
        private List<Renderer> _renderers;

        [Header("Shader to Run on Final Texture")]
        [Tooltip("Enables the some effects on reflection, like blur etc.")]
        public bool EnableFinalShader = false;
        [Tooltip("Effect shader (like blur) must be put here to run on reflection texture.")]
        public Shader FinalShader;
        private Material _finalMaterial = null; //dynamic material to create and shade for above shader
        [Tooltip("(Deprecated, please use T Clear Color if blur is needed) In full transparent AR, blur may mix with black color. This option may reduce the artifacts.")]
        public bool EnableARMode = false;
        [Range(1, 20)]
        [Tooltip("Runs the effect shader on reflection texture X times. For example if it is a blur, larger iterations makes more blury images (but may decrease the fps)!")]
        public int Iterations = 3;
        [Tooltip("Takes the Xth neighbour pixel on blur calculations. Change it until you get desired effect.")]
        public float NeighbourPixels = 5;
        [Tooltip("Does not works right now. Will be used on next releases.")]
        public float BlockSize = 8;

        [Header("Refraction")]
        [Tooltip("You can give refractions to the reflection. Use a refraction normal map here.")]
        public Texture2D RefractionTexture;
        [Tooltip("Refraction intensity according to the 'Refraction Texture'. Bigger values creates more refraction!")]
        public float Refraction = 0.0f;

        [Header("Waves")]
        [Tooltip("Enables a simple wave simulation on the surface.")]
        public bool EnableWaves = false;
        [Tooltip("Noise texture to create the wave effect.")]
        public Texture2D WaveNoiseTexture;
        [Tooltip("Wave size. Just adapt it according to your needs.")]
        public float WaveSize = 15.0f;
        [Tooltip("Refraction distortion size. Just adapt it according to your needs.")]
        public float WaveDistortion = 0.02f;
        [Tooltip("Speed of the wave simulation. Just adapt it according to your needs.")]
        public float WaveSpeed = 0.005f;

        [Header("Ripples")]
        [Tooltip("Enables a simple ripple simulation on the surface.")]
        public bool EnableRipples = false;
        [Tooltip("Ripple normal map to create the ripple effect.")]
        public Texture2D RippleTexture;
        [Tooltip("Ripple size. Just adapt it according to your needs.")]
        public float RippleSize = 5.0f;
        [Tooltip("Refraction distortion size. Just adapt it according to your needs.")]
        public float RippleRefraction = 0.02f;
        [Tooltip("Ripple density size. Just adapt it according to your needs.")]
        public float RippleDensity = 3.0f;
        [Tooltip("Speed of the ripple simulation. Just adapt it according to your needs.")]
        public float RippleSpeed = 20f;

        [Header("Masking")]
        [Tooltip("Enables masking on the surface. So you can create wetness, semi reflective surfaces, ices etc.")]
        public bool EnableMask = false;
        [Tooltip("Alpha based masking texture.")]
        public Texture2D MaskTexture;
        [Tooltip("Tiling for the texture if you want to replicate the masking along the surface.")]
        public Vector2 MaskTiling = new Vector2(1, 1);
        [Tooltip("Mask cutoff to change the alpha based calculations.")]
        [Range(0, 1)]
        public float MaskCutoff = 0.5f;
        [Tooltip("Gives a little darkness to the edges of the alpha base texture. So you can simulate a fake depth (like water) on masking.")]
        [Range(1, 50)]
        public float MaskEdgeDarkness = 1f;

        private int _stereoEye = -1;

        private bool _isObjectVisible = false; //decide if we will render the reflection for this frame

        private Int64 _frameCount = 0; //to not draw for every X frame
        private bool _usePreviousTexture = false; //use texture from previous render for RunForEveryXthFrame option

#if DEBUG_RENDER
    private float _deltaTime = 0.0f;
#endif

        //private Camera _camera; //reflection cam

        //use later as public
        private GameObject ReflectionCameraPrefab = null;
        private GameObject _reflectionCameraPrefabInstance = null;

        private Hashtable _reflectionCameras = new Hashtable(); // Camera -> Camera table

        private RenderTexture _reflectionTexture1;
        private RenderTexture _reflectionTexture1Other;
        private RenderTexture _reflectionTexture2;
        private RenderTexture _reflectionTexture2Other;
        private RenderTexture _reflectionTexture3;
        private RenderTexture _reflectionTexture3Other;

        private RenderTexture _reflectionTextureDepth;
        private RenderTexture _reflectionTextureOtherDepth;

        private List<XRNodeState> nodeStates = new List<XRNodeState>();

        //public static Quaternion QuaternionFromMatrix(Matrix4x4 m) { return Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1)); }
        //public static Vector4 PosToV4(Vector3 v) { return new Vector4(v.x, v.y, v.z, 1.0f); }
        //public static Vector3 ToV3(Vector4 v) { return new Vector3(v.x, v.y, v.z); }

        //public static Vector3 ZeroV3 = new Vector3(0.0f, 0.0f, 0.0f);
        //public static Vector3 OneV3 = new Vector3(1.0f, 1.0f, 1.0f);

        void Start()
        {
            InitializeProperties();
        }

        private void OnEnable()
        {
            InitializeProperties();
        }

        public void InitializeProperties()
        {
            _renderers = new List<Renderer>();
            if (Shades != null && Shades.Length > 0)
            {
                foreach (var shade in Shades)
                {
                    if (shade.ObjectToShade != null && shade.ObjectToShade.GetComponent<Renderer>() != null)
                        _renderers.Add(shade.ObjectToShade.GetComponent<Renderer>());
                }
            }
        }

        private void Update()
        {
            _frameCount++;

            //this.DepthBlurCutoff = GlobalData.DepthCutoff;
#if DEBUG_RENDER
      _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
#endif
        }

        void OnGUI()
        {
#if DEBUG_RENDER
      int w = Screen.width, h = Screen.height;

      GUIStyle style = new GUIStyle();

      Rect rect = new Rect(0, 0, w, h * 2 / 25);
      style.alignment = TextAnchor.UpperLeft;
      style.fontSize = h * 2 / 25;
      style.normal.textColor = new Color(0.0f, 0.0f, 0.5f, 1.0f);
      float msec = _deltaTime * 1000.0f;
      float fps = 1.0f / _deltaTime;
      string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
      GUI.Label(rect, text, style);
#endif
        }

        void OnPreRender()
        {
            if (IsMirrorInMirror) //camera shade multi manager will handle the events!!
                return;

            RenderReflective();
        }

        public IList<Camera> RenderReflective(Camera sentCamera = null, bool invert = true, bool renderCam = true)
        {
            var cameras = new List<Camera>();
            if (DeviceType != DeviceType.Normal && DeviceType != DeviceType.AR && Application.isPlaying)
            {  //draw the scene twice for single pass VR
                _stereoEye = 0;
                var camera0 = RenderMe(sentCamera, invert, renderCam);
                cameras.Add(camera0);

                _stereoEye = 1;
                var camera1 = RenderMe(sentCamera, invert, renderCam);
                cameras.Add(camera1);
            }
            else
            {
                _stereoEye = -1;
                var camera0 = RenderMe(sentCamera, invert, renderCam);
                cameras.Add(camera0);
            }

            return cameras;
        }

        private Camera GetCamera(Camera sentCamera)
        {
            var cameraToUse = sentCamera; // ?? _camera;

            if (cameraToUse == null)
                cameraToUse = GetComponent<Camera>();

            return cameraToUse;
        }

        private Camera RenderMe(Camera sentCamera, bool invert = true, bool renderCam = true)
        {
            var cameraToUse = GetCamera(sentCamera);

            if (!IsEnabled)
                return null;

            if (_renderers == null || _renderers.Count <= 0)
                return null;

            _isObjectVisible = IsObjectVisible(cameraToUse);

            if (!_isObjectVisible ||
             !enabled ||
             cameraToUse == null ||
             _renderers == null || _renderers.Count <= 0 || _renderers[0] == null)
            {
                return null;
            }

            if (cameraToUse.cameraType == CameraType.Reflection)
                return null; //probes may give error on probe reflection bake!!!

            if (_frameCount % RunForEveryXthFrame != 0)
            {
                _usePreviousTexture = true;
            }
            else
            {
                _usePreviousTexture = false;
            }

            //we do not need to draw the reflection because of user selection :)
            if (_usePreviousTexture)
            {
                return null;
            }

            if (InsideRendering)
                return null;
            InsideRendering = true; //!!!!!!

            //Debug.Log("rendering frame: " + Time.frameCount + " renderCamIsTrue?: " +renderCam);

            //do not draw fog
            var previousFog = RenderSettings.fog;
            RenderSettings.fog = false;

            //set shadow distance
            float oldShadowDistance = QualitySettings.shadowDistance;
            if (Shadow)
            {
                QualitySettings.shadowDistance = ShadowDistance;
            }

            // Optionally disable pixel lights for reflection/refraction
            int oldPixelLightCount = QualitySettings.pixelLightCount;
            if (DisablePixelLights)
            {
                QualitySettings.pixelLightCount = 0;
            }

            if (EnableFinalShader)
            {
                if (_finalMaterial == null)
                {
                    _finalMaterial = new Material(FinalShader);
                    _finalMaterial.hideFlags = HideFlags.HideAndDontSave;
                }
            }

            if (EnableDepthBlur)
            {
                if (_depthBlurMaterial == null)
                {
                    _depthBlurMaterial = new Material(DepthBlurShader);
                    _depthBlurMaterial.hideFlags = HideFlags.HideAndDontSave;
                }
            }

            //setup
            Camera reflectionCamera;
            CreateMirrorObjects(cameraToUse, out reflectionCamera);

            UpdateCameraModes(cameraToUse, reflectionCamera);
            //reflectionCamera.cameraType = cameraToUse.cameraType;

            //set cull distance if selected
            if (Cull)
            {
                float[] distances = new float[32]; //for all layers :)
                for (int i = 0; i < distances.Length; i++)
                {
                    distances[i] = Cull ? CullDistance : reflectionCamera.farClipPlane; //the culling distance
                }
                reflectionCamera.layerCullDistances = distances;
                reflectionCamera.layerCullSpherical = Cull;
            }

            //reflectionCamera.cullingMask = ~(1 << 4) &amp; ReflectLayers.value; // never render water layer
            reflectionCamera.cullingMask = ReflectLayers.value;

            var previousLODLevel = QualitySettings.maximumLODLevel;
            QualitySettings.maximumLODLevel = CameraLODLevel;

            Vector3 Normal;
            if (UpVector == FollowVector.GreenY)
            {
                Normal = Shades[0].ObjectToShade.transform.up; //all items must be on same vector direction :) so we can use first one
            }
            else if (UpVector == FollowVector.GreenY_Negative)
            {
                Normal = -Shades[0].ObjectToShade.transform.up;
            }
            else if (UpVector == FollowVector.BlueZ)
            {
                Normal = Shades[0].ObjectToShade.transform.forward;
            }
            else if (UpVector == FollowVector.BlueZ_Negative)
            {
                Normal = -Shades[0].ObjectToShade.transform.forward;
            }
            else if (UpVector == FollowVector.RedX)
            {
                Normal = Shades[0].ObjectToShade.transform.right;
            }
            else //if (UpVector == FollowVector.RedX_Negative)
            {
                Normal = -Shades[0].ObjectToShade.transform.right;
            }
            Normal *= (invert ? 1 : -1); ; //flip normals if drawing reflections reflection :)

            //if (_stereoEye > 0)
            //  cameraToUse.transform.position = cameraToUse.transform.TransformPoint(cameraToUse.stereoSeparation, 0, 0);

            reflectionCamera.stereoTargetEye = _stereoEye < 1 ? StereoTargetEyeMask.Left : StereoTargetEyeMask.Right;

            // find out the reflection plane: position and normal in world space
            Vector3 pos = Shades[0].ObjectToShade.transform.position;
            Vector3 normal = Normal;

            if (WorkType != WorkType.Direct && WorkType != WorkType.WaterBottom)
            {
                // Render reflection
                // Reflect camera around reflection plane
                float d = -Vector3.Dot(normal, pos) - ClipPlaneOffset;
                Vector4 reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);

                Matrix4x4 reflection = Matrix4x4.zero;
                CalculateReflectionMatrix(ref reflection, reflectionPlane);

                Vector3 oldEyePos = Vector3.one;
                Matrix4x4 worldToCameraMatrix = Matrix4x4.identity;
                if (_stereoEye == -1)
                {
                    worldToCameraMatrix = cameraToUse.worldToCameraMatrix * reflection;
                    oldEyePos = cameraToUse.transform.position;
                }
                else
                {
#if STEAMVR_ENABLED
                    if (DeviceType == DeviceType.SteamVR)
                    {
                        worldToCameraMatrix = cameraToUse.GetStereoViewMatrix(
                          _stereoEye < 1 ? Camera.StereoscopicEye.Left : Camera.StereoscopicEye.Right) *reflection;

                        var eyeOffset = SteamVR.instance
                          .eyes[(int)(_stereoEye < 1 ? Camera.StereoscopicEye.Left : Camera.StereoscopicEye.Right)].pos;
                        eyeOffset.z = 0.0f;
                        oldEyePos = cameraToUse.transform.position + cameraToUse.transform.TransformVector(eyeOffset);
                    }
#endif
                    if (DeviceType == DeviceType.GeneralVR ||
                      DeviceType == DeviceType.OculusVR_Quest ||
                      DeviceType == DeviceType.OculusVR_RiftS)
                    {
                        //worldToCameraMatrix = cameraToUse.worldToCameraMatrix * reflection;
                        //oldEyePos = cameraToUse.transform.position + cameraToUse.transform.TransformVector(cameraToUse.stereoSeparation, 0, 0);

                        //worldToCameraMatrix = cameraToUse.GetStereoViewMatrix((_stereoEye < 1 ? Camera.StereoscopicEye.Left : Camera.StereoscopicEye.Right)) * reflection;
                        //Vector3 eyeOffset;
                        //if ((_stereoEye < 1 ? Camera.StereoscopicEye.Left : Camera.StereoscopicEye.Right) == Camera.StereoscopicEye.Left)
                        //    eyeOffset = InputTracking.GetLocalPosition(XRNode.LeftEye);
                        //else
                        //    eyeOffset = InputTracking.GetLocalPosition(XRNode.RightEye);
                        //eyeOffset.z = 0.0f;
                        //oldEyePos = cameraToUse.transform.position + cameraToUse.transform.TransformVector(eyeOffset);

                        worldToCameraMatrix = cameraToUse.GetStereoViewMatrix((_stereoEye < 1 ? Camera.StereoscopicEye.Left : Camera.StereoscopicEye.Right)) * reflection;

                        Vector3 eyeOffset;
                        InputTracking.GetNodeStates(nodeStates);
                        if ((_stereoEye < 1 ? Camera.StereoscopicEye.Left : Camera.StereoscopicEye.Right) ==
                          Camera.StereoscopicEye.Left)
                        {
                            var state = nodeStates.FirstOrDefault(node => node.nodeType == XRNode.LeftEye);
                            state.TryGetPosition(out eyeOffset);
                        }
                        else
                        {
                            var state = nodeStates.FirstOrDefault(node => node.nodeType == XRNode.RightEye);
                            state.TryGetPosition(out eyeOffset);
                        }
                        eyeOffset.z = 0.0f;

                        oldEyePos = cameraToUse.transform.position + cameraToUse.transform.TransformVector(eyeOffset);
                    }
                }

                Vector3 newEyePos = reflection.MultiplyPoint(oldEyePos);
                reflectionCamera.transform.position = newEyePos;

                reflectionCamera.worldToCameraMatrix = worldToCameraMatrix;

                // Setup oblique projection matrix so that near plane is our reflection
                // plane. This way we clip everything below/above it for free.
                Vector4 clipPlane = CameraSpacePlane(worldToCameraMatrix, pos, normal, invert ? 1.0f : -1.0f);

                Matrix4x4 projectionMatrix = Matrix4x4.identity;
                if (_stereoEye == -1)
                    projectionMatrix = cameraToUse.projectionMatrix;
                else
                {
#if STEAMVR_ENABLED
                    if (DeviceType == DeviceType.SteamVR)
                    {
                        projectionMatrix = HMDMatrix4x4ToMatrix4x4(SteamVR.instance.hmd.GetProjectionMatrix(
                          (EVREye)(_stereoEye < 1 ? Camera.StereoscopicEye.Left : Camera.StereoscopicEye.Right),
                          //cameraToUse.nearClipPlane, cameraToUse.farClipPlane));
                          Near, Far));
                    }
#endif
                    if (DeviceType == DeviceType.GeneralVR ||
                      DeviceType == DeviceType.OculusVR_Quest ||
                      DeviceType == DeviceType.OculusVR_RiftS)
                    {
                        //projectionMatrix = cameraToUse.GetStereoNonJitteredProjectionMatrix(
                        //  _stereoEye > 0 ? Camera.StereoscopicEye.Right : Camera.StereoscopicEye.Left);

                        projectionMatrix = cameraToUse.GetStereoProjectionMatrix(
                            (_stereoEye > 0 ? Camera.StereoscopicEye.Right : Camera.StereoscopicEye.Left));
                    }
                }

                //todo: use which one?
                if(reflectionCamera.orthographic)
                    projectionMatrix = cameraToUse.CalculateObliqueMatrix(clipPlane);
                else
                    MakeProjectionMatrixOblique(ref projectionMatrix, clipPlane);

                reflectionCamera.projectionMatrix = projectionMatrix;

                //reflectionCamera.transform.rotation = cameraToUse.transform.rotation;
                //todo: do we need here to calculate the rotation as in VR? NodeStates?
                if (_stereoEye == -1)
                {
                    reflectionCamera.transform.rotation = cameraToUse.transform.rotation;
                }
                else
                {
                    if (DeviceType == DeviceType.GeneralVR ||
                      DeviceType == DeviceType.OculusVR_Quest ||
                      DeviceType == DeviceType.OculusVR_RiftS)
                    {
                        Quaternion eyeRotation;
                        InputTracking.GetNodeStates(nodeStates);
                        if ((_stereoEye < 1 ? Camera.StereoscopicEye.Left : Camera.StereoscopicEye.Right) ==
                          Camera.StereoscopicEye.Left)
                        {
                            var state = nodeStates.FirstOrDefault(node => node.nodeType == XRNode.LeftEye);
                            state.TryGetRotation(out eyeRotation);
                        }
                        else
                        {
                            var state = nodeStates.FirstOrDefault(node => node.nodeType == XRNode.RightEye);
                            state.TryGetRotation(out eyeRotation);
                        }
                        reflectionCamera.transform.rotation = eyeRotation; //!!!
                    }
                }

                var oldInvertCulling = GL.invertCulling;
                GL.invertCulling = invert;

                //set targets
                if (_stereoEye <= 0)
                    reflectionCamera.targetTexture = _reflectionTexture1;
                else
                    reflectionCamera.targetTexture = _reflectionTexture1Other;

                if (renderCam && checkFrustum(reflectionCamera))
                {
#if DEBUG_LOG
          Debug.Log("Rendering frame: " + _frameCount + " for eye: "+ _stereoEye);
#endif

                    reflectionCamera.Render();
                }

                if (EnableSimpleDepth || EnableDepthBlur)
                {
                    if (_stereoEye <= 0)
                        reflectionCamera.targetTexture = _reflectionTextureDepth;
                    else
                        reflectionCamera.targetTexture = _reflectionTextureOtherDepth;

                    if (renderCam && checkFrustum(reflectionCamera))
                    {
#if DEBUG_LOG
            Debug.Log("Rendering depth frame: " + _frameCount + " for eye: " + _stereoEye);
#endif
                        reflectionCamera.Render();
                    }
                }

                GL.invertCulling = oldInvertCulling;

                //reflectionCamera.transform.position = oldEyePos;
            }
            else
            {
                if (ReflectionCameraPrefab == null) //use main camera transform and position if prefab is null
                {
                    reflectionCamera.transform.position = cameraToUse.transform.position;
                    reflectionCamera.transform.rotation = cameraToUse.transform.rotation;
                }

                if (UseCameraClipPlane)
                {
                    // find out the reflection plane: position and normal in world space
                    //Vector3 pos = Shades[0].ObjectToShade.transform.position; //we can use first one :)
                    //Vector3 normal = Normal;

                    Vector4 clipPlane = CameraSpacePlane(reflectionCamera, pos, normal, invert ? 1.0f : -1.0f);
                    Matrix4x4 projection = cameraToUse.CalculateObliqueMatrix(clipPlane);
                    reflectionCamera.projectionMatrix = projection;
                }

                //set targets
                if (_stereoEye <= 0)
                    reflectionCamera.targetTexture = _reflectionTexture1;
                else
                    reflectionCamera.targetTexture = _reflectionTexture1Other;

                if (renderCam && checkFrustum(reflectionCamera))
                {
#if DEBUG_LOG
          Debug.Log("Rendering frame: " + _frameCount + " for eye: " + _stereoEye);
#endif
                    reflectionCamera.Render();
                }

                //var cullResults = new CullResults();
                //LightweightRenderPipeline.RenderSingleCamera(pipelineInstance, context, reflectionCamera, ref cullResults);
                if (EnableSimpleDepth || EnableDepthBlur) //stereo??
                {
                    if (_stereoEye <= 0)
                        reflectionCamera.targetTexture = _reflectionTextureDepth;
                    else
                        reflectionCamera.targetTexture = _reflectionTextureOtherDepth;

                    if (renderCam && checkFrustum(reflectionCamera))
                    {
#if DEBUG_LOG
            Debug.Log("Rendering depth frame: " + _frameCount + " for eye: " + _stereoEye);
#endif
                        reflectionCamera.Render();
                    }
                }
            }

            if (renderCam) //change the textures etc. if the scene is drawn!!
            {
                if (TextureLODLevel > 0)
                {
                    if (_stereoEye <= 0)
                        _reflectionTexture1.GenerateMips();
                    else
                        _reflectionTexture1Other.GenerateMips();
                }

                if (EnableDepthBlur)
                {
                    //hold texture1 unchanged!!
                    if (_stereoEye <= 0)
                        Graphics.Blit(_reflectionTexture1, _reflectionTexture2);
                    else
                        Graphics.Blit(_reflectionTexture1Other, _reflectionTexture2Other);

                    if (TextureLODLevel > 0)
                    {
                        if (_stereoEye <= 0)
                            _reflectionTexture2.GenerateMips();
                        else
                            _reflectionTexture2Other.GenerateMips();
                    }

                    for (int i = 1; i <= DepthBlurIterations; i++)
                    {
                        if (_depthBlurMaterial.HasProperty("_Iteration"))
                            _depthBlurMaterial.SetFloat("_Iteration", i);

                        if (_depthBlurMaterial.HasProperty("_DepthTex"))
                            _depthBlurMaterial.SetTexture("_DepthTex", _reflectionTextureDepth);

                        if (_depthBlurMaterial.HasProperty("_DepthTex") && _stereoEye == 1)
                            _depthBlurMaterial.SetTexture("_DepthTex", _reflectionTextureOtherDepth);

                        if (_depthBlurMaterial.HasProperty("_Lod"))
                            _depthBlurMaterial.SetFloat("_Lod", TextureLODLevel);
                        if (_depthBlurMaterial.HasProperty("_DepthCutoff"))
                            _depthBlurMaterial.SetFloat("_DepthCutoff", DepthBlurCutoff);
                        if (_depthBlurMaterial.HasProperty("_SurfacePower"))
                            _depthBlurMaterial.SetFloat("_SurfacePower", DepthBlurSurfacePower);
                        if (_depthBlurMaterial.HasProperty("_VerticalBlurMultiplier"))
                            _depthBlurMaterial.SetFloat("_VerticalBlurMultiplier", DepthBlurVerticalMultiplier);
                        if (_depthBlurMaterial.HasProperty("_HorizontalBlurMultiplier"))
                            _depthBlurMaterial.SetFloat("_HorizontalBlurMultiplier", DepthBlurHorizontalMultiplier);
                        if (_depthBlurMaterial.HasProperty("_NearClip"))
                            _depthBlurMaterial.SetFloat("_NearClip", cameraToUse.nearClipPlane);
                        if (_depthBlurMaterial.HasProperty("_FarClip"))
                            _depthBlurMaterial.SetFloat("_FarClip", cameraToUse.farClipPlane);

                        if (i % 2 == 1) //a little hack to copy textures in order from 1 to 2 than 2 to 1 and so :)
                        {
                            if (_stereoEye <= 0)
                                Graphics.Blit(_reflectionTexture2, _reflectionTexture3, _depthBlurMaterial);
                            else
                                Graphics.Blit(_reflectionTexture2Other, _reflectionTexture3Other, _depthBlurMaterial);
                            if (TextureLODLevel > 0)
                            {
                                if (_stereoEye <= 0)
                                    _reflectionTexture3.GenerateMips();
                                else
                                    _reflectionTexture3Other.GenerateMips();
                            }
                        }
                        else
                        {
                            if (_stereoEye <= 0)
                                Graphics.Blit(_reflectionTexture3, _reflectionTexture2, _depthBlurMaterial);
                            else
                                Graphics.Blit(_reflectionTexture3Other, _reflectionTexture2Other, _depthBlurMaterial);
                            if (TextureLODLevel > 0)
                            {
                                if (_stereoEye <= 0)
                                    _reflectionTexture2.GenerateMips();
                                else
                                    _reflectionTexture2Other.GenerateMips();
                            }
                        }
                    }
                }

                if (EnableFinalShader)
                {
                    //hold texture1 unchanged!!
                    if (_stereoEye <= 0)
                        Graphics.Blit(_reflectionTexture1, _reflectionTexture2);
                    else
                        Graphics.Blit(_reflectionTexture1Other, _reflectionTexture2Other);

                    if (TextureLODLevel > 0)
                    {
                        if (_stereoEye <= 0)
                            _reflectionTexture2.GenerateMips();
                        else
                            _reflectionTexture2Other.GenerateMips();
                    }

                    for (int i = 1; i <= Iterations; i++)
                    {
                        if (_finalMaterial.HasProperty("_CustomFloatParam1"))
                            _finalMaterial.SetFloat("_CustomFloatParam1", i);
                        if (_finalMaterial.HasProperty("_CustomFloatParam2"))
                            _finalMaterial.SetFloat("_CustomFloatParam2", NeighbourPixels);
                        if (_finalMaterial.HasProperty("_Iteration"))
                            _finalMaterial.SetFloat("_Iteration", i);
                        if (_finalMaterial.HasProperty("_NeighbourPixels"))
                            _finalMaterial.SetFloat("_NeighbourPixels", NeighbourPixels);
                        if (_finalMaterial.HasProperty("_BlockSize"))
                            _finalMaterial.SetFloat("_BlockSize", BlockSize);
                        if (_finalMaterial.HasProperty("_Lod"))
                            _finalMaterial.SetFloat("_Lod", TextureLODLevel);
                        if (_finalMaterial.HasProperty("_AR"))
                            _finalMaterial.SetFloat("_AR", EnableARMode ? 1 : 0);

                        if (i % 2 == 1) //a little hack to copy textures in order from 1 to 2 than 2 to 1 and so :)
                        {
                            if (_stereoEye <= 0)
                                Graphics.Blit(_reflectionTexture2, _reflectionTexture3, _finalMaterial);
                            else
                                Graphics.Blit(_reflectionTexture2Other, _reflectionTexture3Other, _finalMaterial);
                            if (TextureLODLevel > 0)
                            {
                                if (_stereoEye <= 0)
                                    _reflectionTexture3.GenerateMips();
                                else
                                    _reflectionTexture3Other.GenerateMips();
                            }
                        }
                        else
                        {
                            if (_stereoEye <= 0)
                                Graphics.Blit(_reflectionTexture3, _reflectionTexture2, _finalMaterial);
                            else
                                Graphics.Blit(_reflectionTexture3Other, _reflectionTexture2Other, _finalMaterial);
                            if (TextureLODLevel > 0)
                            {
                                if (_stereoEye <= 0)
                                    _reflectionTexture2.GenerateMips();
                                else
                                    _reflectionTexture2Other.GenerateMips();
                            }
                        }
                    }
                }

                UpdateMaterialProperties(cameraToUse);
            }

            //set fog
            RenderSettings.fog = previousFog;

            //restore shadow
            if (Shadow)
            {
                QualitySettings.shadowDistance = oldShadowDistance;
            }

            // Restore pixel light count
            if (DisablePixelLights)
            {
                QualitySettings.pixelLightCount = oldPixelLightCount;
            }

            QualitySettings.maximumLODLevel = previousLODLevel;

            //if (_stereoEye > 0)
            //{
            //  cameraToUse.transform.position = cameraToUse.transform.TransformPoint(
            //    -cameraToUse.stereoSeparation, 0, 0);
            //}

            InsideRendering = false; //!!

            return reflectionCamera;
        }

        /// <summary>
        /// Checks the frustum of the camera to not throw "Frustum Error". If t returns true it means everything is OK
        /// and we can render the scene, if it returns false we should not render the scene!
        /// </summary>
        /// <param name="cam"></param>
        /// <returns></returns>
        public bool checkFrustum(Camera cam)
        {
            if (cam == null)
                return true;

            //Debug.Log(cam.rect);

            bool noFrustumError = true;
            if (cam.rect.x != 0 && cam.rect.y != 0)
                noFrustumError = false;
            else if (cam.nearClipPlane == 0)
                noFrustumError = false;
            else if (cam.orthographicSize == 0)
                noFrustumError = false;

            //"Screen position out of view frustum" error shows up if 3 rotation vectors are 0 on the
            //https://forum.unity.com/threads/solved-screen-position-out-of-view-frustum.60851/
            if (cam.transform.rotation.x == 0 &&
                cam.transform.rotation.y == 0 &&
                cam.transform.rotation.z == 0)
                noFrustumError = false;

            return noFrustumError;
        }

        public bool IsObjectVisible(Camera cam)
        {
            var visible = false;
            foreach (var ren in _renderers)
            {
                if (ren.IsVisibleFrom(cam) && ren.gameObject.activeSelf) //if any of renderer is visible
                {
                    visible = true;
                    break;
                }
            }

            return visible;
        }

        public void UpdateMaterialProperties(Camera sentCamera)
        {
            var cameraToUse = GetCamera(sentCamera);

            foreach (var shade in Shades)
            {
                if (WorkType == WorkType.Direct ||
                 WorkType == WorkType.Reflect ||
                 WorkType == WorkType.Transparent)
                {
                    //SetMaterial(shade.MaterialToChange, "_ReflectionTex");
                    if (EnableFinalShader)
                    {
                        if (Iterations % 2 == 1) //again a hack:)
                        {
                            if (shade.MaterialToChange.HasProperty("_ReflectionTex"))
                                shade.MaterialToChange.SetTexture("_ReflectionTex", _reflectionTexture3);

                            if (shade.MaterialToChange.HasProperty("_ReflectionTexOther") && _stereoEye == 1)
                                shade.MaterialToChange.SetTexture("_ReflectionTexOther", _reflectionTexture3Other);
                        }
                        else
                        {
                            if (shade.MaterialToChange.HasProperty("_ReflectionTex"))
                                shade.MaterialToChange.SetTexture("_ReflectionTex", _reflectionTexture2);

                            if (shade.MaterialToChange.HasProperty("_ReflectionTexOther") && _stereoEye == 1)
                                shade.MaterialToChange.SetTexture("_ReflectionTexOther", _reflectionTexture2Other);
                        }
                    }
                    else if (EnableDepthBlur)
                    {
                        if (DepthBlurIterations % 2 == 1) //again a hack:)
                        {
                            if (shade.MaterialToChange.HasProperty("_ReflectionTex"))
                                shade.MaterialToChange.SetTexture("_ReflectionTex", _reflectionTexture3);

                            if (shade.MaterialToChange.HasProperty("_ReflectionTexOther") && _stereoEye == 1)
                                shade.MaterialToChange.SetTexture("_ReflectionTexOther", _reflectionTexture3Other);
                        }
                        else
                        {
                            if (shade.MaterialToChange.HasProperty("_ReflectionTex"))
                                shade.MaterialToChange.SetTexture("_ReflectionTex", _reflectionTexture2);

                            if (shade.MaterialToChange.HasProperty("_ReflectionTexOther") && _stereoEye == 1)
                                shade.MaterialToChange.SetTexture("_ReflectionTexOther", _reflectionTexture2Other);
                        }
                    }
                    else
                    {
                        if (shade.MaterialToChange.HasProperty("_ReflectionTex"))
                            shade.MaterialToChange.SetTexture("_ReflectionTex", _reflectionTexture1);

                        if (shade.MaterialToChange.HasProperty("_ReflectionTexOther") && _stereoEye == 1)
                            shade.MaterialToChange.SetTexture("_ReflectionTexOther", _reflectionTexture1Other);
                    }
                }

                //if (WorkType == WorkType.WaterTop)
                //  SetMaterial(shade.MaterialToChange, "_ReflectionTex");

                //if (WorkType == WorkType.WaterBottom)
                //SetMaterial(shade.MaterialToChange, "_ReflectionTex2");

                //if (shade.MaterialToChange.HasProperty("_IsReverse"))
                //shade.MaterialToChange.SetFloat("_IsReverse", WorkType == WorkType.Direct ? 1 : 0);
                if (DisableGBuffer)
                    shade.MaterialToChange.EnableKeyword("_FULLMIRROR");
                else
                    shade.MaterialToChange.DisableKeyword("_FULLMIRROR");

                if (DisableRProbes)
                    shade.MaterialToChange.EnableKeyword("_DISABLEPROBES");
                else
                    shade.MaterialToChange.DisableKeyword("_DISABLEPROBES");

                if (shade.MaterialToChange.HasProperty("_ReflectionIntensity"))
                    shade.MaterialToChange.SetFloat("_ReflectionIntensity", Intensity);

                if (shade.MaterialToChange.HasProperty("_LODLevel"))
                    shade.MaterialToChange.SetFloat("_LODLevel", TextureLODLevel);

                //if (shade.MaterialToChange.HasProperty("_WetLevel"))
                //  shade.MaterialToChange.SetFloat("_WetLevel", Wetness);

                if (shade.MaterialToChange.HasProperty("_WorkType"))
                    shade.MaterialToChange.SetFloat("_WorkType", (float)WorkType);

                if (shade.MaterialToChange.HasProperty("_DeviceType"))
                    shade.MaterialToChange.SetFloat("_DeviceType", (int)DeviceType);

                if (shade.MaterialToChange.HasProperty("_MixBlackColor"))
                    shade.MaterialToChange.SetFloat("_MixBlackColor", MixBlackColor == true ? 1 : 0);



                if (shade.MaterialToChange.HasProperty("_EnableDepthBlur"))
                    shade.MaterialToChange.SetFloat("_EnableDepthBlur", EnableDepthBlur == true ? 1 : -1);



                if (shade.MaterialToChange.HasProperty("_EnableSimpleDepth"))
                    shade.MaterialToChange.SetFloat("_EnableSimpleDepth", EnableSimpleDepth == true ? 1 : -1);

                if (shade.MaterialToChange.HasProperty("_SimpleDepthCutoff"))
                    shade.MaterialToChange.SetFloat("_SimpleDepthCutoff", SimpleDepthCutoff);

                //if (shade.MaterialToChange.HasProperty("_DepthBlur"))
                //  shade.MaterialToChange.SetFloat("_DepthBlur", DepthBlur);

                if (shade.MaterialToChange.HasProperty("_ReflectionTexDepth"))
                    shade.MaterialToChange.SetTexture("_ReflectionTexDepth", _reflectionTextureDepth);

                if (shade.MaterialToChange.HasProperty("_ReflectionTexOtherDepth") && _stereoEye == 1)
                    shade.MaterialToChange.SetTexture("_ReflectionTexOtherDepth", _reflectionTextureOtherDepth);

                if (shade.MaterialToChange.HasProperty("_NearClip"))
                    shade.MaterialToChange.SetFloat("_NearClip", cameraToUse.nearClipPlane);

                if (shade.MaterialToChange.HasProperty("_FarClip"))
                    shade.MaterialToChange.SetFloat("_FarClip", cameraToUse.farClipPlane);



                if (shade.MaterialToChange.HasProperty("_ReflectionRefraction"))
                    shade.MaterialToChange.SetFloat("_ReflectionRefraction", Refraction);

                if (shade.MaterialToChange.HasProperty("_RefractionTex"))
                    shade.MaterialToChange.SetTexture("_RefractionTex", RefractionTexture);



                if (shade.MaterialToChange.HasProperty("_MaskTex"))
                    shade.MaterialToChange.SetTexture("_MaskTex", MaskTexture);

                if (shade.MaterialToChange.HasProperty("_MaskCutoff"))
                    shade.MaterialToChange.SetFloat("_MaskCutoff", MaskCutoff);

                if (shade.MaterialToChange.HasProperty("_MaskTiling"))
                    shade.MaterialToChange.SetVector("_MaskTiling", new Vector4(MaskTiling.x, MaskTiling.y, 1, 1));

                if (shade.MaterialToChange.HasProperty("_EnableMask"))
                    shade.MaterialToChange.SetFloat("_EnableMask", EnableMask == true ? 1 : -1);

                if (shade.MaterialToChange.HasProperty("_MaskEdgeDarkness"))
                    shade.MaterialToChange.SetFloat("_MaskEdgeDarkness", MaskEdgeDarkness);



                if (shade.MaterialToChange.HasProperty("_EnableWave"))
                    shade.MaterialToChange.SetFloat("_EnableWave", EnableWaves == true ? 1 : -1);

                if (shade.MaterialToChange.HasProperty("_WaveNoiseTex"))
                    shade.MaterialToChange.SetTexture("_WaveNoiseTex", WaveNoiseTexture);

                if (shade.MaterialToChange.HasProperty("_WaveDistortion"))
                    shade.MaterialToChange.SetFloat("_WaveDistortion", WaveDistortion);

                if (shade.MaterialToChange.HasProperty("_WaveSize"))
                    shade.MaterialToChange.SetFloat("_WaveSize", WaveSize);

                if (shade.MaterialToChange.HasProperty("_WaveSpeed"))
                    shade.MaterialToChange.SetFloat("_WaveSpeed", WaveSpeed);



                if (shade.MaterialToChange.HasProperty("_EnableRipple"))
                    shade.MaterialToChange.SetFloat("_EnableRipple", EnableRipples == true ? 1 : -1);

                if (shade.MaterialToChange.HasProperty("_RippleTex"))
                    shade.MaterialToChange.SetTexture("_RippleTex", RippleTexture);

                if (shade.MaterialToChange.HasProperty("_RippleSize"))
                    shade.MaterialToChange.SetFloat("_RippleSize", RippleSize);

                if (shade.MaterialToChange.HasProperty("_RippleRefraction"))
                    shade.MaterialToChange.SetFloat("_RippleRefraction", RippleRefraction);

                if (shade.MaterialToChange.HasProperty("_RippleDensity"))
                    shade.MaterialToChange.SetFloat("_RippleDensity", RippleDensity);

                if (shade.MaterialToChange.HasProperty("_RippleSpeed"))
                    shade.MaterialToChange.SetFloat("_RippleSpeed", RippleSpeed);
            }
        }

        //use these ones to copy textures
        private RenderTexture _reflectionTexture1_copy;
        private RenderTexture _reflectionTexture1Other_copy;
        private RenderTexture _reflectionTexture2_copy;
        private RenderTexture _reflectionTexture2Other_copy;
        private RenderTexture _reflectionTexture3_copy;
        private RenderTexture _reflectionTexture3Other_copy;

        private RenderTexture _reflectionTextureDepth_copy;
        private RenderTexture _reflectionTextureOtherDepth_copy;

        public IList<RenderTexture> CopyTextures()
        {
            if (!_reflectionTexture1_copy && _reflectionTexture1)
            {
                _reflectionTexture1_copy = new RenderTexture(_reflectionTexture1);
            }

            if (_reflectionTexture1 != null)
            {
                Graphics.Blit(_reflectionTexture1, _reflectionTexture1_copy);
            }

            if (!_reflectionTexture1Other_copy && _reflectionTexture1Other)
            {
                _reflectionTexture1Other_copy = new RenderTexture(_reflectionTexture1Other);
            }

            if (_reflectionTexture1Other != null)
            {
                Graphics.Blit(_reflectionTexture1Other, _reflectionTexture1Other_copy);
            }

            if (!_reflectionTexture2_copy && _reflectionTexture2)
            {
                _reflectionTexture2_copy = new RenderTexture(_reflectionTexture2);
            }

            if (_reflectionTexture2 != null)
            {
                Graphics.Blit(_reflectionTexture2, _reflectionTexture2_copy);
            }

            if (!_reflectionTexture2Other_copy && _reflectionTexture2Other)
            {
                _reflectionTexture2Other_copy = new RenderTexture(_reflectionTexture2Other);
            }

            if (_reflectionTexture2Other != null)
            {
                Graphics.Blit(_reflectionTexture2Other, _reflectionTexture2Other_copy);
            }

            if (!_reflectionTexture3_copy && _reflectionTexture3)
            {
                _reflectionTexture3_copy = new RenderTexture(_reflectionTexture3);
            }

            if (_reflectionTexture3 != null)
            {
                Graphics.Blit(_reflectionTexture3, _reflectionTexture3_copy);
            }

            if (!_reflectionTexture3Other_copy && _reflectionTexture3Other)
            {
                _reflectionTexture3Other_copy = new RenderTexture(_reflectionTexture3Other);
            }

            if (_reflectionTexture3Other != null)
            {
                Graphics.Blit(_reflectionTexture3Other, _reflectionTexture3Other_copy);
            }

            if (!_reflectionTextureDepth_copy && _reflectionTextureDepth)
            {
                _reflectionTextureDepth_copy = new RenderTexture(_reflectionTextureDepth);
            }

            if (_reflectionTextureDepth != null)
            {
                Graphics.Blit(_reflectionTextureDepth, _reflectionTextureDepth_copy);
            }

            if (!_reflectionTextureOtherDepth_copy && _reflectionTextureOtherDepth)
            {
                _reflectionTextureOtherDepth_copy = new RenderTexture(_reflectionTextureOtherDepth);
            }

            if (_reflectionTextureOtherDepth != null)
            {
                Graphics.Blit(_reflectionTextureOtherDepth, _reflectionTextureOtherDepth_copy);
            }

            return new List<RenderTexture>
      {
        _reflectionTexture1_copy,
        _reflectionTexture1Other_copy,
        _reflectionTexture2_copy,
        _reflectionTexture2Other_copy,
        _reflectionTexture3_copy,
        _reflectionTexture3Other_copy,
        _reflectionTextureDepth_copy,
        _reflectionTextureOtherDepth_copy
      };
        }

        public void PasteTextures(IList<RenderTexture> textures)
        {
            if (textures != null && textures.Count >= 8)
            {
                _reflectionTexture1 = textures[0];
                _reflectionTexture1Other = textures[1];
                _reflectionTexture2 = textures[2];
                _reflectionTexture2Other = textures[3];
                _reflectionTexture3 = textures[4];
                _reflectionTexture3Other = textures[5];
                _reflectionTextureDepth = textures[6];
                _reflectionTextureOtherDepth = textures[7];
            }
        }


        // Given position/normal of the plane, calculates plane in camera space.
        private Vector4 CameraSpacePlane(Matrix4x4 worldToCameraMatrix, Vector3 pos, Vector3 normal, float sideSign)
        {
            Vector3 offsetPos = pos + normal * ClipPlaneOffset;
            Vector3 cpos = worldToCameraMatrix.MultiplyPoint(offsetPos);
            Vector3 cnormal = worldToCameraMatrix.MultiplyVector(normal).normalized * sideSign;
            return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
        }

        // Extended sign: returns -1, 0 or 1 based on sign of a
        private static float sgn(float a)
        {
            if (a > 0.0f) return 1.0f;
            if (a < 0.0f) return -1.0f;
            return 0.0f;
        }

        private static void MakeProjectionMatrixOblique(ref Matrix4x4 matrix, Vector4 clipPlane)
        {
            Vector4 q;

            // Calculate the clip-space corner point opposite the clipping plane
            // as (sgn(clipPlane.x), sgn(clipPlane.y), 1, 1) and
            // transform it into camera space by multiplying it
            // by the inverse of the projection matrix

            q.x = (sgn(clipPlane.x) + matrix[8]) / matrix[0];
            q.y = (sgn(clipPlane.y) + matrix[9]) / matrix[5];
            q.z = -1.0F;
            q.w = (1.0F + matrix[10]) / matrix[14];

            // Calculate the scaled plane vector
            Vector4 c = clipPlane * (2.0F / Vector3.Dot(clipPlane, q));

            // Replace the third row of the projection matrix
            matrix[2] = c.x;
            matrix[6] = c.y;
            matrix[10] = c.z + 1.0F;
            matrix[14] = c.w;
        }

#if STEAMVR_ENABLED
    protected Matrix4x4 HMDMatrix4x4ToMatrix4x4(Valve.VR.HmdMatrix44_t input)
    {
      var m = Matrix4x4.identity;

      m[0, 0] = input.m0;
      m[0, 1] = input.m1;
      m[0, 2] = input.m2;
      m[0, 3] = input.m3;

      m[1, 0] = input.m4;
      m[1, 1] = input.m5;
      m[1, 2] = input.m6;
      m[1, 3] = input.m7;

      m[2, 0] = input.m8;
      m[2, 1] = input.m9;
      m[2, 2] = input.m10;
      m[2, 3] = input.m11;

      m[3, 0] = input.m12;
      m[3, 1] = input.m13;
      m[3, 2] = input.m14;
      m[3, 3] = input.m15;

      return m;
    }
#endif

        // Cleanup all the objects we possibly have created
        void OnDisable()
        {
            if (_reflectionTexture1)
            {
                DestroyImmediate(_reflectionTexture1);
                _reflectionTexture1 = null;
            }
            if (_reflectionTexture1Other)
            {
                DestroyImmediate(_reflectionTexture1Other);
                _reflectionTexture1Other = null;
            }
            if (_reflectionTexture2)
            {
                DestroyImmediate(_reflectionTexture2);
                _reflectionTexture2 = null;
            }
            if (_reflectionTexture2Other)
            {
                DestroyImmediate(_reflectionTexture2Other);
                _reflectionTexture2Other = null;
            }
            if (_reflectionTexture3)
            {
                DestroyImmediate(_reflectionTexture3);
                _reflectionTexture3 = null;
            }
            if (_reflectionTexture3Other)
            {
                DestroyImmediate(_reflectionTexture3Other);
                _reflectionTexture3Other = null;
            }
            if (_reflectionTextureDepth)
            {
                DestroyImmediate(_reflectionTextureDepth);
                _reflectionTextureDepth = null;
            }
            if (_reflectionTextureOtherDepth)
            {
                DestroyImmediate(_reflectionTextureOtherDepth);
                _reflectionTextureOtherDepth = null;
            }
            if (_reflectionCameras != null)
            {
                foreach (DictionaryEntry kvp in _reflectionCameras)
                {
                    if (kvp.Value is Camera)
                        DestroyImmediate(((Camera)kvp.Value).gameObject);
                }
                _reflectionCameras.Clear();
            }
        }

        private void UpdateCameraModes(Camera src, Camera dest)
        {
            if (dest == null || ReflectionCameraPrefab != null)
                return;

            // set camera to clear the same way as current camera
            //if (IsMirrorInMirror == true)
            //{
            //    Debug.Log("mirror in mirror");
            //    //Because on full transparency we make transparent black (0,0,0,1) pixels! And no MSAA must be on!
            //    dest.clearFlags = CameraClearFlags.Nothing;//.Color;
            //    dest.backgroundColor = new Color(0, 0, 0, 0); //we will use that to clear background on shader
            //}
            if (WorkType == WorkType.Transparent)
            {
                //Because on full transparency we make transparent black (0,0,0,1) pixels! And no MSAA must be on!
                dest.clearFlags = CameraClearFlags.Color;
                dest.backgroundColor = TClearColor; //new Color(0, 0, 0, 1); we will use that to clear background on shader
            }
            else if (EnableDepthBlur)
            {
                //EnableDepthBlur mixes not nice with the skybox! so we disable it too!
                dest.clearFlags = CameraClearFlags.Color;
                dest.backgroundColor = new Color(0, 0, 0, 1);
            }
            else if (MixBlackColor)
            {
                //Mix the not reflected colors with surface texture for CULL-ing and Reflect-Layer != Everything
                //alpha 0 is important
                dest.clearFlags = CameraClearFlags.Color;
                dest.backgroundColor = new Color(0, 0, 0, 0);
            }
            else if (DeviceType == DeviceType.AR)
            {
                dest.clearFlags = CameraClearFlags.Skybox;
                dest.backgroundColor = src.backgroundColor;
            }
            else
            {
                dest.clearFlags = src.clearFlags;//CameraClearFlags.SolidColor; // src.clearFlags;
                dest.backgroundColor = src.backgroundColor;
            }

            if (dest.clearFlags == CameraClearFlags.Skybox)
            {
                Skybox sky = src.GetComponent(typeof(Skybox)) as Skybox;
                Skybox mysky = dest.GetComponent(typeof(Skybox)) as Skybox;
                if (mysky != null)
                {
                    if (!sky || !sky.material)
                    {
                        mysky.enabled = false;
                    }
                    else
                    {
                        mysky.enabled = true;
                        mysky.material = sky.material;
                    }
                }
            }

            // update other values to match current camera.
            // even if we are supplying custom camera&amp;projection matrices,
            // some of values are used elsewhere (e.g. skybox uses far plane)
            dest.nearClipPlane = src.nearClipPlane;
            dest.farClipPlane = src.farClipPlane;
            dest.orthographic = src.orthographic;
            if (DeviceType == DeviceType.Normal || DeviceType == DeviceType.AR)
                dest.fieldOfView = src.fieldOfView;
            if (EnableSimpleDepth || EnableDepthBlur)
                dest.depthTextureMode = DepthTextureMode.Depth;
            dest.aspect = src.aspect;
            dest.orthographicSize = src.orthographicSize;
            dest.renderingPath = src.renderingPath;
            dest.allowHDR = HDR;
            if (WorkType == WorkType.Transparent)
            {
                dest.allowMSAA = false; //!!!!! do not smooth texture!! it is important not to blend corners :)
            }
            else
            {
                dest.allowMSAA = MSAA;
            }
            dest.useOcclusionCulling = !TurnOffOcclusion;
        }

        public int[] GetTextureSizes()
        {
            //Calculate the render size
            switch (TextureSize)
            {
                case TextureSizeType.Quarter:
                    ManualSize = Screen.width / 4;
                    break;
                case TextureSizeType.Half:
                    ManualSize = Screen.width / 2;
                    break;
                case TextureSizeType.Full:
                    ManualSize = Screen.width;
                    break;
                case TextureSizeType.x2:
                    ManualSize = Screen.width * 2;
                    break;
                case TextureSizeType.x4:
                    ManualSize = Screen.width * 4;
                    break;
                default:
                    break; //do not change Manual size
            }

            int textureSize = ManualSize + ManualSize % 2; //calculate the width and height according to aspect
            if (textureSize <= 128)
                textureSize = 128;

            int textureSizeHeight = (int)((double)textureSize * ((double)Screen.height / (double)Screen.width));
            textureSizeHeight = textureSizeHeight + textureSizeHeight % 2;

            return new int[2] { textureSize, textureSizeHeight };
        }

        // On-demand create any objects we need
        private void CreateMirrorObjects(Camera currentCamera, out Camera reflectionCamera)
        {
            //reflectionCamera = null;
            int depth = 24;

            var textureSizes = GetTextureSizes();
            var textureSize = textureSizes[0];
            var textureSizeHeight = textureSizes[1];

            RenderTextureFormat textureFormatHDR;
            RenderTextureFormat textureFormat;

            if(SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBFloat))
                textureFormatHDR = RenderTextureFormat.ARGBFloat;
            else
                textureFormatHDR = RenderTextureFormat.DefaultHDR;

            if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGB32))
                textureFormat = RenderTextureFormat.ARGB32;
            else
                textureFormat = RenderTextureFormat.Default;

            //Create render textures
            if (!_reflectionTexture1 ||
               !_reflectionTexture1Other ||
               !_reflectionTexture2 ||
               !_reflectionTexture2Other ||
               !_reflectionTexture3 ||
               !_reflectionTexture3Other ||
               !_reflectionTextureDepth ||
               !_reflectionTextureOtherDepth ||
               _oldTextureSize != textureSize ||
               _oldHDR != HDR ||
               _oldTextureLODLevel != TextureLODLevel ||
               _oldEnableSimpleDepth != EnableSimpleDepth ||
               _oldEnableDepthBlur != EnableDepthBlur)
            {
                if (_reflectionTexture1)
                    DestroyImmediate(_reflectionTexture1);
                if (HDR)
                    _reflectionTexture1 = new RenderTexture(textureSize, textureSizeHeight, depth, textureFormatHDR);
                else
                    _reflectionTexture1 = new RenderTexture(textureSize, textureSizeHeight, depth, textureFormat);
                _reflectionTexture1.name = "__MirrorReflection1" + GetInstanceID();
                if (WorkType == WorkType.Transparent)
                {
                    _reflectionTexture1.filterMode = FilterMode.Point; //no filter for transparency, do not smooth :)
                }
                _reflectionTexture1.isPowerOfTwo = true;
                _reflectionTexture1.hideFlags = HideFlags.DontSave;
                if (TextureLODLevel > 0)
                {
                    _reflectionTexture1.useMipMap = true;
                    _reflectionTexture1.autoGenerateMips = false;
                }
                if (MSAA && WorkType != WorkType.Transparent && QualitySettings.antiAliasing > 0)
                    _reflectionTexture1.antiAliasing = QualitySettings.antiAliasing;

                if (_reflectionTexture1Other)
                    DestroyImmediate(_reflectionTexture1Other);
                if (HDR)
                    _reflectionTexture1Other = new RenderTexture(textureSize, textureSizeHeight, depth, textureFormatHDR);
                else
                    _reflectionTexture1Other = new RenderTexture(textureSize, textureSizeHeight, depth, textureFormat);
                _reflectionTexture1Other.name = "__MirrorReflection1Other" + GetInstanceID();
                if (WorkType == WorkType.Transparent)
                {
                    _reflectionTexture1Other.filterMode = FilterMode.Point; //no filter for transparency, do not smooth :)
                }
                _reflectionTexture1Other.isPowerOfTwo = true;
                _reflectionTexture1Other.hideFlags = HideFlags.DontSave;
                if (TextureLODLevel > 0)
                {
                    _reflectionTexture1Other.useMipMap = true;
                    _reflectionTexture1Other.autoGenerateMips = false;
                }
                if (MSAA && WorkType != WorkType.Transparent && QualitySettings.antiAliasing > 0)
                    _reflectionTexture1Other.antiAliasing = QualitySettings.antiAliasing;

                if (_reflectionTexture2)
                    DestroyImmediate(_reflectionTexture2);
                if (HDR)
                    _reflectionTexture2 = new RenderTexture(textureSize, textureSizeHeight, depth, textureFormatHDR);
                else
                    _reflectionTexture2 = new RenderTexture(textureSize, textureSizeHeight, depth, textureFormat);
                _reflectionTexture2.name = "__MirrorReflection2" + GetInstanceID();
                if (WorkType == WorkType.Transparent)
                {
                    _reflectionTexture2.filterMode = FilterMode.Point; //no filter for transparency, do not smooth :)
                }
                _reflectionTexture2.isPowerOfTwo = true;
                _reflectionTexture2.hideFlags = HideFlags.DontSave;
                if (TextureLODLevel > 0)
                {
                    _reflectionTexture2.useMipMap = true;
                    _reflectionTexture2.autoGenerateMips = false;
                }
                if (MSAA && WorkType != WorkType.Transparent && QualitySettings.antiAliasing > 0)
                    _reflectionTexture2.antiAliasing = QualitySettings.antiAliasing;

                if (_reflectionTexture2Other)
                    DestroyImmediate(_reflectionTexture2Other);
                if (HDR)
                    _reflectionTexture2Other = new RenderTexture(textureSize, textureSizeHeight, depth, textureFormatHDR);
                else
                    _reflectionTexture2Other = new RenderTexture(textureSize, textureSizeHeight, depth, textureFormat);
                _reflectionTexture2Other.name = "__MirrorReflection2Other" + GetInstanceID();
                if (WorkType == WorkType.Transparent)
                {
                    _reflectionTexture2Other.filterMode = FilterMode.Point; //no filter for transparency, do not smooth :)
                }
                _reflectionTexture2Other.isPowerOfTwo = true;
                _reflectionTexture2Other.hideFlags = HideFlags.DontSave;
                if (TextureLODLevel > 0)
                {
                    _reflectionTexture2Other.useMipMap = true;
                    _reflectionTexture2Other.autoGenerateMips = false;
                }
                if (MSAA && WorkType != WorkType.Transparent && QualitySettings.antiAliasing > 0)
                    _reflectionTexture2Other.antiAliasing = QualitySettings.antiAliasing;

                if (_reflectionTexture3)
                    DestroyImmediate(_reflectionTexture3);
                if (HDR)
                    _reflectionTexture3 = new RenderTexture(textureSize, textureSizeHeight, depth, textureFormatHDR);
                else
                    _reflectionTexture3 = new RenderTexture(textureSize, textureSizeHeight, depth, textureFormat);
                _reflectionTexture3.name = "__MirrorReflection3" + GetInstanceID();
                if (WorkType == WorkType.Transparent)
                {
                    _reflectionTexture3.filterMode = FilterMode.Point; //no filter for transparency, do not smooth :)
                }
                _reflectionTexture3.isPowerOfTwo = true;
                _reflectionTexture3.hideFlags = HideFlags.DontSave;
                if (TextureLODLevel > 0)
                {
                    _reflectionTexture3.useMipMap = true;
                    _reflectionTexture3.autoGenerateMips = false;
                }
                if (MSAA && WorkType != WorkType.Transparent && QualitySettings.antiAliasing > 0)
                    _reflectionTexture3.antiAliasing = QualitySettings.antiAliasing;

                if (_reflectionTexture3Other)
                    DestroyImmediate(_reflectionTexture3Other);
                if (HDR)
                    _reflectionTexture3Other = new RenderTexture(textureSize, textureSizeHeight, depth, textureFormatHDR);
                else
                    _reflectionTexture3Other = new RenderTexture(textureSize, textureSizeHeight, depth, textureFormat);
                _reflectionTexture3Other.name = "__MirrorReflection3Other" + GetInstanceID();
                if (WorkType == WorkType.Transparent)
                {
                    _reflectionTexture3Other.filterMode = FilterMode.Point; //no filter for transparency, do not smooth :)
                }
                _reflectionTexture3Other.isPowerOfTwo = true;
                _reflectionTexture3Other.hideFlags = HideFlags.DontSave;
                if (TextureLODLevel > 0)
                {
                    _reflectionTexture3Other.useMipMap = true;
                    _reflectionTexture3Other.autoGenerateMips = false;
                }
                if (MSAA && WorkType != WorkType.Transparent && QualitySettings.antiAliasing > 0)
                    _reflectionTexture3Other.antiAliasing = QualitySettings.antiAliasing;

                if (_reflectionTextureDepth)
                    DestroyImmediate(_reflectionTextureDepth);
                _reflectionTextureDepth = new RenderTexture(textureSize, textureSizeHeight, depth, RenderTextureFormat.Depth);
                _reflectionTextureDepth.name = "__MirrorReflectionDepth" + GetInstanceID();
                _reflectionTextureDepth.isPowerOfTwo = true;
                _reflectionTextureDepth.hideFlags = HideFlags.DontSave;

                if (_reflectionTextureOtherDepth)
                    DestroyImmediate(_reflectionTextureOtherDepth);
                _reflectionTextureOtherDepth = new RenderTexture(textureSize, textureSizeHeight, depth, RenderTextureFormat.Depth);
                _reflectionTextureOtherDepth.name = "__MirrorReflectionOtherDepth" + GetInstanceID();
                _reflectionTextureOtherDepth.isPowerOfTwo = true;
                _reflectionTextureOtherDepth.hideFlags = HideFlags.DontSave;

                _oldTextureSize = textureSize;
                _oldEnableSimpleDepth = EnableSimpleDepth;
                _oldEnableDepthBlur = EnableDepthBlur;
                _oldHDR = HDR;
                _oldTextureLODLevel = TextureLODLevel;
            }

            // Camera for reflection
            if (ReflectionCameraPrefab == null)
            {
                reflectionCamera = _reflectionCameras[currentCamera] as Camera;
                if (!reflectionCamera) // catch both not-in-dictionary and in-dictionary-but-deleted-GO
                {
                    GameObject go = new GameObject("Mirror Refl Camera id" + GetInstanceID() + " for " + currentCamera.GetInstanceID(), typeof(Camera), typeof(Skybox));

                    reflectionCamera = go.GetComponent<Camera>();
                    reflectionCamera.enabled = false;
                    reflectionCamera.transform.position = transform.position;
                    reflectionCamera.transform.rotation = transform.rotation;
                    reflectionCamera.gameObject.AddComponent<FlareLayer>();
                    go.hideFlags = HideFlags.HideAndDontSave;
                    _reflectionCameras[currentCamera] = reflectionCamera;
                }
            }
            else
            {
                //reflectionCamera = _reflectionCameras[currentCamera] as Camera;
                if (_reflectionCameraPrefabInstance != null)
                {
                    reflectionCamera = _reflectionCameraPrefabInstance.GetComponent<Camera>();
                }
                else
                //if (!reflectionCamera) // catch both not-in-dictionary and in-dictionary-but-deleted-GO
                {
                    _reflectionCameraPrefabInstance = GameObject.Instantiate(ReflectionCameraPrefab);
                    reflectionCamera = _reflectionCameraPrefabInstance.GetComponent<Camera>();
                    reflectionCamera.enabled = false;
                    //reflectionCamera.transform.position = transform.position;
                    //reflectionCamera.transform.rotation = transform.rotation;
                    //reflectionCamera.gameObject.AddComponent<FlareLayer>();
                    _reflectionCameraPrefabInstance.hideFlags = HideFlags.HideAndDontSave;
                    //reflectionCamera = _reflectionCameraPrefabInstance;
                }
            }
        }

        // Given position/normal of the plane, calculates plane in camera space.
        private Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
        {
            Vector3 offsetPos = pos + normal * ClipPlaneOffset;
            Matrix4x4 m = cam.worldToCameraMatrix;
            Vector3 cpos = m.MultiplyPoint(offsetPos);
            Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;
            return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
        }

        // Calculates reflection matrix around the given plane
        private void CalculateReflectionMatrix(ref Matrix4x4 reflectionMat, Vector4 plane)
        {
            reflectionMat.m00 = (1F - 2F * plane[0] * plane[0]);
            reflectionMat.m01 = (-2F * plane[0] * plane[1]);
            reflectionMat.m02 = (-2F * plane[0] * plane[2]);
            reflectionMat.m03 = (-2F * plane[3] * plane[0]);

            reflectionMat.m10 = (-2F * plane[1] * plane[0]);
            reflectionMat.m11 = (1F - 2F * plane[1] * plane[1]);
            reflectionMat.m12 = (-2F * plane[1] * plane[2]);
            reflectionMat.m13 = (-2F * plane[3] * plane[1]);

            reflectionMat.m20 = (-2F * plane[2] * plane[0]);
            reflectionMat.m21 = (-2F * plane[2] * plane[1]);
            reflectionMat.m22 = (1F - 2F * plane[2] * plane[2]);
            reflectionMat.m23 = (-2F * plane[3] * plane[2]);

            reflectionMat.m30 = 0F;
            reflectionMat.m31 = 0F;
            reflectionMat.m32 = 0F;
            reflectionMat.m33 = 1F;
        }

        T CopyComponent<T>(T original, GameObject destination) where T : Component
        {
            System.Type type = original.GetType();
            Component copy = destination.AddComponent(type);
            System.Reflection.FieldInfo[] fields = type.GetFields();
            foreach (System.Reflection.FieldInfo field in fields)
            {
                field.SetValue(copy, field.GetValue(original));
            }
            return copy as T;
        }
    }

    public enum FollowVector
    {
        None = 0,
        RedX = 1,
        RedX_Negative = 4,
        GreenY = 2,
        GreenY_Negative = 5,
        BlueZ = 3,
        BlueZ_Negative = 6
    }

    public enum WorkType
    {
        Reflect = 1,
        Direct = 2,
        Transparent = 3,
        WaterTop = 4,
        WaterBottom = 5
    }

    public enum TextureSizeType
    {
        Manual = 0,
        x4 = 6,
        x2 = 5,
        Full = 1,
        Half = 2,
        Quarter = 4
    }

    public enum DeviceType
    {
        Normal = 0,
        GeneralVR = 1,
        OculusVR_RiftS = 2,
        OculusVR_Quest = 3,
        //SteamVR = 20,
        AR = 30
    }

    [Serializable]
    public class Shade
    {
        public GameObject ObjectToShade;
        public Material MaterialToChange;
    }
}


