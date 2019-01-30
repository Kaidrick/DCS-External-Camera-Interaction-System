using MathNet.Spatial.Euclidean;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCS_AECIS.Model
{
    class Camera
    {   
        // this class should contain only properties and methods to update camera data

        // game data
        public LoVec3 P { get; set; } = new LoVec3(0, 0, 0);
        public LoVec3 X { get; set; } = new LoVec3(1, 0, 0);
        public LoVec3 Y { get; set; } = new LoVec3(0, 1, 0);
        public LoVec3 Z { get; set; } = new LoVec3(0, 0, 1);

        // vector data
        public UnitVector3D HeadingUnitVector => new UnitVector3D(X.X, 0, X.Z);
        public UnitVector3D OrientationUnitVector => new UnitVector3D(X.X, Y.Y, Z.Z);

        //public double Heading => GetHeading();
        public double Heading => GetHeading();
        public double Roll => GetRoll();
        public double Pitch => GetPitch();
        

        // control data
        public double ZoomLevel { get; set; }  // should be updated when preparing SetCamera data
        public UnitVector3D MoveVector { get; set; } //= new UnitVector3D(0, 0, 0);  // should be updated by UI input
        public double CameraMovementSpeed;  // UI controlled
        public double CameraRotationSpeed;  // UI controlled
        public double heightChangeSpeedScale;  // UI controlled
        public double zoomValue;

        public double yawSpeedScale;
        public double pitchSpeedScale;

        public bool movementFollowsCameraDirection;  // UI input

        // methods
        public double GetHeading()
        {
            double actual_angle = 0;
            try
            {
                UnitVector3D uv = new UnitVector3D(X.X, 0, X.Z);
                UnitVector3D u0 = new UnitVector3D(1, 0, 0);
                MathNet.Spatial.Units.Angle angle = uv.AngleTo(u0);
                
                if (X.Z < 0)
                {
                    actual_angle = 360 - angle.Degrees;
                }
                else
                {
                    actual_angle = angle.Degrees;
                }
            }
            catch(Exception)
            {

            }
            return Math.Floor(actual_angle); // because LO uses floor
        }

        public double GetRoll()  // which unit vector is used to calculate direction?
        {
            double actual_roll = 0;
            try
            {
                UnitVector3D uv = new UnitVector3D(Z.X, Z.Y, Z.Z);
                UnitVector3D u0 = new UnitVector3D(Z.X, 0, Z.Z);

                MathNet.Spatial.Units.Angle angle = uv.AngleTo(u0);

                if (Y.X < 0)
                {
                    actual_roll = -angle.Degrees;
                }
                else
                {
                    actual_roll = angle.Degrees;
                }
            }
            catch(Exception)
            {

            }
            return Math.Floor(actual_roll);
        }

        public double GetPitch()
        {
            double actual_pitch = 0;
            try
            {
                UnitVector3D uv = new UnitVector3D(X.X, X.Y, X.Z);
                UnitVector3D u0 = new UnitVector3D(X.X, 0, X.Z);

                MathNet.Spatial.Units.Angle angle = uv.AngleTo(u0);

                if (X.Y < 0)
                {
                    actual_pitch = -angle.Degrees;
                }
                else
                {
                    actual_pitch = angle.Degrees;
                }
            }
            catch(Exception)
            {

            }
            return Math.Floor(actual_pitch);
        }
    }
}
