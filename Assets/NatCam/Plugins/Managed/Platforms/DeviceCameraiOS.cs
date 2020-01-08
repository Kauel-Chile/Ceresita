/* 
*   NatCam
*   Copyright (c) 2019 Yusuf Olokoba
*/

namespace NatCam.Platforms {

    using AOT;
    using UnityEngine;
    using System;
    using System.Runtime.InteropServices;
    using Dispatch;

    public sealed class DeviceCameraiOS : DeviceCamera {

        #region --Introspection--

        public new static DeviceCameraiOS[] Cameras {
            get {
                // Get native devices
                IntPtr deviceArray;
                int deviceCount;
                DeviceCameraBridge.Devices(out deviceArray, out deviceCount);
                // Check permissions
                if (deviceArray == IntPtr.Zero)
                    return null;
                // Marshal
                var devices = new DeviceCameraiOS[deviceCount];
                for (var i = 0; i < deviceCount; i++) {
                    var device = Marshal.ReadIntPtr(deviceArray, i * Marshal.SizeOf(typeof(IntPtr)));
                    devices[i] = new DeviceCameraiOS(device);
                }
                Marshal.FreeCoTaskMem(deviceArray);
                return devices;
            }
        }
        #endregion


        #region --Properties--
        
        public override string UniqueID {
            get {
                var nativeStr = device.DeviceUID();
                var name = Marshal.PtrToStringAuto(nativeStr);
                Marshal.FreeCoTaskMem(nativeStr);
                return name;
            }
        }

        public override bool IsFrontFacing {
            get { return device.IsFrontFacing(); }
        }

        public override bool IsFlashSupported {
            get { return device.IsFlashSupported(); }
        }

        public override bool IsTorchSupported {
            get { return device.IsTorchSupported(); }
        }

        public override float HorizontalFOV {
            get { return device.HorizontalFOV(); }
        }

        public override float VerticalFOV {
            get { return device.VerticalFOV(); }
        }

        public override float MinExposureBias {
            get { return device.MinExposureBias(); }
        }

        public override float MaxExposureBias {
            get { return device.MaxExposureBias(); }
        }

        public override float MaxZoomRatio {
            get { return device.MaxZoomRatio(); }
        }
        #endregion


        #region --Settings--

        public override Vector2Int PreviewResolution {
            get {
                int width, height;
                device.GetPreviewResolution(out width, out height);
                return new Vector2Int(width, height);
            }
            set { device.SetPreviewResolution(value.x, value.y); }
        }

        public override Vector2Int PhotoResolution {
            get {
                int width, height;
                device.GetPhotoResolution(out width, out height);
                return new Vector2Int(width, height);
            }
            set { device.SetPhotoResolution(value.x, value.y); }
        }
        
        public override int Framerate {
            get { return device.GetFramerate(); }
            set { device.SetFramerate(value); }
        }

        public override float ExposureBias {
            get { return device.GetExposureBias(); }
            set { device.SetExposureBias(value); }
        }

        public override bool ExposureLock {
            get { return device.GetExposureLock(); }
            set { device.SetExposureLock(value); }
        }

        public override Vector2 ExposurePoint {
            set { device.SetExposurePoint(value.x, value.y); }
        }

        public override FlashMode FlashMode {
            get { return device.GetFlashMode(); } 
            set { device.SetFlashMode(value); }
        }

        public override bool FocusLock {
            get { return device.GetFocusLock(); }
            set { device.SetFocusLock(value); }
        }

        public override Vector2 FocusPoint {
            set { device.SetFocusPoint(value.x, value.y); }
        }

        public override bool TorchEnabled {
            get { return device.GetTorchEnabled(); } 
            set { device.SetTorchEnabled(value); }
        }

        public override bool WhiteBalanceLock {
            get { return device.GetWhiteBalanceLock(); }
            set { device.SetWhiteBalanceLock(value); }
        }

        public override float ZoomRatio {
            get { return device.GetZoomRatio(); } 
            set { device.SetZoomRatio(value); }
        }
        #endregion


        #region --DeviceCamera--

        public override bool IsRunning {
            get { return device.IsRunning(); }
        }

        public override void StartPreview (Action<Texture> startCallback, Action<long> frameCallback) {
            this.self = GCHandle.Alloc(this, GCHandleType.Normal); // Keep strong ref
            this.startCallback = startCallback;
            this.frameCallback = frameCallback;
            DispatchUtility.onOrient += OnOrient;
            OnOrient(DispatchUtility.orientation);
            device.StartPreview(OnStart, OnFrame, (IntPtr)self);
        }

        public override void StopPreview () {
            device.StopPreview();
            DispatchUtility.onOrient -= OnOrient;
            Texture2D.Destroy(previewTexture);
            self.Free();
            self = default(GCHandle);
            previewTexture = null;
            startCallback = null;
            frameCallback = null;
        }

        public override void CapturePhoto (Action<Texture2D> callback) {
            this.photoCallback = callback;
            device.CapturePhoto(OnPhoto, (IntPtr)self);
        }

        public override void CaptureFrame (byte[] pixelBuffer) {
            device.CaptureFrame(pixelBuffer);
        }
        #endregion


        #region --Operations--

        private readonly IntPtr device;
        private GCHandle self;
        private Action<Texture> startCallback;
        private Action<long> frameCallback;
        private Action<Texture2D> photoCallback;
        private Texture2D previewTexture;

        private DeviceCameraiOS (IntPtr device) {
            this.device = device;
        }

        ~DeviceCameraiOS () {
            device.FreeDevice();
        }

        [MonoPInvokeCallback(typeof(DeviceCameraBridge.StartCallback))]
        private static void OnStart (IntPtr context, IntPtr texPtr, int width, int height) {
            // Get device
            var cameraRef = (GCHandle)context;
            var camera = cameraRef.Target as DeviceCameraiOS;
            if (!camera)
                return;
            // Update preview texture
            if (!camera.previewTexture)
                camera.previewTexture = Texture2D.CreateExternalTexture(width, height, TextureFormat.RGBA32, false, false, texPtr);
            if (camera.previewTexture.width != width || camera.previewTexture.height != height)
                camera.previewTexture.Resize(width, height);
            camera.previewTexture.UpdateExternalTexture(texPtr);
            // Invoke handler
            camera.startCallback(camera.previewTexture);
        }

        [MonoPInvokeCallback(typeof(DeviceCameraBridge.FrameCallback))]
        private static void OnFrame (IntPtr context, IntPtr texPtr, long timestamp) {
            // Get device
            var cameraRef = (GCHandle)context;
            var camera = cameraRef.Target as DeviceCameraiOS;
            if (!camera)
                return;
            // Update preview texture
            if (!camera.previewTexture)
                return;
            camera.previewTexture.UpdateExternalTexture(texPtr);
            // Invoke handler
            if (camera.frameCallback != null)
                camera.frameCallback(timestamp);
        }

        [MonoPInvokeCallback(typeof(DeviceCameraBridge.PhotoCallback))]
        private static void OnPhoto (IntPtr context, IntPtr imgPtr, int width, int height) {
            // Get device
            var cameraRef = (GCHandle)context;
            var camera = cameraRef.Target as DeviceCameraiOS;
            if (!camera)
                return;
            // Marshal photo data
            var data = new byte[width * height * 4];
            Marshal.Copy(imgPtr, data, 0, data.Length);
            // Create photo
            using (var dispatch = new MainDispatcher())
                dispatch.Dispatch(() => {
                    var photo = new Texture2D(width, height, TextureFormat.BGRA32, false);
                    photo.LoadRawTextureData(data);
                    photo.Apply();
                    camera.photoCallback(photo);
                });
        }

        private void OnOrient (DeviceOrientation orientation) {
            device.SetOrientation((int)orientation);
        }
        #endregion
    }
}