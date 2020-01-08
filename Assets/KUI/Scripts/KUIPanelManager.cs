using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class KUIPanelManager : MonoBehaviour {

	public List<KUIPanelFader> Panels = new List<KUIPanelFader>();

	public int PanelIndex = -1;
	private int LastPanelIndex = -1;

	// Use this for initialization
	void Start () {
		if (Panels.Count <= 0) transform.GetComponentsInChildren<KUIPanelFader> (true, Panels);
			
	}
	
	// Update is called once per frame
	void Update () {
		if(LastPanelIndex!=PanelIndex){
			LastPanelIndex=PanelIndex;
			if(PanelIndex==-10) ShowAll();
			else ShowOnlyThisPanel(PanelIndex);
		}
	}

	//Show Only one Panel
	public void ShowOnlyThisPanel(int index){
		for (int i=0; i<Panels.Count; i++) {
			if(i==index) Panels[i].ActivateWithFadeIn();
			else Panels[i].FadeOut=true;
		}
	}

	public void Show(int index){
		if((index>=0)&&(index<Panels.Count)) Panels[index].ActivateWithFadeIn();
	}

	public void Hide(int index){
		if((index>=0)&&(index<Panels.Count)) Panels[index].FadeOut=true;
	}

	public void ShowAll(){
		for (int i=0; i<Panels.Count; i++) {
			Panels[i].ActivateWithFadeIn();
		}
	}

	public void HideAll(){
		for (int i=0; i<Panels.Count; i++) {
			Panels[i].FadeOut=true;
		}
	}
}
