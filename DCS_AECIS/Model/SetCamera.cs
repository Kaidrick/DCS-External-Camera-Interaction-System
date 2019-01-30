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

        [JsonProperty("command")]
        public int CameraCommand { get; set; }

        [JsonProperty("params")]
        public List<double> CameraParams { get; set; }

        [JsonProperty("zoom")]
        public double CameraZoomLevel { get; set; }
    }
}
