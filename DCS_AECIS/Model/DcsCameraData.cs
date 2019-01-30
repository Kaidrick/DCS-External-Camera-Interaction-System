using MathNet.Spatial.Euclidean;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCS_AECIS.Model
{
    class DcsCameraData
    {
        /* data from dcs camera
         * should contain position, X, Y and Z, and each of them is a vector, position can also be a vector
         * Does this model need a view model? it might be used to display camera data? maybe through game camera model?
         */
        [JsonProperty("p")]
        public LoVec3 P { get; set; }

        [JsonProperty("x")]
        public LoVec3 X { get; set; }

        [JsonProperty("y")]
        public LoVec3 Y { get; set; }

        [JsonProperty("z")]
        public LoVec3 Z { get; set; }
    }
}
