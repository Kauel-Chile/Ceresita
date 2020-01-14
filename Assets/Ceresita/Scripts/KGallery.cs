using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using K.EmguCVExtensions;

public class KGallery : MonoBehaviour
{

    public KUIPanelManager kuiPanelManager;

    public RawImage RawImageEdit;

    public ImageProcessing imageProcessing;

    public Kamera TheKamera = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PickImage(int maxSize)
    {
        NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path) =>
        {
            Debug.Log("Image path: " + path);
            if (path != null)
            {
                // Create Texture from selected image
                Texture2D tex = NativeGallery.LoadImageAtPath(path, maxSize);
                if (tex == null)
                {
                    Debug.Log("Couldn't load texture from " + path);
                    return;
                }

                Alert.Singleton.ShowAlert(Alert.Message.LOADING, false, delegate () {


                    Debug.Log("Kauel: w:" + tex.width + "h:" + tex.height);

                    int w = tex.width;

                    int h = tex.height;

                    if (w > h)
                    {

                        w = 1024;

                        h = tex.height * w / tex.width;

                    }
                    else
                    {

                        h = 1024;

                        w = tex.width * h / tex.height;

                    }

                    Texture2D resizedTex = tex.NewResizedTexture(w, h);

                    //   Destroy(tex); //Esta linea está a prueba.

                    RawImageEdit.texture = resizedTex;

                    Alert.Singleton.CloseAlert(true);

                    TheKamera.StartFile(resizedTex);

                    kuiPanelManager.ShowOnlyThisPanel(3);



                });
            }
        }, "Select a PNG image", "image/png");

        Debug.Log("Permission result: " + permission);
    }
}
