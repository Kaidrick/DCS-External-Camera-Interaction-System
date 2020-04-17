using MathNet.Spatial.Euclidean;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DCS_AECIS.Model
{
    /// <summary>
    /// This class is responsible for controlling camera and generate new SetCamera object sent to DCS
    /// </summary>
    class CameraController
    {
        public Camera camera;  // should be passed to constructor
        public SetCamera setCamera;

        private CameraData data;
        
        private static readonly Vector2D joyForward = new Vector2D(0, 1);
        private static readonly UnitVector3D nY = new UnitVector3D(0, 1, 0);

        public double LeftJoystickVerticalMovement { get; set; } = 0;
        public double LeftJoystickHorizontalMovement { get; set; } = 0;

        public double RightJoystickVerticalMovement { get; set; } = 0;
        public double RightJoystickHorizontalMovement { get; set; } = 0;

        //public bool InvertPitchControl { get; set; } = false;
        public double RollSliderMovement { get; set; } = 0;

        // controlling params
        public double ZoomRate { get; set; } = 0;
        public double HeightChangeRate { get; set; } = 0;

        public double MovementSpeed { get; set; } = 0.05;
        public double RotationSpeed { get; set; } = 0.05;
        public double VerticalSpeed { get; set; } = 0.05;

        public double MaxMovementSpeed { get; set; } = 1;
        public double MaxVerticalSpeed { get; set; } = 1;

        // experimental features
        public bool UseMouseKeyboardControl { get; set; } = false;  // default not use

        public bool IsMovementOrientationCorrelated { get; set; } = false;
        public bool IsForceFeedback { get; set; } = false;

        public bool UseCockpitCameraControl { get; set; } = false;

        public bool DisableHorizontalRotation { get; set; } = false;
        public bool DisableHorizontalMovement { get; set; } = false;

        /// <summary>
        /// Constructor for CameraController class
        /// </summary>
        public CameraController(Camera camera)
        {
            this.camera = camera;  // get game camera object
            setCamera = new SetCamera();
        }

        public void ParseCameraData(string jsonCameraData)
        {
            data = JsonConvert.DeserializeObject<CameraData>(jsonCameraData);  // assign to cameraData field?
        }

        public void UpdateCameraData()
        {
            camera.X = data.X;
            camera.Y = data.Y;
            camera.Z = data.Z;
            camera.P = data.P;
        }


        // how to translate horizontal and vertical movement to camera distance?
        // find where the camera is facing
        public void MoveCamera()
        {
            if (LeftJoystickHorizontalMovement != 0 || LeftJoystickVerticalMovement != 0)
            {
                // TODO: following camera facing direction or not option
                // TODO: movement changes height if pitch is not zero option

                // if DisableHorizontalMovment is true, do not move camera left and right
                LeftJoystickHorizontalMovement = DisableHorizontalMovement ? 0 : LeftJoystickHorizontalMovement;

                // calculate angle between joystick movment and joystick forward direction
                MathNet.Spatial.Units.Angle moveAngle =
                new Vector2D(LeftJoystickHorizontalMovement, LeftJoystickVerticalMovement).SignedAngleTo(joyForward, true);
                var camVector = camera.HeadingUnitVector;  // UnitVector3D? without Y though

                //MessageBox.Show(string.Format("{0} {1} {2}", camera.X.X, camera.X.Y, camera.X.Z));
                
                var strength = new Vector2D(LeftJoystickHorizontalMovement, LeftJoystickVerticalMovement).Length;

                camVector = camVector.Rotate(nY, moveAngle);  // rotate the camera orientation vector about the y-axis for this move angle
                var strengthBasedMovement = camVector.ScaleBy(strength);
                setCamera.P.X = MaxMovementSpeed * MovementSpeed * strengthBasedMovement.X;  // direction * distance rate
                setCamera.P.Z = MaxMovementSpeed * MovementSpeed * strengthBasedMovement.Z;

                setCamera.DirectionalMovement = LeftJoystickVerticalMovement * MaxMovementSpeed * MovementSpeed;
                setCamera.HorizontalMovement = LeftJoystickHorizontalMovement * MaxMovementSpeed * MovementSpeed;

                setCamera.JoystickRawInput = new List<double> { LeftJoystickVerticalMovement, LeftJoystickHorizontalMovement};

                setCamera.OrientationFollowing = IsMovementOrientationCorrelated;

                setCamera.UseCockpitCameraControl = UseCockpitCameraControl;
            }
            else  // both are zero, no movement
            {
                setCamera.P.X = 0;  // direction * distance rate
                setCamera.P.Z = 0;

                setCamera.DirectionalMovement = 0;
                setCamera.HorizontalMovement = 0;

                setCamera.JoystickRawInput = new List<double> { 0, 0 };

                setCamera.OrientationFollowing = IsMovementOrientationCorrelated;
                setCamera.UseCockpitCameraControl = UseCockpitCameraControl;
            }
            // else joystick no movement, do nothing
        }

        public void RotateCamera()
        {
            if (RightJoystickHorizontalMovement != 0 || RightJoystickVerticalMovement != 0 || RollSliderMovement != 0)
            {
                setCamera.UseCockpitCameraControl = UseCockpitCameraControl;

                setCamera.CameraCommand = 1;

                // if DisableHorizontalRotation is true, do not rotate camera left and right
                RightJoystickHorizontalMovement = DisableHorizontalRotation ? 0 : RightJoystickHorizontalMovement;

                setCamera.CameraParams = new List<double>
                {
                    RightJoystickHorizontalMovement * RotationSpeed,
                    -RightJoystickVerticalMovement * RotationSpeed,
                    RollSliderMovement * RotationSpeed
                };
            }
            else  // both are zero, no movement
            {
                setCamera.UseCockpitCameraControl = UseCockpitCameraControl;

                setCamera.CameraCommand = 1;
                setCamera.CameraParams = new List<double>
                {
                    0, 0, 0
                };
            }
            // else joystick no movement, do nothing
        }

        public void ZoomCamera()
        {
            setCamera.CameraZoomLevel = ZoomRate;

            setCamera.ZoomSliderRawInput = ZoomRate;

            setCamera.UseCockpitCameraControl = UseCockpitCameraControl;
        }

        public void HeightChangeCamera()
        {
            setCamera.UseCockpitCameraControl = UseCockpitCameraControl;

            // add height change to current setCamera
            setCamera.P.Y = MaxVerticalSpeed * HeightChangeRate * VerticalSpeed;

            setCamera.VerticalMovement = MaxVerticalSpeed * HeightChangeRate * VerticalSpeed;
        }

        
    }
}
