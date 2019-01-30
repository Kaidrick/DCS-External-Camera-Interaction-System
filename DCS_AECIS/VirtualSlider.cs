using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Threading.Tasks;

namespace DCS_AECIS
{
    class VirtualSlider : VirtualControl, IMouseButtonControl, IMouseMoveControl
    {
        internal new Border _controlElementBase;

        public int controlMode;

        // constructor
        public VirtualSlider(GameCamera gameCamera, Ellipse sliderControlKnob, Border sliderControlKnobBase, Canvas sliderParentCanvas) : base(sliderControlKnob, sliderControlKnobBase, sliderParentCanvas)
        {
            camera = gameCamera;
            _controlElement = sliderControlKnob;
            _controlElementBase = sliderControlKnobBase;
            _controlParentCanvas = sliderParentCanvas;

            InitControl(_controlElement);

            controlSizeBaseHeight = sliderControlKnobBase.Height;
            controlSizeBaseWidth = sliderControlKnobBase.Width;
        }


        // controls
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
            controlCurrentCanvasTop = Canvas.GetTop(_controlElement);

            var s_gap = (controlSizeBaseWidth - controlSizeWidth) / 2;  // gap size between control and border

            // if dist to init pos is within check, set, otherwise set max
            var move = mousePos.Y - controlInitCenterTop;
            var dist = Math.Abs(move);
            double dist_max = controlSizeBaseHeight / 2 - s_gap - controlSizeHeight / 2;

            

            double top;
            if (dist <= dist_max)
            {
                top = mousePos.Y - controlSizeHeight / 2; 
            }
            else  //
            {
                //MessageBox.Show(move.ToString());
                if (move >= 0)
                {
                    top = controlInitCanvasTop + controlSizeBaseHeight / 2 - s_gap - controlSizeHeight / 2 ;
                    Canvas.SetTop(_controlElement, top);
                }
                else
                {
                    top = controlInitCanvasTop - controlSizeBaseHeight / 2 + s_gap + controlSizeHeight / 2;
                    Canvas.SetTop(_controlElement, top);
                }
            }

            Canvas.SetTop(_controlElement, top);

            var travel = controlCurrentCanvasTop - controlInitCanvasTop;


            var direction = new MathNet.Spatial.Euclidean.Vector2D(0, travel).Normalize();


            if (controlMode == 0)  // control height
            {
                await Task.Run(() => CameraControl.VerticalMoveCamera(camera, direction, dist / dist_max));
            }
            else if (controlMode == 1) // control zoom
            {
                camera.zoomValue = -travel / dist_max;  // set zoom value
            }
        }
    }
}
