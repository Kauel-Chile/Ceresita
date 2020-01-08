/* 
*   NatCam
*   Copyright (c) 2019 Yusuf Olokoba
*/

namespace NatCam.Dispatch {

    using UnityEngine;
    using System;
    using System.Collections;
    
    [AddComponentMenu("")]
    public sealed class DispatchUtility : MonoBehaviour {

        #region --Op vars--
        public static event Action onFrame, onQuit;
        public static event Action<bool> onPause;
        public static event Action<DeviceOrientation> onOrient;
        public static DeviceOrientation orientation { get; private set; }
        private static readonly DispatchUtility instance;
        #endregion        


        #region --Operations--

        static DispatchUtility () {
            instance = new GameObject("NatCam Dispatch Utility").AddComponent<DispatchUtility>();
            instance.StartCoroutine(instance.OnFrame());
        }

        void Awake () {
            DontDestroyOnLoad(this.gameObject);
            DontDestroyOnLoad(this);
            Update();
        }

        void Update () {
            var reference = (DeviceOrientation)(int)Screen.orientation; // Input.deviceOrientation
            switch (reference) {
                case DeviceOrientation.FaceDown:
                case DeviceOrientation.FaceUp:
                case DeviceOrientation.Unknown: break;
                default:
                    if (orientation != reference)
                        if (onOrient != null)
                            onOrient(reference);
                    orientation = reference;
                break;
            }
        }
        
        void OnApplicationPause (bool paused) {
            if (onPause != null) onPause(paused);
        }
        
        void OnApplicationQuit () {
            if (onQuit != null) onQuit();
        }

        IEnumerator OnFrame () {
            var yielder = new WaitForEndOfFrame();
            for (;;) {
                yield return yielder;
                if (onFrame != null)
                    onFrame();
            }
        }
        #endregion
    }
}