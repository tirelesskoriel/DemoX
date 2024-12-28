using System.Collections;
using AkilliMum.Standard.Mirror;
using DemoX.Framework.Bridge.Event;
using DemoX.Framework.Core;
using UnityEngine;

namespace DemoX.Framework
{
    public class CameraSetup : MonoBehaviour
    {
        [SerializeField] private ECSceneLoad _ecSceneLoad;

        [Header("Tiantan Config")] [SerializeField] [SceneField]
        private string _postEnableScene;

        [SerializeField] private FlareLayer _flareLayer;

        [Header("Black Scene Config")] [SerializeField] [SceneField]
        private string _blackScene;

        private CameraShade _mainCameraShade;
        private Shade[] _shades;
        public float duaToWait = 3f;
        private GameObject[] _shads;

        private void Awake()
        {
            _ecSceneLoad.OnFinishLoading.AddListener(OnSceneLoaded);
            _ecSceneLoad.OnStartLoading.AddListener(OnSceneStartLoading);
            _ecSceneLoad.OnLoadingAnimStop.AddListener(OnLoadingAnimStop);
            _mainCameraShade = GetComponent<CameraShade>();
        }

        private void OnSceneStartLoading(string scene)
        {
            if (!string.Equals(_postEnableScene, scene))
            {
                _flareLayer.enabled = false;
            }

            if (!string.Equals(_blackScene, scene))
            {
                _mainCameraShade.enabled = false;
            }
        }

        private void OnSceneLoaded(string scene)
        {
            if (string.Equals(_blackScene, scene))
            {
                SetupMirrorPlane();
            }
        }

        private void OnLoadingAnimStop(string scene)
        {
            if (string.Equals(_postEnableScene, scene))
            {
                _flareLayer.enabled = true;
            }
        }

        private void SetupMirrorPlane()
        {
            if (!_mainCameraShade) return;
            _mainCameraShade.Shades = null;
            _mainCameraShade.EnableWaves = false;
            _mainCameraShade.enabled = false;

            StartCoroutine(FindShades());
        }

        private IEnumerator FindShades()
        {
            yield return null;
            _shads = GameObject.FindGameObjectsWithTag("Mirror");
            if (_mainCameraShade && _shads.Length != 0)
            {
                _shades = new Shade[_shads.Length];
                for (int i = 0; i < _shads.Length; i++)
                {
                    Shade s = new Shade();
                    s.ObjectToShade = _shads[i];
                    s.MaterialToChange = _shads[i].GetComponent<MeshRenderer>().material;
                    _shades[i] = s;
                }

                _mainCameraShade.Shades = _shades;

                _mainCameraShade.enabled = true;

                _mainCameraShade.EnableWaves = true;
            }
        }
    }
}