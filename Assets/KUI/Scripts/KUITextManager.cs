using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class KUITextManager : MonoBehaviour {

	public Text managedText = null;

	// Use this for initialization
	void Start () {
		if (managedText == null) managedText = GetComponent<Text> ();
		if (managedText == null) managedText = GetComponentInChildren<Text> ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	//Update from values and Filled Images
	public void UpdateFromValuePercent(float aValue){
		if(managedText==null) return;
		int v = Mathf.RoundToInt (aValue * 100.0f);
		managedText.text = v.ToString ()+"%";
	}

	public void UpdateFromValueInteger(float aValue){
		if(managedText==null) return;
		int v = Mathf.RoundToInt(aValue);
		managedText.text = v.ToString();
	}

	public void UpdateFromValueFloat(float aValue){
		if(managedText==null) return;
		managedText.text = aValue.ToString("F2");
	}

	public void UpdateFromImageFilledPercent(Image aFilledImage){
		if(managedText==null) return;
		if(aFilledImage==null) return;
		int v = Mathf.RoundToInt (aFilledImage.fillAmount * 100.0f);
		managedText.text = v.ToString ()+"%";
	}
}
