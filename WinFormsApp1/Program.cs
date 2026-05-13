using System.Runtime.InteropServices;

namespace WinFormsApp1
{
    internal static class Program
    {
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        [STAThread]
        static void Main()
        {
            SetProcessDPIAware();
            ApplicationConfiguration.Initialize();
            Application.Run(new DashBoard());
        }
    }
}