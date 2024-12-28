using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class SceneJumper : MonoBehaviour
{
    public static SceneJumper instence;

    public MeshRenderer sphereMesh;
    private Material _mat;

    public int next_index;

    public Camera main_Camera;

    public VideoPlayer mediaPlayer_Out;
    public VideoPlayer mediaPlayer_In;
    //public Texture2D black;
    //public play

    private void Awake()
    {
        instence = this;

        _mat = sphereMesh.material;

        AddLinster();
    }
    private void AddLinster()
    {
        mediaPlayer_In.prepareCompleted += MediaPlayer_In_prepareCompleted;


        mediaPlayer_In.loopPointReached += MediaPlayer_In_loopPointReached;

        mediaPlayer_Out.loopPointReached += delegate (VideoPlayer source)
        {
            SetNext();
        };
        
    }

    /// <summary>
    /// 进入下一个场景第一帧开始调用
    /// </summary>
    /// <param name="source"></param>
    private void MediaPlayer_In_prepareCompleted(VideoPlayer source)
    {
        mediaPlayer_In.Play();
        ///
    }
    /// <summary>
    /// 完全进入新场景调用
    /// </summary>
    /// <param name="source"></param>
    private void MediaPlayer_In_loopPointReached(VideoPlayer source)
    {
        source.Stop();
        SetCamerLayer(true);

        ///结束了
        ChangeEnd();
    }
    /// <summary>
    /// 开始淡出旧场景时调用
    /// </summary>
    private void LoadMediaPlayer_Out()
    {
        SetCamerLayer(false);
        mediaPlayer_Out.Play();
    }
    
    //public void SetMainCamerToUV()
    //{
    //    float v_x = Camera.main.transform.eulerAngles.y / 360f;
    //    float v_y = Camera.main.transform.forward.normalized.y;
    //    v_y = Mathf.Clamp(v_y, -0.71f, 0.71f);
    //    //if (v_y > 1)
    //    //{
    //    v_y = 0.5f + 0.5f * (v_y / 0.5f);
    //    ///}
    //    //else if (v_y < 1)
    //    //{
    //    //    v_y = 0.5f - v_y;
    //    //}
    //   // Debug.Log(v_x + "," + v_y);

    //    _mat.SetFloat("_DissolveCenterX", v_x);
    //    _mat.SetFloat("_DissolveCenterY", v_y);
    //}
    public void ToChangeScnene()
    {
        transform.position = Camera.main.transform.position;
        
        LoadMediaPlayer_Out();

    }
    private void SetCamerLayer(bool lookEvery)
    {
        _mat.SetFloat("_ForceZ", lookEvery ? 0 : 1);
    }
    private void SetNext()
    {
        SceneManager.LoadSceneAsync(next_index, LoadSceneMode.Single).completed += SceneJumper_completed; ;

    }
    /// <summary>
    /// 旧场景完全淡出结束时调用
    /// </summary>
    /// <param name="obj"></param>
    private void SceneJumper_completed(AsyncOperation obj)
    {
        
        mediaPlayer_In.Prepare();
    }
    /// <summary>
    /// 转场结束TODO，例如可以移动可以走
    /// </summary>
    private void ChangeEnd()
    {
        
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.W))
        {
            ToChangeScnene();
        }

    }
}
