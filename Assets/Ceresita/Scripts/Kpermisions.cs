using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.SceneManagement;

public class Kpermisions : MonoBehaviour
{

    bool isPermission = false;

    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {

        AndroidRuntimePermissions.Permission[] result = AndroidRuntimePermissions.RequestPermissions("android.permission.WRITE_EXTERNAL_STORAGE", "android.permission.CAMERA");
        if (result[0] == AndroidRuntimePermissions.Permission.Granted && result[1] == AndroidRuntimePermissions.Permission.Granted)
        {
            isPermission = true;
            Debug.Log("KAUEL: permission(s) are granted preload");
            LoadA("Scene01");
        }
        else
        {
            Debug.Log("KAUEL: Some permission(s) are not granted preload...");
            LoadA("ScenePreload");
        }



    }

    public void CheckPermisions ()
    {
   
    }

    public void LoadA(string scenename)
    {
        Debug.Log("sceneName to load: " + scenename);
        SceneManager.LoadScene(scenename);
    }
}
