﻿using Eto.Forms;
using System;

namespace FrankenDrift.Runner.Mac
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
			Eto.Mac.CrashReporter.Attach();
            new Application(Eto.Platforms.Mac64).Run(new MainForm());
        }
    }
}
