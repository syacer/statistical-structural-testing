using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GADEApproach
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// [DllImport("kernel32.dll", SetLastError = true)]
        /// 
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
