using DemoX.Framework.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DemoX.Framework
{
    public class SceneNameTest : MonoBehaviour
    {
        [SceneField] public string _initScene;

        public void Update()
        {
            int activeSceneIndex = SceneManager.sceneCount > 1 ? 1 : 0;
            bool result = string.Equals(_initScene, SceneManager.GetSceneAt(activeSceneIndex).name);

            Scene s = SceneManager.GetSceneAt(activeSceneIndex);
            Game.Log($"{result} {_initScene} {s.name} {s.path}");
        }
    }
}