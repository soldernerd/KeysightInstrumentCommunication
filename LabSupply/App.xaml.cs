using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
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
       public void DoInstrumentIO()
       {
           ResourceManager rm = new ResourceManager();
           FormattedIO488 ioobj = new FormattedIO488();
           //Ivi.Visa.Interop.ResourceManagerClass rm = new Ivi.Visa.Interop.ResourceManagerClass();
           //Ivi.Visa.Interop.FormattedIO488Class ioobj = new Ivi.Visa.Interop.FormattedIO488Class();
           try
           {
                object[] idnItems;
                ioobj.IO = (Ivi.Visa.Interop.IMessage)rm.Open("GPIB0::5::INSTR", AccessMode.NO_LOCK, 0, "");
                //ioobj.WriteString("*IDN?", true);
                //ioobj.WriteString("DISPlay?", true);
                //ioobj.WriteString("VOLT UP", true);
                ioobj.WriteString("VOLT 12.34", true);
                ioobj.WriteString("OUTPUT ON", true);

                idnItems = (object[])ioobj.ReadList(Ivi.Visa.Interop.IEEEASCIIType.ASCIIType_Any, ",");
                foreach (object idnItem in idnItems)
               {
                   //System.Console.Out.WriteLine(“IDN Item of type “ +idnItem.GetType().ToString());
                   //System.Console.Out.WriteLine(“\tValue of item is “ +idnItem.ToString());
               }
           }
           catch (Exception e)
           {
               //System.Console.Out.WriteLine(“An error occurred: “ +e.Message);
           }
           finally
           {
               try { ioobj.IO.Close(); }
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
       }
    }


    public partial class App : Application
    {

        

    }
}

