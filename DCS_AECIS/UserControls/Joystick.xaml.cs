using MathNet.Spatial.Euclidean;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DCS_AECIS
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class Joystick : UserControl
    {
        private bool IsControlDragged;
        //private bool CameraStickyControl = false;
        private double controlCurrentCanvasLeft;
        private double controlCurrentCanvasTop;
        private double controlInitCanvasLeft;
        private double controlInitCanvasTop;
        private double controlSizeHeight;
        private double controlSizeWidth;

        private double controlSizeBaseWidth;
        private double controlSizeBaseHeight;

        // Register Dependency Property
        public static DependencyProperty MoveVerticalProperty = DependencyProperty.Register("MoveVertical", typeof(double), typeof(Joystick));
        public static DependencyProperty MoveHorizontalProperty = DependencyProperty.Register("MoveHorizontal", typeof(double), typeof(Joystick));
        public static DependencyProperty CameraStikcyControlProperty = DependencyProperty.Register("CameraStickyControl", typeof(bool), typeof(Joystick), new PropertyMetadata(StickyCameraPropertyChanged));
        

        public double MoveVertical
        {
            get { return (double)GetValue(MoveVerticalProperty); }
            set { SetValue(MoveVerticalProperty, value); }  
        }

        public double MoveHorizontal
        {
            get { return (double)GetValue(MoveHorizontalProperty); }
            set { SetValue(MoveHorizontalProperty, value); }
        }

        public bool CameraStickyControl
        {
            get { return (bool)GetValue(CameraStikcyControlProperty); }
            set { SetValue(CameraStikcyControlProperty, value); }
        }

        public Joystick()
        {
            InitializeComponent();
            controlInitCanvasLeft = Canvas.GetLeft(EllipseJoystickKnob);
            controlInitCanvasTop = Canvas.GetTop(EllipseJoystickKnob);

            controlSizeHeight = EllipseJoystickKnob.Height;
            controlSizeWidth = EllipseJoystickKnob.Width;

            controlSizeBaseWidth = EllipseJoystickBase.Width;
            controlSizeBaseHeight = EllipseJoystickBase.Height;

            CameraStickyControl = false;

            
        }

        private static void StickyCameraPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //throw new NotImplementedException();
            var control = (Joystick)d;
            
            Canvas.SetLeft(control.EllipseJoystickKnob, control.controlInitCanvasLeft);
            Canvas.SetTop(control.EllipseJoystickKnob, control.controlInitCanvasTop);

            control.MoveHorizontal = 0;
            control.MoveVertical = 0;
        }

        private void EllipseJoystickKnob_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IsControlDragged = true;
            ((UIElement)e.Source).CaptureMouse();
        }

        private void EllipseJoystickKnob_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            IsControlDragged = false;
            ((UIElement)e.Source).ReleaseMouseCapture();

            //MessageBox.Show(CameraStickyControl.ToString());
            if (!CameraStickyControl)
            {
                Canvas.SetLeft(EllipseJoystickKnob, controlInitCanvasLeft);
                Canvas.SetTop(EllipseJoystickKnob, controlInitCanvasTop);

                MoveHorizontal = 0;
                MoveVertical = 0;
            }
        }

        private void EllipseJoystickKnob_MouseMove(object sender, MouseEventArgs e)
        {
            if (!IsControlDragged)
            {
                return;
            }

            // shape is being dragged
            var mousePos = e.GetPosition(CanvasJoystick);
            controlCurrentCanvasLeft = Canvas.GetLeft(EllipseJoystickKnob);
            controlCurrentCanvasTop = Canvas.GetTop(EllipseJoystickKnob);

            var verticalMovement = mousePos.X - controlInitCanvasLeft - controlSizeHeight / 2;  // that is, movement in the Top Axis
            var horizontalMovement = mousePos.Y - controlInitCanvasTop - controlSizeWidth / 2;  // that is, movement in the Left Axis

            // if dist to center if within check, set, otherwise set max
            var dist = Math.Sqrt(
                Math.Pow(verticalMovement, 2) +
                Math.Pow(horizontalMovement, 2)
                );

            var move_range_radius = controlSizeBaseWidth / 2;

            // what is the vertical move direction and what is the scale?
            
            double scale;  // it's like from 0% to 100% range
            Vector2D pointer;
            if (dist <= move_range_radius)
            {
                pointer =
                    new Vector2D(
                        mousePos.X - controlInitCanvasLeft - controlSizeHeight / 2,
                        mousePos.Y - controlInitCanvasTop - controlSizeWidth / 2);

                double left = mousePos.X - (controlSizeWidth / 2);
                double top = mousePos.Y - (controlSizeHeight / 2);
                Canvas.SetLeft(EllipseJoystickKnob, left);
                Canvas.SetTop(EllipseJoystickKnob, top);

                scale = dist / move_range_radius;
            }
            else
            {
                pointer =
                    new Vector2D(
                        mousePos.X - controlInitCanvasLeft - controlSizeHeight / 2,
                        mousePos.Y - controlInitCanvasTop - controlSizeWidth / 2);

                scale = dist / move_range_radius;

                pointer = pointer.ScaleBy(1 / scale);

                double left = controlInitCanvasLeft + pointer.X;
                double top = controlInitCanvasTop + pointer.Y;
                Canvas.SetLeft(EllipseJoystickKnob, left);
                Canvas.SetTop(EllipseJoystickKnob, top);
            }

            MoveVertical = -pointer.Y / move_range_radius;
            MoveHorizontal = pointer.X / move_range_radius;
        }
    }
}
