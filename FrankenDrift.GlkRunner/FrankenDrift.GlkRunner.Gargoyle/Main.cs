using FrankenDrift.GlkRunner;
using System.Reflection;
using System.Runtime.InteropServices;

namespace FrankenDrift.GlkRunner.Gargoyle
{
    static class Garglk_Pinvoke
    {
        // Garglk initialization functions.
        [DllImport("libgarglk")]
        internal static extern void gli_startup(int argc, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] string[] argv);
        [DllImport("libgarglk")]
        internal static extern void garglk_set_program_name([MarshalAs(UnmanagedType.LPStr)] string name);
    }

    public class GarGlkRunner
    {
        [STAThread]
        public static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Error: No file selected!");
                return 1;
            }

            Glk.GlkUtil.SelectGlkLib("libgarglk");
            Startup(args);

            var sess = new MainSession(args[^1]);
            sess.Run();

            return 0;
        }

        private static void Startup(string[] args)
        {
            string[] argv = new string[args.Length + 1];
            argv[0] = Assembly.GetEntryAssembly().Location;
            for (int i = 1; i <= args.Length; i++)
            {
                argv[i] = args[i - 1];
            }
            Garglk_Pinvoke.gli_startup(argv.Length, argv);
            Garglk_Pinvoke.garglk_set_program_name("FrankenDrift for Gargoyle");
        }
    }
}