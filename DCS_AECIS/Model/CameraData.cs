using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCS_AECIS.Model
{
    class CameraData
    {
        // p, x, y and z, used for parsing received data
        [JsonProperty("p")]
        public LoVec3 P { get; set; } = new LoVec3(0, 0, 0);

        [JsonProperty("x")]
        public LoVec3 X { get; set; } = new LoVec3(0, 0, 0);

        [JsonProperty("y")]
        public LoVec3 Y { get; set; } = new LoVec3(0, 0, 0);

        [JsonProperty("z")]
        public LoVec3 Z { get; set; } = new LoVec3(0, 0, 0);
    }
}
