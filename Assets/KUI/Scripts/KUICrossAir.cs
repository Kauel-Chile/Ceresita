using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class KUICrossAir : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IMoveHandler, IDragHandler, IScrollHandler {
	
	public Image imageCursor = null;
	private RectTransform rectTransform = null;
	public bool Horizontal = true;
	public bool Vertical = true;
	public float RadiusLimit = -1;

	// Event to be assigned
	public UnityEvent OnColorChange = new UnityEvent();
		
	// Use this for initialization
	void Start () {
		if(imageCursor == null)	imageCursor = GetComponentInChildren<Image>();
		rectTransform = GetComponent<RectTransform>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	// Interaction Demo
	public void OnPointerDown(PointerEventData eventData){
		//Debug.Log(eventData.ToString());
		Vector2 pCursor = imageCursor.rectTransform.position;
		Vector2 pEvent  = eventData.position;
		if(Horizontal) pCursor.x = pEvent.x;
		if(Vertical) pCursor.y = pEvent.y;
		imageCursor.rectTransform.position = pCursor;
		ApplyRectRestriction ();
		ApplyRadiusRestriction ();
		OnColorChange.Invoke ();
		//eventData.Use ();
	}
	
	public void OnPointerUp(PointerEventData eventData){
		//Debug.Log(eventData.ToString());
	}
	
	public void OnMove(AxisEventData eventData){
		//Debug.Log("Move");
	}
	
	public void OnScroll(PointerEventData eventData){
		//Debug.Log("Scroll");
		Vector2 pCursor = imageCursor.rectTransform.position;
		Vector2 pEvent = eventData.scrollDelta;
		if(Vertical) pCursor.y += pEvent.y;
		else if(Horizontal) pCursor.x += pEvent.y;
		imageCursor.rectTransform.position = pCursor;
		ApplyRectRestriction ();
		ApplyRadiusRestriction ();
		OnColorChange.Invoke ();
		//eventData.Use ();
	}
	
	public void OnDrag(PointerEventData eventData){
		//Debug.Log(eventData.ToString());
		Vector2 pCursor = imageCursor.rectTransform.position;
		Vector2 pEvent  = eventData.position;
		if(Horizontal) pCursor.x = pEvent.x;
		if(Vertical) pCursor.y = pEvent.y;
		imageCursor.rectTransform.position = pCursor;
		ApplyRectRestriction ();
		ApplyRadiusRestriction ();
		OnColorChange.Invoke();
		//eventData.Use ();
	}

	public void ApplyRectRestriction(){
		Vector2 pCursor = imageCursor.rectTransform.position;

		if(pCursor.x<rectTransform.position.x-rectTransform.rect.width*0.5f)  pCursor.x=rectTransform.position.x-rectTransform.rect.width*0.5f;
		if(pCursor.x>rectTransform.position.x+rectTransform.rect.width*0.5f)  pCursor.x=rectTransform.position.x+rectTransform.rect.width*0.5f;
		if(pCursor.y<rectTransform.position.y-rectTransform.rect.height*0.5f) pCursor.y=rectTransform.position.y-rectTransform.rect.height*0.5f;
		if(pCursor.y>rectTransform.position.y+rectTransform.rect.height*0.5f) pCursor.y=rectTransform.position.y+rectTransform.rect.height*0.5f;

		imageCursor.rectTransform.position = pCursor;
	}

	public void ApplyRadiusRestriction(){
		Vector2 pCursor = imageCursor.rectTransform.position;
		Vector2 pCentral = rectTransform.position;
		if (RadiusLimit >= 0) {
			Vector2 delta = pCursor - pCentral;
			if(delta.magnitude>RadiusLimit) delta = delta.normalized * RadiusLimit;
			pCursor = pCentral + delta;
		}
		imageCursor.rectTransform.position = pCursor;

	}

}
