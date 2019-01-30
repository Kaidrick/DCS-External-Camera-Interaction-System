using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Spatial.Euclidean;
using Newtonsoft.Json;

namespace DCS_AECIS
{
    class GameCamera
    {
        // position, x, y and z axes, where x, y and z are unit vectors
        [JsonProperty("p")]
        public LoVec3 P { get; set; } = new LoVec3(0, 0, 0);

        [JsonProperty("x")]
        public LoVec3 X { get; set; } = new LoVec3(0, 0, 0);

        [JsonProperty("y")]
        public LoVec3 Y { get; set; } = new LoVec3(0, 0, 0);

        [JsonProperty("z")]
        public LoVec3 Z { get; set; } = new LoVec3(0, 0, 0);

        [JsonIgnore]
        public UnitVector3D HeadingUnitVector => new UnitVector3D(X.X, 0, X.Z);

        [JsonIgnore]
        public UnitVector3D OrientationUnitVector => new UnitVector3D(X.X, X.Y, X.Z);

        [JsonIgnore]
        public UnitVector3D PitchUnitVector { get; set; }

        [JsonIgnore]
        public UnitVector3D RollUnitVector { get; set; }

        // test
        [JsonIgnore]
        public UnitVector3D moveVector = new UnitVector3D();

        [JsonIgnore]
        public double CameraMovementSpeed;

        [JsonIgnore]
        public double CameraRotationSpeed;


        [JsonIgnore]
        public double heightChangeSpeedScale;

        [JsonIgnore]
        public double moveSpeedScale;

        [JsonIgnore]
        public double yawSpeedScale;

        [JsonIgnore]
        public double pitchSpeedScale;

        [JsonIgnore]
        public double zoomValue;

        [JsonIgnore]
        public bool movementFollowsCameraDirection;  // WIP, follow direction? move camera forward changes altitude, if camera has pitch

        [JsonIgnore]
        public CameraCommandData cameraData = new CameraCommandData();


        /* camera control mode --> control camera angle (pitch / yaw) 
         * or choose camera (F1 ~ F11, etc)
         *      -1 - update mode, default, no command, no params
         *       0 - switch camera F1 to F11
         *       1 - rotate or pitch ( or both at the same time )
        
        */

        //[JsonProperty("command")]
        //public int Camera_command { get; set; }
        //[JsonProperty("params")]
        //public List<double> Camera_params { get; set; }
        //[JsonProperty("zoom")]
        //public double Zoom_level { get; set; }


        // True Heading of the in-game camera
        double Heading { get; set; }
        // Roll
        double Roll { get; set; }
        // Yaw
        double Yaw { get; set; }
        // Speed of the in-game camera, relative to the ground. How to get speed information though? linalg?
        double Speed { get; set; }

        // constructor
        public GameCamera() { }

        public GameCamera(Model.DcsCameraData dcsCameraData)
        {
            P = dcsCameraData.P;
            X = dcsCameraData.X;
            Y = dcsCameraData.Y;
            Z = dcsCameraData.Z;
        }

        GameCamera(LoVec3 position, LoVec3 x_dir, LoVec3 y_dir, LoVec3 z_dir)
        {
            P = position;
            X = x_dir;
            Y = y_dir;
            Z = z_dir;
        }

        public double GetRoll()  // which unit vector is used to calculate direction?
        {
            UnitVector3D uv = new UnitVector3D(Z.X, Z.Y, Z.Z);
            UnitVector3D u0 = new UnitVector3D(Z.X, 0, Z.Z);

            MathNet.Spatial.Units.Angle angle = uv.AngleTo(u0);
            double actual_roll;
            if (Y.X < 0)
            {
                actual_roll = -angle.Degrees;
            }
            else
            {
                actual_roll = angle.Degrees;
            }

            return Math.Floor(actual_roll);
        }

        public double GetHeading() // heading
        {
            UnitVector3D uv = new UnitVector3D(X.X, 0, X.Z);
            UnitVector3D u0 = new UnitVector3D(1, 0, 0);

            MathNet.Spatial.Units.Angle angle = uv.AngleTo(u0);
            double actual_angle;
            if (X.Z < 0)
            {
                actual_angle = 360 - angle.Degrees;
            }
            else
            {
                actual_angle = angle.Degrees;
            }

            return Math.Floor(actual_angle); // because LO uses floor
        }

        public double GetPitch()
        {
            UnitVector3D uv = new UnitVector3D(X.X, X.Y, X.Z);
            UnitVector3D u0 = new UnitVector3D(X.X, 0, X.Z);

            MathNet.Spatial.Units.Angle angle = uv.AngleTo(u0);
            double actual_pitch;
            if (X.Y < 0)
            {
                actual_pitch = -angle.Degrees;
            }
            else
            {
                actual_pitch = angle.Degrees;
            }

            return Math.Floor(actual_pitch);
        }

        // movement in xyz
        // move in a direction (supposedly a unit vector) for N meters
        public void MoveInDirection(UnitVector3D direction, double meters)
        {
            // move in current direction for this many meters
            // what is the current unit vector for heading?
            Vector3D movement = direction.ScaleBy(meters);  // movement vector, and then add to current position
            P.X += movement.X;
            P.Y += movement.Y;
            P.Z += movement.Z;
        }
        // change roll and pitch

        public void Update(GameCamera updateCamera)
        {
            X = updateCamera.X;
            Y = updateCamera.Y;
            Z = updateCamera.Z;
            P = updateCamera.P;
        }

        public static void StopCamera(GameCamera gameCamera)
        {
            gameCamera.moveVector = new UnitVector3D();
            gameCamera.heightChangeSpeedScale = 0;

            // reset pitch and yaw rate
            gameCamera.cameraData.GetCameraData(gameCamera);
            gameCamera.cameraData.Camera_command = 1;
            gameCamera.cameraData.Camera_params = new List<double> { 0, 0 };
            gameCamera.zoomValue = 0;
        }

        public void SetCommand(int cmd, List<double> cameraParams)
        {
            cameraData.Camera_command = cmd;
            cameraData.Camera_params = cameraParams;
        }
    }

    class LoVec3
    {
        [JsonProperty("x")]
        public double X { get; set; }

        [JsonProperty("y")]
        public double Y { get; set; }

        [JsonProperty("z")]
        public double Z { get; set; }

        public LoVec3() { }
        public LoVec3(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    class CameraCommandData  // data send to DCS?
    {
        [JsonProperty("p")]
        public LoVec3 P { get; set; }

        [JsonProperty("x")]
        public LoVec3 X { get; set; }

        [JsonProperty("y")]
        public LoVec3 Y { get; set; }

        [JsonProperty("z")]
        public LoVec3 Z { get; set; }

        [JsonProperty("command")]
        public int Camera_command { get; set; }

        [JsonProperty("params")]
        public List<double> Camera_params { get; set; }

        [JsonProperty("zoom")]
        public double Zoom_level { get; set; }

        public void GetCameraData(GameCamera gameCamera)
        {
            X = gameCamera.X;
            Y = gameCamera.Y;
            Z = gameCamera.Z;
            P = gameCamera.P;
        }
    }

    class CameraParams
    {
        [JsonProperty("p1")]
        public string P1 { get; set; }
        [JsonProperty("p2")]
        public string P2 { get; set; }
        [JsonProperty("p3")]
        public string P3 { get; set; }
    }
}
