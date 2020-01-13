using System.Collections;

using System.Collections.Generic;

using Tastybits.NativeGallery;

using UnityEngine;

using UnityEngine.UI;

using Emgu.CV;

using System.Drawing;

using K.EmguCVExtensions;

public class MobileGallery : MonoBehaviour {



    private static MobileGallery singleton;



    public static MobileGallery Singleton {

        get {

            if (singleton == null) {

                singleton = FindObjectOfType<MobileGallery>();

            }

            return singleton;

        }

    }



    public KUIPanelManager kuiPanelManager;

    public RawImage RawImageEdit;

    public ImageProcessing imageProcessing;

    public Kamera TheKamera = null;



    public void OpenGallery() {

        Debug.Log("Kauel: OpenGallery External");

        ImagePicker.OpenGallery((Texture2D tex, ExifOrientation orientation) => {

            Debug.Log("Kauel: OpenGallery Internal");

            if(tex != null)

            {

                Alert.Singleton.ShowAlert(Alert.Message.LOADING, false, delegate () {


                    Debug.Log("Kauel: w:"+tex.width + "h:"+ tex.height );

                    int w = tex.width;

                    int h = tex.height;

                    if (w > h) {

                        w = 1024;

                        h = tex.height * w / tex.width;

                    } else {

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



		},true,ImagePickerType.UIImagePickerControllerSourceTypePhotoLibrary );

    }



}

