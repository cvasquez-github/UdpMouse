using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Hardcodet.Wpf.TaskbarNotification;
using Newtonsoft.Json;
using SimpleUdp;

namespace UdpMouse
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll", EntryPoint = "SetCursorPos")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        //Mouse DWORD actions
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;
        private const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        private const int MOUSEEVENTF_MIDDLEUP = 0x0040;
        public MainWindow()
        {
            InitializeComponent();
        }

        UdpEndpoint udpServer;
        UdpMouseWebServer webServer;
        TaskbarIcon tbi;
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            udpServer = new UdpEndpoint("127.0.0.1", 8000);
            udpServer.EndpointDetected += EndpointDetected;
            udpServer.DatagramReceived += DatagramReceived;
            udpServer.Start();

            webServer = new UdpMouseWebServer(5000, this);

            tbi = new TaskbarIcon();
            tbi.ToolTipText = "Udp Mouse";
            this.Hide();
        }

        private void DatagramReceived(object sender, Datagram dg)
        {
            string data = Encoding.UTF8.GetString(dg.Data);
            InputCommand input = JsonConvert.DeserializeObject<InputCommand>(data);
            if (input.x >= 0 && input.y >= 0)
            {
                DoMouseAction(input.x, input.y, input.action);
            }
        }

        private void EndpointDetected(object sender, EndpointMetadata endpoint)
        {
            
        }


        public void DoMouseAction(int x, int y, InputCommand.InputAction action)
        {
            Task.Run(() =>TaskDoMouseAction(x,y, action));
        }

        private void TaskDoMouseAction(int x, int y, InputCommand.InputAction action)
        {
            try
            {
                SetCursorPos((int)x, (int)y);

                if (action == InputCommand.InputAction.Click || action == InputCommand.InputAction.DblClick)
                {
                    mouse_event(MOUSEEVENTF_LEFTDOWN, (uint)x, (uint)y, 0, 0);
                    Task.Delay(50);
                    mouse_event(MOUSEEVENTF_LEFTUP, (uint)x, (uint)y, 0, 0);
                }
                
                if (action == InputCommand.InputAction.DblClick)
                {
                    Task.Delay(50);
                    mouse_event(MOUSEEVENTF_LEFTDOWN, (uint)x, (uint)y, 0, 0);
                    Task.Delay(50);
                    mouse_event(MOUSEEVENTF_LEFTUP, (uint)x, (uint)y, 0, 0);
                }
            }
            catch
            {
                
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            udpServer.Stop();
        }
    }

    
}
