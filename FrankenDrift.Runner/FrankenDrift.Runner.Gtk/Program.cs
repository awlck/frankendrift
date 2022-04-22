using Eto.Forms;
using System;

using GtkS = Gtk;

namespace FrankenDrift.Runner.Gtk
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            new Application(Eto.Platforms.Gtk).Run(new MainForm());
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var msg = new GtkS.MessageDialog(null, GtkS.DialogFlags.Modal, GtkS.MessageType.Error, GtkS.ButtonsType.Ok, "An unhandled exception has occurred, and FrankenDrift needs to shut down. Here's all we know:\n\n" + e.ExceptionObject.ToString());
            msg.Run();
            msg.Destroy();
        }
    }
}
