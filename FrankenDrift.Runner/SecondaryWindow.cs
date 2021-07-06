using System;
using Adravalon.Glue;
using Eto.Drawing;
using Eto.Forms;

namespace Adravalon.Runner
{
    public class SecondaryWindow : Form
    {
        public AdriftOutput Output;

        private MainForm _main;

        public SecondaryWindow(MainForm main)
        {
            _main = main;
            InitializeComponent();
        }

        void InitializeComponent()
        {
            Title = "FrankenDrift Secondary";
            MinimumSize = new Size(400, 400);
            Padding = 10;

            Output = new AdriftOutput(_main);
            Content = Output;

            this.Closed += ReportClosing;
        }

        void ReportClosing(object? sender, EventArgs e)
        {
            if (_main is {IsDisposed: false}) _main.ReportSecondaryClosing(this);
        }
    }
}