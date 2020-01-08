using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KUIActivateChildren : MonoBehaviour {

    private KUIPanelFader[] Faders = null;

	// Use this for initialization
	void Start () {
        Faders = GetComponentsInChildren<KUIPanelFader>(true);
	}
	
	// Update is called once per frame
	//void Update () {	}

    public void ActivateAllChildrens() {
        for(int i = 0; i < Faders.Length; i++) {
            Faders[i].ActivateWithFadeIn();
        }
    }

    public void DeActivateAllChildrens() {
        for (int i = 0; i < Faders.Length; i++) {
            Faders[i].FadeOutAndDesactivate();
        }
    }
}
