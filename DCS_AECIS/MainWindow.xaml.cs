﻿using System;
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
using System.Net.Sockets;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using System.Windows.Threading;
using MathNet.Spatial.Euclidean;
using System.Runtime.InteropServices;
using Gma.System.MouseKeyHook;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace DCS_AECIS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// 
    /// General Design-wise Description
    /// * A updater should be getting information from dcs constantly. Currently it should be able to get camera position
    /// * In the future the updater should also acquire export unit data from DCS to implement camera tracking
    /// 
    /// * a Camera object should be updated by the updater, mainly the orientations and positions
    /// * UI need to show heading, pitch, roll and position, but this should be in a viewmodel
    /// 
    /// * A SetCamera object should be used to serialize data to json and send to DCS
    /// * this object should be using data generated by user controls such as joysticks and sliders
    /// * always destroy this object when done, it should contain desired camera position, command params and zoom_level
    /// * UI does not need information from this class at all.
    /// 
    /// 
    /// 
    /// Use Mouse to Control Camera Test?
    /// First: Test if mouse position can be get when in DCS
    /// Second: check if middile button down can be get
    /// Third: try set mouse position to it's original position so the mouse does not mouse
    /// Fourth: get mouse distance
    /// 
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isKeyPressed;

        //ViewModel.CameraDataDisplayerViewModel dataDisplayerViewModel;

        // FIXME: https://stackoverflow.com/questions/1695101/why-are-actualwidth-and-actualheight-0-0-in-this-case


        public MainWindow()
        {
            //System.Threading.Thread.CurrentThread.CurrentUICulture =
            //    new System.Globalization.CultureInfo("zh-CN");


            InitializeComponent();

            //dataDisplayerViewModel = new ViewModel.CameraDataDisplayerViewModel();

            this.PreviewKeyDown += (s1, e1) => { if (e1.Key == Key.LeftCtrl) isKeyPressed = true; };
            this.PreviewKeyUp += (s2, e2) => { if (e2.Key == Key.LeftCtrl) isKeyPressed = false; };
            this.PreviewMouseLeftButtonDown += (s, e) => { if (isKeyPressed) DragMove(); };

            this.Deactivated += MainWindow_Deactivated;
            
        }

        private void MainWindow_Deactivated(object sender, EventArgs e)
        {
            
        }

        /// <summary>
        /// Test get mouse pos
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void EditBoxEnable(object sender, RoutedEventArgs e)
        {
            TextBox_1.IsEnabled = !TextBox_1.IsEnabled;
            TextBox_2.IsEnabled = !TextBox_2.IsEnabled;
        }
    }
}
