using System;

using System.Collections;

using System.Collections.Generic;

using UnityEngine;

using UnityEngine.UI;

using Emgu.CV;

using Emgu.CV.CvEnum;

using Emgu.CV.Structure;

using Emgu.CV.XPhoto;

using Emgu.CV.Util;

using System.Runtime.InteropServices;

using UnityEngine.Events;

using UnityEngine.EventSystems;

using System.Drawing;

using NatCam;

using Image = UnityEngine.UI.Image;

using K.EmguCVExtensions;


public class Kamera : MonoBehaviour {



    //Singleton

    private static Kamera singleton = null;

    public static Kamera Singleton {

        get {

            if (singleton == null) singleton = FindObjectOfType<Kamera>();

            return singleton;

        }

    }



    public enum KameraMode {Camera, File};

    [Tooltip("Modo Camara o Abrir archivo desde la galería")]

    public KameraMode Mode = KameraMode.Camera;



    [Tooltip("RawImage para la cámara")]

    public RawImage RawImageCamera = null;

    [Tooltip("RawImage para edicion")]

    public RawImage RawImageEdit = null;

    [HideInInspector]

    public Texture2D OutputTexture = null; //Textura de salida del procesamiento digital de imagen.

    [HideInInspector]

    public Texture2D OutputMaskTexture = null; //Textura con la mascara de pixeles que deben cambiar de color.



    //private WebCamTexture CameraTexture = null; //Textura de la cámara.

    private Color32[] CameraColors32 = null; //Buffer de 32 bytes por pixel. (RGBA o ARGB o BGRA o ABGR)

    private Mat CameraMat32 = null; //Matriz de OpenCV apuntando a la memoria del Buffer de 32 bytes por pixel.

    private Byte[] CameraColors24 = null; //Buffer de 24 bytes por pixel. (RGB o BGR)

    private Mat CameraMat24 = null; //Matriz de OpenCV apuntando a la memoria del Buffer de 24 bytes por pixel.

    private Byte[] WallMaskBuffer = null; //Buffer Mascara.

    private Mat WallMask = null; //Matriz de OpenCV apuntando a la memoria del Buffer Mascara.



    //buffer para Undo

    private List<Mat> Undo = null;

    public int UndoMax = 10;



    [Tooltip("Umbral para FloodFill Absoluto (recomendado 32). Los colores con diferencia menor a este umbral respecto del pixel seleccionado serán considerados como píxeles del mismo color")]

    public float DeltaColor1 = 32;

    [Tooltip("Umbral para FloodFill Relativo (recomendado 2). Los colores con diferencia menor a este umbral respecto del pixel vecino serán considerados como píxeles del mismo color")]

    public float DeltaColor2 = 1;



    [Tooltip("Apertura para el filtro de Bordes (recomendado 1, 3 o 5). Indica el tamaño de la vecindad para calcular si un pixel corresponde a un borde o no")]

    public int BorderAperture = 1;



    [Tooltip("Parámetro del Filtro Canny para detectar bordes")]

    public float CannyLow = 20;

    [Tooltip("Parámetro del Filtro Canny para detectar bordes")]

    public float CannyHigh = 100;

    [Tooltip("Parámetro del Filtro Canny para detectar bordes")]

    public int CannyAperture = 3;

    [Tooltip("Umbral de contraste respecto al promedio de la vecindad para decidir si un pixel es borde o no (recomendado 32)")]

    public float Contrast = 32;

    [Tooltip("Apertura (vecindad) para decidir si un pixel es borde o no (recomendado 32)")]

    public int ContrastAperture = 9;



    [Tooltip("Resolucion radial (recomendado 1)")]

    public float HoughLineRho = 1;

    [Tooltip("Resolucion angular en radianes (recomendado PI/180)")]

    public float HoughLineAngle = 0.017453f;

    [Tooltip("Cuantos votos se requieren para que la posible línea sea considerada como línea")]

    public int HoughLineThreshold = 10;

    [Tooltip("Largo mínimo de la línea")]

    public float HoughLineMinLineLength = 10;

    [Tooltip("Máxima distancia entre líneas")]

    public float HoughLineMaxGap = 10;

    [Tooltip("Variable para Debug")]

    public bool testing = false;



    [Tooltip("Logo para compartir en redes sociales")]

    public Texture2D Corner = null;

    [Tooltip("Objeto que contiene el color seleccionado")]

    public CopyColor[] SelectedColors = null;

    [Tooltip("Índice del color seleccionado, 0=PrimerColor, 1=SegundoColor")]

    public int SelectedColorIndex = 0;



    private Mat EdgeMap = null; //Mapa con los bordes de la imagen para separar una zona de otra al momento de hacer floodfill.



    [Tooltip("Control de Paneles para fotografía")]

    public KUIPanelManager PanelSelector = null;

    [Tooltip("Canvas principal de la aplicación")]

    public KUIPanelManager Canvas = null;



    [Tooltip("Material sobre el cual se aplicarán las texturas OutputTexture y OutputMaskTexture obtenidas con OpenCV")]

    public Material TargetMaterial = null;



//    [HideInInspector]

    //public byte[] EncodedImage = null; //Bytes correspondientes a la imagen PNG

    [HideInInspector]

    public byte[] EncodedImageAsPNG = null; //Bytes correspondientes a la imagen PNG

    public Texture2D RawTexture2D;



    public enum PAINTING_MODE { BRUSH, FLOODFILL, ERASER }



    //Constantes

    public static MCvScalar Black = new MCvScalar(0, 0, 0, 0);

    public static MCvScalar White = new MCvScalar(255, 255, 255, 255);

    public static MCvScalar Pink = new MCvScalar(255, 0, 255);

    public static MCvScalar Red = new MCvScalar(255, 0, 0);

    public static MCvScalar Green = new MCvScalar(0, 255, 0);

    public static MCvScalar Blue = new MCvScalar(0, 0, 255);

    public static Point  Anchor = new Point(-1,-1);

    private bool CamCreated = false;

    private DeviceCamera NatCamCamera;
    private Texture previewTexture;
    
    private Texture2D photo;
    // Use this for initialization
    //private AspectRatioFitter aspectFitter;


    void Start () {

        Debug.Log("Kauel: Start()");

        singleton = this;
    }



    public void StartCamera() {

        if (NatCamCamera == null) {

            NatCamCamera = DeviceCamera.FrontCamera;
            //NatCamCamera.StartPreview(OnStart, OnFrame);
            NatCamCamera.StartPreview(OnStart);
        }

        
        Debug.Log("Kauel: StartCamera()");

        singleton = this;

        Mode = KameraMode.Camera;

        RawImageCamera.texture = previewTexture;
        //RawImageCamera.texture = preview;
        //previewTexture = preview;
        
        #if UNITY_EDITOR
          //  NatCamCamera = CameraDevice.GetDevices()[0];

        #elif UNITY_ANDROID

            Debug.Log("Kauel: Android");

          /*  if (NatCam.HasPermissions) {

                Debug.Log("Kauel: NatCam permissions OK :)");

            } else {

                Debug.Log("Kauel: NatCam has NO PERMISSIONS!!!");

            } 

            NatCamCamera = CameraDevice.GetDevices()[0];
          */

            if(!CamCreated){

                //CamCreated = true;
                //NatCam.Camera = DeviceCamera.RearCamera;
                //var NatCamCamera = CameraDevice.GetDevices()[0]; 
                //NatCamCamera.SetPreviewResolution(ResolutionPreset.FullHD);
                //NatCamCamera.SetPhotoResolution(ResolutionPreset.FullHD);
                //NatCamCamera.SetFramerate(FrameratePreset.Default);
            
                Debug.Log("Kauel: CamCreated");

            }


		#elif UNITY_IOS

		Debug.Log("Kauel: iOS");



		if(!CamCreated){

			CamCreated = true;

			//NatCam.Camera = DeviceCamera.RearCamera;
            var NatCam.Camera = CameraDevice.GetDevices()[0];

			NatCam.Camera.SetPreviewResolution(ResolutionPreset.FullHD);

			NatCam.Camera.SetPhotoResolution(ResolutionPreset.FullHD);

			Debug.Log("Kauel: CamCreated");

		}

		#endif


		//NatCam.StartPreview(DeviceCamera.RearCamera);

        //NatCam.OnStart -= OnPreviewStart;
        //NatCam.OnStart += OnPreviewStart;

        PanelSelector.ShowOnlyThisPanel(0); //Solo el panel de fotos

        Debug.Log("Kauel: StartCamera() ha finalizado");



    }

        void OnStart (Texture preview) {
            Debug.Log("NatCam2: On Start");
            // Display the camera preview on a RawImage
            RawImageCamera.texture = preview;
            previewTexture = preview;
           // aspectFitter.aspectRatio = preview.width / (float)preview.height;
        }

/* 
		void OnStart (Texture preview) {
			// Create texture
			texture = new Texture2D(preview.width, preview.height, TextureFormat.RGBA32, false, false);
			rawImage.texture = texture;
			// Scale the panel to match aspect ratios
            aspectFitter.aspectRatio = preview.width / (float)preview.height;
			// Create pixel buffer
			buffer = new byte[preview.width * preview.height * 4];
		}

		void OnFrame () {
			// Capture the preview frame
			deviceCamera.CaptureFrame(buffer);
			// Convert to greyscale
			ConvertToGrey(buffer);
			// Fill the texture with the greys
			texture.LoadRawTextureData(buffer);
			texture.Apply();
		}

*/
    public void Init() {

        Debug.Log("Kauel: Init()");

        singleton = this;

    }



    public void StartFile(Texture tex) {

        Debug.Log("Kauel: StartFile()");

        singleton = this;

        Mode = KameraMode.File;

        // if (NatCam.IsPlaying) NatCam.Pause();
        // if (NatCam.IsRunning) NatCam.StopPreview();
        ConfigureForTexture(tex);

        DetectEdges();

        PanelSelector.HideAll();

    }



    public void FreeMem() {

        Debug.Log("Kauel: FreeMem()");

        CameraColors32 = null;

        if(CameraMat32 != null) CameraMat32.Dispose();

        CameraMat32 = null;

        CameraColors24 = null;

        if (CameraMat24 != null) CameraMat24.Dispose();

        CameraMat24 = null;

        if (WallMask != null) WallMask.Dispose();

        WallMaskBuffer = null;

        WallMask = null;

        DestroyImmediate(OutputTexture);

        DestroyImmediate(OutputMaskTexture);



        //FreeMem from Undo

        if (Undo != null) {

            for (int i = 0; i < Undo.Count; i++) {

                Undo[i].Dispose();

            }

            Undo.Clear();

            Undo = null;

        }



    }



    public void ConfigureForTexture(Texture tex) {

        Debug.Log("Kauel: ConfigureForTexture()");

        //FreeMem

        if (CameraMat32 != null) {

            FreeMem();

        }



        //Allocate Mem

        if (tex is Texture2D) CameraColors32 = ((Texture2D)tex).GetPixels32();

        else CameraColors32 = new Color32[tex.width * tex.height];

        IntPtr CameraPointer32 = Marshal.UnsafeAddrOfPinnedArrayElement(CameraColors32, 0);

        CameraMat32 = new Mat(tex.height, tex.width, DepthType.Cv8U, 4, CameraPointer32, tex.width * 4);



        CameraColors24 = new Byte[tex.width * tex.height * 3];

        IntPtr CameraPointer24 = Marshal.UnsafeAddrOfPinnedArrayElement(CameraColors24, 0);

        CameraMat24 = new Mat(tex.height, tex.width, DepthType.Cv8U, 3, CameraPointer24, tex.width * 3);

        WallMaskBuffer = new byte[tex.height * tex.width * 3];

        IntPtr WallMaskPointer = Marshal.UnsafeAddrOfPinnedArrayElement(WallMaskBuffer, 0);

        WallMask = new Mat(tex.height, tex.width, DepthType.Cv8U, 3,WallMaskPointer, tex.width * 3);

        WallMask.SetTo(Black);

        OutputTexture = new Texture2D(tex.width, tex.height, TextureFormat.RGB24, false);

        OutputMaskTexture = new Texture2D(tex.width, tex.height, TextureFormat.RGB24, false);

        WallMask.ApplyToTexture2D(OutputMaskTexture);



        CvInvoke.CvtColor(CameraMat32, CameraMat24, ColorConversion.Rgba2Rgb, 3);

        //XPhotoInvoke.BalanceWhite(CameraMat24, CameraMat24, WhiteBalanceMethod.Simple, 0, 255, 20, 230);

        CameraMat24.ApplyToTexture2D(OutputTexture);



        RawImageEdit.texture = OutputTexture;



        //Undo Textures

        Undo = new List<Mat>();



        var PanZoomRotation = RawImageEdit.GetComponent<KPanZoomRotation>();

        if (PanZoomRotation) {

            Debug.Log("Kauel: ResetPositionAndSize ");

            PanZoomRotation.ResetPositionAndSize();

        }



		TargetMaterial.SetTexture("_MaskTex", OutputMaskTexture);



		Debug.Log("Kauel: Camera Buffers Created with resolution " + tex.width + "x" + tex.height);



	}



	void Update() {



	if (testing) {

		DetectEdges();

		Mat temp = new Mat();

		CvInvoke.CvtColor(EdgeMap, temp, ColorConversion.Gray2Bgr);

		temp.ApplyToTexture2D(OutputTexture);

		temp.Dispose();

		return;

		}

	}





    public void SaveToUndoState() {

        if(Undo != null) {



            Mat newmat = WallMask.Clone();

            Undo.Add(newmat);

            if (Undo.Count >= UndoMax) {

                Undo[0].Dispose();

                Undo.RemoveAt(0);

            }

        }

    }



    public void RestoreFromUndoState() {



		Alert.Singleton.ShowAlert(Alert.Message.BETTER_EXPERIENCE);



        if (Undo != null) {

            int last = Undo.Count - 1;

            if (last >= 0) {

                Undo[last].CopyTo(WallMask);

                if (OutputMaskTexture) WallMask.ApplyToTexture2D(OutputMaskTexture);

                else Debug.LogError("Kauel: Error OutputMaskTexture == null");

                Undo[last].Dispose();

                Undo.RemoveAt(last);

            } else {

                //Reset Position and Rotation

                var panZoom = RawImageEdit.GetComponent<KPanZoomRotation>();

                if (panZoom) {

                    panZoom.ResetPositionAndSize();

                }

            }

        }

    }



    public void PauseCamera() {

        Debug.Log("Kauel: PauseCamera()");

//        NatCam.Pause();

    }



    public void PlayCamera() {

        Debug.Log("Kauel: PlayCamera()");

        if (Mode == KameraMode.Camera) {
            Debug.Log("KAmera Mode");
            RawImageCamera.texture = previewTexture;
            // Free the photo texture
            Texture2D.Destroy(photo); photo = null;
        }

    }


    public void StopCamera() {

        Debug.Log("Kauel: StopCamera()");

        //NatCam.Pause();
        //NatCam.StopPreview();

    }



    public void ExpandAndBlurMask() {

        Debug.Log("Kauel: ExpandAndBlurMask()");

        PauseCamera();

        Alert.Singleton.ShowAlert(Alert.Message.LOADING, false, delegate

        {

            Canvas.ShowOnlyThisPanel(3);

            Alert.Singleton.CloseAlert(true);

        });

    }



    public void ClearWallMask() {

        Debug.Log("Kauel: ClearWallMask()");

        if ( (WallMask != null) && (OutputMaskTexture != null) ) {

            WallMask.SetTo(Black);

            WallMask.ApplyToTexture2D(OutputMaskTexture);

        }

    }



    public void DetectEdgesWithAlert() {

        Alert.Singleton.ShowAlert(Alert.Message.LOADING, false, delegate () {

            DetectEdges();

            Alert.Singleton.CloseAlert(true);

            Canvas.ShowOnlyThisPanel(3);

        });

    }



    public void TestImage(string filename) {

        PauseCamera();

        Texture2D img = KEmguCVExtensions.NewTextureFromFile(filename);

        int w = img.width;

        int h = img.height;

        if (w > h) {

            w = 1024;

            h = img.height * w / img.width;

        } else {

            h = 1024;

            w = img.width * h / img.height;

        }

        Texture2D resizedTex = img.NewResizedTexture(w, h);

        Destroy(img);

        ConfigureForTexture(resizedTex);

        DetectEdges();

    }



    //DetectEdges

    public void DetectEdges() {

        CvInvoke.Normalize(CameraMat24, CameraMat24, 1, 254, NormType.MinMax);

        Debug.Log("Kauel: DetectEdges()");

        PauseCamera();

        if (EdgeMap != null) EdgeMap.Dispose();

        EdgeMap = new Mat(CameraMat24.Rows + 2, CameraMat24.Cols + 2, DepthType.Cv8U, 1);

        Rectangle roi = new Rectangle(1, 1, CameraMat24.Width, CameraMat24.Height);

        Mat EdgeMapCenter = new Mat(EdgeMap, roi);



        Mat img1 = CameraMat24.Clone();

        Mat img2 = img1.Clone();

        Mat img3 = img1.Clone();



        CvInvoke.FastNlMeansDenoising(img1, img1); //Elimina el ruido.

        CvInvoke.GaussianBlur(img1, img2, new Size(9, 9), 9); //Blur

        CvInvoke.AddWeighted(img1, 1.5, img2, -0.5, 0, img1, DepthType.Cv8U);

        //img1.Save("C:/dnn/Filter.png");







        Mat imgCanny = img1.Clone();

        CvInvoke.Canny(img1, imgCanny, CannyLow, CannyHigh, CannyAperture);



        CvInvoke.CvtColor(img1, img1, ColorConversion.Bgr2Gray); //Bordes en escala de grises.



        CvInvoke.Sobel(img1, img2, DepthType.Cv32F, 1, 0, BorderAperture, 1);

        CvInvoke.Sobel(img1, img3, DepthType.Cv32F, 0, 1, BorderAperture, 1);



        CvInvoke.ConvertScaleAbs(img2, img2, 1, 0);

        CvInvoke.ConvertScaleAbs(img3, img3, 1, 0);

        CvInvoke.AddWeighted(img2, 1, img3, 1, 0, img3);

        img3.ConvertTo(img3, DepthType.Cv8U);



        //img3.Save("C:/dnn/SobelEroded.png");

        CvInvoke.AdaptiveThreshold(img3, img3, 255, AdaptiveThresholdType.MeanC, ThresholdType.Binary, ContrastAperture, -Contrast);

        //img3.Save("C:/dnn/Adaptive.png");

        CvInvoke.BitwiseOr(imgCanny, img3, img3);

        img3.CopyTo(img2);

        LineSegment2D[] lines = CvInvoke.HoughLinesP(img2, HoughLineRho, HoughLineAngle, HoughLineThreshold, HoughLineMinLineLength, HoughLineMaxGap);

        //img2.SetTo(Black);

        for(int i = 0; i < lines.Length; i++) {

            CvInvoke.Line(img3, lines[i].P1, lines[i].P2, White, 1);

        }

        lines = null;

        img3.CopyTo(EdgeMapCenter);



        img1.Dispose();

        img2.Dispose();

        img3.Dispose();

        imgCanny.Dispose();

        EdgeMapCenter.Dispose();



    }





    public void GrabCut3(Vector2 point,PAINTING_MODE pm) {



        SaveToUndoState();



        if(pm == PAINTING_MODE.BRUSH) {

            CvInvoke.Circle(WallMask, point.toPoint(), 30, White, -1);

            WallMask.ApplyToTexture2D(OutputMaskTexture);

            return;

        }



        if (pm == PAINTING_MODE.ERASER) {

            CvInvoke.Circle(WallMask, point.toPoint(), 30, Black, -1);

            WallMask.ApplyToTexture2D(OutputMaskTexture);

            return;

        }



        Mat img2 = CameraMat24.Clone();

        Mat imgTarget = CameraMat24.Clone(); //Color de llenado (Rojo para el primer color, Verde para el segundo color

        switch (SelectedColorIndex) {

            case 0: imgTarget.SetTo(Red); break;

            case 1: imgTarget.SetTo(Green); break;

            case 2: imgTarget.SetTo(Blue); break;

        }



        Mat imgref = CameraMat24.Clone(); //Color de Referencia (Rosado Perfecto)

        imgref.SetTo(Pink);





        Mat maskEdges2 = EdgeMap.Clone();



        Mat mask1 = new Mat(img2.Rows, img2.Cols, DepthType.Cv8U, 1);

        Mat mask3 = new Mat(img2.Rows, img2.Cols, DepthType.Cv8U, 3);

        mask3.SetTo(Black);



        Point PixelPoint = point.toPoint();

        int Offset = (PixelPoint.Y * img2.Width + PixelPoint.X) * 3;

        int R = CameraColors24[Offset];

        int G = CameraColors24[Offset + 1];

        int B = CameraColors24[Offset + 2];



        MCvScalar LoDiff = new MCvScalar(Mathf.Min(DeltaColor2, B), Mathf.Min(DeltaColor2, G), Mathf.Min(DeltaColor2, R));

        MCvScalar UpDiff = new MCvScalar(Mathf.Min(DeltaColor2, 255 - B), Mathf.Min(DeltaColor2, 255 - G), Mathf.Min(DeltaColor2, 255 - R));



        Rectangle rect = new Rectangle();



        switch (pm) {

            case PAINTING_MODE.FLOODFILL:

                CvInvoke.FloodFill(img2, maskEdges2, PixelPoint, Pink, out rect, LoDiff, UpDiff, Connectivity.FourConnected);

                CvInvoke.Compare(img2, imgref, mask3, CmpType.Equal); //Deben ser todos de 3 canales, mask3 contiene la máscara a pintar

                CvInvoke.Dilate(mask3, mask3, null, Anchor, 2, BorderType.Default, Black);

                CvInvoke.Blur(mask3, mask3, new Size(4, 4), Anchor);



                CvInvoke.ExtractChannel(mask3, mask1, 0); //Se extrae un canal para usar como máscara

                CvInvoke.BitwiseNot(mask3, mask3); //Negativo de la zona a pintar, para despintar antes de pintar



                CvInvoke.BitwiseAnd(WallMask, mask3, WallMask); //Despinta primero

                CvInvoke.BitwiseOr(WallMask, imgTarget, WallMask, mask1); //Pinta de rojo, verde o azul la textura objetivo



                break;



        }



        WallMask.ApplyToTexture2D(OutputMaskTexture);



        img2.Dispose();

        imgref.Dispose();

        imgTarget.Dispose();

        mask1.Dispose();

        mask3.Dispose();

        maskEdges2.Dispose();





    }



    //Esta función genera un JPG con la comparación de un antes y un después y agrega una barra que muestra los colores usados y el logo de Ceresita

    public void PreprocessTextureFromRawImage() {



		bool color1used = false;

		bool color2used = false;



        Debug.Log("Kauel: PreprocessTextureFromRawImage()");



		Debug.Log ("WallMask = " + WallMask.str());



		Mat Channel0 = new Mat ();

		Mat Channel1 = new Mat ();

		CvInvoke.ExtractChannel (WallMask, Channel0, 0);

		CvInvoke.ExtractChannel (WallMask, Channel1, 1);

		Debug.Log ("Channel0 = " + Channel0.str());

		Debug.Log ("Channel1 = " + Channel1.str());



		color1used = CvInvoke.CountNonZero (Channel0) > 10;

		color2used = CvInvoke.CountNonZero (Channel1) > 10;

		Debug.Log ("color1used = " + color1used);

		Debug.Log ("color2used = " + color2used);

		Channel0.Dispose ();

		Channel1.Dispose ();



        EncodedImageAsPNG = null;



        if ((OutputTexture == null) || (TargetMaterial == null)) {

            Debug.Log("Kauel: OutputTextures == null");

            return;

        }



        Texture2D tex = OutputTexture;

        Texture2D result = new Texture2D(tex.width, tex.height, tex.format,false);



        RenderTexture temp = RenderTexture.GetTemporary(tex.width, tex.height); //Textura temporal para aplicar el material

        //Siguiente linea comentada por Actualizacion de Plugin
        //Graphics.Blit(tex, temp, TargetMaterial); //Aplica el material



        //Copia la textura temporal

        RenderTexture.active = temp;

        result.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);

        RenderTexture.active = null;

        RenderTexture.ReleaseTemporary(temp); //Libera memoria





        Mat before = CameraMat24.Clone();

        CvInvoke.Flip(before, before, FlipType.Vertical);

        CvInvoke.CvtColor(before, before, ColorConversion.Bgr2Rgb);

        Mat after = result.GetNewMat();

        Debug.Log("Kauel: Before =" + before.str());

        Debug.Log("Kauel: After =" + after.str());



        //Junta las 2 imagenes de forma horizontal

        Mat beforeafter = new Mat();

        CvInvoke.HConcat(before, after, beforeafter);



        //Logo de Ceresita

        Mat corner = Corner.GetNewMat(); //Logo de Ceresita



        //Ancho de cada barra

        int w1 = (beforeafter.Width - corner.Width) / 2;

        int w2 = beforeafter.Width - corner.Width - w1;



		//Uso de dos colores

		if (color1used && color2used) {



		//Uso de uno o ningun color

		} else {

			w1 = beforeafter.Width - corner.Width;

			w2 = w1;

		}



        Mat bar1 = new Mat(corner.Height, w1, DepthType.Cv8U, 3);



        Kolores k = SelectedColors[0].SelectedKolor;

        float r = k.RGBA.r * 255;

        float g = k.RGBA.g * 255;

        float b = k.RGBA.b * 255;

        float l = k.HSL.z;

        bar1.SetTo(new MCvScalar(b, g, r)); //Pinta la Barra del primer color



        MCvScalar textcolor = White;

        if (l > 0.5f) textcolor = Black;

        CvInvoke.PutText(bar1, k.Name, new Point(40, 50), FontFace.HersheyDuplex, 1.0, textcolor, 2);

        CvInvoke.PutText(bar1, k.Code, new Point(40, 100), FontFace.HersheyDuplex, 1.0, textcolor, 2);



        //Aplica la segunda barra de color

        Mat bar2 = new Mat(corner.Height, w2, DepthType.Cv8U, 3);

        k = SelectedColors[1].SelectedKolor;

        r = k.RGBA.r * 255;

        g = k.RGBA.g * 255;

        b = k.RGBA.b * 255;

        l = k.HSL.z;

        bar2.SetTo(new MCvScalar(b, g, r)); //Pinta la Barra del segundo color



        textcolor = White;

        if (l > 0.5f) textcolor = Black;

        CvInvoke.PutText(bar2, k.Name, new Point(40, 50), FontFace.HersheyDuplex, 1.0, textcolor, 2);

        CvInvoke.PutText(bar2, k.Code, new Point(40, 100), FontFace.HersheyDuplex, 1.0, textcolor, 2);



		//Uso de dos colores

		if (color1used && color2used) {

			CvInvoke.HConcat(bar1, bar2, bar2);

			CvInvoke.HConcat(bar2, corner, bar2);

		} else {

			//Solo color 2

			if (color2used) {

				CvInvoke.HConcat(bar2, corner, bar2);

			} else {

				CvInvoke.HConcat(bar1, corner, bar2);

			}

		}



        CvInvoke.VConcat(beforeafter, bar2, beforeafter);



        CvInvoke.Flip(beforeafter, beforeafter, FlipType.Vertical);

        CvInvoke.CvtColor(beforeafter, beforeafter, ColorConversion.Bgr2Rgb);



        beforeafter.ApplyToTexture2D(result);



        before.Dispose();

        after.Dispose();

        beforeafter.Dispose();

        corner.Dispose();

        bar1.Dispose();

        bar2.Dispose();





        EncodedImageAsPNG = result.EncodeToPNG();



        RawTexture2D = result;

    }



    public Mat CameraMat() {

        return CameraMat24;

    }



    public Mat CameraMask() {

        return WallMask;

    }

/* 
    public void OnPreviewStart() {

        Debug.Log("Kauel: OnPreviewStart()");

        RawImageCamera.texture = NatCamCamera.Preview;

    }

*/

    public void TakePhoto() {

        NatCamCamera.CapturePhoto(OnPhoto);
        //PauseCamera();

    }

        private void OnPhoto (Texture2D photo) {
            // Save the photo
            this.photo = photo;
            // Display the photo
            RawImageCamera.texture = photo;
            
        }

    public void SelectPhoto() {
 

        Alert.Singleton.ShowAlert(Alert.Message.LOADING, false, delegate () {


            Texture _tex = RawImageCamera.texture;
            int w = _tex.width;
            int h = _tex.height;

            if (w > h) {
                
                w = 1024;
                h = _tex.height * w / _tex.width;

            } else {
                
                h = 1024;
                w = _tex.width * h / _tex.height;
            }


            Texture2D resizedTex = _tex.NewResizedTexture(w, h);

            RawImageEdit.texture = resizedTex;
            
            Alert.Singleton.CloseAlert(true);

            StartFile(resizedTex);

            Canvas.ShowOnlyThisPanel(3);

       });
            
    }



    public void SelectColor(int aIndex) {

        SelectedColorIndex = aIndex;

        for(int i = 0; i <= SelectedColors.Length - 1; i++) {

            Image img = SelectedColors[i].transform.parent.GetComponent<Image>();

            if(img){

                if (i == SelectedColorIndex) img.color = KColorManager.instance.Highlighted;

                else img.color = KColorManager.instance.Normal;



            }

        }

    }



    public void SelectColorFromImage(GameObject go) {

        KolorContainer kc = go.GetComponent<KolorContainer>();

        if (kc) {

            SelectedColors[SelectedColorIndex].CopyColorFromKolor(kc.Kolor);

        }

    }



}

