using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//Copia el color desde un Objeto Kolores o KolorContainer a un objeto Image
public class CopyColor : MonoBehaviour {

    public Material TargetMaterial = null;
    public TextMeshProUGUI TargetText = null;
    public int TargetColor = 0; //Color Objetivo en el Material
    [HideInInspector]
    public Kolores SelectedKolor = null;

    public void CopyColorFromImage(GameObject go) {
        KolorContainer kc = go.GetComponent<KolorContainer>();
        if (kc != null) CopyColorFromKolor(kc.Kolor);
    }

    public void CopyColorFromKolorContainer(KolorContainer kc) {
        CopyColorFromKolor(kc.Kolor);
    }

    public void CopyColorFromKolor(Kolores kolor) {
        SelectedKolor = kolor;
        Image img2 = GetComponent<Image>();

        if (img2) img2.color = kolor.RGBA;

        if (TargetMaterial) {
            string stargetcolor = "_TargetColor" + TargetColor;
            string stargetlum = "_LumWall" + TargetColor;

            TargetMaterial.SetColor(stargetcolor, kolor.RGBA);
            float targetlum = (kolor.HSL.z - 0.5f) * 0.5f + 0.75f;
            TargetMaterial.SetFloat(stargetlum, targetlum);
        }

        if (TargetText) TargetText.text = kolor.FullName;
    }
}
