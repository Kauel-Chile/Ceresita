using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent (typeof(CanvasGroup))]
public class KUIPanelControlASR : MonoBehaviour {

	[Range(0f,1f)]
	public float MasterAlpha = 1.0f;

	[Range(0f,2f)]
	public float MasterScale = 1.0f;

	[Range(-360f,360f)]
	public float MasterRotation = 0.0f;

	private CanvasGroup canvasgroup = null;
	private RectTransform rect = null;

	// Use this for initialization
	void Start() {
		canvasgroup = GetComponent<CanvasGroup>();
		rect = GetComponent<RectTransform>();
	}


	// Update is called once per frame
	void Update(){
		if (canvasgroup!=null) canvasgroup.alpha = MasterAlpha;
		if (rect!=null){
			rect.localScale = new Vector3 (MasterScale, MasterScale, MasterScale);
			rect.localRotation = Quaternion.Euler (0, 0, MasterRotation);
		}
	}

}
