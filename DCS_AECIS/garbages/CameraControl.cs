using MathNet.Spatial.Euclidean;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCS_AECIS
{
    class CameraControl
    {
        private static readonly UnitVector3D nY = new UnitVector3D(0, 1, 0); // in game y-axis, pointing upwards

        // keyboard control --> forward, backward, left and right
        //                  --> rotate left right, pitch up and down
        public static void MoveCamera(GameCamera gameCamera, Vector2D direction, double scale)
        {

            gameCamera.moveSpeedScale = scale;

            // direction is where the keyboard or the joystick or what is moving in the direction of joystick canvas
            // say, forward is  new Vector2D(0, -1)
            //      backward is new Vector2D(0,  1)

            // move camera
            direction /= direction.Length;  // Normalize()

            Vector2D joystick_forward = new Vector2D(0, -1);
            MathNet.Spatial.Units.Angle moveAngle = direction.SignedAngleTo(joystick_forward, true);

            Vector3D camVector = gameCamera.HeadingUnitVector.ToVector3D();  // where the camera is pointing

            try
            {
                camVector = camVector.Rotate(nY, -moveAngle);  // find the direction the movment should go in
                gameCamera.moveVector = camVector.Normalize();
            }
            catch (Exception what_ex)
            {

            }
        }


        public static void RotateCamera(GameCamera gameCamera, Vector2D direction, double scale)
        {
            // gameCamera.rotationSpeedScale = scale;

            // joystick movement vector

            // show test info

            // need to know how much to pitch and how much to yaw respectively

            var yaw_dist = direction.X;  // how much to rotate left or right
            var pitch_dist = direction.Y;  // how much to pitch up or down <--- maybe need to add options to invert axis

            gameCamera.yawSpeedScale = yaw_dist;
            gameCamera.pitchSpeedScale = pitch_dist;

            //camera.cameraData.GetCameraData(camera);
            gameCamera.cameraData.Camera_command = 1;
            gameCamera.cameraData.Camera_params = new List<double>
            {
                gameCamera.CameraRotationSpeed * yaw_dist,   // this shold be the rotation speed scale control
                gameCamera.CameraRotationSpeed * pitch_dist
            };
        }


        public static void VerticalMoveCamera(GameCamera gameCamera, Vector2D direction, double scale)
        {
            // gameCamera.moveSpeedScale = scale;
            // TODO: add a height change rate here

            // direction is always the positive of the negative direction of the y-axis
            // height increment should be added to existing moveVector, 
            // because this.CameraMove methods may also change camera alt

            // move camera
            direction /= direction.Length;  // Normalize()

            gameCamera.heightChangeSpeedScale = -scale * direction.Y;


            //Vector2D joystick_forward = new Vector2D(0, -1);
            //MathNet.Spatial.Units.Angle moveAngle = direction.SignedAngleTo(joystick_forward, true);

            //Vector3D camVector = gameCamera.HeadingUnitVector.ToVector3D();  // where the camera is pointing

            //try
            //{
            //    camVector = camVector.Rotate(nY, -moveAngle);  // find the direction the movment should go in
            //    gameCamera.moveVector = camVector.Normalize();
            //}
            //catch (Exception what_ex)
            //{

            //}
        }


        //public static void ZoomCamera(GameCamera gameCamera, )
        //{

        //}
    }
}
