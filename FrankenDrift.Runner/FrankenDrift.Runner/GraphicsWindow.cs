using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using FrankenDrift.Adrift;
using Eto.Forms;
using SysImage = System.Drawing.Image;
using EtoBitmap = Eto.Drawing.Bitmap;
using Size = Eto.Drawing.Size;

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
        }

        // Needs libgdiplus on linux
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
                    SysImage img = SharedModule.Blorb.GetImage(res, true, ref scratch);
                    if (img is null) return;
                    using var stream = new MemoryStream();
                    img.Save(stream, ImageFormat.Bmp);
                    _view.Image = new EtoBitmap(stream.ToArray());
                }
            }
            else
            {
                _view.Image = new EtoBitmap(path);
            }
        }
    }
}