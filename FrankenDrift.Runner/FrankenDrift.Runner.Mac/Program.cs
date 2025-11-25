using Eto.Forms;
using System;
using System.IO;

namespace FrankenDrift.Runner.Mac
{
    class Program
    {
        
        [STAThread]
        public static void Main(string[] args)
        {
			Eto.Mac.CrashReporter.Attach();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            new Application(Eto.Platforms.Mac64).Run(new MainForm());
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            File.WriteAllText("fdcrash.txt", e.ExceptionObject.ToString());
            Eto.Forms.MessageBox.Show($"An unhandled exception occurred within FrankenDrift: {e.ExceptionObject}", "Unhandled Exception", MessageBoxButtons.OK);
        }
    }
}
