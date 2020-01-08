using System.Collections;

using System.Collections.Generic;

using UnityEngine;

using UnityEngine.EventSystems;

using UnityEngine.UI;

using Emgu.CV;

using Emgu.CV.CvEnum;

using K.EmguCVExtensions;

public class ProjectPreview : MonoBehaviour, IPointerClickHandler

{

    private RawImage targetRawImage;

    private int id;

    public int KolorIndex1;

    public int KolorIndex2;



    void Start()

    {

        targetRawImage = GetComponent<RawImage>();

    }



    public void OnPointerClick(PointerEventData p)

    {

        LoadCoroutine();

    }



    public void LoadPreview(int id)

    {

        this.id = id;

        StartCoroutine(LoadPreviewCoroutine(id));

    }



    private IEnumerator LoadPreviewCoroutine(int id)

    {

        if (!KProjectManager.CheckIfExists(id))

        {

            if (targetRawImage.texture != null) DestroyImmediate(targetRawImage.texture);

        }

        else

        {

            string filepreview = Application.persistentDataPath + "/preview" + id + ".png";

            WWW www = new WWW("file://" + filepreview);

            yield return www;

            if (targetRawImage.texture != null) DestroyImmediate(targetRawImage.texture);

            targetRawImage.texture = www.texture;

            www.Dispose();

			Debug.Log("Kauel: Fin IEnumrator");

        }

    }



    public void LoadCoroutine()

    {

        Alert.Singleton.ShowAlert(Alert.Message.LOADING, false, delegate ()

        {

            StartCoroutine(Load());

        });

    }



    private IEnumerator Load()

    {

        if (!KProjectManager.CheckIfExists(id))

        {

            Debug.LogError("Kauel: No existe el proyecto " + id);

        }

        else

        {

            string filecolor = Application.persistentDataPath + "/color" + id + ".png";

            string filemask = Application.persistentDataPath + "/mask" + id + ".png";

            string fileinfo = Application.persistentDataPath + "/info" + id + ".ceresita";



			Debug.Log("Kauel: Inicio Load");



            //Imagen de Color

            WWW www1 = new WWW("file://" + filecolor);

            yield return www1;

			Texture2D www1Tex = www1.texture;

            Kamera.Singleton.StartFile(www1Tex);

            www1.Dispose();



            //Mascara

            WWW www2 = new WWW("file://" + filemask);

            yield return www2;

			Texture2D www2Tex = www2.texture;

			Mat mask = www2Tex.GetNewMat();

            CvInvoke.Flip(mask, mask, FlipType.Vertical);

            Mat wall = Kamera.Singleton.CameraMask();

            mask.CopyTo(wall);

            wall.ApplyToTexture2D(Kamera.Singleton.OutputMaskTexture);

            mask.Dispose();

            www2.Dispose();



            //Color

            WWW www3 = new WWW("file://" + fileinfo);

            yield return www3;

            string json = www3.text;

            JsonUtility.FromJsonOverwrite(json, this);

            Kamera.Singleton.SelectedColors[0].CopyColorFromKolor(Kolores.FullList[KolorIndex1]);

            Kamera.Singleton.SelectedColors[1].CopyColorFromKolor(Kolores.FullList[KolorIndex2]);

            www3.Dispose();



            Alert.Singleton.CloseAlert(true);



            Kamera.Singleton.Canvas.ShowOnlyThisPanel(3);

        }

    }

}

