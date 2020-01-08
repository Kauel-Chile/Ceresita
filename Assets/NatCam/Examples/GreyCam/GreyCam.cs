/* 
*   NatCam
*   Copyright (c) 2019 Yusuf Olokoba
*/

namespace NatCam.Examples {

	using UnityEngine;
	using UnityEngine.UI;
	using System;

	/*
	* GreyCam Example
	* Example showcasing NatCam Preview Data Pipeline
	* Make sure to run this on the lowest camera resolution as it is heavily computationally expensive
	*/
	public class GreyCam : MonoBehaviour {

		[Header("Camera")]
		public bool useFrontCamera;

		[Header("UI")]
		public RawImage rawImage;
		public AspectRatioFitter aspectFitter;

		private DeviceCamera deviceCamera;
		private Texture2D texture;
		private byte[] buffer;

		void Start () {
			deviceCamera = useFrontCamera ? DeviceCamera.FrontCamera : DeviceCamera.RearCamera;
			if (!deviceCamera) {
                Debug.LogError("Camera is null. Consider using "+(useFrontCamera ? "rear" : "front")+" camera");
                return;
            }
			deviceCamera.PreviewResolution = new Vector2Int(640, 480);
			deviceCamera.StartPreview(OnStart, OnFrame);
		}
		
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

		static void ConvertToGrey (byte[] buffer) {
			for (int i = 0; i < buffer.Length; i += 4) {
				byte
				r = buffer[i + 0], g = buffer[i + 1],
				b = buffer[i + 2], a = buffer[i + 3],
				// Use quick luminance approximation to save time and memory
				l = (byte)((r + r + r + b + g + g + g + g) >> 3);
				buffer[i] = buffer[i + 1] = buffer[i + 2] = l; buffer[i + 3] = a;
			}
		}
	}
}