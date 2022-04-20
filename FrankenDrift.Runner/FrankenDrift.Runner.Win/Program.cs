using Eto.Forms;
using System;

namespace FrankenDrift.Runner.Win
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            AppDomain current = AppDomain.CurrentDomain;
            current.UnhandledException += Current_UnhandledException;
            new Application(Eto.Platforms.WinForms).Run(new MainForm());
        }

        private static void Current_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("An unhandled exception has occurred, and FrankenDrift needs to shut down. Here's all we know:\n\n" + e.ExceptionObject.ToString(), "FrankenDrift Critical Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
        }
    }
}
