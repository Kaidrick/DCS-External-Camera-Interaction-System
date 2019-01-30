using MathNet.Spatial.Euclidean;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace DCS_AECIS
{
    class VirtualJoystick : VirtualControl, IMouseButtonControl, IMouseMoveControl
    {
        // joystick controller specific fields

        // control mode:
        // Joystick control can have the following modes:
        // 1. Horizontal Movement Control, that is, forward backward and strafe left and right
        // 2. Camera Panning and Pitch
        // 3. Altitude control? what about the other axis? <--- can not roll
        public int controlMode;

        // constructor
        public VirtualJoystick
            (GameCamera gameCamera, Ellipse joystickControlKnob, Ellipse joystickControlKnobBase, Canvas joystickParentCanvas)
            : base(gameCamera, joystickControlKnob, joystickControlKnobBase, joystickParentCanvas) { }


        // set control mode
        public void SetControlMode(int mode)
        {
            controlMode = mode;
        }

        public int GetControlMode()
        {
            return controlMode;
        }


        public void MouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            IsControlDragged = true;
            ((UIElement)e.Source).CaptureMouse();

            if (sender == _controlElementBase)  // if mouse click on base, treat as mouse move
            {
                MouseMove(_controlElement, e);  // MouseButtonEventArgs --> MouseEventArgs ???
            }
        }

        public void MouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            IsControlDragged = false;
            ((UIElement)e.Source).ReleaseMouseCapture();

            if (!CameraStickyControl)
            {
                ResetControl(this);
                GameCamera.StopCamera(camera);
            }
        }

        public async void MouseMove(object sender, MouseEventArgs e)
        {
            if (!IsControlDragged)
            {
                return;
            }

            // shape is being dragged
            var mousePos = e.GetPosition(_controlParentCanvas);
            controlCurrentCanvasLeft = Canvas.GetLeft(_controlElement);
            controlCurrentCanvasTop = Canvas.GetTop(_controlElement);

            // if dist to center if within check, set, otherwise set max
            var dist = Math.Sqrt(
                Math.Pow(mousePos.X - controlInitCanvasLeft - controlSizeHeight / 2, 2) +
                Math.Pow(mousePos.Y - controlInitCanvasTop - controlSizeWidth / 2, 2)
                );

            var move_range_radius = controlSizeBaseWidth / 2;


            double scale;  // it's like from 0% to 100% range
            if (dist <= move_range_radius)
            {
                double left = mousePos.X - (controlSizeWidth / 2);
                double top = mousePos.Y - (controlSizeHeight / 2);
                Canvas.SetLeft(_controlElement, left);
                Canvas.SetTop(_controlElement, top);

                scale = dist / move_range_radius;
            }
            else
            {
                Vector2D pointer =
                    new Vector2D(
                        mousePos.X - controlInitCanvasLeft - controlSizeHeight / 2,
                        mousePos.Y - controlInitCanvasTop - controlSizeWidth / 2);

                scale = dist / move_range_radius;

                pointer = pointer.ScaleBy(1 / scale);

                double left = controlInitCanvasLeft + pointer.X;
                double top = controlInitCanvasTop + pointer.Y;
                Canvas.SetLeft(_controlElement, left);
                Canvas.SetTop(_controlElement, top);
            }

            // which mode?
            if (controlMode == 0)  // movement control
            {
                await Task.Run(() => MoveCameraWithControl(scale));
            }
            else if (controlMode == 1)  // pitch and yaw control
            {
                await Task.Run(() => RotateCameraWithControl(scale));
            }

        }




        // control methods
        private void MoveCameraWithControl(double scale)
        {
            //camera.moveSpeedScale = scale;

            var moveVector = new Vector2D(
                controlCurrentCanvasLeft - controlInitCanvasLeft,
                controlCurrentCanvasTop - controlInitCanvasTop
                );

            CameraControl.MoveCamera(camera, moveVector, scale);
        }

        private void RotateCameraWithControl(double scale)
        {
            //camera.rotationSpeedScale = scale;

            // joystick movement vector
            var moveVector = new Vector2D(
                (controlCurrentCanvasLeft - controlInitCanvasLeft) / (controlSizeBaseWidth / 2),
                (controlCurrentCanvasTop - controlInitCanvasTop) / (controlSizeBaseHeight / 2)
                );

            // need to know how much to pitch and how much to yaw respectively

            CameraControl.RotateCamera(camera, moveVector, scale);


            //var yaw_dist = moveVector.X;  // how much to rotate left or right
            //var pitch_dist = moveVector.Y;  // how much to pitch up or down <--- maybe need to add options to invert axis

            ////camera.cameraData.GetCameraData(camera);
            //camera.cameraData.Camera_command = 1;
            //camera.cameraData.Camera_params = new List<double>
            //{
            //    0.1 * yaw_dist / (controlSizeBaseWidth / 2),   // this shold be the rotation speed scale control
            //    0.1 * pitch_dist / (controlSizeBaseHeight / 2)
            //};
        }
    }
}
