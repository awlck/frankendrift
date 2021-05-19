using System;
using Eto.Forms;
using Eto.Drawing;

namespace Adravalon.Runner
{
	public partial class AdriftInput : TextBox, Glue.RichTextBox
	{
        public int TextLength => Text.Length;
        public int SelectionStart { get => Selection.Start; set => Selection = Selection.WithStart(value); }
        public int SelectionLength { get => Selection.Length(); set => Selection = Selection.WithLength(value); }

        public void Clear() => Text = "";
    }
}
