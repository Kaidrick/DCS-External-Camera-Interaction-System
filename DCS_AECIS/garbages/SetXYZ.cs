//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace DCS_AECIS.garbages
//{
//    class SetXYZ
//    {
//    }
//}


//namespace DCS_AECIS
//{
//    /// <summary>
//    /// Interaction logic for MainWindow.xaml
//    /// </summary>
//    public partial class MainWindow : Window
//    {
//        private int Port = 3012;
//        private string IPAddress = "127.0.0.1";

//        GameCamera camera = new GameCamera();

//        // FIXME: https://stackoverflow.com/questions/1695101/why-are-actualwidth-and-actualheight-0-0-in-this-case

//        VirtualJoystick joystickControllerLeft;  // write left one (new) with new type of structure

//        VirtualJoystick joystickControllerRight;
//        VirtualSlider sliderControl;  // only have a single slider at the moment though


//        // if stick mode, controls do not go back to neutral position when released
//        public bool CameraControlStickyMode { get; set; }


//        //GameCamera cameraMovement = new GameCamera();  // used to update camera next position
//        //CameraData cameraData = new CameraData();  // data prepared to send to server, can be replace? why though? can update

//        //UnitVector3D cameraMoveVector = new UnitVector3D();  
//        //double move_speed_scale;

//        //double cameraZoomValue;  // more like direction and zoom rate

//        // pitch yaw roll delta (lua add this delta in order to get correct camera angle?)
//        //Vector3D nX = new Vector3D();
//        //Vector3D nY = new Vector3D();
//        //Vector3D nZ = new Vector3D();

//        //double pitch_delta;  // obsolete
//        //double yaw_delta;  // obsolete
//        //double rotate_speed_scale;


//        bool _isZoomsliderDragged;
//        double zoom_slider_init_canvas_top;

//        bool _isJoystickDragged;
//        double joystick_init_canvas_top;
//        double joystick_init_canvas_left;

//        double joystick_current_canvas_left;
//        double joystick_current_canvas_top;

//        double joystick_init_center_left;
//        double joystick_init_center_top;

//        double joystick_size_width;
//        double joystick_size_height;

//        double joystick_size_base_width;
//        double joystick_size_base_height;


//        DispatcherTimer timer = new DispatcherTimer
//        {
//            Interval = TimeSpan.FromSeconds(0.01)
//        };

//        public MainWindow()
//        {
//            InitializeComponent();

//            cb_sticky_controls.DataContext = this;

//            // remember init position of the joystick neutral
//            joystick_init_canvas_top = Canvas.GetTop(ellipse_joystick_control_knob);
//            joystick_init_canvas_left = Canvas.GetLeft(ellipse_joystick_control_knob);

//            joystick_init_center_left = joystick_init_canvas_left + ellipse_joystick_control_knob.ActualWidth / 2;
//            joystick_init_center_top = joystick_init_canvas_top + ellipse_joystick_control_knob.ActualWidth / 2;

//            joystick_size_height = ellipse_joystick_control_knob.ActualHeight;
//            joystick_size_width = ellipse_joystick_control_knob.ActualWidth;

//            joystick_size_base_width = ellipse_joystick_control_base.Height;
//            joystick_size_base_height = ellipse_joystick_control_base.Width;  // FIXME: cannot get actual height and width? what's the problem?


//            zoom_slider_init_canvas_top = Canvas.GetTop(zoom_slider);


//            // test new class JoystickController
//            joystickControllerLeft = new VirtualJoystick(camera, ellipse_joystick_left_control_knob, ellipse_joystick_left_control_base, canvas_joystick_left);
//            joystickControllerRight = new VirtualJoystick(camera, ellipse_joystick_control_knob, ellipse_joystick_control_base, canvas_joystick);

//            joystickControllerLeft.SetControlMode(0);
//            joystickControllerRight.SetControlMode(1);

//            sliderControl = new VirtualSlider(zoom_slider, border_zoom_slider, canvas_zoom_slider);  // Border, not Ellipse
//        }

//        private void Btn_connect_Click(object sender, RoutedEventArgs e)
//        {
//            timer.Tick += Update_camera_data;
//            timer.Start();

//        }

//        private void Update_camera_data(object sender, EventArgs e)
//        {
//            //if (CameraControl == false)  // if controlling camera with button, abort updating data, give way to btn controller
//            //{
//            //                }

//            // always make connection and always only make connection once here
//            TcpClient client = new TcpClient(IPAddress, Port);
//            NetworkStream nws = client.GetStream();

//            StreamReader reader = new StreamReader(nws);
//            StreamWriter writer = new StreamWriter(nws) { AutoFlush = true };

//            string cam_pos_json = reader.ReadLine();

//            string current_pos_json = JsonConvert.SerializeObject(camera);
//            lbl_disp_raw_data.Content = current_pos_json;
//            tb_raw_data_copying.Text = current_pos_json;



//            // parse camera data  <--- camera is replace here

//            // rather than replace it, it should be updated using camera.Update()


//            var updated_camera = JsonConvert.DeserializeObject<GameCamera>(cam_pos_json);
//            camera.Update(updated_camera);
//            //lbl_disp_raw_data.Content = String.Format(
//            //    "x --> x: {0}, y: {1}, z: {2}\n" +
//            //    "y --> x: {3}, y: {4}, z: {5}\n" + "z --> x: {6}, y: {7}, z: {8}",
//            //    camera.X.X, camera.X.Y, camera.X.Z,
//            //    camera.Y.X, camera.Y.Y, camera.Y.Z,
//            //    camera.Z.X, camera.Z.X, camera.Z.X
//            //);

//            double test_hdg = camera.GetHeading();
//            double test_pitch = camera.GetPitch();
//            double test_roll = camera.GetRoll();


//            lbl_disp_cam_roll.Content = string.Format("CAM ROLL {0:0.0}", test_roll);
//            lbl_disp_cam_pitch.Content = string.Format("CAM PITCH {0:0.0}", test_pitch);
//            lbl_disp_cam_heading.Content = string.Format("CAM HDG {0:000.0}", test_hdg);
//            lbl_disp_cam_pos.Content = string.Format("CAM POS\nx = {0}\ny = {1}\nz = {2}", camera.P.X, camera.P.Y, camera.P.Z);


//            // try set new camera position
//            //camera.MoveInDirection(cameraMoveVector, 0.1);
//            //var cameraMovement = new GameCamera
//            //{
//            //    //var nX = new LoVec3(0, 0, 0);  // delta
//            //    //var nY = new LoVec3(0, 0, 0);
//            //    //var nZ = new LoVec3(0, 0, 0);
//            //    X = new LoVec3(nX.X, nX.Y, nX.Z),
//            //    Y = new LoVec3(nY.X, nY.Y, nY.Z),
//            //    Z = new LoVec3(nZ.X, nZ.Y, nZ.Z)
//            //};


//            // try rotate camera angles



//            //var adjusted_cameraMoveVector = cameraMoveVector.ScaleBy(0.1 * move_speed_scale);
//            var adjusted_cameraMoveVector = camera.moveVector.ScaleBy(0.1 * camera.moveSpeedScale);
//            camera.cameraData.P = new LoVec3(adjusted_cameraMoveVector.X, adjusted_cameraMoveVector.Y, adjusted_cameraMoveVector.Z);  // position difference
//            //cameraMovement.Zoom_level = cameraZoomValue; // sld_camera_zoom.Value;
//            camera.cameraData.Zoom_level = camera.zoomValue; // sld_camera_zoom.Value;

//            string test_json = JsonConvert.SerializeObject(camera.cameraData);
//            writer.Write(test_json + "\n");

//            lbl_disp_out_data.Content = test_json;


//            // reset camera control deltas and params
//            camera.cameraData = new CameraData();
//            //CameraControl = false;
//        }

//        private void Btn_cam_move_forward_Click(object sender, RoutedEventArgs e)
//        {
//            // get current camera direction unit vector, use default speed of 1m ?
//            camera.MoveInDirection(camera.HeadingUnitVector, 1);
//            string test_json = JsonConvert.SerializeObject(camera);
//            lbl_disp_cam_new_pos.Content = test_json;

//            TcpClient client = new TcpClient(IPAddress, Port);
//            NetworkStream nws = client.GetStream();
//            StreamWriter writer = new StreamWriter(nws) { AutoFlush = true };
//            writer.WriteLine(test_json + "\n");

//        }

//        private void Btn_cam_move_backward_Click(object sender, RoutedEventArgs e)
//        {
//            // get current camera direction unit vector
//            //MathNet.Spatial.Euclidean.UnitVector3D backward_vector = camera.HeadingUnitVector.Negate();
//            camera.MoveInDirection(camera.HeadingUnitVector.Negate(), 1);
//            string test_json = JsonConvert.SerializeObject(camera);
//            lbl_disp_cam_new_pos.Content = test_json;

//            TcpClient client = new TcpClient(IPAddress, Port);
//            NetworkStream nws = client.GetStream();
//            StreamWriter writer = new StreamWriter(nws) { AutoFlush = true };
//            writer.WriteLine(test_json + "\n");

//        }

//        private void joystick_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
//        {
//            _isJoystickDragged = true;
//            Control_joystick_MouseLeftButtonDown(sender, e);
//        }

//        private void joystick_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
//        {
//            _isJoystickDragged = false;
//            Control_joystick_MouseLeftButtonUp(sender, e);
//        }


//        // control methods
//        private void Control_joystick_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
//        {
//            ((UIElement)e.Source).CaptureMouse();
//            lbl_disp_cam_new_pos.Content = "Dragging";

//            if (sender == ellipse_joystick_control_base)
//            {
//                joystick_MouseMove(ellipse_joystick_control_knob, e);
//            }
//        }

//        private void Control_joystick_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
//        {
//            ((UIElement)e.Source).ReleaseMouseCapture();

//            if (!CameraControlStickyMode)
//            {
//                lbl_disp_cam_new_pos.Content = "Idle";


//                Canvas.SetLeft(ellipse_joystick_control_knob, joystick_init_canvas_left);
//                Canvas.SetTop(ellipse_joystick_control_knob, joystick_init_canvas_top);

//                StopCamera();  // stop movement, pitch and yaw
//            }
//        }


//        // FIXME: maybe only send message to server when mouse moves?
//        private async void joystick_MouseMove(object sender, MouseEventArgs e)
//        {
//            if (!_isJoystickDragged)
//            {
//                return;
//            }

//            // shape is being dragged
//            var mousePos = e.GetPosition(canvas_joystick);
//            joystick_current_canvas_left = Canvas.GetLeft(ellipse_joystick_control_knob);
//            joystick_current_canvas_top = Canvas.GetTop(ellipse_joystick_control_knob);

//            // if dist to center if within check, set, otherwise set max
//            double dist = Math.Sqrt(
//                Math.Pow(mousePos.X - joystick_init_canvas_left - ellipse_joystick_control_knob.ActualHeight / 2, 2) +
//                Math.Pow(mousePos.Y - joystick_init_canvas_top - ellipse_joystick_control_knob.ActualWidth / 2, 2)
//                );

//            double move_range_radius = ellipse_joystick_control_base.ActualWidth / 2;

//            // btn_connect.Content = dist;

//            // FIXME

//            if (dist <= ellipse_joystick_control_base.ActualWidth / 2)
//            {
//                double left = mousePos.X - (ellipse_joystick_control_knob.ActualWidth / 2);
//                double top = mousePos.Y - (ellipse_joystick_control_knob.ActualHeight / 2);
//                Canvas.SetLeft(ellipse_joystick_control_knob, left);
//                Canvas.SetTop(ellipse_joystick_control_knob, top);

//                double scale = dist / move_range_radius;
//                btn_connect.Content = string.Format("{0}\n{1}\n{2}", "within", scale, move_range_radius);

//                camera.rotationSpeedScale = scale;
//            }
//            else
//            {
//                Vector2D pointer =
//                    new Vector2D(
//                        mousePos.X - joystick_init_canvas_left - ellipse_joystick_control_knob.ActualWidth / 2,
//                        mousePos.Y - joystick_init_canvas_top - ellipse_joystick_control_knob.ActualWidth / 2);

//                double scale = dist / move_range_radius;
//                btn_connect.Content = string.Format("{0}\n{1}\n{2}", "outside", scale, move_range_radius);

//                pointer = pointer.ScaleBy(1 / scale);

//                double left = joystick_init_canvas_left + pointer.X;
//                double top = joystick_init_canvas_top + pointer.Y;
//                Canvas.SetLeft(ellipse_joystick_control_knob, left);
//                Canvas.SetTop(ellipse_joystick_control_knob, top);

//                camera.rotationSpeedScale = 1;  // always 1
//            }



//            // after setting joystick position, move camera

//            // need to know what does the joystick controls here
//            // -1 None
//            //  0 Horizontal Movement
//            //  1 Pitch and Yaw
//            //  2 Vertical Movement and Roll?
//            //  3 Zoom
//            int joystick_function = cb_joystick_function.SelectedIndex;
//            if (joystick_function == 0)
//            {
//                await Task.Run(() => Move_camera_with_joystick());
//            }
//            else if (joystick_function == 1)
//            {
//                // call pitch and yaw control function
//                await Task.Run(() => Yaw_and_pitch_camera_with_joystick());
//            }
//            else
//            {

//            }

//        }

//        private void Move_camera_with_joystick()
//        {
//            // after set joystick position, need to actually move the in-game camera here
//            // find joystick direction vector, from joystick init center to current mousePos

//            //Vector2D moveVector = new Vector2D(
//            //    Canvas.GetLeft(ellipse_joystick_control_knob) + ellipse_joystick_control_knob.ActualWidth / 2 - joystick_init_canvas_left,
//            //    Canvas.GetTop(ellipse_joystick_control_knob) + ellipse_joystick_control_knob.ActualWidth / 2 - joystick_init_canvas_top
//            //    );

//            var moveVector = new Vector2D(
//                joystick_current_canvas_left + joystick_size_width / 2 - joystick_init_canvas_left,
//                joystick_current_canvas_top + joystick_size_height / 2 - joystick_init_canvas_top
//                );

//            // show test info
//            Dispatcher.Invoke(() => { btn_show_cam_pos.Content = string.Format("X: {0}\nY: {1}", moveVector.X, moveVector.Y); });



//            // move camera
//            moveVector /= moveVector.Length;

//            Vector2D joystick_forward = new Vector2D(0, -1);
//            MathNet.Spatial.Units.Angle moveAngle = moveVector.SignedAngleTo(joystick_forward, true);


//            // test change UI
//            Dispatcher.Invoke(() => { btn_show_cam_pos.Content = moveAngle.Degrees; });



//            Vector3D camVector = camera.HeadingUnitVector.ToVector3D();  // direction the camera is pointing at


//            // TODO: maybe add an option to choose to rotate about y-axis or Y?
//            var normal_vector = new UnitVector3D(0, 1, 0);  // in game y axis, assume always rotate about y-axis

//            //var test_vector = new Vector3D(1, 0, 0);

//            try
//            {
//                camVector = camVector.Rotate(normal_vector, -moveAngle);
//                UnitVector3D camUnitVector = camVector.Normalize();
//                //camera.MoveInDirection(camUnitVector, 1);
//                camera.moveVector = camUnitVector;
//            }
//            catch (Exception what_ex)
//            {

//            }

//            string test_json = JsonConvert.SerializeObject(camera);

//            // test change UI
//            Dispatcher.Invoke(() => { lbl_disp_cam_new_pos.Content = test_json; });
//        }

//        // controls Yaw and pitch
//        private void Yaw_and_pitch_camera_with_joystick()
//        {
//            // joystick movement vector
//            var moveVector = new Vector2D(
//                joystick_current_canvas_left + joystick_size_width / 2 - joystick_init_canvas_left,
//                joystick_current_canvas_top + joystick_size_height / 2 - joystick_init_canvas_top
//                );

//            // show test info

//            // need to know how much to pitch and how much to yaw respectively

//            var yaw_dist = moveVector.X;  // how much to rotate left or right
//            var pitch_dist = moveVector.Y;  // how much to pitch up or down <--- maybe need to add options to invert axis

//            //camera.cameraData.GetCameraData(camera);
//            camera.cameraData.Camera_command = 1;
//            camera.cameraData.Camera_params = new List<double>
//            {
//                0.1 * yaw_dist / (joystick_size_base_width / 2),
//                0.1 * pitch_dist / (joystick_size_base_height / 2)
//            };

//            // test: only do yaw at the moment to see if it works at all
//            // yaw vector

//            // yaw unit vector
//            moveVector /= moveVector.Length;




//            // Below are codes that might be useful if one day ED let us move the camera with XYZ rather than api calls ...

//            // how many to yaw: each pixel is 0.01 degree
//            // rotate X (maybe) about Y for this many degree
//            // and maybe roate Z (maybe) about Y for the same amout of degrees?

//            UnitVector3D camera_X = new UnitVector3D(camera.X.X, camera.X.Y, camera.X.Z);
//            UnitVector3D camera_Y = new UnitVector3D(camera.Y.X, camera.Y.Y, camera.Y.Z);
//            UnitVector3D camera_Z = new UnitVector3D(camera.Z.X, camera.Z.Y, camera.Z.Z);

//            var cam_yaw_angle = MathNet.Spatial.Units.Angle.FromDegrees(yaw_dist * 0.1);  // use 0.1 to test
//            var cam_pitch_angle = MathNet.Spatial.Units.Angle.FromDegrees(pitch_dist * 0.1);

//            // rotate vector delta?
//            var camera_dX = camera_X.Rotate(camera_Y, cam_yaw_angle) - camera_X.ToVector3D();
//            var camera_dZ = camera_Z.Rotate(camera_Y, cam_yaw_angle) - camera_Z.ToVector3D();


//            // test change UI
//            Dispatcher.Invoke(() => {
//                btn_show_cam_pos.Content =
//                string.Format("Joystick Yaw: {0}\nJoystick Pitch: {1}", cam_yaw_angle, cam_pitch_angle);
//            });


//            // get a delta XYZ?
//            //nX = camera_dX;
//            //nY = new Vector3D(0, 0, 0);  // no delta for Y
//            //nZ = camera_dZ;




//            //Vector3D camVector = camera.HeadingUnitVector.ToVector3D();

//            //var normal_vector = new UnitVector3D(0, 1, 0);  // in game y axis

//            //var test_vector = new Vector3D(1, 0, 0);

//            //try
//            //{
//            //    camVector = camVector.Rotate(normal_vector, -moveAngle);
//            //    UnitVector3D camUnitVector = camVector.Normalize();
//            //    camera.MoveInDirection(camUnitVector, 1);
//            //    cameraMoveVector = camUnitVector;
//            //}
//            //catch (Exception what_ex)
//            //{

//            //}

//            //string test_json = JsonConvert.SerializeObject(camera);

//            //// test change UI
//            //Dispatcher.Invoke(() => { lbl_disp_cam_new_pos.Content = test_json; });

//            //CameraControl = true;
//        }

//        private void Joystick_got_mouse_capture(object sender, MouseEventArgs e)
//        {

//        }


//        private void Zoom_slider_mouse_move(object sender, MouseEventArgs e)
//        {
//            if (!_isZoomsliderDragged)
//            {
//                return;
//            }
//            // shape is being dragged
//            var mousePos = e.GetPosition(canvas_zoom_slider);
//            var zoom_slider_current_canvas_top = Canvas.GetTop(zoom_slider);

//            // if dist to init pos is within check, set, otherwise set max
//            var move = mousePos.Y - (zoom_slider_init_canvas_top + zoom_slider.ActualHeight / 2);
//            var dist = Math.Abs(move);

//            var s_gap = (border_zoom_slider.ActualWidth - zoom_slider.ActualWidth) / 2 + zoom_slider.ActualHeight / 2;

//            btn_cam_move_forward.Content = move;

//            if (dist + s_gap < border_zoom_slider.ActualHeight / 2)
//            {
//                double top = mousePos.Y - zoom_slider.ActualHeight / 2;
//                Canvas.SetTop(zoom_slider, top);
//            }
//            else  //
//            {
//                if (move >= 0)
//                {
//                    double top = zoom_slider_init_canvas_top + border_zoom_slider.ActualHeight / 2 - s_gap;
//                    Canvas.SetTop(zoom_slider, top);
//                }
//                else
//                {
//                    double top = zoom_slider_init_canvas_top - border_zoom_slider.ActualHeight / 2 + s_gap;
//                    Canvas.SetTop(zoom_slider, top);
//                }
//            }

//            var travel = Canvas.GetTop(zoom_slider) - zoom_slider_init_canvas_top;


//            camera.zoomValue = -travel / (border_zoom_slider.ActualHeight / 2 - s_gap);
//            btn_cam_move_forward.Content = camera.zoomValue;
//            //cameraZoomValue = -travel / (border_zoom_slider.ActualHeight / 2 - s_gap);
//            //btn_cam_move_forward.Content = cameraZoomValue;

//        }

//        private void Zoom_slider_mouse_down(object sender, MouseButtonEventArgs e)
//        {
//            _isZoomsliderDragged = true;
//            ((UIElement)e.Source).CaptureMouse();
//        }

//        private void Zoom_slider_mouse_up(object sender, MouseButtonEventArgs e)
//        {
//            _isZoomsliderDragged = false;

//            if (!CameraControlStickyMode)  // if sticky is off, return neutral position on release
//            {
//                Canvas.SetTop(zoom_slider, zoom_slider_init_canvas_top);
//                //cameraZoomValue = 0;
//                camera.zoomValue = 0;
//            }

//            ((UIElement)e.Source).ReleaseMouseCapture();


//        }

//        private void Btn_show_cam_pos_click(object sender, RoutedEventArgs e)
//        {
//            MessageBox.Show(CameraControlStickyMode.ToString());
//        }


//        private void StopCamera()
//        {
//            camera.moveVector = new UnitVector3D();

//            // reset pitch and yaw rate
//            camera.cameraData.GetCameraData(camera);
//            camera.cameraData.Camera_command = 1;
//            camera.cameraData.Camera_params = new List<double>
//            {
//                0,
//                0
//            };
//        }



//        private void Checkbox_sticky_control_checked(object sender, RoutedEventArgs e)
//        {
//            joystickControllerLeft.CameraStickyControl = true;
//        }

//        private void Checkbox_sticky_control_unchecked(object sender, RoutedEventArgs e)
//        {
//            joystickControllerLeft.CameraStickyControl = false;

//            // slider
//            Canvas.SetTop(zoom_slider, zoom_slider_init_canvas_top);
//            //cameraZoomValue = 0;
//            camera.zoomValue = 0;


//            // joystick
//            lbl_disp_cam_new_pos.Content = "Idle";
//            Canvas.SetLeft(ellipse_joystick_control_knob, joystick_init_canvas_left);
//            Canvas.SetTop(ellipse_joystick_control_knob, joystick_init_canvas_top);

//            //cameraMoveVector = new UnitVector3D();

//            // reset pitch and yaw rate
//            VirtualControl.ResetControl(joystickControllerLeft);
//            GameCamera.StopCamera(camera);
//        }

//        private void JoystickKnobMouseDown(object sender, MouseButtonEventArgs e)
//        {
//            if (sender == ellipse_joystick_left_control_knob) { joystickControllerLeft.MouseButtonDown(sender, e); }
//            else if (sender == ellipse_joystick_control_knob) { joystickControllerRight.MouseButtonDown(sender, e); }
//        }

//        private void JoystickKnobMouseMove(object sender, MouseEventArgs e)  // FIXME, left button should be held down while moving
//        {
//            if (sender == ellipse_joystick_left_control_knob) { joystickControllerLeft.MouseMove(sender, e); }
//            else if (sender == ellipse_joystick_control_knob) { joystickControllerRight.MouseMove(sender, e); }
//        }

//        private void JoystickKnobMouseUp(object sender, MouseButtonEventArgs e)
//        {
//            if (sender == ellipse_joystick_left_control_knob) { joystickControllerLeft.MouseButtonUp(sender, e); }
//            else if (sender == ellipse_joystick_control_knob) { joystickControllerRight.MouseButtonUp(sender, e); }
//        }

//        private void Cb_joystick_mode_changed(object sender, SelectionChangedEventArgs e)
//        {
//            if (cb_joystick_function.SelectedIndex == 0)
//            {
//                if (joystickControllerLeft != null && joystickControllerRight != null)
//                {
//                    joystickControllerLeft.SetControlMode(0);
//                    joystickControllerRight.SetControlMode(1);
//                }

//                //joystickControllerRight.SetControlMode(1);
//            }
//            else if (cb_joystick_function.SelectedIndex == 1)
//            {
//                if (joystickControllerLeft != null && joystickControllerRight != null)
//                {
//                    joystickControllerLeft.SetControlMode(1);
//                    joystickControllerRight.SetControlMode(0);
//                }
//            }
//        }
//    }
//}
