using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;

public class Kolores : Object {

    public string Code;
    public string Name;
    public string FullName;
    public string tag;
    public Color RGBA;
    public Vector3 HSL;
    public int index;
    public bool Selected = false;

    public static Color hexToColor(string hex) {
        hex = hex.ToUpperInvariant();
        byte a = 255;//assume fully visible unless specified in hex
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        return new Color32(r, g, b, a);
    }

    public static Vector3 RGBtoHCV(Vector3 RGB) {
        // Based on work by Sam Hocevar and Emil Persson
        Vector4 P = (RGB.y < RGB.z) ? new Vector4(RGB.z, RGB.y, -1.0f, 2.0f / 3.0f) : new Vector4(RGB.y,RGB.z, 0.0f, -1.0f / 3.0f);
        Vector4 Q = (RGB.x < P.x) ? new Vector4(P.x,P.y,P.w, RGB.x) : new Vector4(RGB.x, P.y, P.z, P.x);
        float C = Q.x - Mathf.Min(Q.w, Q.y);
        float H = Mathf.Abs((Q.w - Q.y) / (6 * C + Mathf.Epsilon) + Q.z);
        return new Vector3(H, C, Q.x);
    }

    public static Vector3 RGBtoHSL(Vector3 RGB) {
        Vector3 HCV = RGBtoHCV(RGB);
        float L = HCV.z - HCV.y * 0.5f;
        float S = HCV.y / (1 - Mathf.Abs(L * 2 - 1) + Mathf.Epsilon);
        return new Vector3(HCV.x, S, L);
    }

    public static List<Kolores> FullList = new List<Kolores>();
    public static List<Kolores> SelectedList = new List<Kolores>();
    public static void ParseFromFile(TextAsset file) {
        string fs = file.text;
        string[] Lines = Regex.Split(fs, "\n|\r|\r\n");
        fs = null;
        FullList.Clear();
        //var Lines = File.ReadAllLines(filename);
        for(int i = 0; i < Lines.Length; i++) {
            var Col = Lines[i].Split(";"[0]);

            if (Col.Length >= 5) {
                Kolores K = new Kolores();
                K.Code = Col[0];
                K.Name = Col[3];
                K.FullName = Col[1];
                K.RGBA = hexToColor(Col[4]);
                K.HSL = RGBtoHSL(new Vector3(K.RGBA.r, K.RGBA.g, K.RGBA.b));
                K.index = FullList.Count;
                K.Selected = false;

                //Solo algunos colores tienen este campo. 
                //Se implemento inicialmente para identificar manualmente 
                //los colores por grupos, por ejemplo: los grises.
                if (Col.Length > 5) {
                    K.tag = Col[5];
                } else {
                    K.tag = "None";
                }

                FullList.Add(K);
            }
            Col = null;
        }
        Lines = null;
        Debug.Log("Kauel: Total de Colores = " + FullList.Count);
    }

    public static void ClearSelectStatus(bool status) {
        for (int i = 0; i < FullList.Count; i++) {
            Kolores k = FullList[i];
            k.Selected = status;
        }
    }

    public static void Select(string categoria) {
        SelectedList.Clear();
        ClearSelectStatus(false);
        for(int i = 0; i < FullList.Count; i++) {
            Kolores k = FullList[i];
            bool add = false;
            float hue = k.HSL.x * 360;
            float sat = k.HSL.y;
            float lum = k.HSL.z;
            float deltahue = 40; //60
            float minsat = 0.2f; //0.1
            string cat = categoria.ToLowerInvariant();

            if (k.tag == cat) {
                add = true;
                Debug.Log("Kauel: Selected by Tag " + k.Code);
            }

            switch (cat) {
                case "verdes":
                    if ((hue > 135 - deltahue) && (hue < 135 + deltahue) && (sat > minsat)) add = true;
                    break;
                case "purpuras":
                    if ((hue > 270 - deltahue) && (hue < 270 + deltahue) && (sat > minsat)) add = true;
                    break;
                case "azules":
                    if ((hue > 240 - deltahue * 0.5f) && (hue < 240 + deltahue * 0.5f) && (sat > minsat)) add = true;
                    break;
                case "blancos":
                    if ( (sat < 0.2f) && (lum > 0.7f) ) add = true;
                    break;
                case "aqua":
                    if ((hue > 180 - deltahue * 0.5f) && (hue < 180 + deltahue * 0.5f) && (sat > minsat)) add = true;
                    break;
                case "grises":
                    if (k.tag.CompareTo("gray") == 0)
                        add = true;
                    break;
                case "amarillos":
                    if ((hue > 60 - deltahue * 0.25f) && (hue < 60 + deltahue * 0.25f) && (sat > minsat)) add = true;
                    break;
                case "cafes":
                    if ((hue > 30 - deltahue * 0.25f) && (hue < 30 + deltahue * 0.25f) && (sat > minsat) && (sat < 0.5f) ) add = true;
                    break;
                case "naranjos":
                    if ((hue > 30 - deltahue * 0.25f) && (hue < 30 + deltahue * 0.25f) && (sat > 0.5f) ) add = true;
                    break;
                case "rojos":
                    if (( (hue < deltahue * 0.25f) || (hue > 360 - deltahue * 0.25f) ) && (sat > 0.35f)) add = true;
                    break;
                case "rosados":
                    if ((hue > 320 - deltahue * 0.5f) && (hue < 320 + deltahue * 0.5f) && (sat > minsat)) add = true;
                    break;
                case "todos":
                    add = true;
                    break;
                default:
                    add = true;
                    break;
            }
            if (add) {
                k.Selected = true;
                SelectedList.Add(k);
            };
        }
        Debug.Log("Seleccionados = " + SelectedList.Count);

    }







}
