using DemoX.Framework.Bridge.Event;
using DemoX.Framework.Core;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

namespace DemoX.Framework
{
    public class SceneLoading : MonoBehaviour
    {
        public Animator sphereAni;
        public MeshRenderer sphereMesh;

        [SerializeField] private ECSceneLoad _ecSceneLoad;

        [FormerlySerializedAs("leftCubemap")] [SerializeField]
        private RenderTexture _leftCubemap;

        [FormerlySerializedAs("RightCubemap")] [SerializeField]
        private RenderTexture _rightCubemap;

        [FormerlySerializedAs("equirect")] [SerializeField]
        private RenderTexture _equirect;

        [SerializeField] private float _startLoadingAnimDuration = 3.0f;
        [SerializeField] private float _finishLoadingAnimDuration = 3.0f;

        private Material _mat;
        private static readonly int OutAnimKey = Animator.StringToHash("out");

        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        //public play

        private Camera _mainCamera;
        private Camera _renderLoadingCamera;

        private string _lastLoadingScene;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            _mat = sphereMesh.material;
            sphereMesh.enabled = false;

            // _ecSceneLoad.OnStartLoading.AddListener(StartLoading);
            // _ecSceneLoad.OnFinishLoading.AddListener(OnSceneLoaded);

            // GetComponent<Vulture>();
        }

        private bool RetrieveCamera()
        {
            _mainCamera = Camera.main;
            if (_mainCamera)
            {
                _renderLoadingCamera = _mainCamera.GetComponentInChildren<Camera>();
            }

            return _mainCamera && _renderLoadingCamera;
        }

        private void StartLoading(string _)
        {
            Load();
            // if (!RetrieveCamera())
            // {
            //     Load();
            //     return;
            // }
            //
            // RenderOneEqu();
            // SetDissoiveTex();
            // sphereAni.SetBool(OutAnimKey, true);
            // sphereMesh.enabled = true;
            // SetCameraLayer(false);
            // Invoke(nameof(Load), _startLoadingAnimDuration);
        }

        private void Load()
        {
            _ecSceneLoad.OnLoadingStop.Invoke();
        }

        private void SetCameraLayer(bool lookEvery)
        {
            _mainCamera.cullingMask = lookEvery ? -1 : 1 << 19;
        }

        private void OnSceneLoaded(string scene)
        {
            _lastLoadingScene = scene;
            if (!RetrieveCamera())
            {
                sphereMesh.enabled = false;
                _ecSceneLoad.OnLoadingAnimStop.Invoke(scene);
                return;
            }

            ///场景加载结束后渲染
            RenderOneEqu();
            SetDissoiveTex();
            sphereAni.SetBool(OutAnimKey, false);
            Invoke(nameof(ChangeEnd), _finishLoadingAnimDuration);
        }

        public void ChangeEnd()
        {
            sphereMesh.enabled = false;
            SetCameraLayer(true);
            _ecSceneLoad.OnLoadingAnimStop.Invoke(_lastLoadingScene);
        }

        public void SetDissoiveTex()
        {
            _mat.SetTexture(MainTexId, _equirect);
        }

        public void RenderOneEqu()
        {
            if (!_renderLoadingCamera) return;
            _renderLoadingCamera.stereoSeparation = 0.064f;
            _renderLoadingCamera.RenderToCubemap(_rightCubemap, 63, Camera.MonoOrStereoscopicEye.Mono);
            _rightCubemap.ConvertToEquirect(_equirect, Camera.MonoOrStereoscopicEye.Mono);
        }
    }
}