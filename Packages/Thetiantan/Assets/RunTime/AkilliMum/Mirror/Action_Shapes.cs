using AkilliMum.Standard.Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Camera))]
public class Action_Shapes : MonoBehaviour
{
    private CameraShade mainCameraShade;


    private Shade[] _cshades;
    public float duaToWait = 3f;
    private GameObject[] shads;


    private void Awake()
    {
        mainCameraShade = GetComponent<CameraShade>();
        ///
        SceneManager.sceneLoaded += AwakeAsync;
    }

    private void AwakeAsync(Scene scene, LoadSceneMode sceneMode)
    {
        if (!mainCameraShade) return;
       
        mainCameraShade.Shades = null;


        mainCameraShade.EnableWaves = false;
        mainCameraShade.enabled = false;

        StartCoroutine(FindShades());
    }

    IEnumerator FindShades()
    {
        float duas = 0;
        shads = GameObject.FindGameObjectsWithTag("Mirror");

        while (shads.Length == 0 && duas < duaToWait)
        {
            yield return null;

            duas += Time.deltaTime;
            shads = GameObject.FindGameObjectsWithTag("Mirror");

        }
        if (mainCameraShade && shads.Length != 0)
        {
            _cshades = new Shade[shads.Length];
            for (int i = 0; i < shads.Length; i++)
            {
                Shade s = new Shade();
                s.ObjectToShade = shads[i];
                s.MaterialToChange = shads[i].GetComponent<MeshRenderer>().material;
                _cshades[i] = s;
            }

            mainCameraShade.Shades = _cshades;

            mainCameraShade.enabled = true;

            mainCameraShade.EnableWaves = true;
        }

    }

}
