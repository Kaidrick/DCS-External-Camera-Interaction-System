using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCS_AECIS
{
    class GameCameraViewModel
    {
        private GameCamera Camera = new GameCamera();

        public string LblCameraPosition
        {
            get
            {
                var cameraPositionString = string.Format(
                    "X: {0}\n" +
                    "Y: {1}\n" +
                    "Z: {2}", Camera.P.X.ToString(), Camera.P.Y.ToString(), Camera.P.Z.ToString());
                return cameraPositionString;
            }
        }

        public string LblCameraHeading
        {
            get
            {
                return Convert.ToString(Camera.GetHeading());
            }
        }

        public string TextBlockCameraHeading
        {
            get
            {
                return Camera.GetHeading().ToString();
            }
        }

        public string TextBlockCameraPitch
        {
            get
            {
                return Camera.GetPitch().ToString();
            }
        }

        public string TextBlockCameraRoll
        {
            get
            {
                return Camera.GetRoll().ToString();
            }
        }

    }
}
