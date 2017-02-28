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

namespace Multimeter
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    /// 
    


    public class KeysightMultimeter : Instrument.KeysightInstrument
    {

        public KeysightMultimeter(string ConnectionString)
        {
            Open(ConnectionString);
        }

        public double ReadVoltageDc
        {
            get
            {
                SendString("MEAS:VOLT:DC?");
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
    public class MultimeterViewModel : INotifyPropertyChanged
    {
        private KeysightMultimeter multimeter;
        DispatcherTimer timer;
        uint timerTickCount = 0;
        private UiCommand buttonCommand;
        private DateTime ConnectedTimestamp = DateTime.Now;
        public string ActivityLogTxt { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public MultimeterViewModel()
        {
            multimeter = new KeysightMultimeter("USB0::0x2A8D::0x0101::MY54502615::0::INSTR");

            buttonCommand = new UiCommand(this.OnButtonClicked, this.ButtonClickValid);

            WriteLog("Program started", true);

            //Configure and start timer
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1000);
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
        
        public string DcVoltageTxt
        {
            get
            {
                return string.Format("{0:0.000000}V", multimeter.ReadVoltageDc);
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
                PropertyChanged(this, new PropertyChangedEventArgs("DcVoltageTxt"));
                ++timerTickCount;
            }
        }

    }


    public partial class App : Application
    {

    }
}

