using UnityEngine;



/// <summary>

/// Ingreso automatico a la aplicación sin tener que pasar por la ventana

/// de inicio de sesión. Una vez se ingresa a la aplicación mediante Facebook o

/// usando un correo electronico, los proximos inicios de sesion usarán estas credenciales.

/// </summary>

public class AutoLogin : MonoBehaviour

{

    public Login loginWithEmail;

    public KFacebook loginWithFacebook;

    private bool isActive = false;

    private bool activeOnlyOneTime = true;



    void Update()

    {

        if(isActive && CeresitaWebService.Singleton.isWebServiceReady)

        {

            isActive = false;

            activeOnlyOneTime = false;

            Login();

        }

    }



    public void InitAutoLogin()

    {

        if(activeOnlyOneTime)

        {

            isActive = true;

        }

    }



    private void Login()

    {

        //Para saber si se ha inicado sesion exitosamente anteriormente, hay un PlayerPrefs.

        //Si no existe, entonces nunca se ha iniciado sesión en la aplicación.

        if (PlayerPrefs.HasKey("LOGIN_MODE"))

        {

            string loginMode = PlayerPrefs.GetString("LOGIN_MODE");



            switch (loginMode)

            {

                case "EMAIL":

                    loginWithEmail.LoginWithEmail(PlayerPrefs.GetString("CERESITA_USERNAME"), PlayerPrefs.GetString("CERESITA_PASSWORD"));

                    break;



                case "FACEBOOK":

                    loginWithFacebook.LogInRead();

                    break;





            }

        }

    }

}

