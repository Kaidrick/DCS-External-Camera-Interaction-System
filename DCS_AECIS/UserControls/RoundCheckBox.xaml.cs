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
    /// Interaction logic for RoundCheckBox.xaml
    /// </summary>
    public partial class RoundCheckBox : UserControl
    {
        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register("IsChecked", typeof(bool), typeof(RoundCheckBox));
        
        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); SetSliderPosistionBasedOnValue(value); }
        }

        // control position
        private double controlInitCanvasTop;
        private double controlMaxCanvasTop;

        private double controlSizeBaseWidth;
        private double controlSizeBaseHeight;
        private double controlBaseInitTop;

        private double controlSizeWidth;

        public RoundCheckBox()
        {
            InitializeComponent();

            controlSizeBaseWidth = BorderSliderBase.Width;
            controlSizeBaseHeight = BorderSliderBase.Height;

            controlBaseInitTop = Canvas.GetTop(BorderSliderBase);  // not in the canvas?

            controlSizeWidth = EllipseSliderKnob.Width;

            var s_gap = (controlSizeBaseWidth - controlSizeWidth) / 2;  // 4

            controlInitCanvasTop = Canvas.GetTop(EllipseSliderKnob);
            controlMaxCanvasTop = controlBaseInitTop + s_gap;  // NaN???

            //MessageBox.Show(string.Format("{0} {1}", controlInitCanvasTop, controlMaxCanvasTop));

            IsChecked = false;
        }

        private void SetSliderPosistionBasedOnValue(bool value)
        {
            if (value)
            {
                // move to checked position
                Canvas.SetTop(EllipseSliderKnob, controlMaxCanvasTop);
                BorderSliderBase.Background = new SolidColorBrush(Color.FromArgb(100, 0, 245, 0));
            }
            else
            {
                // move to unchecked position
                Canvas.SetTop(EllipseSliderKnob, controlInitCanvasTop);
                BorderSliderBase.Background = new SolidColorBrush(Colors.Gray);
            }
        }

        private void EllipseSliderKnob_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IsChecked = !IsChecked;
            SetSliderPosistionBasedOnValue(IsChecked);
        }
    }
}
