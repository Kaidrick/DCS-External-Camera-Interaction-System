using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCS_AECIS.ViewModel
{
    class CameraControllerViewModel : INotifyPropertyChanged
    {
        private Model.CameraController CameraController;
        private Model.Camera Camera;

        public event PropertyChangedEventHandler PropertyChanged;

        public CameraControllerViewModel()
        {
            Camera = new Model.Camera();
            CameraController = new Model.CameraController(Camera);
        }

        public double LeftJoystickVerticalMovement
        {
            get
            {
                return CameraController.LeftJoystickVerticalMovement;
            }
            set
            {
                CameraController.LeftJoystickVerticalMovement = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LeftJoystickVerticalMovement"));
            }
        }

        public double LeftJoystickHorizontalMovement
        {
            get
            {
                return CameraController.LeftJoystickHorizontalMovement;
            }
            set
            {
                CameraController.LeftJoystickHorizontalMovement = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LeftJoystickHorizontalMovement"));
            }
        }

    }
}
