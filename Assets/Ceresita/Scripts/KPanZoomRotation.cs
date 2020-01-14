using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;

using System.Linq;



[RequireComponent(typeof(RectTransform), typeof(RawImage))]

public class KPanZoomRotation : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler {

    
    [Tooltip("Multiplicador para el movimiento de Translación 0 = Sin Translación")]
    public float TranslationMultiplier = 1;
    [Tooltip("Multiplicador para el movimiento de Rotación 0 = Sin Rotación")]
    public float RotationMultiplier = 1;
    [Tooltip("Multiplicador para el movimiento de Escalado 0 = Sin Escalado")]
    public float ScaleMultiplier = 1;
    [Tooltip("Flag que indica si se pintará o no")]
    public bool paint = true;
    
    private RectTransform rt = null; //Zona de interacción
    private RawImage rawImage = null; //Imagen con la Textura de Salida
    private Dictionary<int, int> TouchIDs = new Dictionary<int, int>(); //Diccionario para almacenar los touch que caigan dentro de la zona de interacción

    private Vector3 mouseLastPosition; //Ultima posicion del mouse

    private bool FloodFill = true; //Flag que indica si está permitido hacer floodfill



    // Use this for initialization

    void Start() {

        rt = GetComponent<RectTransform>(); //Zona de interacción

        rawImage = GetComponent<RawImage>(); //Imagen con la Textura de Salida

    }



    // Update is called once per frame

    void Update() {

        //int c = Input.touchCount;
        int c = TouchIDs.Count;

        //Debug.Log("Touch count = " + c);

        Vector2 deltaPos = Vector2.zero;
        float deltaRotation = 0;
        float scaleFactor = 1;



        Vector3 currentPos = Input.mousePosition;



        //Un solo toque, solo traslación

        if (c == 1) {

            int FirstKey = TouchIDs.Keys.First();

            //Debug.Log("Touch First Key ID = " + FirstKey);

            if (FirstKey >= 0) {

                Touch t0 = Input.touches[FirstKey];

                if (t0.phase != TouchPhase.Began) {

                    deltaPos = t0.deltaPosition * TranslationMultiplier;

                    rt.Translate(deltaPos.x, deltaPos.y, 0, Space.World);

                }



            //Mouse has ID -1

            } else {

                deltaPos = (currentPos - mouseLastPosition) * TranslationMultiplier;

                rt.Translate(deltaPos.x, deltaPos.y, 0, Space.World);



            }



        }


        //Dos toques permite traslación, rotación y escalado
        if (c > 1) {

            int FirstKey = TouchIDs.Keys.First();

            int LastKey = TouchIDs.Keys.Last();

            Touch t0 = Input.touches[FirstKey];

            Touch t1 = Input.touches[LastKey];

            Vector2 pos0 = t0.position;

            Vector2 pos1 = t1.position;

            Vector2 pos0old = t0.position - t0.deltaPosition;

            Vector2 pos1old = t1.position - t1.deltaPosition;



            if (t0.phase == TouchPhase.Began) pos0old = pos0;

            if (t1.phase == TouchPhase.Began) pos1old = pos1;



            Vector2 v0 = pos1old - pos0old;

            Vector2 v1 = pos1 - pos0;



            //Traslación acorde a la traslación del centro de masa

            if (TranslationMultiplier != 0.0f) {

                deltaPos = (pos0 + pos1) * 0.5f - (pos0old + pos1old) * 0.5f;

                deltaPos *= TranslationMultiplier;

                rt.Translate(deltaPos.x, deltaPos.y, 0, Space.World);

            }



            //Rotación acorde al cambio de angulo entre los toques

            if (RotationMultiplier != 0.0f) {

                float angle1 = Mathf.Atan2(v1.y, v1.x);

                float angle0 = Mathf.Atan2(v0.y, v0.x);

                deltaRotation = (angle1 - angle0) * Mathf.Rad2Deg;

                deltaRotation *= RotationMultiplier;

                rt.Rotate(0, 0, deltaRotation);

            }



            //Escalado acorde al cambio de escala

            if (ScaleMultiplier != 0.0f) {



                //Mueve el pivote sin mover el objeto

                Vector2 sd = rt.sizeDelta;



                Vector2 centerPos = (pos0 + pos1) * 0.5f;

                Vector2 localPos = rt.InverseTransformPoint(centerPos.x, centerPos.y, 0);

                localPos.x = localPos.x / sd.x + 0.5f;

                localPos.y = localPos.y / sd.y + 0.5f;

                //Siguiente linea comentada por actualizacion de Plugin
                //rt.SetPivot(localPos);
                rt.pivot = (localPos);


                /*Vector2 deltaPivot = rt.pivot - localPos;

                Vector3 deltaPosition = new Vector3(deltaPivot.x * sd.x, deltaPivot.y * sd.y);

                

                rt.pivot = localPos;

                rt.localPosition -= deltaPosition;*/



                //Escala en torno al nuevo pivote

                scaleFactor = v1.magnitude / (v0.magnitude + float.Epsilon);

                scaleFactor = (scaleFactor - 1) * ScaleMultiplier + 1;

                

                rt.sizeDelta = sd * scaleFactor;



                //Reestable el pivote


                //Siguiente linea comentada por actualizacion de Plugin
                //rt.SetPivot(new Vector2(0.5f, 0.5f));

                rt.pivot = new Vector2(0.5f, 0.5f);
                                

            }



        }



        mouseLastPosition = currentPos;



    }



    



    public void OnDrag(PointerEventData eventData) {

        FloodFill = false;

        TouchIDs[eventData.pointerId] = eventData.pointerId;

    }



    public void OnBeginDrag(PointerEventData eventData) {

        FloodFill = false;

        TouchIDs[eventData.pointerId] = eventData.pointerId;

    }



    public void OnEndDrag(PointerEventData eventData) {

        FloodFill = false;

        if (TouchIDs.ContainsKey(eventData.pointerId)) {

            TouchIDs.Remove(eventData.pointerId);

        }

    }



    public void OnPointerDown(PointerEventData eventData) {

        FloodFill = true;

        TouchIDs[eventData.pointerId] = eventData.pointerId;

    }



    public void OnPointerUp(PointerEventData eventData) {

        if (TouchIDs.ContainsKey(eventData.pointerId)) {

            TouchIDs.Remove(eventData.pointerId);

        }

    }



    



    private Vector2 TransformEventDataPositionToPixelCoords(Vector3 EventDataPosition) {

        Vector2 result = Vector2.zero;

        if(!rt) rt = GetComponent<RectTransform>();

        if(!rawImage) rawImage = GetComponent<RawImage>();

        Vector2 sd = rt.sizeDelta;

        if ((sd.x <= 0) || (sd.y <= 0)) return result;

        int TextureWidth = rawImage.texture.width;

        int TextureHeight = rawImage.texture.height;



        Vector3 localpos = rt.InverseTransformPoint(EventDataPosition);

        Vector3 normalizedpos = localpos;

        normalizedpos.x = normalizedpos.x / sd.x + 0.5f;

        normalizedpos.y = normalizedpos.y / sd.y + 0.5f;

        Vector2 pixelpos = normalizedpos;

        pixelpos.x = pixelpos.x * (TextureWidth - 1);

        pixelpos.y = pixelpos.y * (TextureHeight - 1);

        return pixelpos;

    }



    public void OnPointerClick(PointerEventData eventData) {

        Debug.Log("Kauel: OnPointerClick");



        if (paint) {

            int TextureWidth = rawImage.texture.width;

            int TextureHeight = rawImage.texture.height;



            Vector2 pixelpos = TransformEventDataPositionToPixelCoords(eventData.position);

            if ((pixelpos.x >= 0) && (pixelpos.y >= 0) && (pixelpos.x < TextureWidth) && (pixelpos.y < TextureHeight)) {

                if (FloodFill) Kamera.Singleton.GrabCut3(pixelpos, Kamera.PAINTING_MODE.FLOODFILL);

            }

            

        }



        FloodFill = true; //Vuelve a activar el Floodfill

    }



    public void ResetPositionAndSize() {

        if (!rt) rt = GetComponent<RectTransform>();

        if (!rawImage) rawImage = GetComponent<RawImage>();

        Debug.Log("Kauel: RectTransform w:" + rt.rect.width + "h:" + rt.rect.height);

        rt.localPosition = new Vector3(0, 0, 0);

        rt.localEulerAngles =  new Vector3(0, 0, 0);

        rt.localScale = new Vector3(1, 1, 1);

        rt.pivot = new Vector2(0.5f, 0.5f);

        

        float texW = rawImage.texture.width;

        float texH = rawImage.texture.height;

        Debug.Log("Kauel: rawimage w:" + texW + "h:" + texH);


        if ((texW <= 0) || (texH <= 0)) return;



        float factor1 = 1920 / texW;

        float h1 = texH * factor1;

        if (h1 <= 1080) {

            rt.sizeDelta = new Vector2(1920, h1);

        } else {

            float factor2 = 1080 / texH;

            float w2 = texW * factor2;

            rt.sizeDelta = new Vector2(w2, 1080);

        }

        Debug.Log("Kauel: RectTransform2 w:" + rt.rect.width + "h:" + rt.rect.height);
        Debug.Log("Kauel: screenSize w:" + Screen.width + "h:" + Screen.height);

    }

}