using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DCS_AECIS.Model
{
    /// <summary>
    /// This class use Camera, CameraData and SetCamera to interact with DCS camera
    /// </summary>
    class CameraDataDisplayer
    {
        // initialization
        public Camera camera = new Camera();
        public CameraData cameraData;
        public SetCamera setCamera = new SetCamera();  // TODO: how to set camera? set here? set in camera controller?

        public bool DisplayerConnected { get; set; } = false;
        public int DisplayerUpdatedInterval { get; set; } = 10;  // in millisecond(s)
        public string IPAddress { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 3012;

        /// <summary>
        /// This method use JsonConvert.Deserialize to parse a json string containing camera data
        /// 
        /// </summary>
        /// <param name="jsonCameraData"></param>
        public void ParseCameraData(string jsonCameraData)
        {
            cameraData = JsonConvert.DeserializeObject<CameraData>(jsonCameraData);  // assign to cameraData field?
        }

        public void UpdateCameraDisplay()
        {
            camera.X = cameraData.X;
            camera.Y = cameraData.Y;
            camera.Z = cameraData.Z;
            camera.P = cameraData.P;
        }

        public SetCamera PreparedSetCamera()
        {
            return new SetCamera();
        }

        public void DisplayerStep()
        {
            // move timer to model? current implementation is in view model
        }

        public bool ReadyConnect()
        {
            if (!DisplayerConnected)
            {
                return true;  // not connected, return true
            }
            else
            {
                return false;
            }
        }

        public bool ReadyDisconnect()
        {
            if (DisplayerConnected)
            {
                return true;  // currently connected, return true
            }
            else
            {
                return false;  // not connected, no need to disconnet, return false
            }
        }
    }
}
