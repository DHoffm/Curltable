using System;
using System.Windows.Forms;

namespace Curl
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            CurlTable.Start();
        }
    }
}