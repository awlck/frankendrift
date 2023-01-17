namespace FrankenDrift.Gargoyle
{
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

            Glk.GarGlk.Startup(args);

            var sess = new MainSession(args[args.Length - 1]);

            sess.Run();

            return 0;
        }
    }
}