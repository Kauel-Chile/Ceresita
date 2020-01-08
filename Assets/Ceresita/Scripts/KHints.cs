using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using TMPro;

public class KHints : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    public KUIPanelFader TargetHint = null;
    [TextArea(3,10)]
    public string Hint = "";

    // Use this for initialization
    void Start () {
		
	}

    // Update is called once per frame
    //void Update () { }

    public void OnPointerEnter(PointerEventData eventData) {
        if(TargetHint != null && Hint != "") {
            TextMeshProUGUI text = TargetHint.GetComponentInChildren<TextMeshProUGUI>(true);
            if (text != null) {
                text.text = Hint;
            }
            TargetHint.ActivateWithFadeIn();
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (TargetHint != null) {
            TargetHint.FadeOutAndDesactivate();
        }
    }
}
