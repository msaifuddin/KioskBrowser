using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using MouseKeyboardActivityMonitor;
using MouseKeyboardActivityMonitor.WinApi;
using System.Diagnostics;
using Microsoft.Win32;

namespace Kiosk
{
    public partial class MainWindow : Form
    {
        //add keyHook
        private KeyboardHookListener m_KeyboardHookManager;
        private bool m_leftwin;
        private bool m_rightwin;
        private bool m_lalt;
        private bool m_ralt;

        //Taskbar hack
        [DllImport("user32.dll")]
        private static extern int ShowWindow(int hwnd, int cmd);

        [DllImport("user32.dll")]
        private static extern int FindWindow(string cls, string wndwText);

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 1;

        int hwnd = FindWindow("Shell_TrayWnd", "");

        public MainWindow()
        {
            InitializeComponent();

            // start keyHook
            m_KeyboardHookManager = new KeyboardHookListener(new GlobalHooker());
            m_KeyboardHookManager.KeyDown += hookManager_keyDown;
            m_KeyboardHookManager.Enabled = true;

            //start hiding taskbar
            ShowWindow(hwnd, SW_HIDE);
        }

        private void examApp_Load(object sender, EventArgs e) //load app
        {
            //run batch in hidden window
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = @"/c hideScreen.exe";
            process.StartInfo = startInfo;
            process.Start();

            string url = System.IO.File.ReadAllText(@"url.lst");
            webBrowser1.Navigate(url, "_self", null, "User-Agent: Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:23.0) Gecko/20131011 Firefox/23.0");
            webBrowser1.ScriptErrorsSuppressed = true;
        }

        private void exitButton_Click(object sender, EventArgs e) //on exit event
        {
            //display exit confirm
            if (MessageBox.Show("Are you sure you want to exit?",
                "Confirm Exit", MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) ==
                DialogResult.No) { }
            else
            {
                m_KeyboardHookManager.Dispose(); //exit keyhook
                ShowWindow(hwnd, SW_SHOW); //restore taskbar
                //restore batch in hidden window
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = @"/c showScreen.exe";
                process.StartInfo = startInfo;
                process.Start();
                Application.Exit(); //bye!
            }
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
        }

        private void bgPanel_Paint(object sender, PaintEventArgs e)
        {
        }

        private void hookManager_keyDown(object sender, KeyEventArgs e)
        {   
            //disable WinKeys
            if (e.KeyCode == Keys.LWin) e.Handled = true;
            if (e.KeyCode == Keys.RWin) e.Handled = true;
            //disable Apps key
            if (e.KeyCode == Keys.Apps) e.Handled = true;
            //disable Ctrl
            if (e.KeyCode == Keys.ControlKey) e.Handled = true;
            //disable Escape
            if (e.KeyCode == Keys.Escape) e.Handled = true;
            //disable PrntScr
            if (e.KeyCode == Keys.PrintScreen) e.Handled = true;
            //disable Alt+Tab
            if (e.KeyCode == Keys.RMenu) m_ralt = true;
            if (e.KeyCode == Keys.LMenu) m_lalt = true;

            if ((m_ralt || m_lalt) && e.KeyCode == Keys.Tab) e.Handled = true;
            //disable Fn keys
            if (e.KeyCode == Keys.F1 || e.KeyCode == Keys.F2 || e.KeyCode == Keys.F3 || e.KeyCode == Keys.F4 || e.KeyCode == Keys.F5
                || e.KeyCode == Keys.F6 || e.KeyCode == Keys.F7 || e.KeyCode == Keys.F8 || e.KeyCode == Keys.F9 || e.KeyCode == Keys.F10
                ) e.Handled = true;

            /*
            disable win+shortcut
            Ref: http://support.microsoft.com/kb/126449
            */

            if (e.KeyCode == Keys.LWin) m_leftwin = true;
            if (e.KeyCode == Keys.RWin) m_rightwin = true;

            //disable win+R (Run)
            if ((m_leftwin || m_rightwin) && e.KeyCode == Keys.R) e.Handled = true;
            //disable win+L (Lock)
            if ((m_leftwin || m_rightwin) && e.KeyCode == Keys.L) e.Handled = true;
            //disable win+M (Minimize)
            if ((m_leftwin || m_rightwin) && e.KeyCode == Keys.M) e.Handled = true;
            //disable shift+win+M (Undo Minimize All)
            if (e.KeyCode == Keys.ShiftKey && (m_leftwin || m_rightwin) && e.KeyCode == Keys.M) e.Handled = true;
            //disable win+F1 (Help)
            if ((m_leftwin || m_rightwin) && e.KeyCode == Keys.F1) e.Handled = true;
            //disable win+E (Explorer)
            if ((m_leftwin || m_rightwin) && e.KeyCode == Keys.E) e.Handled = true;
            //disable win+F (Find)
            if ((m_leftwin || m_rightwin) && e.KeyCode == Keys.F) e.Handled = true;
            //disable win+D (Minimize All)
            if ((m_leftwin || m_rightwin) && e.KeyCode == Keys.D) e.Handled = true;
            //disable ctr+win+F (Find Computers in AD)
            if (e.KeyCode == Keys.ControlKey && (m_leftwin || m_rightwin) && e.KeyCode == Keys.F) e.Handled = true;
            //disable ctrl+win+tab (Moves focus from Start, to the Quick Launch toolbar, to the system tray
            if (e.KeyCode == Keys.ControlKey && (m_leftwin || m_rightwin) && e.KeyCode == Keys.Tab) e.Handled = true;
            //disable win+tab (Cycle through taskbar)
            if ((m_leftwin || m_rightwin) && e.KeyCode == Keys.Tab) e.Handled = true;

            //disable browser key combo
            if ((e.KeyCode == Keys.ControlKey) && e.KeyCode == Keys.A) e.Handled = true;  //Ctrl+A (Select All)
            if ((e.KeyCode == Keys.ControlKey) && e.KeyCode == Keys.B) e.Handled = true;  //Ctrl+B (Favourites)
            if ((e.KeyCode == Keys.ControlKey) && e.KeyCode == Keys.C) e.Handled = true;  //Ctrl+C (Copy)
            if ((e.KeyCode == Keys.ControlKey) && e.KeyCode == Keys.F) e.Handled = true;  //Ctrl+F (Find)
            if ((e.KeyCode == Keys.ControlKey) && e.KeyCode == Keys.H) e.Handled = true;  //Ctrl+H (History)
            if ((e.KeyCode == Keys.ControlKey) && e.KeyCode == Keys.L) e.Handled = true;  //Ctrl+L (Locate)
            if ((e.KeyCode == Keys.ControlKey) && e.KeyCode == Keys.N) e.Handled = true;  //Ctrl+N (New Window)
            if ((e.KeyCode == Keys.ControlKey) && e.KeyCode == Keys.O) e.Handled = true;  //Ctrl+O (Open)
            if ((e.KeyCode == Keys.ControlKey) && e.KeyCode == Keys.R) e.Handled = true;  //Ctrl+R (Refresh)
            if ((e.KeyCode == Keys.ControlKey) && e.KeyCode == Keys.S) e.Handled = true;  //Ctrl+S (Save)
            if ((e.KeyCode == Keys.ControlKey) && e.KeyCode == Keys.V) e.Handled = true;  //Ctrl+V (Paste)
            if ((e.KeyCode == Keys.ControlKey) && e.KeyCode == Keys.W) e.Handled = true;  //Ctrl+W (Close)
            if ((e.KeyCode == Keys.ControlKey) && e.KeyCode == Keys.X) e.Handled = true;  //Ctrl+X (Cut)

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string url = System.IO.File.ReadAllText(@"url.lst");
            webBrowser1.Navigate(url, "_self", null, "User-Agent: Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:23.0) Gecko/20131011 Firefox/23.0");
            webBrowser1.ScriptErrorsSuppressed = true;
        }
    }
}
