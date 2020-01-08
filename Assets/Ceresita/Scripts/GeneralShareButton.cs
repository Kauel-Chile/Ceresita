using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GeneralShareButton : MonoBehaviour
{
    public TMPro.TMP_InputField message;

    public void Share() {
        Alert.Singleton.ShowAlert(Alert.Message.LOADING, true, delegate () {
            Kamera.Singleton.PreprocessTextureFromRawImage();
            Debug.Log("texture: " + (Kamera.Singleton.RawTexture2D == null).ToString());

            GeneralSharing.Singleton.Share("", Kamera.Singleton.RawTexture2D);
//#if UNITY_ANDROID//            //GeneralSharing.Singleton.Share("", Kamera.Singleton.RawTexture2D);

//#elif UNITY_IOS || UNITY_IPAD
//            GeneralSharing.Singleton.Share("", Kamera.Singleton.RawTexture2D);
//#endif
            Alert.Singleton.CloseAlert(true);

        });
    }
}
