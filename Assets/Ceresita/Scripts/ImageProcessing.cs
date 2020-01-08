using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageProcessing : MonoBehaviour { 
    public delegate void OnEndProcessingImageEvent(Texture2D texture);
    OnEndProcessingImageEvent _OnEndProcessingImageEvent;

    public void ProcessImage(Texture2D tex, OnEndProcessingImageEvent callback = null) {
        _OnEndProcessingImageEvent = callback;
        if(_OnEndProcessingImageEvent != null) _OnEndProcessingImageEvent(tex);
    }

    public static void ProcessNothing(Action callback = null) {
        if (callback != null) callback();
    }
}
