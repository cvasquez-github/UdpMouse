using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
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

namespace UdpMouseClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SendUdpMouseAction(int x, int y, InputCommand.InputAction action)
        {
            SendUdpMouseMessage(new InputCommand() { action = action, x = x, y = y });
        }
        
        private void SendUdpMouseMessage(InputCommand command)
        {
            var client = new UdpClient();
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8000);
            client.Connect(ep);

            string output = JsonConvert.SerializeObject(command);
            byte[] outputBytes = Encoding.UTF8.GetBytes(output); ;

            client.Send(outputBytes, outputBytes.Length);

            client.Close();
        }

        private void BtnClick_Click(object sender, RoutedEventArgs e)
        {
            SendUdpMouseAction(int.Parse(InputX.Text), int.Parse(InputY.Text), InputCommand.InputAction.Click);
        }

        private void BtnDblClick_Click(object sender, RoutedEventArgs e)
        {
            SendUdpMouseAction(int.Parse(InputX.Text), int.Parse(InputY.Text), InputCommand.InputAction.DblClick);
        }

        private void BtnMove_Click(object sender, RoutedEventArgs e)
        {
            SendUdpMouseAction(int.Parse(InputX.Text), int.Parse(InputY.Text), InputCommand.InputAction.Move);
        }
    }
}
