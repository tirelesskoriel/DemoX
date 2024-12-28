using DemoX.Framework.Bridge.Event;
using DemoX.Framework.Core;
using UnityEngine;
using UnityEngine.Video;

namespace DemoX.Framework
{
    public class SceneJumper : MonoBehaviour
    {
        public ECSceneLoad _ecSceneLoad;
        public static SceneJumper instence;

        public MeshRenderer sphereMesh;
        private Material _mat;

        public int next_index;

        public VideoPlayer mediaPlayer_Out;

        public VideoPlayer mediaPlayer_In;
        //public Texture2D black;
        //public play

        private string _loadSceneName;

        private void Awake()
        {
            instence = this;

            _mat = sphereMesh.material;

            AddLinster();
        }

        private void AddLinster()
        {
            mediaPlayer_In.prepareCompleted += source => { source.Play(); };

            mediaPlayer_In.loopPointReached += MediaPlayer_In_loopPointReached;

            mediaPlayer_Out.loopPointReached += source =>
            {
                source.Pause();
                _ecSceneLoad.OnLoadingStop.Invoke();
            };
            
            mediaPlayer_Out.prepareCompleted += source => { source.Play(); };

            _ecSceneLoad.OnStartLoading.AddListener(ToChangeScene);
            _ecSceneLoad.OnFinishLoading.AddListener(SceneJumper_completed);
        }

        /// <summary>
        /// 进入下一个场景第一帧开始调用
        /// </summary>
        /// <param name="source"></param>
        private void MediaPlayer_In_prepareCompleted(VideoPlayer source)
        {
            mediaPlayer_In.Play();
        }

        /// <summary>
        /// 完全进入新场景调用
        /// </summary>
        /// <param name="source"></param>
        private void MediaPlayer_In_loopPointReached(VideoPlayer source)
        {
            source.Stop();
            SetCamerLayer(true);
            _ecSceneLoad.OnLoadingAnimStop.Invoke(_loadSceneName);
        }

        public void ToChangeScene(string sceneName)
        {
            _loadSceneName = sceneName;
            transform.position = Camera.main.transform.position;
            SetCamerLayer(false);
            mediaPlayer_Out.Stop();
            mediaPlayer_Out.Prepare();
        }

        private void SetCamerLayer(bool lookEvery)
        {
            _mat.SetFloat("_ForceZ", lookEvery ? 0 : 1);
        }

        /// <summary>
        /// 旧场景完全淡出结束时调用
        /// </summary>
        /// <param name="sceneName"></param>
        private void SceneJumper_completed(string sceneName)
        {
            mediaPlayer_In.Prepare();
        }

        /// <summary>
        /// 转场结束TODO，例如可以移动可以走
        /// </summary>
        private void ChangeEnd()
        {
        }
    }
}