using UnityEngine;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;


public class GeneralSharing : MonoBehaviour
{
	#region PUBLIC_VARIABLES
	public Texture2D MyImage;
    public string message;
	#endregion
	
	#region UNITY_DEFAULT_CALLBACKS
	public void OnEnable ()
	{
		ScreenshotHandler.ScreenshotFinishedSaving += ScreenshotSaved;
	}
	
	void OnDisable ()
	{
		ScreenshotHandler.ScreenshotFinishedSaving -= ScreenshotSaved;
	}
	#endregion
	
	#region DELEGATE_EVENT_LISTENER
	void ScreenshotSaved ()
	{
		#if UNITY_IOS || UNITY_IPAD
		GeneralSharingiOSBridge.ShareTextWithImage (ScreenshotHandler.savedImagePath, message);
		#endif
	}
	#endregion
	
	#region CO_ROUTINES
	IEnumerator ShareAndroidText ()
	{
		yield return new WaitForEndOfFrame ();
		#if UNITY_ANDROID
		byte[] bytes = MyImage.EncodeToPNG();
		string path = Application.persistentDataPath + "/MyImage.png";
		File.WriteAllBytes(path, bytes);
		
		AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
		AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent");
		
		intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
		intentObject.Call<AndroidJavaObject>("setType", "image/*");
		intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_SUBJECT"), "Text Sharing ");
		intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TITLE"), "Text Sharing ");
		intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), message);
		
		AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");
		
		AndroidJavaObject fileObject = new AndroidJavaObject("java.io.File", path);// Set Image Path Here
		
		AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>("fromFile", fileObject);
		
		//			string uriPath =  uriObject.Call<string>("getPath");
		bool fileExist = fileObject.Call<bool>("exists");
		Debug.Log("File exist : " + fileExist);
		AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");
		currentActivity.Call("startActivity", intentObject);
		
		#endif
	}
    /// <summary>
    ///  Compartir imagen utilizando Native share plugin
    /// </summary>
    /// <returns></returns>
    private IEnumerator NativeShareCoroutine()
    {
        yield return new WaitForEndOfFrame();

        byte[] bytes = MyImage.EncodeToPNG();

        string path = Application.persistentDataPath + "/MyImage.png";

        File.WriteAllBytes(path, bytes);

        new NativeShare().AddFile(path).SetSubject("Ceresita").SetText("Compara el antes y después\n¿Té encantó?\nHazlo realidad").Share();
    }

    IEnumerator SaveAndShare ()
	{
		yield return new WaitForEndOfFrame ();
		#if UNITY_ANDROID
		
		byte[] bytes = MyImage.EncodeToPNG();
		string path = Application.persistentDataPath + "/MyImage.png";
		File.WriteAllBytes(path, bytes);
		
		AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
		AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent");

        AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");

        AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");

        intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
		intentObject.Call<AndroidJavaObject>("setType", "image/*");
		intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_SUBJECT"), "Ceresita");
		intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TITLE"), "");
        intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), "Compara el antes y después\n¿Té encantó?\nHazlo realidad");
		
		AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");
		AndroidJavaClass fileClass = new AndroidJavaClass("java.io.File");
		
		AndroidJavaObject fileObject = new AndroidJavaObject("java.io.File", path);// Set Image Path Here
		
		AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>("fromFile", fileObject);

        //			string uriPath =  uriObject.Call<string>("getPath");

        bool fileExist = fileObject.Call<bool>("exists");
		Debug.Log("File exist : " + fileExist);
		if (fileExist)
			intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_STREAM"), uriObject);
        AndroidJavaObject jChooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", intentObject, "Compartir");
        currentActivity.Call("startActivity", jChooser);
#endif

    }
	#endregion
	
	#region BUTTON_CLICK_LISTENER
	
	public void OnShareSimpleText ()
	{
		#if UNITY_ANDROID
		StartCoroutine (ShareAndroidText ());
		#elif UNITY_IOS || UNITY_IPAD
		GeneralSharingiOSBridge.ShareSimpleText (message);
		#endif
	}
	
	public void OnShareTextWithImage ()
	{
		Debug.Log ("Media Share");
		#if UNITY_ANDROID
		StartCoroutine (SaveAndShare ());
		#elif UNITY_IOS || UNITY_IPAD
		byte[] bytes = MyImage.EncodeToPNG ();
		string path = Application.persistentDataPath + "/MyImage.png";
		File.WriteAllBytes (path, bytes);
		string path_ = "MyImage.png";
		
		StartCoroutine (ScreenshotHandler.Save (path_, "Media Share", true));
		#endif
	}
    #endregion

    private static GeneralSharing singleton;

    public static GeneralSharing Singleton
    {
        get
        {
            if(singleton == null)
            {
                singleton = FindObjectOfType<GeneralSharing>();
            }

            return singleton;
        }
    }

    public void Share(string message, Texture2D image = null)
    {
        if(image != null)
        {
            MyImage = image;





#if UNITY_ANDROID            
		//StartCoroutine (SaveAndShare ());        StartCoroutine(NativeShareCoroutine());
#elif UNITY_IOS || UNITY_IPAD
		byte[] bytes = MyImage.EncodeToPNG ();
		string path = Application.persistentDataPath + "/MyImage.png";
		File.WriteAllBytes (path, bytes);
		string path_ = "MyImage.png";
		
		StartCoroutine (ScreenshotHandler.Save (path_, "Media Share", true));
#endif
        }
    }
}
