using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Facebook.Unity;

public class KFacebook : MonoBehaviour
{

    public KUIPanelManager kuiPanelManager;
    public TMPro.TMP_InputField message;

    public List<string> permsRead = new List<string>() { "public_profile", "email", "user_friends" };
    public List<string> permsPublish = new List<string>() { "publish_actions", "user_photos"};
    
    public delegate void OnLoggedEvent();
    OnLoggedEvent _OnLoggedEvent;


    // Awake function from Unity's MonoBehavior
    void Awake() {
        Debug.Log("Kauel: Awake Started");
        if (!FB.IsInitialized) {
            // Initialize the Facebook SDK
            Debug.Log("Kauel: Calling Init");
            FB.Init(InitCallback, OnHideUnity);
        } else {
            // Already initialized, signal an app activation App Event
            Debug.Log("Kauel: ActivateApp");
            FB.ActivateApp();
        }
    }

    private void InitCallback() {
        Debug.Log("Kauel: InitCallBack");
        if (FB.IsInitialized) {
            // Signal an app activation App Event
            Debug.Log("Kauel: ActivateApp");
            FB.ActivateApp();
        } else {
            Debug.Log("Kauel: Failed to Initialize the Facebook SDK");
        }
    }

    private void OnHideUnity(bool isGameShown) {
        if (!isGameShown) {
            // Pause the game - we will need to hide
            //Time.timeScale = 0;
        } else {
            // Resume the game - we're getting focus again
            //Time.timeScale = 1;
        }
    }

    public void LogInRead()
	{
		Debug.Log ("Kauel: LogInRead");

		if (!FB.IsLoggedIn) {

			Debug.Log ("Kauel: No Loged");
			Alert.Singleton.ShowAlert (Alert.Message.LOADING, false);
			FB.LogInWithReadPermissions (permsRead, LoginAuthCallback);

		} else {
			Debug.Log ("Kauel: Loged");

			// AccessToken class will have session details
			var aToken = Facebook.Unity.AccessToken.CurrentAccessToken;
			// Print current access token's User ID
			Debug.Log("UserId: " + aToken.UserId);
			// Print current access token's granted permissions
			foreach (string perm in aToken.Permissions)
			{
				Debug.Log("perm: " + perm);
			}

			CeresitaWebService.Singleton.user.facebookId = aToken.UserId;

			CeresitaWebService.Singleton.GetUserByFacebook(CeresitaWebService.Singleton.user.facebookId, delegate(CeresitaWebService.WEBSERVICE_RETURN ret1)
				{
					if(ret1 == CeresitaWebService.WEBSERVICE_RETURN.USER_NOT_FOUND)
					{
						CeresitaWebService.Singleton.CreateUser(CeresitaWebService.AUTHENTICATION_TYPE.FACEBOOK, CeresitaWebService.Singleton.user.facebookId, delegate (CeresitaWebService.WEBSERVICE_RETURN ret2)
							{
								if(ret2 == CeresitaWebService.WEBSERVICE_RETURN.USER_CREATED)
								{
									FB.API("/me?fields=name,email", HttpMethod.GET, UserInfoRequestCallback);
								}
								else
								{
									CeresitaWebService.Singleton.user.facebookId = "";
									Alert.Singleton.CloseAlert(true);
									Alert.Singleton.ShowAlert(Alert.Message.LOGIN_FAILED);
								}
							});
					}
					else
					{
						FB.API("/me?fields=name,email", HttpMethod.GET, UserInfoRequestCallback);
					}
				});

		}
	}

    public void LogInWithCallback(OnLoggedEvent _OnLoggedEvent)
    {
        this._OnLoggedEvent = _OnLoggedEvent;
        FB.LogInWithReadPermissions(permsRead, LoginAuthCallback);
    }

    public void LogInPublish()
    {
        Debug.Log("Kauel: LogInPublish");
        Alert.Singleton.ShowAlert(Alert.Message.LOADING, false);
        Debug.Log("Kauel: Alert.Singleton.ShowAler Passed");
        FB.LogInWithPublishPermissions(permsPublish, AuthCallback);
    }

    public void PhotoToFacebook() {

        Debug.Log("Kauel: PhotoToFacebook");
		byte[] screenshot = Kamera.Singleton.EncodedImageAsPNG;

        if(screenshot != null) {
            var wwwForm = new WWWForm();
            wwwForm.AddBinaryData("image", screenshot, "Ceresita.png");
            wwwForm.AddField("message", message.text);
            FB.API("me/photos", HttpMethod.POST, APICallback, wwwForm);
        } else {
            Debug.Log("Kauel: Screenshot is null");
        }
    }

    private void APICallback(IResult result) {

        bool bSuccess = false;

        if (result == null) {
            Debug.Log("Kauel: no result");
            return;
        }

        // Some platforms return the empty string instead of null.
        if (!string.IsNullOrEmpty(result.Error)) Debug.Log("Kauel: Error " + result.ToString());
        else if (result.Cancelled) Debug.Log("Kauel: Cancelled");
        else if (!string.IsNullOrEmpty(result.RawResult))
        {
            Debug.Log("Kauel: Sucess, check log" + result.RawResult);
            bSuccess = true;
        } 
        else Debug.Log("Kauel: empty");

        if(bSuccess)
        {
            Alert.Singleton.ShowAlert(Alert.Message.IMAGE_SHARED);
        }
        else
        {
            Alert.Singleton.ShowAlert(Alert.Message.ERROR);
        }
    }

    private void AuthCallback(ILoginResult result) {
        Debug.Log("Kauel: AuthCallback");
        if (FB.IsLoggedIn) {
            Debug.Log("Kauel: FB.IsLoggedIn");
            // AccessToken class will have session details
            var aToken = Facebook.Unity.AccessToken.CurrentAccessToken;
            if(aToken != null) {
                // Print current access token's User ID
                Debug.Log("Kauel: UserId=" + aToken.UserId);
                // Print current access token's granted permissions
                foreach (string perm in aToken.Permissions) {
                    Debug.Log("Kauel: Permission: " + perm);
                }
            } else {
                Debug.Log("Kauel: aToken = NULL");
            }

            PhotoToFacebook();

        } else {
            Debug.Log("Kauel: User cancelled login");
        }
    }

    private void LoginAuthCallback(ILoginResult result)
    {
        if (FB.IsLoggedIn)
        {
            // AccessToken class will have session details
            var aToken = Facebook.Unity.AccessToken.CurrentAccessToken;
            // Print current access token's User ID
            Debug.Log("UserId: " + aToken.UserId);
            // Print current access token's granted permissions
            foreach (string perm in aToken.Permissions)
            {
                Debug.Log("perm: " + perm);
            }

            CeresitaWebService.Singleton.user.facebookId = aToken.UserId;

            CeresitaWebService.Singleton.GetUserByFacebook(CeresitaWebService.Singleton.user.facebookId, delegate(CeresitaWebService.WEBSERVICE_RETURN ret1)
            {
                if(ret1 == CeresitaWebService.WEBSERVICE_RETURN.USER_NOT_FOUND)
                {
                    CeresitaWebService.Singleton.CreateUser(CeresitaWebService.AUTHENTICATION_TYPE.FACEBOOK, CeresitaWebService.Singleton.user.facebookId, delegate (CeresitaWebService.WEBSERVICE_RETURN ret2)
                    {
                        if(ret2 == CeresitaWebService.WEBSERVICE_RETURN.USER_CREATED)
                        {
                            FB.API("/me?fields=name,email", HttpMethod.GET, UserInfoRequestCallback);
                        }
                        else
                        {
                            CeresitaWebService.Singleton.user.facebookId = "";
                            Alert.Singleton.CloseAlert(true);
                            Alert.Singleton.ShowAlert(Alert.Message.LOGIN_FAILED);
                        }
                    });
                }
                else
                {
                    FB.API("/me?fields=name,email", HttpMethod.GET, UserInfoRequestCallback);
                }
            });
        }
        else
        {
            Alert.Singleton.CloseAlert(true);
            Alert.Singleton.ShowAlert(Alert.Message.LOGIN_FAILED);
            Debug.Log("Kauel: User cancelled login");
        }
    }

    private void UserInfoRequestCallback(IGraphResult result)
    {
        try
        {
            CeresitaWebService.Singleton.user.name = result.ResultDictionary["name"].ToString();
        }
        catch
        {

        }

        try
        {
            if(CeresitaWebService.Singleton.user.email == null)
            {
				
				CeresitaWebService.Singleton.user.email = result.ResultDictionary["email"].ToString();
			}
            
        }
        catch
        {

        }

        CeresitaWebService.Singleton.UpdateUser(delegate (CeresitaWebService.WEBSERVICE_RETURN ret)
        {
			AccountManager.Singleton.UpdateInformation();
            Alert.Singleton.CloseAlert(true);

            if (_OnLoggedEvent != null)
            {
                _OnLoggedEvent();
                _OnLoggedEvent = null;
            }
            else
            {
                kuiPanelManager.ShowOnlyThisPanel(2);
                CeresitaWebService.Singleton.UpdateLastActivity();

                //Guarda la última forma de ingreso, para que la siguiente vez que se ingresa
                //a la aplicación se ingrese automaticamente.
                PlayerPrefs.SetString("LOGIN_MODE", "FACEBOOK");
                PlayerPrefs.SetString("FACEBOOK_ID", CeresitaWebService.Singleton.user.facebookId);
            }
        });
    }

    public void LogOut() {
        FB.LogOut();
    }

    // Use this for initialization
    void Start () { }
	
	// Update is called once per frame
	void Update () { }
}
