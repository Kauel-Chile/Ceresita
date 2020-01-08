using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class UpdateLineRenderer : MonoBehaviour {

    private LineRenderer line;

    public Transform[] nodes = new Transform[2];

    // Use this for initialization
    void Start () {
        line = GetComponent<LineRenderer>();

    }
	
	// Update is called once per frame
	void Update () {
        line.positionCount = nodes.Length;
        for(int i = 0; i < nodes.Length; i++) {
            line.SetPosition(i, nodes[i].position);
        }
	}
}
