using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

public class AndroidPermissions : MonoBehaviour
{

    bool isPermission = false;
    bool isPermission2 = false;

    void Update ()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!isPermission)
        {
            //AndroidRuntimePermissions.Permission externalStorage = AndroidRuntimePermissions.RequestPermission("android.permission.WRITE_EXTERNAL_STORAGE");
            //AndroidRuntimePermissions.Permission camera = AndroidRuntimePermissions.RequestPermission("android.permission.CAMERA");
            //if (camera == AndroidRuntimePermissions.Permission.Granted && camera == AndroidRuntimePermissions.Permission.Granted)
            //    Debug.Log("We have permission to access external storage!");

            //Requesting WRITE_EXTERNAL_STORAGE and CAMERA permissions simultaneously
            AndroidRuntimePermissions.Permission[] result = AndroidRuntimePermissions.RequestPermissions("android.permission.WRITE_EXTERNAL_STORAGE", "android.permission.CAMERA");
            if (result[0] == AndroidRuntimePermissions.Permission.Granted && result[1] == AndroidRuntimePermissions.Permission.Granted)
            {
                isPermission = true;
                Debug.Log("KAUEL: permission(s) are granted");
            }
            else
            {
                Debug.Log("KAUEL: Some permission(s) are not granted...");
            }


            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                Permission.RequestUserPermission(Permission.Camera);
                Debug.Log("KAUEL: Request Permission");
            }
            else
            {
                Debug.Log("KAUEL: Permission granted");
            }

            isPermission2 = true;
        }
#endif
#if PLATFORM_ANDROID
        if (!isPermission2)
        {

            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                Permission.RequestUserPermission(Permission.Camera);
                Debug.Log("KAUEL: Request Permission");
            }
            else
            {
                Debug.Log("KAUEL: Permission granted");
            }

            isPermission2 = true;
        }

#endif
    }
}
