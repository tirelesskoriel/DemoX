using System.Collections;
using System.Collections.Generic;
using DemoX.Framework.Core;
using Mirror;
using UnityEngine;

namespace DemoX.Framework.Net
{
    public class NetSceneSetting : MonoBehaviour
    {
        [Scene] public string Offline;
        [Scene] public string Online;
        [Scene] public List<string> Scenes;

        // private void Start()
        // {
        //     StartCoroutine(Enumerator0());
        // }
        //
        // private IEnumerator Enumerator0()
        // {
        //     yield return new WaitForSeconds(5.0f);
        //     yield return XNetManager.Ins.UnloadSubScenes();
        // }
        //
        // private IEnumerator Enumerator1()
        // {
        //     for (int i = 0; i < 10; i++)
        //     {
        //         Game.Log($"Enumerator1 ==== {i}");
        //         yield return null;
        //     }
        // }
    }
}