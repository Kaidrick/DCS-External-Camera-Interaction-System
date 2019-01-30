using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DCS_AECIS.ViewModel
{
    class DataUpdaterViewModel : INotifyPropertyChanged
    {
        private DataUpdater _updater;

        private ButtonConnectCommand _connectCommand;
        private ButtonDisconnectCommand _disconnectCommand;

        public event PropertyChangedEventHandler PropertyChanged;

        public string TextBlockConnCounter {
            get
            {
                return Convert.ToString(_updater.ConnCounter);
            }
        }

        public string TextBlockUpdateInterval
        {
            get
            {
                return string.Format("{0}ms", _updater.UpdateInterval);
            }
            set
            {
                _updater.UpdateInterval = Convert.ToInt32(value);
            }
        }

        public string TextBlockDcsConnected
        {
            get
            {
                if (_updater.DcsConnected)
                {
                    return "Connected";
                }
                else
                {
                    return "Disconnected";
                }
            }
        }

        public string TextBlockIpAddress
        {
            get
            {
                return _updater.IpAddress;  // is already a string, no need to convert
            }
            set
            {
                _updater.IpAddress = value;
            }
        }

        public string TextBlockPort
        {
            get
            {
                return Convert.ToString(_updater.Port);
            }
            set
            {
                _updater.Port = Convert.ToInt32(value);
            }
        }

        // constructor
        public DataUpdaterViewModel(GameCamera gameCamera)
        {
            _updater = new DataUpdater(gameCamera);
            _connectCommand = new ButtonConnectCommand(this);
            _disconnectCommand = new ButtonDisconnectCommand(this);
        }

        public ICommand BtnConnectClick
        {
            get
            {
                return _connectCommand;
            }
        }

        public ICommand BtnDisconnectClick
        {
            get
            {
                return _disconnectCommand;
            }
        }

        

        // methods
        // start update
        public void Connect()
        {
            _updater.Connect();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TextBlockDcsConnected"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TextBlockConnCounter"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TextBlockCameraPosition"));
            
        }

        public void Disconnect()
        {
            _updater.Disconnect();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TextBlockDcsConnected"));
        }

        
        

    }

    class ButtonConnectCommand : ICommand
    {
        private DataUpdaterViewModel _updaterViewModel;
        public ButtonConnectCommand(DataUpdaterViewModel updaterViewModel)
        {
            _updaterViewModel = updaterViewModel;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _updaterViewModel.Connect();
        }
    }

    class ButtonDisconnectCommand : ICommand
    {
        private DataUpdaterViewModel _updaterViewModel;
        public ButtonDisconnectCommand(DataUpdaterViewModel updaterViewModel)
        {
            _updaterViewModel = updaterViewModel;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _updaterViewModel.Disconnect();
        }
    }


}
