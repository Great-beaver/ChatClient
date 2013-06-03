using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Server
{
    static class Program
    {

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        private const int SW_SHOWMAXIMIZED = 3;
        private const int SW_RESTORE = 9;

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            
            var processFound = false;

            Process me = Process.GetCurrentProcess();
            foreach (Process process in Process.GetProcessesByName(me.ProcessName))
            {
                if (process.Id != me.Id)
                {
                    ShowWindow(process.MainWindowHandle, SW_SHOWMAXIMIZED);
                    SetForegroundWindow(process.MainWindowHandle);

                    processFound = true;
                    break;
                }
            }

            if (processFound)
            {
                return;
            } 

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            DevExpress.Skins.SkinManager.EnableFormSkins();
            DevExpress.UserSkins.BonusSkins.Register();
            Application.Run(new ServerMainForm());
        }
    }
}
