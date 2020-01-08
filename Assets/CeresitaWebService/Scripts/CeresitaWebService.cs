using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System;
using System.Text;
using System.IO;

public class CeresitaWebService : MonoBehaviour
{
    private static CeresitaWebService singleton;

    public static CeresitaWebService Singleton
    {
        get
        {
            if(singleton == null)
            {
                singleton = FindObjectOfType<CeresitaWebService>();
            }

            return singleton;
        }
    }

    public enum WEBSERVICE_RETURN
    {
        ERROR,
        USER_UPDATED,
        USER_CREATED,
        PASSWORD_INCORRECT,
        USER_NOT_FOUND,
        OK
    }

    public enum AUTHENTICATION_TYPE
    {
        EMAIL,
        FACEBOOK
    }

    public string webServiceURL;
    public string securityString;
    public bool isWebServiceReady = false;

    public delegate void onUserLoaded(WEBSERVICE_RETURN ret);
    onUserLoaded onUserLoadedEvent;
    [System.Serializable]    public class Country
    {
        public int id;
        public string name;
    }    [System.Serializable]    public class Region    {        public int id;        public string name;
        public Country country;
    }
    [System.Serializable]
    public class User
    {
        public int id;
        public string email;
        public string name;
        public string password;
        public Region region;
        public string facebookId;
        public string twitterId;
        public string dateCreation;
        public string dateLast;
        public string recoveryCode;

        public User()
        {
            id = -1;
            email = "";
            name = "";
            password = "";
            region = new Region();
            region.id = 1;
            facebookId = "";
            twitterId = "";
            dateCreation = "";
            dateLast = "";
            recoveryCode = "";
        }
    }

    public User user;    public Region[] regions;    public Country[] countries;

    void Start()
    {
        isWebServiceReady = false;
        StartCoroutine(GetRegions());
    }

    public void UpdateLastActivity()
    {
        if(!isWebServiceReady)
        {
            throw new Exception("WebService is not ready yet.");
        }

        isWebServiceReady = false;
        StartCoroutine(UpdateLastActivityCoroutine());
    }

    public void GetUserByemail(string email, string password, onUserLoaded callback = null)
    {
        if (!isWebServiceReady)
        {
            throw new Exception("WebService is not ready yet.");
        }

        isWebServiceReady = false;
        onUserLoadedEvent = callback;
        StartCoroutine(LoadUser(email, password));
    }

    public void GetUserByFacebook(string facebookId, onUserLoaded callback = null)
    {
        if (!isWebServiceReady)
        {
            throw new Exception("WebService is not ready yet.");
        }

        isWebServiceReady = false;
        onUserLoadedEvent = callback;
        StartCoroutine(LoadUserWithFacebook(facebookId));
    }

    public void CreateUser(AUTHENTICATION_TYPE authType, string authString, onUserLoaded callback = null)
    {
        if (!isWebServiceReady)
        {
            throw new Exception("WebService is not ready yet.");
        }

        isWebServiceReady = false;
        onUserLoadedEvent = callback;
		StartCoroutine(CreateNewUser(authType, authString, authString));
    }

    public void UpdateUser(onUserLoaded callback = null)
    {
        if (!isWebServiceReady)
        {
            throw new Exception("WebService is not ready yet.");
        }

        isWebServiceReady = false;
        onUserLoadedEvent = callback;
        StartCoroutine(UpdateCurrentUser());
    }

    string GetWebService(string request, string parameters = "")
    {
        string toRet = webServiceURL + "/" + request + "?SECURITY_STRING=" + securityString;

        if(parameters.Length > 0)
        {
            toRet += "&" + parameters;
        }

        return toRet;
    }

    string Utf8Decode(string inputDate)
    {
        return Encoding.UTF8.GetString(Encoding.GetEncoding("iso-8859-1").GetBytes(inputDate));
    }

    public void SendImageToMail(string path, onUserLoaded _onUserLoaded)
    {
        if (!isWebServiceReady)
        {
            throw new Exception("WebService is not ready yet.");
        }

        isWebServiceReady = false;
        onUserLoadedEvent = _onUserLoaded;
        StartCoroutine(SendImageToMailCoroutine(path));
    }

    public void SendImageToMail(string message, byte[] path, onUserLoaded _onUserLoaded)
    {
        if (!isWebServiceReady)
        {
            throw new Exception("WebService is not ready yet.");
        }

        isWebServiceReady = false;
        onUserLoadedEvent = _onUserLoaded;
        StartCoroutine(SendImageToMailCoroutine(message, path));
    }

    IEnumerator SendImageToMailCoroutine(string path)
    {
        WWWForm form = new WWWForm();
        form.AddField("SECURITY_STRING", securityString);
        form.AddField("USER_ID", user.id);

        WWW imageFile = new WWW("file://" + path);
        yield return imageFile;

        form.AddBinaryData("IMAGE", imageFile.bytes, user.id + ".png", "text/plain");

        WWW www = new WWW(webServiceURL + "/send_image_to_mail.php", form);
        yield return www;

        if(www.text != null)
        {
            JSONNode n = JSON.Parse(www.text);

            try
            {
                if(n["success"].ToString().Trim() != "")
                {
                    if (onUserLoadedEvent != null)
                    {
                        onUserLoadedEvent(WEBSERVICE_RETURN.OK);
                    }
                }
                else
                {
                    Debug.Log(n["error"]);
                    if (onUserLoadedEvent != null)
                    {
                        onUserLoadedEvent(WEBSERVICE_RETURN.ERROR);
                    }
                }
            }
            catch
            {
                Debug.Log(n["error"]);

                if(onUserLoadedEvent != null)
                {
                    onUserLoadedEvent(WEBSERVICE_RETURN.ERROR);
                }
            }
        }
        else
        {
            Debug.Log(www.error);

            if (onUserLoadedEvent != null)
            {
                onUserLoadedEvent(WEBSERVICE_RETURN.ERROR);
            }
        }

        isWebServiceReady = true;
    }

    public void SendRecoveryMail(string email, onUserLoaded _onUserLoaded)
    {
        if (!isWebServiceReady)
        {
            throw new Exception("WebService is not ready yet.");
        }

        isWebServiceReady = false;
        onUserLoadedEvent = _onUserLoaded;
        StartCoroutine(SendRecoveryMailCoroutine(email));
    }

    IEnumerator SendRecoveryMailCoroutine(string email)
    {
        WWWForm form = new WWWForm();
        form.AddField("SECURITY_STRING", securityString);
        form.AddField("EMAIL", email);

        WWW www = new WWW(webServiceURL + "/send_recovery_mail.php", form);

        yield return www;

        if (www.text != null)
        {
            Debug.Log(www.text);
            JSONNode n = JSON.Parse(www.text);
            
            try
            {
                user.recoveryCode = n["code"];

                if(onUserLoadedEvent != null)
                {
                    onUserLoadedEvent(GetWebServiceReturn(n["success"]));
                }
            }
            catch
            {
                if (onUserLoadedEvent != null)
                {
                    onUserLoadedEvent(GetWebServiceReturn(n["error"]));
                }
            }
        }
        else
        {
            Debug.Log(www.error);
            Alert.Singleton.ShowAlert(Alert.Message.ERROR);
        }

        isWebServiceReady = true;
    }

    IEnumerator SendImageToMailCoroutine(string message, byte[] path)
    {
        WWWForm form = new WWWForm();
        form.AddBinaryData("IMAGE", path, user.id + ".png", "text/plain");
        form.AddField("SECURITY_STRING", securityString);
        form.AddField("USER_ID", user.id);
        form.AddField("MESSAGE", message);

        WWW www = new WWW(webServiceURL + "/send_image_to_mail.php", form);
        yield return www;

        if (www.text != null)
        {
            JSONNode n = JSON.Parse(www.text);

            try
            {
                if (n["success"].ToString().Trim() != "")
                {
                    if (onUserLoadedEvent != null)
                    {
                        onUserLoadedEvent(WEBSERVICE_RETURN.OK);
                    }
                }
                else
                {
                    Debug.Log(n["error"]);
                    if (onUserLoadedEvent != null)
                    {
                        onUserLoadedEvent(WEBSERVICE_RETURN.ERROR);
                    }
                }
            }
            catch
            {
                Debug.Log(n["error"]);

                if (onUserLoadedEvent != null)
                {
                    onUserLoadedEvent(WEBSERVICE_RETURN.ERROR);
                }
            }
        }
        else
        {
            Debug.Log(www.error);

            if (onUserLoadedEvent != null)
            {
                onUserLoadedEvent(WEBSERVICE_RETURN.ERROR);
            }
        }

        isWebServiceReady = true;
    }

    public string CalculateMD5Hash(string input)
    {
        // step 1, calculate MD5 hash from input
        System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
        byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
        byte[] hash = md5.ComputeHash(inputBytes);

        // step 2, convert byte array to hex string
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < hash.Length; i++)
        {
            sb.Append(hash[i].ToString("x2"));
        }

        return sb.ToString();
    }

    WEBSERVICE_RETURN GetWebServiceReturn(string msg)
    {
        switch(msg)
        {
            case "User updated.":
                return WEBSERVICE_RETURN.USER_UPDATED;
            case "User created.":
                return WEBSERVICE_RETURN.USER_CREATED;
            case "PASSWORD incorrect.":
                return WEBSERVICE_RETURN.PASSWORD_INCORRECT;
            case "User not found.":
                return WEBSERVICE_RETURN.USER_NOT_FOUND;
            default:
                return WEBSERVICE_RETURN.ERROR;
        }
    }

    IEnumerator LoadUserWithFacebook(string facebookId)
    {
        WWWForm form = new WWWForm();
        form.AddField("SECURITY_STRING", securityString);
        form.AddField("FACEBOOK", facebookId);

        WWW www = new WWW(webServiceURL + "/get_user.php", form);
        yield return www;

        isWebServiceReady = true;

        if (www.text != null)
        {
            JSONNode n = JSON.Parse(www.text);

            Debug.Log(www.text);

            try
            {
                FillUserObject(n);

                if (onUserLoadedEvent != null)
                {
                    onUserLoadedEvent(WEBSERVICE_RETURN.OK);
                }
            }
            catch (Exception e)
            {
                if (onUserLoadedEvent != null)
                {
                    onUserLoadedEvent(GetWebServiceReturn(n["error"]));
                }
            }
        }
        else
        {
            Debug.LogError("LoadUser Method: " + www.error);
        }
    }

    IEnumerator LoadUser(string email, string password)
    {
        WWWForm form = new WWWForm();
        form.AddField("SECURITY_STRING", securityString);
        form.AddField("EMAIL", email);
        form.AddField("PASSWORD", CalculateMD5Hash(password));

        WWW www = new WWW(webServiceURL + "/get_user.php", form);
        yield return www;

        isWebServiceReady = true;

        if (www.text != null)
        {
            Debug.Log(www.text);

           try
           {
                JSONNode n = JSON.Parse(www.text);

                FillUserObject(n);
                Debug.Log("User filled: " + www.text);
                if (onUserLoadedEvent != null)
                {
                    onUserLoadedEvent(WEBSERVICE_RETURN.OK);
                }
            }
            catch(Exception e)
            {
                JSONNode n = JSON.Parse(www.text);

                if (onUserLoadedEvent != null)
                {
                    onUserLoadedEvent(GetWebServiceReturn(n["error"]));
                }
            }
        }
        else
        {
            Debug.LogError("LoadUser Method: " + www.error);
        }
    }

    void FillUserObject(JSONNode n)
    {
        user.id = n["id"].AsInt;
        user.email = n["email"];
        user.name = n["name"];
        user.password = n["password"];
        user.region = regions[n["region"].AsInt - 1];
        user.facebookId = n["facebookId"];
        user.twitterId = n["twitterId"];
        user.dateCreation = n["dateCreation"];
        user.dateLast = n["dateLast"];
    }

    public void ClearUserObject()
    {
        user.id = 0;
        user.email = "";
        user.name = "";
        user.password = "";
        user.region = regions[0];
        user.facebookId = "";
        user.twitterId = "";
        user.dateCreation = "";
        user.dateLast = "";

        AccountManager.Singleton.Clear();
    }

	IEnumerator CreateNewUser(AUTHENTICATION_TYPE authType, string authString, string authString2)
    {
        WWW www = null;
        
        if(authType == AUTHENTICATION_TYPE.EMAIL)
        {
            user.email = authString;

            WWWForm form = new WWWForm();
            form.AddField("SECURITY_STRING", securityString);
            form.AddField("EMAIL", authString);

            www = new WWW(webServiceURL + "/update_user.php", form);
        }
        else if(authType == AUTHENTICATION_TYPE.FACEBOOK)
        {
            user.facebookId = authString;

            WWWForm form = new WWWForm();
            form.AddField("SECURITY_STRING", securityString);
            form.AddField("FACEBOOK", authString);

            www = new WWW(webServiceURL + "/update_user.php", form);
        }
        
        yield return www;

        isWebServiceReady = true;

        if (www.text != null)
        {
            try
            {
                JSONNode n = JSON.Parse(www.text);

                user.id = n["userId"].AsInt;

                if (onUserLoadedEvent != null)
                {
                    onUserLoadedEvent(GetWebServiceReturn(n["success"]));
                }
            }
            catch (Exception e)
            {
                Debug.Log("LoadUser Method: " + www.text);
            }
        }
        else
        {
            Debug.Log("LoadUser Method: " + www.error);
        }
    }

    string normalizeOutput(string str)
    {
        if(str == null)
        {
            return "";
        }

        return str;
    }

    IEnumerator UpdateCurrentUser()
    {
        WWWForm form = new WWWForm();

        form.AddField("SECURITY_STRING", securityString);
        form.AddField("FACEBOOK", normalizeOutput(user.facebookId));
        form.AddField("EMAIL", normalizeOutput(user.email));
        form.AddField("NAME", normalizeOutput(user.name));
        form.AddField("PASSWORD", normalizeOutput(user.password));
        form.AddField("REGION", user.region.id);
        form.AddField("TWITTER_ID", normalizeOutput(user.twitterId));

        WWW www = new WWW(webServiceURL + "/update_user.php", form);
        yield return www;

        isWebServiceReady = true;

        if (www.text != null)
        {
            JSONNode n = JSON.Parse(www.text);

            try
            {
                Debug.Log("Success: " + n["success"]);

                if (onUserLoadedEvent != null)
                {
                    onUserLoadedEvent(GetWebServiceReturn(n["success"]));
                }
            }
            catch (Exception e)
            {
                Debug.Log("Error:" + n["error"]);

                if (onUserLoadedEvent != null)
                {
                    onUserLoadedEvent(GetWebServiceReturn(n["error"]));
                }
            }
        }
        else
        {
            Debug.LogError("LoadUser Method: " + www.error);
        }
    }

    IEnumerator GetRegions()    {        WWWForm form = new WWWForm();
        form.AddField("SECURITY_STRING", securityString);
        WWW www = new WWW(webServiceURL + "/get_regions.php", form);
        yield return www;
        Debug.Log("GetRegions Text: " + www.text);
        //Debug.Log(www.text);
        isWebServiceReady = true;
        if (www.text != null)        {
            JSONNode n = JSON.Parse(www.text);
            try            {                regions = new Region[n.Count];                List<Country> countryList = new List<Country>();                List<int> distinctCountry = new List<int>();
                for(int i = 0; i < n.Count; i++)                {                    regions[i] = new Region();                    regions[i].id = n[i]["id"].AsInt;                    regions[i].name = Utf8Decode(n[i]["name"]);                    Country country = new Country();
                    country.id = n[i]["country"]["id"].AsInt;
                    country.name = Utf8Decode(n[i]["country"]["name"]);                    regions[i].country = country;                    if(distinctCountry.IndexOf(n[i]["country"]["id"].AsInt) < 0)
                    {
                        countryList.Add(country);
                        distinctCountry.Add(n[i]["country"]["id"].AsInt);
                    }                }                countries = new Country[countryList.Count];                for(int i = 0; i < countryList.Count; i++)
                {
                    countries[i] = countryList[i];
                }
                isWebServiceReady = true;
                ClearUserObject();            }            catch(Exception e)            {                Debug.LogError("GetRegions method: " + e.Message);            }        }    }

    IEnumerator UpdateLastActivityCoroutine()
    {
        WWWForm form = new WWWForm();
        form.AddField("SECURITY_STRING", securityString);
        form.AddField("EMAIL", user.email);
        form.AddField("UPDATE_ACTIVITY", 1);

        WWW www = new WWW(webServiceURL + "/update_user.php", form);
        yield return www;

        isWebServiceReady = true;

        if (www.error == null)
        {
            Debug.Log("Last Activity Updated.");
        }
        else
        {
            Debug.Log(www.error);
        }
    }

    public void UpdateUserPassword(string email, string newPass, onUserLoaded _onUserLoaded)
    {
        if (!isWebServiceReady)
        {
            throw new Exception("WebService is not ready yet.");
        }

        isWebServiceReady = false;
        onUserLoadedEvent = _onUserLoaded;
        StartCoroutine(UpdateUserPasswordCoroutine(email, newPass));
    }

    IEnumerator UpdateUserPasswordCoroutine(string email, string newPass)
    {
        WWWForm form = new WWWForm();
        form.AddField("SECURITY_STRING", securityString);
        form.AddField("EMAIL", email);
        form.AddField("NEW_PASS", newPass);

        WWW www = new WWW(webServiceURL + "/update_user_password_using_email.php", form);

        yield return www;

        if (www.text != null)
        {
            Debug.Log(www.text);

            if(onUserLoadedEvent != null)
            {
                onUserLoadedEvent(WEBSERVICE_RETURN.OK);
            }
        }
        else
        {
            Alert.Singleton.ShowAlert(Alert.Message.ERROR);
        }

        isWebServiceReady = true;
    }
}
