using System.IO;
using Eto.Drawing;
using FrankenDrift.Adrift;
using Eto.Forms;

namespace FrankenDrift.Runner
{
    public class GraphicsWindow : Form
    {
        private MainForm _main;
        private ImageView _view;

        public GraphicsWindow(MainForm main)
        {
            _main = main;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Title = "FrankenDrift Graphics";
            MinimumSize = new Size(400, 400);
            Padding = 10;

            _view = new ImageView();
            Content = _view;

            Closed += GraphicsWindow_Closed;
        }

        private void GraphicsWindow_Closed(object sender, System.EventArgs e)
        {
            _main.ReportGraphicsClosing(this);
            this.Dispose();
        }

        public void DisplayImage(string path)
        {
            if (SharedModule.Adventure.BlorbMappings is {Count: > 0})
            {
                int res = 0;
                if (SharedModule.Adventure.BlorbMappings.ContainsKey(path))
                    res = SharedModule.Adventure.BlorbMappings[path];
                if (res > 0)
                {
                    var scratch = "";
                    var img = SharedModule.Blorb.GetImage(res, true, ref scratch);
                    if (img is null) return;
                    _view.Image = new Bitmap(img);
                }
            }
            else if (File.Exists(path))
            {
                _view.Image = new Bitmap(path);
            }
        }

        public void DisplayImage(Image img)
        {
            _view.Image = img;
        }
    }
}