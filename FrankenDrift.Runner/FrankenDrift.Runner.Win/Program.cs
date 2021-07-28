using Eto.Forms;
using System;

namespace FrankenDrift.Runner.Win
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            new Application(Eto.Platforms.WinForms).Run(new MainForm());
        }
    }
}
