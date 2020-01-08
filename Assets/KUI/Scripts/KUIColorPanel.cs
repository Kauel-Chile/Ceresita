using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;

public class KUIColorPanel : MonoBehaviour {

	public Color ColorA = Color.white;
	public Color ColorB = Color.black;

	public KUICrossAir ColorWheel = null;
	public KUICrossAir ColorTriangle = null;
	public KUICrossAir ColorHue = null;
	public Text ColorText = null;

	public Image ColorAImage = null;
	public Image ColorBImage = null;

	public UnityEvent OnColorChange = new UnityEvent();

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.X)) {
			Color Temp = ColorA;
			ColorA = ColorB;
			ColorB = Temp;
			ColorAImage.color=ColorA;
			ColorBImage.color=ColorB;
			UpdateFromColor(ColorA);
		}
	}

	public void UpdateFromKUICrossAir(KUICrossAir aCrossAir){
		HSLColor newColor = ColorA;
		RectTransform rtCrossAir = aCrossAir.imageCursor.rectTransform;
		RectTransform rtCenter = aCrossAir.gameObject.GetComponent<RectTransform>();

		if(aCrossAir==ColorWheel){
			Vector2 delta = rtCrossAir.position - rtCenter.position;
			newColor.h = Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg-30;
			if(newColor.h<0) newColor.h+=360;
			newColor.s = Mathf.Clamp01(delta.magnitude/(rtCenter.rect.width*0.5f));
		
		}else if(aCrossAir==ColorTriangle){
			float xmin = rtCenter.position.x-rtCenter.rect.width*0.5f;
			float xmax = rtCenter.position.x+rtCenter.rect.width*0.5f;
			float ymin = rtCenter.position.y-rtCenter.rect.height*0.5f;
			float ymax = rtCenter.position.y+rtCenter.rect.height*0.5f;
			newColor.l = Mathf.Clamp01((rtCrossAir.position.y - ymin)/(ymax - ymin));
			newColor.s = Mathf.Clamp01((rtCrossAir.position.x - xmin)/(xmax - xmin));
		}else if(aCrossAir==ColorHue){
			float min = rtCenter.position.x-rtCenter.rect.width*0.5f;
			float max = rtCenter.position.x+rtCenter.rect.width*0.5f;
			float h = 360.0f * (rtCrossAir.position.x - min) / (max - min);
			newColor.h = h;
		};


		UpdateFromColor (newColor);

	}

	//TODO THIS FUNCTION
	public void UpdateFromColor(Color aColor){

		//ColorImages
		ColorA = aColor;
		if(ColorAImage!=null) ColorAImage.color = aColor;
				
		//Event
		OnColorChange.Invoke ();

		//HUE VARIABLES
		HSLColor newColor = aColor;
		HSLColor saturatedColorHSL = aColor;
		saturatedColorHSL.s = 1.0f;
		Color saturatedColorRGB = saturatedColorHSL;
		saturatedColorHSL = saturatedColorRGB;

		RectTransform rtColorWheel = ColorWheel.GetComponent<RectTransform> ();
		RectTransform rtColorTriangle = ColorTriangle.GetComponent<RectTransform> ();
		RectTransform rtColorHue = ColorHue.GetComponent<RectTransform> ();
		KUIColorTriangle KCT = ColorTriangle.gameObject.GetComponent<KUIColorTriangle> ();

		RectTransform rtColorWheelCursor = ColorWheel.imageCursor.GetComponent<RectTransform> ();
		RectTransform rtColorTriangleCursor = ColorTriangle.imageCursor.GetComponent<RectTransform> ();
		RectTransform rtColorHueCursor = ColorHue.imageCursor.GetComponent<RectTransform> ();

		//Hue Cursor
		float w = rtColorHue.rect.width;
		Vector3 p = rtColorHue.position;
		p.x = p.x - w * 0.5f + w * newColor.h / 360.0f;
		rtColorHueCursor.position = p;

		//Triangle Cursor
		w = rtColorTriangle.rect.width;
		float h = rtColorTriangle.rect.height;
		p = rtColorTriangle.position;
		p.x = p.x - w * 0.5f + w * newColor.s;
		p.y = p.y - h * 0.5f + h * newColor.l;
		rtColorTriangleCursor.position = p;

		//Triangle Point position and Color
		KCT.color3 = saturatedColorRGB;
		KCT.pointCenter = saturatedColorHSL.l;
		KCT.SetVerticesDirty ();

		//Wheel
		w = rtColorWheel.rect.width;
		h = rtColorWheel.rect.height;
		p = rtColorWheel.position;
		p.x = p.x + Mathf.Cos ((newColor.h + 30.0f) * Mathf.Deg2Rad) * w * 0.5f * newColor.s;
		p.y = p.y + Mathf.Sin ((newColor.h + 30.0f) * Mathf.Deg2Rad) * h * 0.5f * newColor.s;
		rtColorWheelCursor.position = p;


		//Text
		ColorText.text = newColor.ToString ();
				
						
	}


}
