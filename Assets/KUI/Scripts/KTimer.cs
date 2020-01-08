using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class KTimer : MonoBehaviour {

    public float TimeInSeconds = 1.0f;
    public bool Loop = false;
    public UnityEvent OnTimer = new UnityEvent();

    private float CurrentTime = -1;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (CurrentTime >= 0) CurrentTime += Time.deltaTime;
        if (CurrentTime >= TimeInSeconds) {
            if (Loop) {
                CurrentTime = 0;
            } else {
                CurrentTime = -1;
            }
            OnTimer.Invoke();
        }
	}

    public void StartTimer() {
        CurrentTime = 0;
    }

    public void StartTimer(float TargetTimeInSeconds) {
        TimeInSeconds = TargetTimeInSeconds;
        CurrentTime = 0;
    }
}
