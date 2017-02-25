using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Runtime.InteropServices;
using System.Management;
using System.IO;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Threading;
using Ivi.Visa.Interop;

namespace LabSupply
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    /// 

    /*
     * E36103A
     * MY56476536
     * USB0::0x2A8D::0x0702::MY56476536::0::INSTR
     * 
     * E36104A
     * MY55506105
     * USB0::0x2A8D::0x0802::MY55506105::0::INSTR
     * 
     * E3633A
     * unknown
     * GPIB0::5::INSTR
     */

    public class KeySightInstrument
    {
        protected string ConnectionString;
        protected ResourceManager rm;
        public FormattedIO488 ioobj;

        protected KeySightInstrument()
        {
            
        }

        /*
        ~KeySightInstrument()
        {
            Close();
        }
        */

        protected void Open(string ConnectionString)
        {
            this.ConnectionString = ConnectionString;
            rm = new ResourceManager();
            ioobj = new FormattedIO488();
            ioobj.IO = (Ivi.Visa.Interop.IMessage)rm.Open(ConnectionString, AccessMode.NO_LOCK, 0, "");
        }

        protected void Close()
        {
            try
            {
                ioobj.IO.Close();
            }
            catch { }
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(ioobj);
            }
            catch { }
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(rm);
            }
            catch { }
        }
            
        public void SendString(string Message)
        {
            ioobj.WriteString(Message, true);
        }

        public string ReadString()
        {
            try
            {   
                return ioobj.ReadString();
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public double ReadDouble()
        {
            try
            {
                return (double) ioobj.ReadNumber();
            }
            catch (Exception e)
            {
                return -9.999;
            }
        }

        public bool ReadBool()
        {
            try
            {
                if (ioobj.ReadNumber() == 1)
                    return true;
                else
                    return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }

    public class KeySightLabSupply : KeySightInstrument
    {

        public KeySightLabSupply(string ConnectionString)
        {
            Open(ConnectionString);
        }

        public double SetVoltage
        {
            get
            {
                SendString("VOLTAGE?");
                return ReadDouble();
            }
            set
            {
                string cmd = string.Format("VOLTAGE {0:0.000}", value);
                SendString(cmd);
            }
        }

        public double SetCurrent
        {
            get
            {
                SendString("CURRENT?");
                return ReadDouble();
            }
            set
            {
                string cmd = string.Format("CURRENT {0:0.000}", value);
                SendString(cmd);
            }
        }

        public bool OutputOn
        {
            get
            {
                SendString("OUTPUT?");
                return ReadBool();
            }
            set
            {
                if (value)
                    SendString("OUTPUT ON");
                else
                    SendString("OUTPUT OFF");
            }
        }

        public double MeasuredVoltage
        {
            get
            {
                SendString("MEASURE:VOLTAGE?");
                return ReadDouble();
            }
        }

        public double MeasuredCurrent
        {
            get
            {
                SendString("MEASURE:CURRENT?");
                return ReadDouble();
            }
        }
    }

    /*
     *  The Command Class
     */

    public class UiCommand : ICommand
    {
        private Action _Execute;
        private Func<bool> _CanExecute;
        public event EventHandler CanExecuteChanged;

        public UiCommand(Action Execute, Func<bool> CanExecute)
        {
            _Execute = Execute;
            _CanExecute = CanExecute;
        }
        public bool CanExecute(object parameter)
        {
            return _CanExecute();
        }
        public void Execute(object parameter)
        {
            _Execute();
        }
    }


    /*
     *  The ViewModel 
     */
    public class LabSupplyViewModel : INotifyPropertyChanged
    {
        private KeySightLabSupply supply;
        DispatcherTimer timer;
        private UiCommand buttonCommand;
        private DateTime ConnectedTimestamp = DateTime.Now;
        public string ActivityLogTxt { get; private set; }

        private double SetVoltage;
        private double _SetVoltage;
        private double SetCurrent;
        private double _SetCurrent;

        public event PropertyChangedEventHandler PropertyChanged;

        public LabSupplyViewModel()
        {
            supply = new KeySightLabSupply("USB0::0x2A8D::0x0702::MY56476536::0::INSTR");
            /*
            _SetCurrent = 0.0;
            SetCurrent = supply.SetCurrent;
            _SetVoltage = 0.0;
            SetVoltage = supply.SetVoltage;
            */
            //WriteLog(supply.SetVoltageString, false);
            //communicator.HidUtil.RaiseDeviceAddedEvent += DeviceAddedEventHandler;
            //communicator.HidUtil.RaiseDeviceRemovedEvent += DeviceRemovedEventHandler;
            //communicator.HidUtil.RaiseConnectionStatusChangedEvent += ConnectionStatusChangedHandler;

            buttonCommand = new UiCommand(this.OnButtonClicked, this.ButtonClickValid);

            WriteLog("Program started", true);

            //Configure and start timer
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(50);
            timer.Tick += TimerTickHandler;
            timer.Start();
        }

        /*
         * Local function definitions
         */

        public void OnButtonClicked()
        {
            WriteLog("Button clicked", false);
        }

        public bool ButtonClickValid()
        {
            return true;
        }

        private double StringToDouble(string InputString)
        {
            try
            {
                InputString = InputString.Trim();
                InputString = InputString.ToUpper();
                InputString = InputString.Replace("A", "");
                InputString = InputString.Replace("V", "");
                return double.Parse(InputString);
            }
            catch
            {
                return -9.999;
            }
        }

        // Add a line to the activity log text box
        void WriteLog(string message, bool clear)
        {
            // Replace content
            if (clear)
            {
                ActivityLogTxt = string.Format("{0}: {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), message);
            }
            // Add new line
            else
            {
                ActivityLogTxt += Environment.NewLine + string.Format("{0}: {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), message);
            }
        }
        
        public string SetVoltageTxt
        {
            get
            {
                return string.Format("{0:0.000}V", SetVoltage);
            }
            set
            {
                double SetVoltage = StringToDouble(value);
                if (SetVoltage == -9.999)
                    SetVoltage = _SetVoltage;
                else
                    supply.SetVoltage = SetVoltage;
            }
        }

        public string SetCurrentTxt
        {
            get
            {
                return string.Format("{0:0.000}A", SetCurrent);
            }
            set
            {
                double SetCurrent = StringToDouble(value);
                if (SetCurrent == -9.999)
                    SetCurrent = _SetCurrent;
                else
                    supply.SetCurrent = SetCurrent;
            }
        }

        public bool OutputOn
        {
            get
            {
                return supply.OutputOn;
            }
            set
            {
                supply.OutputOn = value;
            }
        }

        public string MeasuredVoltageTxt
        {
            get
            {
                return string.Format("{0:0.000}V", supply.MeasuredVoltage);
            }
        }

        public string MeasuredCurrentTxt
        {
            get
            {
                double current = supply.MeasuredCurrent;
                if(current < 0.001)
                    return string.Format("{0:0.000}mA", current*1000);
                else
                    return string.Format("{0:0.000}A", current);
            }
        }

        public ICommand ToggleClick
        {
            get
            {
                return buttonCommand;
            }
        }


        public void TimerTickHandler(object sender, EventArgs e)
        {
            if (PropertyChanged != null)
            {
                SetVoltage = supply.SetVoltage;
                if(_SetVoltage!=SetVoltage)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("SetVoltageTxt"));
                    _SetVoltage = SetVoltage;
                }
                SetCurrent = supply.SetCurrent;
                if (_SetCurrent != SetCurrent)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("SetCurrentTxt"));
                    _SetCurrent = SetCurrent;
                }
                PropertyChanged(this, new PropertyChangedEventArgs("OutputOn"));
                PropertyChanged(this, new PropertyChangedEventArgs("ActivityLogTxt"));
                PropertyChanged(this, new PropertyChangedEventArgs("MeasuredVoltageTxt"));
                PropertyChanged(this, new PropertyChangedEventArgs("MeasuredCurrentTxt"));
            }
        }

    }


    public partial class App : Application
    {

    }
}

