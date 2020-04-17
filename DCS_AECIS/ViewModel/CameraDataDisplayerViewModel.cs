using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using DCS_AECIS.Model;
using Gma.System.MouseKeyHook;
using Newtonsoft.Json;

namespace DCS_AECIS.ViewModel
{
    class CameraDataDisplayerViewModel : INotifyPropertyChanged
    {
        private CameraDataDisplayer _dataDisplayer;
        private DispatcherTimer _timer;
        private CameraController _cameraController;
        //private CameraControllerViewModel _cameraControllerViewModel;
        private Camera camera;


        // singleton connection
        private TcpClient tcpClient;
        private NetworkStream networkStream;
        private StreamReader streamReader;
        private StreamWriter streamWriter;


#region Mouse and Keyboard Hook
private IKeyboardMouseEvents m_GlobalHook;  // mouse hook
        private bool mouseMiddileButtonPressed;
        private double mouseLastX = 0;
        private double mouseLastY = 0;

        // register mouse and keyboard global hook
        public void Subscribe()
        {
            // Note: for the application hook, use the Hook.AppEvents() instead
            m_GlobalHook = Hook.GlobalEvents();

            // keyboard
            m_GlobalHook.KeyPress += GlobalHookKeyPress;
            m_GlobalHook.KeyUp += M_GlobalHook_KeyUp;

            // mouse key
            m_GlobalHook.MouseUpExt += M_GlobalHook_MouseUpExt;
            m_GlobalHook.MouseDownExt += GlobalHookMouseDownExt;

            // mouse move
            //m_GlobalHook.MouseMove += M_GlobalHook_MouseMove;
            m_GlobalHook.MouseMoveExt += M_GlobalHook_MouseMoveExt;

            //System.Windows.MessageBox.Show("Successfully subscribed to MouseKeyboardEvent");
        }

        #region KeyboardEvents
        private void M_GlobalHook_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W)
            {
                JoystickLeftVerticalMovement = 0;
            }
            else if (e.KeyCode == Keys.D)
            {
                JoystickLeftHorizontalMovement = 0;
            }
            else if (e.KeyCode == Keys.A)
            {
                JoystickLeftHorizontalMovement = 0;
            }
            else if (e.KeyCode == Keys.S)
            {
                JoystickLeftVerticalMovement = 0;
            }
        }

        private void GlobalHookKeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (e.KeyChar.ToString() == "w")
            {
                JoystickLeftVerticalMovement = 1;
            }
            else if (e.KeyChar.ToString() == "d")
            {
                JoystickLeftHorizontalMovement = 1;
            }
            else if (e.KeyChar.ToString() == "a")
            {
                JoystickLeftHorizontalMovement = -1;
            }
            else if (e.KeyChar.ToString() == "s")
            {
                JoystickLeftVerticalMovement = -1;
            }

            //throw new NotImplementedException();
        }
        #endregion

        #region Mouse Events
        private void M_GlobalHook_MouseUpExt(object sender, MouseEventExtArgs e)
        {
            if (e.Button == MouseButtons.Middle && !e.IsMouseButtonDown)
            {
                //System.Windows.MessageBox.Show(string.Format("X {0} Y {1}", e.X.ToString(), e.Y.ToString()));
                MouseMBPressed = false;
                JoystickRightHorizontalMovement = 0;
                JoystickRightVerticalMovement = 0;
            }
        }

        private void GlobalHookMouseDownExt(object sender, MouseEventExtArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                //System.Windows.MessageBox.Show(string.Format("X {0} Y {1}", e.X.ToString(), e.Y.ToString()));
                MouseMBPressed = true;
                // get init pos when MB is first pressed
                _majInitPos[0] = e.X;
                _majInitPos[1] = e.Y;
            }
        }

        private bool _mouseMBPressed;

        // Mouse As Joystick Init Position - what a name ...
        private double[] _majInitPos = { 0, 0 };

        private string _mouseMoveDiff;

        public bool MouseMBPressed
        {
            get { return _mouseMBPressed; }
            set { _mouseMBPressed = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MouseMBPressed")); }
        }

        public string MouseMoveDiff
        {
            get { return _mouseMoveDiff; }
            set { _mouseMoveDiff = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MouseMoveDiff")); }
        }

        private void M_GlobalHook_MouseMoveExt(object sender, MouseEventExtArgs e)
        {
            if (MouseMBPressed)
            {
                // move camera here?
                //System.Windows.MessageBox.Show("Mouse moved");

                // if to implement this function as it is in the ChasePlane, then
                // keep rotate speed until target camera angle?


                // mouseLastX - e.X, mouseLastY - e.Y

                JoystickRightHorizontalMovement = -(_majInitPos[0] - e.X) / 50;
                JoystickRightVerticalMovement = (_majInitPos[1] - e.Y) / 50;


            }
            mouseLastX = e.X;
            mouseLastY = e.Y;
        }

        #endregion

        public void Unsubscribe()
        {
            // keyboard
            m_GlobalHook.KeyPress -= GlobalHookKeyPress;
            m_GlobalHook.KeyUp -= M_GlobalHook_KeyUp;

            // mouse key
            m_GlobalHook.MouseUpExt -= M_GlobalHook_MouseUpExt;
            m_GlobalHook.MouseDownExt -= GlobalHookMouseDownExt;

            // mouse move
            //m_GlobalHook.MouseMove -= M_GlobalHook_MouseMove;
            m_GlobalHook.MouseMoveExt -= M_GlobalHook_MouseMoveExt;

            //It is recommened to dispose it
            m_GlobalHook.Dispose();
        }

        #endregion

        // Fire ICommand on window close
        public ICommand WindowClosing { get { return new ButtonCommand(DisplayerWindowsClose); } }

        private void DisplayerWindowsClose()
        {
            _timer.Stop();
            try
            {
                Unsubscribe();
                //System.Windows.MessageBox.Show("Global Mouse and Keyboard Events Unsubscribed");
            }
            catch (Exception)
            {
                //System.Windows.MessageBox.Show("Failed to Unsubscribe MouseKeyboardEvent Hook. Maybe it's already unhooked?");
            }

        }

        private void DisplayerMouseKeyboardHookDisposalOnException(object sender, EventArgs eventArgs)
        {
            m_GlobalHook.Dispose();
        }



        public event PropertyChangedEventHandler PropertyChanged;


        // user input processor class
        // how does user control the camera though?
        // use move joystick --> joystick knob canvas position change --> (UI)left and top binding to properties
        // --> properties changes changes corresponding object in _dataDisplayer.setCamera

        // ViewModel
        #region Databinding Properties

        #region Joystick Left Control

        public double JoystickLeftVerticalMovement
        {
            get
            {
                return _cameraController.LeftJoystickVerticalMovement;
            }
            set
            {
                _cameraController.LeftJoystickVerticalMovement = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("JoystickLeftVerticalMovement"));
            }
        }

        public double JoystickLeftHorizontalMovement
        {
            get
            {
                return _cameraController.LeftJoystickHorizontalMovement;
            }
            set
            {
                _cameraController.LeftJoystickHorizontalMovement = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("JoystickLeftHorizontalMovement"));
            }
        }

        #endregion

        #region Joystick Right Control

        public double JoystickRightVerticalMovement
        {
            get
            {
                return _cameraController.RightJoystickVerticalMovement;
            }
            set
            {
                _cameraController.RightJoystickVerticalMovement = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("JoystickRightVerticalMovement"));
            }
        }

        public double JoystickRightHorizontalMovement
        {
            get
            {
                return _cameraController.RightJoystickHorizontalMovement;
            }
            set
            {
                _cameraController.RightJoystickHorizontalMovement = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("JoystickRightHorizontalMovement"));
            }
        }

        #endregion

        #region Camera Zoom Slider Control
        public double ZoomCameraSlider
        {
            get
            {
                return _cameraController.ZoomRate;
            }
            set
            {
                _cameraController.ZoomRate = Convert.ToDouble(value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ZoomCameraSlider"));
            }
        }
        #endregion

        #region Camera Height Control
        public double HeightChangeCameraSlider
        {
            get
            {
                return _cameraController.HeightChangeRate;
            }
            set
            {
                _cameraController.HeightChangeRate = Convert.ToDouble(value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HeightChangeCameraSlider"));
            }
        }

        public bool CameraOrientationFollowing
        {
            get
            {
                return _cameraController.IsMovementOrientationCorrelated;
            }
            set
            {
                _cameraController.IsMovementOrientationCorrelated = Convert.ToBoolean(value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CameraOrientationFollowing"));
            }
        }


        // data port
        public int TcpDataPort
        {
            get
            {
                return _dataDisplayer.Port;
            }
            set
            {
                _dataDisplayer.Port = Convert.ToInt32(value);
            }
        }

        public bool CanChangeConnectionData
        {
            get
            {
                if (_dataDisplayer.DisplayerConnected)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        #endregion

        #region Camera Speed Control
        // Speed Control
        public double SpeedMovement
        {
            get { return _cameraController.MovementSpeed; }
            set { _cameraController.MovementSpeed = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SpeedMovement")); }
        }

        public double SpeedRotation
        {
            get { return _cameraController.RotationSpeed; }
            set { _cameraController.RotationSpeed = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SpeedRotation")); }
        }

        public double SpeedVertical
        {
            get { return _cameraController.VerticalSpeed; }
            set { _cameraController.VerticalSpeed = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SpeedVertical")); }
        }

        public string MaxMovementSpeed
        {
            get { return Convert.ToString(_cameraController.MaxMovementSpeed); }
            set
            {
                try
                {
                    _cameraController.MaxMovementSpeed = Convert.ToDouble(value);
                }
                catch (Exception)
                {
                    _cameraController.MaxMovementSpeed = 0;
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MaxMovementSpeed"));
            }
        }

        public string MaxVerticalSpeed
        {
            get { return Convert.ToString(_cameraController.MaxVerticalSpeed); }
            set
            {
                try
                {
                    _cameraController.MaxVerticalSpeed = Convert.ToDouble(value);
                }
                catch (Exception)
                {
                    _cameraController.MaxVerticalSpeed = 0;
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MaxVerticalSpeed"));
            }
        }
        #endregion Camera Speed Control

        // test roll
        public double CameraRoll
        {
            get
            {
                return _cameraController.RollSliderMovement;
            }
            set
            {
                _cameraController.RollSliderMovement = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CameraRoll"));
            }
        }

        public bool CameraForceFeedback
        {
            get
            {
                return _cameraController.IsForceFeedback;
            }
            set
            {
                _cameraController.IsForceFeedback = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CameraForceFeedback"));
            }
        }

        public bool EnableCockpitCameraControl
        {
            get
            {
                return _cameraController.UseCockpitCameraControl;
            }
            set
            {
                _cameraController.UseCockpitCameraControl = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EnableCockpitCameraControl"));
            }
        }

        public bool DisableHorizontalRotation
        {
            get
            {
                return _cameraController.DisableHorizontalRotation;
            }
            set
            {
                _cameraController.DisableHorizontalRotation = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DisableHorizontalRotation"));
            }
        }

        public bool DisableHorizontalMovement
        {
            get
            {
                return _cameraController.DisableHorizontalMovement;
            }
            set
            {
                _cameraController.DisableHorizontalMovement = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DisableHorizontalMovement"));
            }
        }

        #endregion Databinding Properties

        // ICommand
        public ICommand BtnConnectClick { get { return new ButtonCommand(DataDisplayerConnectionControl); } }

        public ICommand BtnMoveForward { get { return new ButtonCommand(DataDisplayerDirectCameraControl_Forward); } }
        public ICommand BtnMoveBack { get { return new ButtonCommand(DataDisplayerDirectCameraControl_Back); } }
        public ICommand BtnMoveLeft { get { return new ButtonCommand(DataDisplayerDirectCameraControl_Left); } }
        public ICommand BtnMoveRight { get { return new ButtonCommand(DataDisplayerDirectCameraControl_Right); } }
        public ICommand BtnStopMove { get { return new ButtonCommand(DataDisplayerDirectCameraControl_Stop); } }

        public ICommand BtnSubscribe { get { return new ButtonCommand(Subscribe); } }
        public ICommand BtnUnsubscribe { get { return new ButtonCommand(Unsubscribe); } }



        public bool Experimental_MouseKeyboardControl
        {
            get
            {
                return _cameraController.UseMouseKeyboardControl;
            }
            set
            {
                _cameraController.UseMouseKeyboardControl = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Experimental_MouseKeyboardControl"));
            }
        }


        #region Databinding Properties For Display Purpose
        public string TextBlockSetCameraData
        {
            get
            {
                var output = _cameraController.setCamera;
                return JsonConvert.SerializeObject(output);
            }
        }


        public string TextBlockCameraHeading
        {
            get
            {
                return Convert.ToString(_cameraController.camera.Heading);
            }
        }

        public string TextBlockCameraRoll
        {
            get
            {
                return Convert.ToString(_cameraController.camera.Roll);
            }
        }

        public string TextBlockCameraPitch
        {
            get
            {
                return Convert.ToString(_cameraController.camera.Pitch);
            }
        }

        public string TextBlockCameraPosition
        {
            get
            {
                var positionData = _cameraController.camera.P;
                var posX = positionData.X;
                var posY = positionData.Y;
                var posZ = positionData.Z;
                return string.Format("X: {0}\nY: {1}\nZ: {2}", posX, posY, posZ);
            }
        }

        public string TextBlockUpdateInterval
        {
            get
            {
                return string.Format("{0}ms", _dataDisplayer.DisplayerUpdatedInterval);
            }
            set
            {
                _dataDisplayer.DisplayerUpdatedInterval = Convert.ToInt32(value);
                _timer.Interval = TimeSpan.FromMilliseconds(_dataDisplayer.DisplayerUpdatedInterval);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TextBlockUpdateInterval"));
            }
        }

        public string TextBlockDcsConnected
        {
            get
            {
                if (!_dataDisplayer.DisplayerConnected)
                {
                    return Properties.Resources.connect;
                }
                else
                {
                    return Properties.Resources.disconnect;
                }
            }
        }
        #endregion

        /// <summary>
        /// Contructor for CameraDataDisplayerViewModel class
        /// </summary>
        public CameraDataDisplayerViewModel()
        {
            camera = new Camera();
            _dataDisplayer = new CameraDataDisplayer();
            //_cameraController = new CameraController(_dataDisplayer.camera);
            _cameraController = new CameraController(camera);

            PropertyChanged += CameraDataDisplayerViewModel_PropertyChanged;

            //_connectCommand = new ButtonConnectCommand(this);
            //_disconnectCommand = new ButtonDisconnectCommand(this);

            // implement a update timer heres
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(_dataDisplayer.DisplayerUpdatedInterval)
            };

            _timer.Tick += _timer_Tick;

            AppDomain.CurrentDomain.UnhandledException += DisplayerMouseKeyboardHookDisposalOnException;
        }

        private void CameraDataDisplayerViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "JoystickLeftVerticalMovement" || e.PropertyName == "JoystickLeftHorizontalMovement")
            {
                // generate new set camera
                _cameraController.MoveCamera();
                // MessageBox.Show(string.Format("{0} {1} {2}", _cameraController.setCamera.P.X, _cameraController.setCamera.P.Y, _cameraController.setCamera.P.Z));
            }

            if (e.PropertyName == "JoystickRightVerticalMovement" || e.PropertyName == "JoystickRightHorizontalMovement" || e.PropertyName == "CameraRoll")
            {
                // generate new set camera
                _cameraController.RotateCamera();
                // MessageBox.Show(string.Format("{0} {1} {2}", _cameraController.setCamera.P.X, _cameraController.setCamera.P.Y, _cameraController.setCamera.P.Z));
            }

            if (e.PropertyName == "ZoomCameraSlider")
            {
                _cameraController.ZoomCamera();
            }

            if (e.PropertyName == "HeightChangeCameraSlider")
            {
                _cameraController.HeightChangeCamera();
            }

            if (e.PropertyName == "Experimental_MouseKeyboardControl")
            {
                if (Experimental_MouseKeyboardControl)  // hook enabled
                {
                    Subscribe();
                }
                else
                {
                    Unsubscribe();
                }
            }
        }

        //private void VirtualJoystickViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        //{
        //    MessageBox.Show("vm property changed!");
        //}

        /// <summary>
        /// Timer will run every 0.01 second. It create a connection via TCP to dcs. Read and then write data to stream
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _timer_Tick(object sender, EventArgs e)
        {
            // check if connection can be established by try to create a socket with given port number
            bool connectedCheck;
            string jsonCameraData;

            try
            {
                // SETCAMERA IS A DELTA VALUE, NOT CAMERA DATA VALUE

                var jsonSetCameraData = JsonConvert.SerializeObject(_cameraController.setCamera);
                ////MessageBox.Show(jsonSetCameraData);
                streamWriter.WriteLine(jsonSetCameraData);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TextBlockSetCameraData"));

                // currently this create a new tcp connection every time a set of data needs to be sent to lua server
                // 

                jsonCameraData = streamReader.ReadLine();

                connectedCheck = true;
            }
            catch (Exception exception)
            {
                System.Windows.MessageBox.Show(exception.Message);

                streamWriter = null;
                jsonCameraData = null;
                connectedCheck = false;
                _dataDisplayer.DisplayerConnected = false;
                //System.Windows.MessageBox.Show(exception.ToString());
                // abort connection here?

                tcpClient.Close();

                _timer.Stop();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TextBlockDcsConnected"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CanChangeConnectionData")); 
            }

            if (connectedCheck)
            {
                //MessageBox.Show(jsonCameraData);
                if(jsonCameraData != null)
                {
                    _cameraController.ParseCameraData(jsonCameraData);  // parsing data
                }
                
                _cameraController.UpdateCameraData();

                //_cameraController.PrepareSetCamera(_dataDisplayer.camera);  -- WRONG


                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TextBlockDcsConnected"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CanChangeConnectionData"));

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TextBlockCameraPosition"));

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TextBlockCameraHeading"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TextBlockCameraRoll"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TextBlockCameraPitch"));


                

                _dataDisplayer.DisplayerConnected = true;
            }

        }


        /// <summary>
        /// This method is the behavior on connect button click
        /// </summary>
        public void DataDisplayerConnectionControl()  // control connect and disconnect
        {
            if (!_dataDisplayer.DisplayerConnected)
            {
                // not connected, can connect

                // first check if connection can be established
                try
                {
                    tcpClient = new TcpClient(_dataDisplayer.IPAddress, _dataDisplayer.Port);
                    networkStream = tcpClient.GetStream();
                    networkStream.WriteTimeout = 100;
                    networkStream.ReadTimeout = 100;

                    streamReader = new StreamReader(networkStream);
                    streamWriter = new StreamWriter(networkStream) { AutoFlush = true };

                    streamWriter.WriteLine();
                    string str = streamReader.ReadLine();

                    //System.Windows.MessageBox.Show(str);

                    _timer.Start();
                } 
                catch (Exception exception)
                {
                    System.Windows.MessageBox.Show(exception.Message);
                }
            }
            else
            {
                // connected, should disconnect
                tcpClient.Close();
                _timer.Stop();
                _dataDisplayer.DisplayerConnected = false;
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TextBlockDcsConnected"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CanChangeConnectionData"));
        }


        /// <summary>
        /// This method is for the command to control camera movement with keyboard directly, always use max speed = 1
        /// </summary>

        public void DataDisplayerDirectCameraControl_Forward()
        {
            JoystickLeftVerticalMovement = 1;
        }

        public void DataDisplayerDirectCameraControl_Back()
        {
            JoystickLeftVerticalMovement = -1;
        }

        public void DataDisplayerDirectCameraControl_Left()
        {
            JoystickLeftHorizontalMovement = -1;
        }

        public void DataDisplayerDirectCameraControl_Right()
        {
            JoystickLeftHorizontalMovement = 1;
        }

        private void DataDisplayerDirectCameraControl_Stop()
        {
            JoystickLeftVerticalMovement = 0;
            JoystickLeftHorizontalMovement = 0;
        }


        class ButtonCommand : ICommand
        {
            private Action task;
            //private Func<bool> commit;

            //public ButtonCommand(Action action, Func<bool> func)
            public ButtonCommand(Action action)
            {
                task = action;
                //commit = func;
            }

            public event EventHandler CanExecuteChanged;

            public bool CanExecute(object parameter)  // Validation
            {
                //return commit();
                // just a few examples here:
                // if
                return true;
            }

            public void Execute(object parameter)  // Execution
            {
                task();
            }
        }
    }
}