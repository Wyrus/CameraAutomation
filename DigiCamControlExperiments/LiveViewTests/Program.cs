using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LiveViewTests
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"{ex.Message}({ex.GetType()})");
                System.Diagnostics.Debug.WriteLine($"{ex.StackTrace}");
            }
        }
    }
}
