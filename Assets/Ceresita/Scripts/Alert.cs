using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Alert : MonoBehaviour
{
    public KUIPanelFader kuiPanelFader;
    public TMPro.TextMeshProUGUI message;

    //public delegate void OnFinishFadeInEvent();
    //OnFinishFadeInEvent _OnFinishFadeInEvent;
    Action _OnFinishFadeInEvent;

    public enum Message
    {
        INCORRECT_USER_OR_PASS,
        LOADING,
        USER_EXISTS,
        PASSWORD_SHORT,
        PASSWORD_NOT_MATCH,
        PASSWORD_UPDATED,
        INVALID_EMAIL,
        LOGIN_FAILED,
        IMAGE_SENT,
        ERROR,
        IMAGE_SHARED,
        USER_NOT_FOUND,
        IMAGE_SENDING,
        SAVING_FILE,
        FILE_SAVED,
        MUST_SAVE_FILE,
		BETTER_EXPERIENCE
    }

    bool closeOnTouch = true;

    private static Alert singleton;

    public static Alert Singleton
    {
        get
        {
            if(singleton == null)
            {
                singleton = FindObjectOfType<Alert>();
            }

            return singleton;
        }
    }

    public void ShowAlert(Message m, bool closeOnTouch = true, Action _OnFinishFadeInEvent = null)
    {
        this._OnFinishFadeInEvent = _OnFinishFadeInEvent;

        kuiPanelFader.ActivateWithFadeIn();
        this.closeOnTouch = closeOnTouch;

        switch(m)
        {
            case Message.INCORRECT_USER_OR_PASS:
                message.SetText("Usuario o contraseña incorrectas.");
                break;
            case Message.LOADING:
                message.SetText("Espere un momento, por favor...");
                break;
            case Message.USER_EXISTS:
                message.SetText("El nombre de usuario ya existe.");
                break;
            case Message.PASSWORD_SHORT:
                message.SetText("La contraseña es demasiado corta.");
                break;
            case Message.PASSWORD_NOT_MATCH:
                message.SetText("Las contraseñas no coinciden.");
                break;
            case Message.PASSWORD_UPDATED:
                message.SetText("Contraseña actualizada.");
                break;
            case Message.INVALID_EMAIL:
                message.SetText("Correo no valido.");
                break;
            case Message.LOGIN_FAILED:
                message.SetText("Error al ingresar.");
                break;
            case Message.IMAGE_SENT:
                message.SetText("La imagen ha sido enviada a " + CeresitaWebService.Singleton.user.email);
                break;
            case Message.IMAGE_SENDING:
                message.SetText("En unos minutos tu imagen será enviada a " + CeresitaWebService.Singleton.user.email);
                break;
            case Message.ERROR:
                message.SetText("Un error ha ocurrido.");
                break;
            case Message.IMAGE_SHARED:
                message.SetText("Imagen compartida con éxito.");
                break;
            case Message.USER_NOT_FOUND:
                message.SetText("Usuario no encontrado.");
                break;
            case Message.SAVING_FILE:
                message.SetText("Guardando imagen.");
                break;
            case Message.FILE_SAVED:
                message.SetText("Imagen guardada con éxito.");
                break;
            case Message.MUST_SAVE_FILE:
                message.SetText("Debe guardar la imagen antes.");
                break;
			case Message.BETTER_EXPERIENCE:
				message.SetText("Para una mejor experiencia, verificar que los objetos de decoración no posean colores similares a las paredes, ni usar fotografías con paredes texturizadas.");
				break;	
            default:
                message.SetText("");
                break;
        }
    }

    public void CloseAlert(bool bForceToClose = false)
    {
        if(closeOnTouch || bForceToClose)
        {
            kuiPanelFader.FadeOutAndDesactivate();
        }
    }

    public void OnFinishFadeIn()
    {
        Invoke("OnFinishFadeInDelayed", 0.6f);
    }

    void OnFinishFadeInDelayed()
    {
        if (_OnFinishFadeInEvent != null)
        {
            _OnFinishFadeInEvent();
        }
    }
}
