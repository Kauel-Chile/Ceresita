using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class EmailSharing : MonoBehaviour
{
    public KUIPanelFader kuiPanelFader;
    public KUIPanelFader addEmailPanel;
    public RawImage target;

    public TMPro.TMP_InputField emailInputField;
    public TMPro.TMP_InputField message;

    public void ShareToMail()
    {
        kuiPanelFader.FadeOutAndDesactivate();

        if(CeresitaWebService.Singleton.user.email != null)
        {
            UploadImage();
        }
        else
        {
            addEmailPanel.ActivateWithFadeIn();
        }
    }

    public void UpdateEmail()
    {
        string emailString = emailInputField.text;
        bool isEmail = Regex.IsMatch(emailString, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);

        if(isEmail)
        {
            CeresitaWebService.Singleton.user.email = emailString;

            CeresitaWebService.Singleton.UpdateUser(delegate(CeresitaWebService.WEBSERVICE_RETURN ret)
            {
                if (ret == CeresitaWebService.WEBSERVICE_RETURN.USER_UPDATED)
                {
                    addEmailPanel.FadeOutAndDesactivate();
                    UploadImage();
                }
                else
                {
                    Alert.Singleton.ShowAlert(Alert.Message.USER_EXISTS);
                }
            });
        }
        else
        {
            Alert.Singleton.ShowAlert(Alert.Message.INVALID_EMAIL);
        }
    }

    void UploadImage()
    {
        Alert.Singleton.ShowAlert(Alert.Message.LOADING, false, delegate ()
        {
            byte[] imageAsByteArr = Kamera.Singleton.EncodedImageAsPNG;

            Alert.Singleton.ShowAlert(Alert.Message.IMAGE_SENDING);

            CeresitaWebService.Singleton.SendImageToMail(message.text, imageAsByteArr, delegate (CeresitaWebService.WEBSERVICE_RETURN ret)
            {
                if (ret == CeresitaWebService.WEBSERVICE_RETURN.OK)
                {
                    //Alert.Singleton.ShowAlert(Alert.Message.IMAGE_SENT);
                }
                else
                {
                    //Alert.Singleton.ShowAlert(Alert.Message.ERROR);
                }
            });
        });
    }
}
