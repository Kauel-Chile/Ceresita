/* 
*   NatCam
*   Copyright (c) 2019 Yusuf Olokoba
*/

namespace NatCam.Platforms {

    using UnityEngine;
    using System;
    using System.Runtime.InteropServices;
    using Dispatch;
    using Stopwatch = System.Diagnostics.Stopwatch;

    public sealed class DeviceCameraLegacy : DeviceCamera {

        #region --Introspection--

        public new static DeviceCameraLegacy[] Cameras {
            get {
                var devices = WebCamTexture.devices;
                var result = new DeviceCameraLegacy[devices.Length];
                for (var i = 0; i < devices.Length; i++)
                    result[i] = new DeviceCameraLegacy(devices[i]);
                return result;
            }
        }
        #endregion


        #region --Properties--

        public override string UniqueID {
            get {
                return device.name;
            }
        }

        public override bool IsFrontFacing {
            get {
                return device.isFrontFacing;
            }
        }

        public override bool IsFlashSupported {
            get {
                Debug.LogWarning("NatCam Error: Flash is not supported on legacy backend");
                return false;
            }
        }

        public override bool IsTorchSupported {
            get {
                Debug.LogWarning("NatCam Error: Torch is not supported on legacy backend");
                return false;
            }
        }

        public override float HorizontalFOV {
            get {
                Debug.LogWarning("NatCam Error: Field of view is not supported on legacy backend");
                return 0f;
            }
        }

        public override float VerticalFOV {
            get {
                Debug.LogWarning("NatCam Error: Field of view is not supported on legacy backend");
                return 0f;
            }
        }

        public override float MinExposureBias {
            get {
                Debug.LogWarning("NatCam Error: Exposure is not supported on legacy backend");
                return 0f;
            }
        }

        public override float MaxExposureBias {
            get {
                Debug.LogWarning("NatCam Error: Exposure is not supported on legacy backend");
                return 0f;
            }
        }

        public override float MaxZoomRatio {
            get {
                Debug.LogWarning("NatCam Error: Zoom is not supported on legacy backend");
                return 1f;
            }
        }
        #endregion


        #region --Settings--

        public override Vector2Int PreviewResolution { get; set; }

        public override Vector2Int PhotoResolution {
            get {
                Debug.LogWarning("NatCam Error: Photo resolution is not supported on legacy backend");
                return PreviewResolution;
            }
            set { Debug.LogWarning("NatCam Error: Photo resolution is not supported on legacy backend"); }
        }

        public override int Framerate { get; set; }

        public override float ExposureBias {
            get {
                Debug.LogWarning("NatCam Error: Exposure is not supported on legacy backend");
                return 0f;
            } 
            set { Debug.LogWarning("NatCam Error: Exposure is not supported on legacy backend"); }
        }

        public override bool ExposureLock {
            get {
                Debug.LogWarning("NatCam Error: Exposure mode is not supported on legacy backend");
                return false;
            } 
            set { Debug.LogWarning("NatCam Error: Exposure mode is not supported on legacy backend"); }
        }

        public override Vector2 ExposurePoint {
            set { Debug.LogWarning("NatCam Error: Exposure is not supported on legacy backend"); }
        }

        public override FlashMode FlashMode {
            get {
                Debug.LogWarning("NatCam Error: Flash mode is not supported on legacy backend");
                return 0;
            } 
            set { Debug.LogWarning("NatCam Error: Flash mode is not supported on legacy backend"); }
        }

        public override bool FocusLock {
            get {
                Debug.LogWarning("NatCam Error: Focus mode is not supported on legacy backend");
                return false;
            } 
            set { Debug.LogWarning("NatCam Error: Focus mode is not supported on legacy backend"); }
        }

        public override Vector2 FocusPoint {
            set { Debug.LogWarning("NatCam Error: Focus is not supported on legacy backend"); }
        }

        public override bool TorchEnabled {
            get {
                Debug.LogWarning("NatCam Error: Torch is not supported on legacy backend");
                return false;
            } 
            set { Debug.LogWarning("NatCam Error: Torch is not supported on legacy backend"); }
        }

        public override bool WhiteBalanceLock {
            get {
                Debug.LogWarning("NatCam Error: White balance is not supported on legacy backend");
                return false;
            } 
            set { Debug.LogWarning("NatCam Error: White balance is not supported on legacy backend"); }
        }

        public override float ZoomRatio {
            get {
                Debug.LogWarning("NatCam Error: Zoom is not supported on legacy backend");
                return 1f;
            } 
            set { Debug.LogWarning("NatCam Error: Zoom is not supported on legacy backend"); }
        }
        #endregion
        

        #region --DeviceCamera--

        public override bool IsRunning {
            get { return previewTexture; }
        }

        public override void StartPreview (Action<Texture> startCallback, Action<long> frameCallback) {
            this.startCallback = startCallback;
            this.frameCallback = frameCallback;
            previewTexture = PreviewResolution.x == 0 ? new WebCamTexture(device.name) : new WebCamTexture(device.name, PreviewResolution.x, PreviewResolution.y, Framerate == 0 ? 30 : Framerate);
            previewTexture.Play();
            firstFrame = true;
            DispatchUtility.onFrame += OnFrame;
        }

        public override void StopPreview () {
            DispatchUtility.onFrame -= OnFrame;
            previewTexture.Stop();
            WebCamTexture.Destroy(previewTexture);
            previewTexture = null;
            startCallback = null;
            frameCallback = null;
        }

        public override void CapturePhoto (Action<Texture2D> callback) {
            var photo = new Texture2D(previewTexture.width, previewTexture.height, TextureFormat.RGBA32, false, false);
            photo.SetPixels32(previewTexture.GetPixels32());
            photo.Apply();
            callback(photo);
        }

        public override void CaptureFrame (byte[] pixels) {
            GCHandle pin = GCHandle.Alloc(previewBuffer, GCHandleType.Pinned);
            Marshal.Copy(pin.AddrOfPinnedObject(), pixels, 0, previewBuffer.Length * Marshal.SizeOf(typeof(Color32)));
            pin.Free();
        }
        #endregion


        #region --Operations--
        private readonly WebCamDevice device;
        private Action<Texture> startCallback;
        private Action<long> frameCallback;
        private WebCamTexture previewTexture;
        private bool firstFrame;
        private Color32[] previewBuffer;
        
        private DeviceCameraLegacy (WebCamDevice device) {
            this.device = device;
        }

        private void OnFrame () {
            // Check that we are playing
            if (!previewTexture.didUpdateThisFrame || previewTexture.width == 16 || previewTexture.height == 16)
                return;
            // Update preview buffer
            if (previewBuffer == null)
                previewBuffer = previewTexture.GetPixels32();
            else
                previewTexture.GetPixels32(previewBuffer);
            // Invoke events
            if (firstFrame)
                startCallback(previewTexture);
            if (frameCallback != null)
                frameCallback(Stopwatch.GetTimestamp() * 100L);
            firstFrame = false;
        }
        #endregion
    }
}