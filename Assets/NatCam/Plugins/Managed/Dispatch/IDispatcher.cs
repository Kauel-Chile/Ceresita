/* 
*   NatCam
*   Copyright (c) 2019 Yusuf Olokoba
*/

namespace NatCam.Dispatch {

    using System;

    public interface IDispatcher : IDisposable {
        
        /// <summary>
        /// Dispatch a workload
        /// </summary>
        void Dispatch (Action action);
    }
}