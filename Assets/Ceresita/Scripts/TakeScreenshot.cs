using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TakeScreenshot : MonoBehaviour
{
    public Kamera kamera;
    public ImageProcessing imageProcessing;
    public RawImage paintingRawImage;
    public KUIPanelManager kuiPanelManager;

    public void TakeScreenshotAction()
    {
        Alert.Singleton.ShowAlert(Alert.Message.LOADING, false, delegate()
        {
            imageProcessing.ProcessImage(kamera.OutputTexture, delegate (Texture2D tex)
            {
                paintingRawImage.texture = tex;
                Alert.Singleton.CloseAlert(true);
                kuiPanelManager.ShowOnlyThisPanel(3);
            });
        });
    }
}
