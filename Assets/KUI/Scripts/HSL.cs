using UnityEngine;

public struct HSLColor {
	public float h;
	public float s;
	public float l;
	public float a;
	
	
	public HSLColor(float h, float s, float l, float a) {
		this.h = h;
		this.s = s;
		this.l = l;
		this.a = a;
	}
	
	public HSLColor(float h, float s, float l) {
		this.h = h;
		this.s = s;
		this.l = l;
		this.a = 1f;
	}
	
	public HSLColor(Color c) {
		HSLColor temp = FromRGBA(c);
		h = temp.h;
		s = temp.s;
		l = temp.l;
		a = temp.a;
	}
	
	public static HSLColor FromRGBA(Color c) {		
		float h, s, l, a;
		a = c.a;
		
		float cmin = Mathf.Min(Mathf.Min(c.r, c.g), c.b);
		float cmax = Mathf.Max(Mathf.Max(c.r, c.g), c.b);

		//Luma
		l = 0.3f * c.r + 0.59f * c.g + 0.11f * c.b;

		//Chroma
		s = cmax - cmin;

		//Hue
		if (cmin == cmax) {
			h = 0;
		} else {
			h = 0;
			if (c.r == cmax) {
				h = (c.g - c.b) / s;
			} else if (c.g == cmax) {
				h = 2f + (c.b - c.r) / s;
			} else if (c.b == cmax) {
				h = 4f + (c.r - c.g) / s;
			}
			
			h = Mathf.Repeat(h * 60f, 360f);
		}
		
		return new HSLColor(h, s, l, a);
	}
	
	
	public Color ToRGBA() {
		float r = 0;
		float g = 0;
		float b = 0;
		float a = this.a;
		this.h = Mathf.Repeat(this.h,360f);

		float Hp = this.h / 60.0f;
		float C = this.s;
		float X = C * (1 - Mathf.Abs (Hp % 2 - 1));
		float Y = this.l;
		if((0<=Hp)&&(Hp<1)){
			r=C;g=X;b=0;
		}else if((1<=Hp)&&(Hp<2)){
			r=X;g=C;b=0;
		}else if((2<=Hp)&&(Hp<3)){
			r=0;g=C;b=X;
		}else if((3<=Hp)&&(Hp<4)){
			r=0;g=X;b=C;
		}else if((4<=Hp)&&(Hp<5)){
			r=X;g=0;b=C;
		}else if((5<=Hp)&&(Hp<=6)){
			r=C;g=0;b=X;
		};
		float m = Mathf.Clamp01(Y - (0.3f * r + 0.59f * g + 0.11f * b));
		Vector3 v = new Vector3 (r + m, g + m, b + m);
		//v.Normalize ();
		float e = 0;
		if(v.x>1){e=v.x-1;v=v-new Vector3(e,e,e);}
		if(v.y>1){e=v.y-1;v=v-new Vector3(e,e,e);}
		if(v.z>1){e=v.z-1;v=v-new Vector3(e,e,e);}


		return new Color(v.x,v.y,v.z, a);
	}
	
	public static implicit operator HSLColor(Color src) {
		return FromRGBA(src);
	}
	
	public static implicit operator Color(HSLColor src) {
		return src.ToRGBA();
	}
	
	override public string ToString(){
		string result = "H:" + this.h.ToString ("F0");
		result += " S:" + (this.s*100.0f).ToString("F0")+"%";
		result += " L:" + (this.l*100.0f).ToString("F0")+"%";
		return result;
	}

}

