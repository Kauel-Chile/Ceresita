using System.Collections;

using System.Collections.Generic;

using UnityEngine;



public class AccountManager : MonoBehaviour

{

    public TMPro.TextMeshProUGUI email, facebook, twitter;



    private static AccountManager singleton;



    public static AccountManager Singleton

    {

        get

        {

            if(singleton == null)

            {

                singleton = FindObjectOfType<AccountManager>();

            }



            return singleton;

        }

    }



    public void Clear()

    {

        email.text = "Ingresar con email.";

        facebook.text = "Ingresar con cuenta de Facebook.";

        twitter.text = "No tiene cuenta asociada.";

    }



	public void UpdateInformation()

    {



        if(CeresitaWebService.Singleton.user.email != null &&

            CeresitaWebService.Singleton.user.email.Length > 0)

        {

            email.text = CeresitaWebService.Singleton.user.email;

        }



			

        if (CeresitaWebService.Singleton.user.facebookId != null &&

            CeresitaWebService.Singleton.user.facebookId.Length > 0)

        {

            facebook.text = CeresitaWebService.Singleton.user.name;

        }



        if (CeresitaWebService.Singleton.user.twitterId != null &&

            CeresitaWebService.Singleton.user.twitterId.Length > 0)

        {

          //  twitter.text = CeresitaWebService.Singleton.user.twitterId.Split(",")[1];

        }

    }

}

