using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NLog;

namespace EclipseAutomation
{
    /// <summary>
    /// By configuring project as a console application, we get a free console window
    /// for the NLog ColoredConsole output, and we can still spawn a windows form for interactivity.
    /// (If you don't like the console window, just set the project to build as a Windows Application.)
    /// </summary>
    internal static class Program
    {
        private static Logger _logger = LogManager.GetLogger("Main");

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            _logger.Info("=================");
            _logger.Info("Application Start"); 
            _logger.Info("=================");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
