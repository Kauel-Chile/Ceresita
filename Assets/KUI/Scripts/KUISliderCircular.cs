using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class KUISliderCircular : Selectable, /*IPointerDownHandler, IPointerUpHandler, IMoveHandler,*/ IDragHandler, IScrollHandler {

	public enum Mode{percentValue,integerValue,floatValue};

	public Image imageFilledCircle = null;
	public KUITextManager text = null;
	public Mode mode = Mode.percentValue;

	public float valueSpeed = 0.005f;
	public float minValue = 0.0f;
	public float maxValue = 1.0f;
	public float CurrentValue = 1.0f;

	public Color ColorMin = Color.white;
	public Color ColorMax = Color.white;

	private float LastValue = 1.0f;
		
	// Update is called once per frame
	void Update () {

		if(imageFilledCircle == null){
			imageFilledCircle = GetComponentInChildren<Image>();
		}
		if(text == null) text = GetComponentInChildren<KUITextManager>();

		if(imageFilledCircle==null){
			Debug.LogWarning("No image filled asigned!");
			return;
		}
		if(text==null){
			Debug.LogWarning("No text KUITextManager asigned!");
			return;
		}

		//Update only on changes
		if(LastValue!=CurrentValue){
			LastValue=CurrentValue;
			if(mode==Mode.percentValue)	text.UpdateFromValuePercent(CurrentValue);
			if(mode==Mode.integerValue)	text.UpdateFromValueInteger(CurrentValue);
			if(mode==Mode.floatValue)	text.UpdateFromValueFloat(CurrentValue);
			imageFilledCircle.fillAmount = (CurrentValue - minValue) / (maxValue - minValue);
			imageFilledCircle.color = Color.Lerp(ColorMin,ColorMax,imageFilledCircle.fillAmount);
		}
	}
	
	// Interaction Demo
	/*public void OnPointerDown(PointerEventData eventData){

	}
	
	public void OnPointerUp(PointerEventData eventData){

	}
	
	public void OnMove(AxisEventData eventData){

	}*/
	
	public void OnScroll(PointerEventData eventData){
		CurrentValue += eventData.scrollDelta.y*valueSpeed;
		CurrentValue = Mathf.Clamp (CurrentValue, minValue, maxValue);
	}
	
	public void OnDrag(PointerEventData eventData){
		Vector2 delta = eventData.delta;
		CurrentValue += delta.y*valueSpeed;
		CurrentValue = Mathf.Clamp (CurrentValue, minValue, maxValue);
	}

}