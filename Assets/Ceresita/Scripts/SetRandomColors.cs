using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetRandomColors : MonoBehaviour {

    public TextAsset Colores = null;
    public CopyColor[] Targets;
    public int DefaultColor = 500;
    public Kamera TheKamera = null;

	// Use this for initialization
	void Start () {
        LoadKolores();
        
	}

    public void LoadKolores() {
        if (TheKamera != null) TheKamera.Init();

        if ((Colores != null) && (Kolores.FullList.Count <= 0)) {
            Kolores.ParseFromFile(Colores);
            SetChildrenCount(Kolores.FullList.Count);
            Select("grises");
            foreach(CopyColor t in Targets) {
                if(t) t.CopyColorFromKolor(Kolores.FullList[DefaultColor]);
            }
        }

        int projectCount = KProjectManager.SearchForProjects();
        Debug.Log("Kauel: Proyectos existentes = " + projectCount);
    }
	
	// Update is called once per frame
	// void Update () {}

    public void Select(string categoria) {
        Kolores.Select(categoria);
        for (int i = 0;i < Kolores.FullList.Count; i++) {
            Kolores k = Kolores.FullList[i];
            Image img = transform.GetChild(k.index).GetComponent<Image>();
            KolorContainer kc = img.GetComponent<KolorContainer>();
            kc.Kolor = k;
            img.color = k.RGBA;
            if (k.Selected) {
                img.gameObject.SetActive(true);
            } else {
                img.gameObject.SetActive(false);
            }
        }
    }

    private void SetChildrenCount(int count) {
        if (count <= 0) {
            Debug.LogError("SetChildrenCount intentó eliminar a todos los hijos");
            return;
        }

        //Destruye hasta tener la cantidad de hijos correcta
        while (transform.childCount > count) {
            DestroyImmediate(transform.GetChild(transform.childCount - 1).gameObject);
        }

        //Clona hasta tener la cantidad de hijos correcta
        while (transform.childCount < count) {
            Instantiate(transform.GetChild(0).gameObject, transform);
        }
    }
}
