using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KColorManager : MonoBehaviour {

    public static KColorManager instance = null;

    public Color Normal = Color.white;
    public Color Highlighted = Color.yellow;
    public Color Pressed = Color.red;
    public Color Disabled = Color.gray;
    public Color LightBackground = Color.white;
    public Color DarkBackground = Color.black;
    public Color LightForeground = Color.white;
    public Color DarkForeground = Color.black;

    // Use this for initialization
    void Start () {
        instance = this;
	}
	
}
