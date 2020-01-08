using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent (typeof(CanvasGroup))]
public class KUIPanelFader : MonoBehaviour {

	public float AlphaStart = 0f;
	public float AlphaNormal = 1.0f;
	public float AlphaEnd = 0f;
	private float masterAlpha = 1.0f;

	public Vector3 ScaleStart = Vector3.one;
	public Vector3 ScaleNormal = Vector3.one;
	public Vector3 ScaleEnd = Vector3.zero;
	private Vector3 masterScale = Vector3.one;

	public Vector3 RotationStart = Vector3.zero;
	public Vector3 RotationNormal = Vector3.zero;
	public Vector3 RotationEnd = Vector3.zero;
	public Vector3 masterRotation = Vector3.zero;

	public  bool FadeOut = false;
	public float FadeSpeed = 4.0f;
    public float Delay = 0.0f;

    public bool StartHidden = false;

	private bool FadeIn = false;
	private float FadeTime = 0;

	private CanvasGroup canvasgroup = null;
	private RectTransform rect = null;

    public Vector3 OscillationAFP = Vector3.zero;

    public UnityEvent OnFadeInFinalized = new UnityEvent();
    public UnityEvent OnFadeOutFinalized = new UnityEvent();

    // Use this for initialization
    void Start() {
		canvasgroup = GetComponent<CanvasGroup>();
		rect = GetComponent<RectTransform>();
		FadeIn = true;
		FadeOut = false;
        FadeTime = 0;

        masterAlpha = AlphaStart;
        masterScale = ScaleStart;
        masterRotation = RotationStart;

        if (StartHidden) {
            gameObject.SetActive(false);
        } else {
            //masterAlpha = AlphaNormal;
            //masterScale = ScaleNormal;
            //masterRotation = RotationNormal;
        }

        if (canvasgroup != null) canvasgroup.alpha = masterAlpha;
        if (rect != null) {
            rect.localScale = masterScale;
            rect.localRotation = Quaternion.Euler(masterRotation);
        }

    }

	void OnEnable() {
        FadeTime = 0;
        FadeIn = true;
		FadeOut = false;
	}

	public void ActivateWithFadeIn(){
        StartHidden = false;
        FadeTime = 0;
        FadeIn = true;
		FadeOut = false;
		gameObject.SetActive(true);
    }

    public void FadeOutAndDesactivate() {
        if (!gameObject.activeSelf) return;
        FadeIn = false;
        FadeOut = true;
        FadeTime = 0;
        DoFadeOut();
    }

    public void Toogle() {
        if (gameObject.activeSelf) {
            FadeOutAndDesactivate();
        } else {
            ActivateWithFadeIn();
        }
    }

    // Update is called once per frame
    void Update(){

        //Oscilacion
        if (OscillationAFP.x != 0) {
            masterRotation.z = OscillationAFP.x*Mathf.Cos(Time.timeSinceLevelLoad * OscillationAFP.y + OscillationAFP.z);
        }


		if(FadeIn) DoFadeIn();
		if(FadeOut) DoFadeOut();
		if(canvasgroup!=null) canvasgroup.alpha = masterAlpha;
		if (rect!=null){
			rect.localScale = masterScale;
			rect.localRotation = Quaternion.Euler (masterRotation);
		}
	}

	void DoFadeIn(){
		FadeTime += Time.deltaTime*FadeSpeed;
        float ft = Mathf.Clamp01(FadeTime-Delay*FadeSpeed);
        masterAlpha 	=   Mathf.Lerp (AlphaStart, 	AlphaNormal, ft);
		masterScale 	= Vector3.Lerp (ScaleStart, 	ScaleNormal, ft);
		masterRotation 	= Vector3.Lerp (RotationStart, 	RotationNormal, ft);
		if(ft>=1.0f){
			FadeIn=false;
			FadeTime=0;
			masterAlpha=AlphaNormal;
			masterScale=ScaleNormal;
			masterRotation=RotationNormal;
            OnFadeInFinalized.Invoke();
		}
	}

	void DoFadeOut(){
        FadeTime += Time.deltaTime * FadeSpeed;
        float ft = Mathf.Clamp01(FadeTime);
        masterAlpha = Mathf.Lerp(AlphaNormal, AlphaEnd, ft);
        masterScale = Vector3.Lerp(ScaleNormal, ScaleEnd, ft);
        masterRotation = Vector3.Lerp(RotationNormal, RotationEnd, ft);
        if (ft>=1.0f){
			FadeOut=false;
			FadeTime=0;
			masterAlpha=AlphaEnd;
			masterScale=ScaleEnd;
			masterRotation=RotationEnd;
            gameObject.SetActive(false);
            OnFadeOutFinalized.Invoke();
        }
	}
}
