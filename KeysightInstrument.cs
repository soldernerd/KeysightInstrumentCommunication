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

namespace Instrument
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
     * 
     * 34465A
     * MY54502615
     * USB0::0x2A8D::0x0101::MY54502615::0::INSTR
     */

    public class KeysightInstrument
    {
        protected string ConnectionString;
        protected ResourceManager rm;
        public FormattedIO488 ioobj;

        protected KeysightInstrument()
        {
            
        }

        ~KeysightInstrument()
        {
            Close();
        }

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
}

