using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;

public class Login : MonoBehaviour
{
    public KUIPanelManager kuiPanelManager;

    //Login with email
    public TMPro.TMP_InputField emailInputField;
    public TMPro.TMP_InputField passwordInputField;

	//Login with email under Facebook
	public TMPro.TMP_InputField email2InputField;
	public TMPro.TMP_InputField password2InputField;

    //register
	public TMPro.TMP_InputField newNameInputField;
    public TMPro.TMP_InputField newEmailInputField;
    public TMPro.TMP_InputField newPasswordInputField;
    public TMPro.TMP_InputField confirmNewPasswordInputField;
    public UnityEngine.UI.Dropdown regionListDropdown;    public UnityEngine.UI.Dropdown countryListDropdown;

    public TMPro.TMP_InputField passwordRecoveryInputField;
    public TMPro.TMP_InputField passwordRecoveryCodeInputField;
    public TMPro.TMP_InputField passwordRecoveryNewPassInputField;

    public KUIPanelFader passwordRecoveryPass01;
    public KUIPanelFader passwordRecoveryPass02;
    public KUIPanelFader passwordRecoveryPass03;
	public KUIPanelFader mailLinkPanel;


    bool loadRegionList = true;    //Lista de regiones disponibles en el dropdown, una vez seleccionado el pais.    private List<CeresitaWebService.Region> regionToSelect = new List<CeresitaWebService.Region>();

    void Start()
    {
        if(PlayerPrefs.HasKey("CERESITA_USERNAME"))
        {
            emailInputField.text = PlayerPrefs.GetString("CERESITA_USERNAME");
        }

        if (PlayerPrefs.HasKey("CERESITA_PASSWORD"))
        {
            passwordInputField.text = PlayerPrefs.GetString("CERESITA_PASSWORD");
        }
    }

    void Update()
    {
        if(loadRegionList)
        {
            if(CeresitaWebService.Singleton.isWebServiceReady)            {                loadRegionList = false;
                countryListDropdown.options.Clear();

                List<string> countryOptions = new List<string>();

                for(int i = 0; i < CeresitaWebService.Singleton.countries.Length; i++)
                {
                    countryOptions.Add(CeresitaWebService.Singleton.countries[i].name);
                }
                countryListDropdown.AddOptions(countryOptions);                LoadRegionsWithCountryId(CeresitaWebService.Singleton.countries[0].id);            }        }    }
    //Modificar el dropdown de regiones dependiendo del pais seleccionado
	public void OnCountryChange()
    {
        LoadRegionsWithCountryId(CeresitaWebService.Singleton.countries[countryListDropdown.value].id);
    }    public void LoadRegionsWithCountryId(int id)
    {
        Debug.Log("country id: " + id);
        regionToSelect.Clear();
        regionListDropdown.ClearOptions();
        List<string> options = new List<string>();

        for(int i = 0; i < CeresitaWebService.Singleton.regions.Length; i++)
        {
            CeresitaWebService.Region curr = CeresitaWebService.Singleton.regions[i];

            if(curr.country.id == id)
            {
                Debug.Log("region name: " + curr.id);
                options.Add(curr.name);
                regionToSelect.Add(curr);
            }
        }

        regionListDropdown.AddOptions(options);
        regionListDropdown.gameObject.SetActive(options.Count > 1);
    }
    public void LoginWithEmail(string email, string password)
    {

		Debug.Log ("Kauel login email: " +email);

        CeresitaWebService.Singleton.GetUserByemail(email, password, delegate (CeresitaWebService.WEBSERVICE_RETURN ret)
        {
            if (ret == CeresitaWebService.WEBSERVICE_RETURN.OK)
            {
                //Guarda la última forma de ingreso, para que la siguiente vez que se ingresa
                //a la aplicación se ingrese automaticamente.
                PlayerPrefs.SetString("CERESITA_USERNAME", email);
                PlayerPrefs.SetString("CERESITA_PASSWORD", password);
                PlayerPrefs.SetString("LOGIN_MODE", "EMAIL");

                kuiPanelManager.ShowOnlyThisPanel(2);
                CeresitaWebService.Singleton.UpdateLastActivity();
                AccountManager.Singleton.UpdateInformation();
				mailLinkPanel.gameObject.SetActive(false);

            }
            else
            {
                Alert.Singleton.ShowAlert(Alert.Message.INCORRECT_USER_OR_PASS);
            }
        });
    }

    public void LoginWithEmail()
    {

		if (mailLinkPanel.isActiveAndEnabled) 
		{
			Debug.Log ("Kauel: Vincular Mail");
			emailInputField = email2InputField;
			passwordInputField = password2InputField;

		}	
        LoginWithEmail(emailInputField.text, passwordInputField.text);
    }

    public void CreateNewUser()
    {
		string nameString = newNameInputField.text;
        string emailString = newEmailInputField.text;
        bool isEmail = Regex.IsMatch(emailString, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);

        if(isEmail)
        {
            if(newPasswordInputField.text.CompareTo(confirmNewPasswordInputField.text) == 0)
            {
                if(newPasswordInputField.text.Length >= 3)
                {
                    CeresitaWebService.Singleton.CreateUser(CeresitaWebService.AUTHENTICATION_TYPE.EMAIL, newEmailInputField.text, delegate (CeresitaWebService.WEBSERVICE_RETURN ret)
                    {
                        Alert.Singleton.ShowAlert(Alert.Message.LOADING, false);
                        if (ret == CeresitaWebService.WEBSERVICE_RETURN.USER_CREATED)
                        {
							CeresitaWebService.Singleton.user.name = nameString;
                            CeresitaWebService.Singleton.user.password = CeresitaWebService.Singleton.CalculateMD5Hash(newPasswordInputField.text);
                            CeresitaWebService.Singleton.user.region = regionToSelect[regionListDropdown.value];
                            CeresitaWebService.Singleton.UpdateUser(delegate (CeresitaWebService.WEBSERVICE_RETURN ret2)
                            {
                                Alert.Singleton.CloseAlert(true);
                                kuiPanelManager.ShowOnlyThisPanel(2);
                                CeresitaWebService.Singleton.UpdateLastActivity();
                                AccountManager.Singleton.UpdateInformation();
                            });
                        }
                        else
                        {
                            Alert.Singleton.CloseAlert(true);
                            Alert.Singleton.ShowAlert(Alert.Message.USER_EXISTS);
                        }
                    });
                }
                else
                {
                    Alert.Singleton.ShowAlert(Alert.Message.PASSWORD_SHORT);
                }
            }
            else
            {
                Alert.Singleton.ShowAlert(Alert.Message.PASSWORD_NOT_MATCH);
            }
        }
        else
        {
            Alert.Singleton.ShowAlert(Alert.Message.INVALID_EMAIL);
        }
    }

    public void RecoverPassword()
    {
        passwordRecoveryPass01.ActivateWithFadeIn();
    }

    public void RecoverPasswordSendMail()
    {
        Alert.Singleton.ShowAlert(Alert.Message.LOADING, false, delegate
        {
            CeresitaWebService.Singleton.SendRecoveryMail(passwordRecoveryInputField.text, delegate(CeresitaWebService.WEBSERVICE_RETURN ret)
            {
                if(ret == CeresitaWebService.WEBSERVICE_RETURN.USER_UPDATED)
                {
                    Alert.Singleton.CloseAlert(true);
                    passwordRecoveryPass01.FadeOutAndDesactivate();
                    passwordRecoveryPass02.ActivateWithFadeIn();
                }
                else
                {
                    Alert.Singleton.ShowAlert(Alert.Message.USER_NOT_FOUND);
                }
            });
        });
    }

    public void ConfirmRecoveryCode()
    {
        passwordRecoveryPass02.FadeOutAndDesactivate();
        if (CeresitaWebService.Singleton.user.recoveryCode.CompareTo(passwordRecoveryCodeInputField.text) == 0)
        {
            passwordRecoveryPass03.ActivateWithFadeIn();
        }
        else
        {
            Alert.Singleton.ShowAlert(Alert.Message.ERROR);
        }
    }

    public void UpdatePasswordUsingRecoveryCode()
    {
        string newPass = CeresitaWebService.Singleton.CalculateMD5Hash(passwordRecoveryNewPassInputField.text);
        CeresitaWebService.Singleton.UpdateUserPassword(passwordRecoveryInputField.text, newPass, delegate (CeresitaWebService.WEBSERVICE_RETURN ret)
        {
            passwordRecoveryPass03.FadeOutAndDesactivate();
            Alert.Singleton.ShowAlert(Alert.Message.PASSWORD_UPDATED);
        });
    }

	public void LinkMail()
	{

		mailLinkPanel.gameObject.SetActive (true);

	
	}

}


