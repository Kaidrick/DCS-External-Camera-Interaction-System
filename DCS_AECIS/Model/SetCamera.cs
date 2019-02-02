using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCS_AECIS.Model
{
    class SetCamera : CameraData
    {
        //public SetCamera() : base() { }
        [JsonProperty("dX")]
        public double DirectionalMovement { get; set; }

        [JsonProperty("dZ")]
        public double HorizontalMovement { get; set; }

        [JsonProperty("dY")]
        public double VerticalMovement { get; set; }

        [JsonProperty("command")]
        public int CameraCommand { get; set; }

        [JsonProperty("params")]
        public List<double> CameraParams { get; set; }  // pitch yaw roll

        [JsonProperty("zoom")]
        public double CameraZoomLevel { get; set; }

        [JsonProperty("o_f")]
        public bool OrientationFollowing { get; set; }

        [JsonProperty("joy_raw")]
        public List<double> JoystickRawInput { get; set; }

        [JsonProperty("zoom_raw")]
        public double ZoomSliderRawInput { get; set; }

        [JsonProperty("pit_cam")]
        public bool UseCockpitCameraControl { get; set; }
    }
}
