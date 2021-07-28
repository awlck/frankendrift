using System;
using Eto.Forms;
using Eto.Drawing;

namespace FrankenDrift.Runner
{
	public class AdriftInput : TextBox, FrankenDrift.Glue.RichTextBox
	{
        public int TextLength => Text.Length;
        public int SelectionStart { get => Selection.Start; set => Selection = Selection.WithStart(value); }
        public int SelectionLength { get => Selection.Length(); set => Selection = Selection.WithLength(value); }

        public void Clear() => Text = "";
    }
}
