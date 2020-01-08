using System.Collections;

using System.Collections.Generic;

using UnityEngine;

using UnityEngine.UI;

using System.IO;

using Emgu.CV;

using Emgu.CV.CvEnum;



public class KProjectManager: MonoBehaviour

{

    public ProjectPreview[] projectPreviewList;



    public static int LastId = -1;

    public static int maxNumProjectsAllowed = 100;



    public int id = 0;

    string filecolor;

    string filemask;

    string filepreview;

    public int KolorIndex1 = 0;

    public int KolorIndex2 = 0;



    public void Save()

    {

        Alert.Singleton.ShowAlert(Alert.Message.SAVING_FILE, false, delegate

        {

            NextProject();



            //Codifica a JPG

            Kamera.Singleton.PreprocessTextureFromRawImage();

            filecolor = Application.persistentDataPath + "/color" + id + ".png";

            filemask = Application.persistentDataPath + "/mask" + id + ".png";

            //filepreview = Application.persistentDataPath + "/preview" + id + ".jpg";

			filepreview = Application.persistentDataPath + "/preview" + id + ".png";

            Debug.Log("Kauel: Saved to " + filepreview);

            KolorIndex1 = Kamera.Singleton.SelectedColors[0].SelectedKolor.index;

            KolorIndex2 = Kamera.Singleton.SelectedColors[1].SelectedKolor.index;

            string fileinfo = Application.persistentDataPath + "/info" + id + ".ceresita";



            //Guarda las Texturas

			File.WriteAllBytes(filepreview, Kamera.Singleton.EncodedImageAsPNG);

            byte[] color = Kamera.Singleton.OutputTexture.EncodeToPNG();

            File.WriteAllBytes(filecolor, color);

            color = null;

            byte[] mask = Kamera.Singleton.OutputMaskTexture.EncodeToPNG();

            File.WriteAllBytes(filemask, mask);

            mask = null;



            //Guarda la info

            string info = JsonUtility.ToJson(this, true);

            File.WriteAllText(fileinfo, info);



            Alert.Singleton.ShowAlert(Alert.Message.FILE_SAVED);

        });

    }



    public void Load(int startingIndex)

    {

        for(int i = 0; i < projectPreviewList.Length; i++) {

            projectPreviewList[i].LoadPreview(startingIndex + i);

        }

    }



    /// <summary>

    /// Verifica que existan los archivos necesario para abrir un proyecto

    /// </summary>

    /// <param name="id"></param>

    /// <returns>Retorna true en caso de que todos los archivos existan, false en caso de que algun archivo no exista.</returns>

    public static bool CheckIfExists(int id) {

        string afilecolor = Application.persistentDataPath + "/color" + id + ".png";

        string afilemask = Application.persistentDataPath + "/mask" + id + ".png";

        //string afilepreview = Application.persistentDataPath + "/preview" + id + ".jpg";

		string afilepreview = Application.persistentDataPath + "/preview" + id + ".png";

        string afileinfo = Application.persistentDataPath + "/info" + id + ".ceresita";

        if (!File.Exists(afilecolor)) return false;

        if (!File.Exists(afilemask)) return false;

        if (!File.Exists(afilepreview)) return false;

        if (!File.Exists(afileinfo)) return false;

        return true;

    }



    /// <summary>

    /// Retorna la cantidad de proyectos existentes y actualiza el valor del último proyecto.

    /// </summary>

    public static int SearchForProjects() {

        int count = 0;

        for(int i = 0; i < maxNumProjectsAllowed; i++) {

            if (CheckIfExists(i)) {

                LastId = i;

                count++;

            }

        }

        return count;

    }



    public void NextProject() {

        LastId++;

        if (LastId >= maxNumProjectsAllowed) LastId = 0;

        id = LastId;

    }

}

