/* 
*   NatCam
*   Copyright (c) 2019 Yusuf Olokoba
*/

namespace NatCam.Platforms {

    using UnityEngine;
    using UnityEngine.Scripting;
    using System;
    using Dispatch;

    public sealed class DeviceCameraAndroid : DeviceCamera {

        #region --Introspection--

        public new static DeviceCameraAndroid[] Cameras {
            get {
                DeviceCameraClass = DeviceCameraClass ?? new AndroidJavaClass(DeviceCameraID);
                try {
                    using (var devicesArray = DeviceCameraClass.CallStatic<AndroidJavaObject>(@"cameras")) {
                        var devices = AndroidJNIHelper.ConvertFromJNIArray<AndroidJavaObject[]>(devicesArray.GetRawObject());
                        var result = new DeviceCameraAndroid[devices.Length];
                        for (var i = 0; i < devices.Length; i++)
                            result[i] = new DeviceCameraAndroid(devices[i]);
                        return result;
                    }
                } catch (Exception) { // Permissions denied
                    return null;
                }
            }
        }
        #endregion


        #region --Properties--

        public override string UniqueID {
            get { return device.Call<string>(@"uniqueID"); }
        }

        public override bool IsFrontFacing {
            get { return device.Call<bool>(@"isFrontFacing"); }
        }

        public override bool IsFlashSupported {
            get { return device.Call<bool>(@"isFlashSupported"); }
        }

        public override bool IsTorchSupported {
            get { return device.Call<bool>(@"isTorchSupported"); }
        }

        public override float HorizontalFOV {
            get { return device.Call<float>(@"horizontalFOV"); }
        }
        
        public override float VerticalFOV {
            get { return device.Call<float>(@"verticalFOV"); }
        }

        public override float MinExposureBias {
            get { return device.Call<float>(@"minExposureBias"); }
        }

        public override float MaxExposureBias {
            get { return device.Call<float>(@"maxExposureBias"); }
        }

        public override float MaxZoomRatio {
            get { return device.Call<float>(@"maxZoomRatio"); }
        }
        #endregion


        #region --Settings--

        public override Vector2Int PreviewResolution {
            get {
                var size = device.Call<AndroidJavaObject>(@"getPreviewResolution");
                var res = new Vector2Int(size.Call<int>(@"getWidth"), size.Call<int>(@"getHeight"));
                size.Dispose();
                return res;
            }
            set { device.Call(@"setPreviewResolution", value.x, value.y); }
        }

        public override Vector2Int PhotoResolution {
            get {
                var size = device.Call<AndroidJavaObject>(@"getPhotoResolution");
                var res = new Vector2Int(size.Call<int>(@"getWidth"), size.Call<int>(@"getHeight"));
                size.Dispose();
                return res;
            }
            set { device.Call(@"setPhotoResolution", value.x, value.y); }
        }
        
        public override int Framerate {
            get { return device.Call<int>(@"getFramerate"); }
            set { device.Call(@"setFramerate", value); }
        }

        public override float ExposureBias {
            get { return device.Call<float>(@"getExposureBias"); } 
            set { device.Call(@"setExposureBias", (int)value); }
        }

        public override bool ExposureLock {
            get { return device.Call<bool>(@"getExposureLock"); }
            set { device.Call(@"setExposureLock", value); }
        }

        public override Vector2 ExposurePoint {
            set { device.Call(@"setExposurePoint", value.x, value.y); }
        }

        public override FlashMode FlashMode {
            get { return (FlashMode)device.Call<int>(@"getFlashMode"); } 
            set { device.Call(@"setFlashMode", (int)value); }
        }

        public override bool FocusLock {
            get { return device.Call<bool>(@"getFocusLock"); }
            set { device.Call(@"setFocusLock", value); }
        }

        public override Vector2 FocusPoint {
            set { device.Call(@"setFocusPoint", value.x, value.y); }
        }

        public override bool TorchEnabled {
            get { return device.Call<bool>(@"getTorchEnabled"); } 
            set { device.Call(@"setTorchEnabled", value); }
        }

        public override bool WhiteBalanceLock {
            get { return device.Call<bool>(@"getWhiteBalanceLock"); }
            set { device.Call(@"setWhiteBalanceLock", value); }
        }

        public override float ZoomRatio {
            get { return device.Call<float>(@"getZoomRatio"); } 
            set { device.Call(@"setZoomRatio", value); }
        }
        #endregion


        #region --DeviceCamera--

        public override bool IsRunning {
            get { return cameraDelegate != null; }  //device.Call<bool>(@"isRunning");
        }

        public override void StartPreview (Action<Texture> startCallback, Action<long> frameCallback) {
            this.startCallback = startCallback;
            this.frameCallback = frameCallback;
            DispatchUtility.onPause += OnPause;
            DispatchUtility.onOrient += OnOrient;
            OnOrient(DispatchUtility.orientation);
            this.cameraDelegate = new DeviceCameraDelegate(this);
            using (var dispatcher = new RenderDispatcher())
                dispatcher.Dispatch(() => device.Call(@"startPreview", cameraDelegate));
        }

        public override void StopPreview () {
            using (var dispatcher = new RenderDispatcher())
                dispatcher.Dispatch(() => device.Call(@"stopPreview"));
            Texture2D.Destroy(previewTexture);
            previewTexture = null;
            cameraDelegate = null;
            DispatchUtility.onOrient -= OnOrient;
            if (!retainPauseSubsription) {
                DispatchUtility.onPause -= OnPause;
                startCallback = null;
                frameCallback = null;
            }
        }

        public override void CapturePhoto (Action<Texture2D> callback) {
            this.photoCallback = callback;
            device.Call(@"capturePhoto");
        }

        public override void CaptureFrame (byte[] pixelBuffer) {
            var framebuffer = RenderTexture.GetTemporary(previewTexture.width, previewTexture.height, 0);
            Graphics.Blit(previewTexture, framebuffer);
            var prevActive = RenderTexture.active;
            RenderTexture.active = framebuffer;
            previewTexture.ReadPixels(new Rect(0, 0, previewTexture.width, previewTexture.height), 0, 0);
            RenderTexture.active = prevActive;
            RenderTexture.ReleaseTemporary(framebuffer);
            var data = previewTexture.GetRawTextureData();
            Array.Copy(data, pixelBuffer, data.Length);
        }
        #endregion


        #region --Operations--

        private readonly AndroidJavaObject device;
        private Action<Texture> startCallback;
        private Action<long> frameCallback;
        private Action<Texture2D> photoCallback;
        private Texture2D previewTexture;
        private DeviceCameraDelegate cameraDelegate; // Used to make IsRunning immediate
        private bool retainPauseSubsription;
        private static AndroidJavaClass DeviceCameraClass;
        private const string DeviceCameraID = @"com.yusufolokoba.natcam.DeviceCamera";

        private DeviceCameraAndroid (AndroidJavaObject device) {
            this.device = device;
        }

        ~DeviceCameraAndroid () {
            device.Dispose();
        }

        private void OnStart (IntPtr texPtr, int width, int height) {
            // Update preview texture
            previewTexture = previewTexture ?? new Texture2D(width, height, TextureFormat.RGBA32, false, false); //Texture2D.CreateExternalTexture(width, height, TextureFormat.RGBA32, false, false, texPtr);
            if (previewTexture.width != width || previewTexture.height != height)
                previewTexture.Resize(width, height);
            previewTexture.UpdateExternalTexture(texPtr);
            // Invoke handler
            startCallback(previewTexture);
        }

        private void OnFrame (IntPtr texPtr, long timestamp) {
            // Update preview texture
            if (previewTexture == null)
                return;
            previewTexture.UpdateExternalTexture(texPtr);
            // Invoke handler
            if (frameCallback != null)
                frameCallback(timestamp);
        }

        private void OnPhoto (byte[] data, int width, int height) {
            var photo = new Texture2D(width, height, TextureFormat.RGBA32, false);
            photo.LoadRawTextureData(data);
            photo.Apply();
            photoCallback(photo);
        }

        private void OnPause (bool pausing) {
            if (pausing) {
                retainPauseSubsription = true;
                StopPreview();
            }
            else {
                DispatchUtility.onPause -= OnPause;
                retainPauseSubsription = false;
                StartPreview(startCallback, frameCallback);
            }
        }

        private void OnOrient (DeviceOrientation orientation) {
            device.Call(@"setOrientation", (int)orientation);
        }

        private class DeviceCameraDelegate : AndroidJavaProxy {
            private readonly DeviceCameraAndroid parent;
            public DeviceCameraDelegate (DeviceCameraAndroid parent) : base(@"com.yusufolokoba.natcam.DeviceCameraDelegate") { this.parent = parent; }
            [Preserve] private void onStart (int texPtr, int width, int height) { parent.OnStart((IntPtr)texPtr, width, height); }
            [Preserve] private void onFrame (int texPtr, long timestamp) { parent.OnFrame((IntPtr)texPtr, timestamp); }
            [Preserve] private void onPhoto (AndroidJavaObject photo) { parent.OnPhoto(AndroidJNI.FromByteArray(photo.Get<AndroidJavaObject>(@"pixelBuffer").GetRawObject()), photo.Get<int>(@"width"), photo.Get<int>(@"height")); }
        }
        #endregion
    }
}