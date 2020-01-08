using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;



public class KEvents : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, 
    IBeginDragHandler, IEndDragHandler, IDropHandler, ISelectHandler, IDeselectHandler, IMoveHandler, ISubmitHandler, ICancelHandler, IDragHandler {

    [Tooltip("Called when a pointer enters the object")]
    public UnityEvent OnPointEnter = new UnityEvent();

    [Tooltip("Called when a pointer exits the object")]
    public UnityEvent OnPointExit = new UnityEvent();

    [Tooltip("Called when a pointer is pressed on the object")]
    public UnityEvent OnPointDown = new UnityEvent();

    [Tooltip("Called when a pointer is released (called on the original the pressed object)")]
    public UnityEvent OnPointUp = new UnityEvent();

    [Tooltip("Called when a pointer is pressed and released on the same object")]
    public UnityEvent OnPointClick = new UnityEvent();

    [Tooltip("Called on the drag object when dragging is about to begin")]
    public UnityEvent OnDragBegin = new UnityEvent();

    [Tooltip("Called on the drag object when a drag finishes")]
    public UnityEvent OnDragEnd = new UnityEvent();

    [Tooltip("Called on the object where a drag finishes")]
    public UnityEvent OnDropBegin = new UnityEvent();

    [Tooltip("Called when the object becomes the selected object")]
    public UnityEvent OnSelected = new UnityEvent();

    [Tooltip("Called on the selected object becomes deselected")]
    public UnityEvent OnDeselected = new UnityEvent();

    [Tooltip("Called when a move event occurs in the object (left, right, up, down, etc)")]
    public UnityEvent OnMoved = new UnityEvent();

    [Tooltip("Called when the submit button is pressed")]
    public UnityEvent OnSubmited = new UnityEvent();

    [Tooltip("Called when the cancel button is pressed")]
    public UnityEvent OnCanceled = new UnityEvent();

    public void OnBeginDrag(PointerEventData eventData) {
        Debug.Log("On Begin Drag");
        OnDragBegin.Invoke();
    }

    public void OnCancel(BaseEventData eventData) {
        OnCanceled.Invoke();
    }

    public void OnDeselect(BaseEventData eventData) {
        OnDeselected.Invoke();
    }

    public void OnDrop(PointerEventData eventData) {
        OnDropBegin.Invoke();
    }

    public void OnEndDrag(PointerEventData eventData) {
        OnDragEnd.Invoke();
    }

    public void OnMove(AxisEventData eventData) {
        OnMoved.Invoke();
    }

    public void OnPointerClick(PointerEventData eventData) {
        OnPointClick.Invoke();
    }

    public void OnPointerDown(PointerEventData eventData) {
        OnPointDown.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData) {
        OnPointEnter.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData) {
        OnPointExit.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData) {
        OnPointUp.Invoke();
    }

    public void OnSelect(BaseEventData eventData) {
        OnSelected.Invoke();
    }

    public void OnSubmit(BaseEventData eventData) {
        OnDeselected.Invoke();
    }

    // Use this for initialization
    void Start () {
		
	}

    public void OnDrag(PointerEventData eventData) {
        //Do Nothing
    }

    // Update is called once per frame
    //void Update () {}
}
