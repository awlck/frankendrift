using Eto.Forms;
using System;

using WinForms = System.Windows.Forms;

namespace FrankenDrift.Runner.Win
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            var app = new Application(Eto.Platforms.WinForms);
            WinForms.Application.SetHighDpiMode(WinForms.HighDpiMode.DpiUnawareGdiScaled);
            WinForms.Application.EnableVisualStyles();
            WinForms.Application.SetCompatibleTextRenderingDefault(false);
            app.Run(new MainForm());
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            WinForms.MessageBox.Show("An unhandled exception has occurred, and FrankenDrift needs to shut down. Here's all we know:\n\n" + e.ExceptionObject.ToString(), "FrankenDrift Critical Error",
                WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Error);
        }
    }
}
