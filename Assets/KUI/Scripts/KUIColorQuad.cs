using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


[ExecuteInEditMode]
public class KUIColorQuad : MaskableGraphic 
{
	public Color color2 = Color.black;
	public Color color3 = Color.red;
	public Color color4 = Color.blue;
	[Range(0f, 1f)]
	public float masterAlpha = 1.0f;
	private float lastmasterAlpha = 1.0f;
	public float tilt = 0.0f;

	void Update(){
		if(masterAlpha!=lastmasterAlpha){
			lastmasterAlpha=masterAlpha;
			canvasRenderer.SetAlpha (masterAlpha);
		}
	}

    protected override void OnPopulateMesh(VertexHelper vbo) {
        //toFill.Clear();
        //var vbo = new VertexHelper(toFill);

        Vector2 corner1 = Vector2.zero;
		Vector2 corner2 = Vector2.zero;
		Vector2 corner3 = Vector2.zero;
		Vector2 corner4 = Vector2.zero;

		corner1.x = 0f;
		corner1.y = 0f;
		corner2.x = 0f;
		corner2.y = 1f;
		corner3.x = 1f;
		corner3.y = 1f;
		corner4.x = 1f;
		corner4.y = 0f;

		if(tilt>0){
			corner2.x+=tilt;corner4.x-=tilt;
		}else if(tilt<0){
			corner1.x-=tilt;corner3.x+=tilt;
		}

		corner1.x -= rectTransform.pivot.x;
		corner1.y -= rectTransform.pivot.y;
		corner2.x -= rectTransform.pivot.x;
		corner2.y -= rectTransform.pivot.y;
		corner3.x -= rectTransform.pivot.x;
		corner3.y -= rectTransform.pivot.y;
		corner4.x -= rectTransform.pivot.x;
		corner4.y -= rectTransform.pivot.y;

		corner1.x *= rectTransform.rect.width;
		corner1.y *= rectTransform.rect.height;
		corner2.x *= rectTransform.rect.width;
		corner2.y *= rectTransform.rect.height;
		corner3.x *= rectTransform.rect.width;
		corner3.y *= rectTransform.rect.height;
		corner4.x *= rectTransform.rect.width;
		corner4.y *= rectTransform.rect.height;

        UIVertex[] vert = new UIVertex[4];
        vert[0] = UIVertex.simpleVert;
        vert[1] = UIVertex.simpleVert;
        vert[2] = UIVertex.simpleVert;
        vert[3] = UIVertex.simpleVert;

        vert[0].position = corner1;
		vert[0].color = color;
		
		vert[1].position = corner2;
		vert[1].color = color2;
		
		vert[2].position = corner3;
		vert[2].color = color3;
		
		vert[3].position = corner4;
		vert[3].color = color4;

		vbo.AddUIVertexQuad(vert);
	}
}