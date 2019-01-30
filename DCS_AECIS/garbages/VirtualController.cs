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
    interface IMouseButtonControl
    {
        void MouseButtonUp(object sender, MouseButtonEventArgs e);
        void MouseButtonDown(object sender, MouseButtonEventArgs e);
    }

    interface IMouseMoveControl
    {
        void MouseMove(object sender, MouseEventArgs e);
    }

    class VirtualControl
    {
        internal bool   IsControlDragged { get; set; }
        public bool     CameraStickyControl { get; set; }

        internal double controlInitCanvasLeft;
        internal double controlInitCanvasTop;

        internal double controlCurrentCanvasLeft;
        internal double controlCurrentCanvasTop;

        internal double controlInitCenterTop;
        internal double controlInitCenterLeft;

        internal double controlSizeWidth;
        internal double controlSizeHeight;

        internal double controlSizeBaseHeight;  // that is, if it has a base element? but since I draw the UI it should have one
        internal double controlSizeBaseWidth;

        internal Shape _controlElement;
        internal Shape _controlElementBase;
        internal Canvas _controlParentCanvas;

        internal GameCamera camera;  // need to be passed to constructor when init


        // constructors 
        public VirtualControl(GameCamera gameCamera, Ellipse controlElement, Ellipse controlElementBase, Canvas controlParentCanvs)
        {
            camera = gameCamera;
            _controlElement = controlElement;
            _controlElementBase = controlElementBase;
            _controlParentCanvas = controlParentCanvs;

            InitControl(controlElement);

            controlSizeBaseHeight = controlElementBase.Height;
            controlSizeBaseWidth = controlElementBase.Width;  // FIXME: cannot get actual height and width? what's the problem?
        }

        public VirtualControl(Ellipse controlElement, Border controlElementBase, Canvas controlParentCanvs)
        {
            _controlElement = controlElement;


            InitControl(controlElement);

            controlSizeBaseHeight = controlElementBase.Height;
            controlSizeBaseWidth = controlElementBase.Width;
        }



        internal void InitControl(Shape controlElement)  // at for now, all controls has knob to move around
        {
            // get actual element, base and parent canvas


            // remember init position of the joystick neutral
            controlInitCanvasLeft = Canvas.GetLeft(controlElement);
            controlInitCanvasTop = Canvas.GetTop(controlElement);

            controlInitCenterTop = controlInitCanvasTop + controlElement.Height / 2;
            controlInitCenterLeft = controlInitCanvasLeft + controlElement.Width / 2;

            controlSizeHeight = controlElement.Height;
            controlSizeWidth = controlElement.Width;
        }


        public static void ResetControl(VirtualControl virtualControl)
        {
            // reset position of this virtual control
            Canvas.SetLeft(virtualControl._controlElement, virtualControl.controlInitCanvasLeft);
            Canvas.SetTop(virtualControl._controlElement, virtualControl.controlInitCanvasTop);
        }
    }
}
