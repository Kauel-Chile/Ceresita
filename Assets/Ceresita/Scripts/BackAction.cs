using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackAction : MonoBehaviour {
    public bool closeApp = false;
    public KUIPanelManager kuiPanelManager;
    public int screenId;
    public string code = "";
    public GameObject[] Targets;



	// Update is called once per frame
	void Update() {
        if(Input.GetKeyUp(KeyCode.Escape)) {

            //Cierra la aplicación
            if(closeApp) {
                Application.Quit();

            //Procesa un código
            } else if (code != "") {
                Alert.Singleton.CloseAlert(true);
                ProcessCode(code);

            //Muestra una pantalla
            } else {
                Alert.Singleton.CloseAlert(true);
                if(kuiPanelManager) kuiPanelManager.ShowOnlyThisPanel(screenId); 
            }
        }
    }

    public void SetCode(string aCode) {
        code = aCode;
    }

    public void ProcessCode(string aCode) {

        var cols = aCode.Split(" "[0]);
        if (cols.Length <= 0) return;

        switch (cols[0]) {
            case "camera":
                if (kuiPanelManager) kuiPanelManager.ShowOnlyThisPanel(int.Parse(cols[1]));
                Kamera.Singleton.StartCamera();
                break;
            case "gallery":
                Targets[0].GetComponent<MobileGallery>().OpenGallery();
                break;
            case "projects":
                if (kuiPanelManager) kuiPanelManager.ShowOnlyThisPanel(int.Parse(cols[1]));
                break;
        }
    }
}
