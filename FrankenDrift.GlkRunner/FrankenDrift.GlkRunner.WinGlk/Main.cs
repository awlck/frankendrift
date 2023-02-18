using FrankenDrift.GlkRunner;
using System.Runtime.InteropServices;

namespace FrankenDrift.GlkRunner.WinGlk
{
    // WinGlk startup functions.
    internal static class Winglk_Pinvoke
    {
        [DllImport("Glk")]
        internal static extern int InitGlk(uint version);
        [DllImport("Glk")]
        internal static extern void winglk_app_set_name([MarshalAs(UnmanagedType.LPStr)] string name);
    }


    public class WinGlkRunner
    {
        [STAThread]
        public static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Error: No file selected!");
                return 1;
            }

            Glk.GlkUtil.SelectGlkLib("Glk");
            if (Winglk_Pinvoke.InitGlk(0x00000704) == 0) { return 2; }
            Winglk_Pinvoke.winglk_app_set_name("Windows Glk FrankenDrift");

            var sess = new MainSession(args[^1]);
            sess.Run();

            return 0;
        }
    }
}