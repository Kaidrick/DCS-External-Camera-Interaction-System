using System;
using System.Collections.Generic;
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
    /// Interaction logic for RoundSlider.xaml
    /// </summary>
    public partial class RoundSlider : UserControl
    {
        private bool IsControlDragged;
        private bool CameraStickyControl = false;
        private double controlCurrentCanvasLeft;
        private double controlCurrentCanvasTop;
        private double controlInitCanvasLeft;
        private double controlInitCanvasTop;
        private double controlSizeHeight;
        private double controlSizeWidth;

        private double controlInitCenterTop;
        private double controlInitCenterLeft;

        private double controlSizeBaseWidth;
        private double controlSizeBaseHeight;

        // Register Dependency Property
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(RoundSlider));
        public static readonly DependencyProperty StickyControlProperty = DependencyProperty.Register("StickyControl", typeof(bool), typeof(RoundSlider), new PropertyMetadata(StickyCameraPropertyChanged));

        private static void StickyCameraPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //throw new NotImplementedException();
            var control = (RoundSlider)d;

            Canvas.SetLeft(control.EllipseSliderKnob, control.controlInitCanvasLeft);
            Canvas.SetTop(control.EllipseSliderKnob, control.controlInitCanvasTop);

            control.Value = 0;
        }

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }  // TODO: set position according to value!
        }

        public bool StickyControl
        {
            get { return (bool)GetValue(StickyControlProperty); }
            set { SetValue(StickyControlProperty, value); }
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public RoundSlider()
        {
            InitializeComponent();

            controlInitCanvasLeft = Canvas.GetLeft(EllipseSliderKnob);
            controlInitCanvasTop = Canvas.GetTop(EllipseSliderKnob);

            controlSizeHeight = EllipseSliderKnob.Height;
            controlSizeWidth = EllipseSliderKnob.Width;

            controlSizeBaseWidth = BorderSliderBase.Width;
            controlSizeBaseHeight = BorderSliderBase.Height;

            controlInitCenterTop = controlInitCanvasTop + EllipseSliderKnob.Height / 2;
            controlInitCenterLeft = controlInitCanvasLeft + EllipseSliderKnob.Width / 2;

            // set position according to value here?
        }

        private void EllipseSliderKnob_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IsControlDragged = true;
            ((UIElement)e.Source).CaptureMouse();
        }

        private void EllipseSliderKnob_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            IsControlDragged = false;
            ((UIElement)e.Source).ReleaseMouseCapture();

            if (!StickyControl)
            {
                Canvas.SetLeft(EllipseSliderKnob, controlInitCanvasLeft);
                Canvas.SetTop(EllipseSliderKnob, controlInitCanvasTop);

                Value = 0;
            }
        }

        private void EllipseJoystickKnob_MouseMove(object sender, MouseEventArgs e)
        {
            if (!IsControlDragged)
            {
                return;
            }

            // shape is being dragged
            var mousePos = e.GetPosition(CanvasSlider);
            controlCurrentCanvasTop = Canvas.GetTop(EllipseSliderKnob);

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
                    top = controlInitCanvasTop + controlSizeBaseHeight / 2 - s_gap - controlSizeHeight / 2;
                    Canvas.SetTop(EllipseSliderKnob, top);
                }
                else
                {
                    top = controlInitCanvasTop - controlSizeBaseHeight / 2 + s_gap + controlSizeHeight / 2;
                    Canvas.SetTop(EllipseSliderKnob, top);
                }
            }

            Canvas.SetTop(EllipseSliderKnob, top);

            var travel = controlCurrentCanvasTop - controlInitCanvasTop;

            Value = -travel / dist_max;

            //var direction = new MathNet.Spatial.Euclidean.Vector2D(0, travel).Normalize();
        }
    }
}
