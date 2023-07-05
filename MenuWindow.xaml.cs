using Newtonsoft.Json.Linq;
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
using System.Windows.Shapes;

namespace Gui_client
{
    public partial class MenuWindow : Window
    {
        private NetworkStream clientStream;

        public MenuWindow()
        {
            InitializeComponent();
            clientStream = GolbalClient.ClientStream;

        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
           
            JObject data = new JObject();
            bool flag = true;

            string json = data.ToString();
            // Example: Display the data in a message box
            MessageBox.Show(json);
            byte code = 10;
            int sizeOfJson = json.Length;
            byte[] size = BitConverter.GetBytes(sizeOfJson);
            Array.Reverse(size);
            byte[] dataBytes = Encoding.ASCII.GetBytes(json);
            byte[] message = new byte[1 + size.Length + dataBytes.Length];
            message[0] = code;
            Buffer.BlockCopy(size, 0, message, 1, size.Length);
            Buffer.BlockCopy(dataBytes, 0, message, 1 + size.Length, dataBytes.Length);

            flag = SendAndReceiveData(message);
            if (flag == true)
            {
                LoginWindow loginW = new LoginWindow();
                loginW.Show();
                this.Close();

            }
        }

        private void GetRoomsButton_Click(object sender, RoutedEventArgs e)
        {

           ButtonListWindow b = new ButtonListWindow();
            b.Show();
            this.Close();

            
        }

        private void CreateRoomButton_Click(object sender, RoutedEventArgs e)
        {
            CreateRoomWindow cr = new CreateRoomWindow();
            cr.Show();
            this.Close();
        }
        private bool SendAndReceiveData(byte[] message)
        {
            clientStream.Write(message, 0, message.Length);
            clientStream.Flush();

            byte[] responseBuffer = new byte[4096];
            int bytesRead = clientStream.Read(responseBuffer, 0, responseBuffer.Length);
            string response = Encoding.UTF8.GetString(responseBuffer, 0, bytesRead);
            MessageBox.Show(response); // Example: Display the data in a message box
            int len = Convert.ToInt32(response.Substring(8, 32), 2);
            JObject jsonObject = JObject.Parse(response.Substring(40, len));
            MessageBox.Show(jsonObject.ToString());
            if (jsonObject.ContainsKey("status"))
            {
                return true;
            }
            return false;

        }
       
    }
}
