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
            try
            {
                File.WriteAllText(Path.Combine(Environment.GetEnvironmentVariable("HOME"), "fdcrash.txt"), e.ExceptionObject.ToString());
            }
            catch (Exception exception)
            { /* do nothing */ }

            try
            {
                Eto.Forms.MessageBox.Show($"An unhandled exception occurred within FrankenDrift: {e.ExceptionObject}",
                    "Unhandled Exception", MessageBoxButtons.OK);
            }
            catch (Exception exception) { /* do nothing */ }
        }
    }
}
