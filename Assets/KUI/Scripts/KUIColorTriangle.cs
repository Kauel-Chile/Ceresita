using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


[ExecuteInEditMode]
public class KUIColorTriangle : MaskableGraphic 
{
	public enum Direction {up,down,left,right};

	public Color color2 = Color.black;
	public Color color3 = Color.red;
	[Range(0f, 1f)]
	public float masterAlpha = 1.0f;

	[Range(0f, 1f)]
	public float pointCenter = 0.5f;

	public Direction direction = Direction.right;

	private float lastmasterAlpha = 1.0f;

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

        switch (direction) {
            case Direction.right:
                corner1.x = 0f;
                corner1.y = 0f;
                corner2.x = 0f;
                corner2.y = 1f;
                corner3.x = 1f;
                corner3.y = pointCenter;
                break;
            case Direction.left:
                corner1.x = 1f;
                corner1.y = 0f;
                corner2.x = 1f;
                corner2.y = 1f;
                corner3.x = 0f;
                corner3.y = pointCenter;
                break;
            case Direction.up:
                corner1.x = 0f;
                corner1.y = 0f;
                corner2.x = 1f;
                corner2.y = 0f;
                corner3.x = pointCenter;
                corner3.y = 1.0f;
                break;
            case Direction.down:
                corner1.x = 0f;
                corner1.y = 1f;
                corner2.x = 1f;
                corner2.y = 1f;
                corner3.x = pointCenter;
                corner3.y = 0.0f;
                break;
            default:
                corner1.x = 0f;
                corner1.y = 0f;
                corner2.x = 0f;
                corner2.y = 1f;
                corner3.x = 1f;
                corner3.y = pointCenter;
                break;
        }

        corner1.x -= rectTransform.pivot.x;
        corner1.y -= rectTransform.pivot.y;
        corner2.x -= rectTransform.pivot.x;
        corner2.y -= rectTransform.pivot.y;
        corner3.x -= rectTransform.pivot.x;
        corner3.y -= rectTransform.pivot.y;

        corner1.x *= rectTransform.rect.width;
        corner1.y *= rectTransform.rect.height;
        corner2.x *= rectTransform.rect.width;
        corner2.y *= rectTransform.rect.height;
        corner3.x *= rectTransform.rect.width;
        corner3.y *= rectTransform.rect.height;

        UIVertex[] vert = new UIVertex[4];
        vert[0] = UIVertex.simpleVert;
        vert[1] = UIVertex.simpleVert;
        vert[2] = UIVertex.simpleVert;
        vert[3] = UIVertex.simpleVert;

        vert[0].position = new Vector2(corner1.x, corner1.y);
        vert[0].color = color;
        //vbo.AddVert(vert);

        vert[1].position = new Vector2(corner2.x, corner2.y);
        vert[1].color = color2;
        //vbo.AddVert(vert);

        vert[2].position = new Vector2(corner3.x, corner3.y);
        vert[2].color = color3;
        //vbo.AddVert(vert);

        vert[3].position = new Vector2(corner3.x, corner3.y);
        vert[3].color = color3;
        //vbo.AddVert(vert);

        vbo.AddUIVertexQuad(vert);

        //vbo.FillMesh(toFill);
        

    }

}