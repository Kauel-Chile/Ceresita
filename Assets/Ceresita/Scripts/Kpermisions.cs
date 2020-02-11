using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.SceneManagement;

public class Kpermisions : MonoBehaviour
{

    bool isDialogPermission = false;
    bool isStartPermissions = true;

    // Start is called before the first frame update
    void Start()
    {
        if (Permission.HasUserAuthorizedPermission(Permission.Camera) && Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
        {
            Debug.Log("KAUEL: Los permisos ya están, vamos a la siguiente escena");
            LoadA("Scene01");
        }
        else
        {
            Debug.Log("KAUEL: no hay permisos aún");
            isStartPermissions = false;
        }

    }

    // Update is called once per frame
    void Update()
    {
        if(isDialogPermission)
        {
            isDialogPermission = false;

            Debug.Log("KAUEL: Vamos a la siguiente escena");
            LoadA("Scene01");
        }

        if(!isStartPermissions)
        {
            isStartPermissions = true;
            AndroidRuntimePermissions.Permission[] result = AndroidRuntimePermissions.RequestPermissions("android.permission.WRITE_EXTERNAL_STORAGE", "android.permission.CAMERA");
            if (result[0] == AndroidRuntimePermissions.Permission.Granted && result[1] == AndroidRuntimePermissions.Permission.Granted)
            {
                isDialogPermission = true;
                Debug.Log("KAUEL: permission(s) are granted by dialog");
                CargarEscena("Scene01");
            }
            else
            {
                Debug.Log("KAUEL: Some permission(s) are not granted, reaload same scene");
                LoadA("ScenePreload");
            }
        }

    }

    public void CargarEscena (string _nombre)
    {
        Debug.Log("KAUEL: Llegué aki");
        StartCoroutine(LoadSceneAD("Scene01", 1f));
   
    }

    IEnumerator LoadSceneAD(string scenename,float delayTime)
    {
        Debug.Log("KAUEL: sceneName to load: " + scenename + "delay:" + delayTime);
        yield return new WaitForSeconds(delayTime);
        SceneManager.LoadScene(scenename);
    }
    public void LoadA(string scenename)
    {
        Debug.Log("KAUEL: sceneName to load: " + scenename);
        SceneManager.LoadScene(scenename);
    }
}
