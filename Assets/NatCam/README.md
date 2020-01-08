# NatCam API
NatCam provides a clean, functional, and extremely performant API for accessing and controlling device cameras. The API is designed to be device camera-oriented. Hence all operations--whether it be introspection, running the preview, or capturing a photo--happen directly on `DeviceCamera` instances.

Using NatCam is as simple as acquiring a device camera and using it:
```csharp
var deviceCamera = DeviceCamera.RearCamera;
deviceCamera.StartPreview(OnStart);
```

The camera starts running and the client-provided callback is invoked with the preview texture:
```csharp
void OnStart (Texture preview) {
    // Display the camera preview on a RawImage
    rawImage.texture = preview;
}
```

## Camera Control
NatCam features a full camera control pipeline for utilizing camera functionality such as focusing, zooming, exposure, and so on. All these properties are in the `DeviceCamera` class. For example:
```csharp
DeviceCamera.RearCamera.ExposureBias = 1.3f;
```

## Capturing Photos
NatCam also allows for high-resolution photo capture from the camera. To do so, simply call the `CapturePhoto` method with an appropriate callback to accept the photo texture:
```csharp
deviceCamera.CapturePhoto(OnPhoto);

void OnPhoto (Texture2D photo) {
    // Do stuff with the photo...
    ...
    // Remember to release the texture when you are done with it so as to avoid memory leak
    Texture2D.Destroy(photo); 
}
```

## Accessing the Preview Data
The camera stream data can be accessed using the `CaptureFrame` method. The preview data is always guaranteed to be in the `RGBA32` format, regardless of platform. Here is an example illustrating how to use it:
```csharp
DeviceCamera deviceCamera;
byte[] previewData;

void OnStart (Texture preview) {
    // Allocate a preview data buffer
    var previewData = new byte[preview.width * preview.height * 4];
}

void OnFrame () {
    // Get the preview data
    deviceCamera.CaptureFrame(previewData);
    // Use the preview data
    // ...
}
```

## Using NatCam with OpenCV
NatCam supports OpenCV with the [OpenCVForUnity](https://assetstore.unity.com/packages/tools/integration/opencv-for-unity-21088) package. Check out the [official examples](https://github.com/EnoxSoftware/NatCamWithOpenCVForUnityExample). Using NatCam with OpenCV is pretty easy. On every frame, simply get the preview data from `DeviceCamera.CaptureFrame` and copy it into an `OpenCVForUnity.Mat`:
```csharp
void OnFrame () {
    // Allocate a preview data buffer
    var previewData = new byte[preview.width * preview.height * 4];
    // Copy the preview data into it
    NatCam.CaptureFrame(previewData);
    // Create a matrix
    var previewMatrix = new Mat(preview.height, preview.width, CvType.CV_8UC4);
    // Copy the preview data into the matrix
    Utils.copyToMat(previewData, previewMatrix);
    // Use the preview matrix
    // ...
}
```

___

With the simplicity of NatCam, you have the power and speed to create interactive, responsive camera apps. Happy coding!

## Requirements
- On Android, NatCam requires API level 23 and up.
- On iOS, NatCam requires iOS 8 and up.

## Tutorials
1. [Starting Off](https://medium.com/@olokobayusuf/natcam-tutorial-series-1-starting-off-dc3990f5dab6)
2. [Controls](https://medium.com/@olokobayusuf/natcam-tutorial-series-2-controls-d2e2d0738223)
3. [Photos](https://medium.com/@olokobayusuf/natcam-tutorial-series-3-photos-e28361b83cf8)
4. [Preview Data](https://medium.com/@olokobayusuf/natcam-tutorial-series-5-preview-data-9ac36eafd1f0)

## Quick Tips
- Please peruse the included scripting reference in the `Docs` folder.
- To discuss or report an issue, visit Unity forums [here](http://forum.unity3d.com/threads/natcam-device-camera-api.374690/).
- Check out more NatCam examples on Github [here](https://github.com/olokobayusuf?tab=repositories).
- Contact me at [olokobayusuf@gmail.com](mailto:olokobayusuf@gmail.com).

Thank you very much!