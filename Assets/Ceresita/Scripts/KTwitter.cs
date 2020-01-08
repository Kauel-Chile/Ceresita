using System;

using System.Collections;

using System.Collections.Generic;

using Twitter;

using UnityEngine;

using UnityEngine.UI;



public class KTwitter : MonoBehaviour

{

    public string consumerKey;

    public string consumerSecret;

    string token;



    bool bShareImage = true;



    public RawImage target;

    public KUIPanelFader panelSocial;

    public KUIPanelFader panelTwitterPINRequest;

    public KUIPanelFader panelTwitterOnlyPINRequest;



    public KUIPanelFader twitterLinkPanel;



    public TMPro.TMP_InputField tweet;



    public void ShareImage()

    {

        panelSocial.FadeOutAndDesactivate();

        

        if(CeresitaWebService.Singleton.user.twitterId == null ||

            CeresitaWebService.Singleton.user.twitterId.Length <= 0)

        {

            panelTwitterPINRequest.ActivateWithFadeIn();

            Alert.Singleton.ShowAlert(Alert.Message.LOADING, false);

            StartCoroutine(Twitter.API.GetRequestToken(consumerKey,

                                                       consumerSecret,

                                                       new Twitter.RequestTokenCallback(this.OnRequestTokenCallback)));

        }

        else

        {

            UploadImage();

        }

    }



    public void Login()

    {

        panelTwitterOnlyPINRequest.ActivateWithFadeIn();

        twitterLinkPanel.FadeOutAndDesactivate();



        Alert.Singleton.ShowAlert(Alert.Message.LOADING, false);

        StartCoroutine(Twitter.API.GetRequestToken(consumerKey,

                                                   consumerSecret,

                                                   new Twitter.RequestTokenCallback(this.OnRequestTokenCallback)));

    }



    private void OnRequestTokenCallback(bool success, RequestTokenResponse response)

    {

        if(success)

        {

            Alert.Singleton.CloseAlert(true);

            token = response.Token;

            Twitter.API.OpenAuthorizationPage(response.Token);

        }

        else

        {

            Alert.Singleton.ShowAlert(Alert.Message.ERROR);

        }

    }



    public void UpdatePIN(TMPro.TMP_InputField pin)

    {

        panelTwitterPINRequest.FadeOutAndDesactivate();

        Alert.Singleton.ShowAlert(Alert.Message.LOADING, false);

        StartCoroutine(Twitter.API.GetAccessToken(consumerKey, 

                                                  consumerSecret, 

                                                  token, 

                                                  pin.text,

                                                  new Twitter.AccessTokenCallback(this.OnAccessTokenCallback)));

    }



    public void OnlyUpdatePIN(TMPro.TMP_InputField pin)

    {

        panelTwitterOnlyPINRequest.FadeOutAndDesactivate();

        Alert.Singleton.ShowAlert(Alert.Message.LOADING, false);

        StartCoroutine(Twitter.API.GetAccessToken(consumerKey,

                                                  consumerSecret,

                                                  token,

                                                  pin.text,

                                                  new Twitter.AccessTokenCallback(this.OnlyOnAccessTokenCallback)));

    }



    private void OnlyOnAccessTokenCallback(bool success, AccessTokenResponse response)

    {

        if (success)

        {

            CeresitaWebService.Singleton.user.twitterId = response.UserId + "," + response.ScreenName + "," + response.Token + "," + response.TokenSecret;

            CeresitaWebService.Singleton.UpdateUser(delegate (CeresitaWebService.WEBSERVICE_RETURN ret)

            {

                if (ret == CeresitaWebService.WEBSERVICE_RETURN.USER_UPDATED)

                {

                    Alert.Singleton.CloseAlert(true);

                    AccountManager.Singleton.UpdateInformation();

                }

                else

                {

                    Alert.Singleton.ShowAlert(Alert.Message.ERROR);

                }

            });

        }

        else

        {

            Alert.Singleton.ShowAlert(Alert.Message.ERROR);

        }

    }



    private void OnAccessTokenCallback(bool success, AccessTokenResponse response)

    {

        if (success)

        {

            CeresitaWebService.Singleton.user.twitterId = response.UserId + "," + response.ScreenName + "," + response.Token + "," + response.TokenSecret;

            CeresitaWebService.Singleton.UpdateUser(delegate (CeresitaWebService.WEBSERVICE_RETURN ret)

            {

                if(ret == CeresitaWebService.WEBSERVICE_RETURN.USER_UPDATED)

                {

                    AccountManager.Singleton.UpdateInformation();

                    ShareImage();

                }

                else

                {

                    Alert.Singleton.ShowAlert(Alert.Message.ERROR);

                }

            });

        }

        else

        {

            Alert.Singleton.ShowAlert(Alert.Message.ERROR);

        }

    }



    private void UploadImage()

    {

        Alert.Singleton.ShowAlert(Alert.Message.LOADING, false);



        //StartCoroutine(SendTweet(tweet.text, Kamera.Singleton.EncodedImage));

		StartCoroutine(SendTweet(tweet.text, Kamera.Singleton.EncodedImageAsPNG));

    }



    IEnumerator SendTweet(string message, byte[] path)

    {

        AccessTokenResponse accessToken = GetTwitterAccess();



        WWWForm form = new WWWForm();

        form.AddBinaryData("IMAGE", path, CeresitaWebService.Singleton.user.id + ".png", "text/plain");

        form.AddField("CONSUMER_KEY", consumerKey);

        form.AddField("CONSUMER_SECRET", consumerSecret);

        form.AddField("TOKEN_KEY", accessToken.Token);

        form.AddField("TOKEN_SECRET", accessToken.TokenSecret);

        form.AddField("MESSAGE", message);

        form.AddField("ID", CeresitaWebService.Singleton.user.id);



        WWW www = new WWW(CeresitaWebService.Singleton.webServiceURL + "/share_image_using_twitter.php", form);

        yield return www;



        if(www.error == null) {

            //SimpleJSON.JSONNode n = SimpleJSON.JSON.Parse(www.text);

            Alert.Singleton.ShowAlert(Alert.Message.IMAGE_SHARED);

        } else {

            Alert.Singleton.ShowAlert(Alert.Message.ERROR);

        }

    }



    private AccessTokenResponse GetTwitterAccess()

    {

        AccessTokenResponse response = new AccessTokenResponse();


        /*
        string[] keys = CeresitaWebService.Singleton.user.twitterId.Split(",");

        response.UserId = keys[0];

        response.ScreenName = keys[1];

        response.Token = keys[2];

        response.TokenSecret = keys[3];

        */

        return response;

    }



    private void OnPostTweet(bool success)

    {

        if(success)

        {

            Alert.Singleton.ShowAlert(Alert.Message.IMAGE_SHARED);

        }

        else

        {

            Alert.Singleton.ShowAlert(Alert.Message.ERROR);

        }

    }



    public void LinkUnlinkTwitter()

    {

        if(CeresitaWebService.Singleton.user.twitterId == null ||

           CeresitaWebService.Singleton.user.twitterId.Length == 0)

        {

            twitterLinkPanel.ActivateWithFadeIn();

        }

    }

}

