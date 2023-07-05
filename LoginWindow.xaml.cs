using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
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
using System.Net.Sockets;
using System.Net;

namespace Gui_client
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private NetworkStream clientStream;
        public LoginWindow()
        {
            InitializeComponent();
            clientStream = GolbalClient.ClientStream;
        }
        
        private void Login_Handler(object sender, RoutedEventArgs e)
        {
            string password = txtPassword.Password;
            string username = txtUsername.Text;
            JObject data = new JObject();
            bool flag = true;
            data["username"] = username;
            data["password"] = password;
            string json = data.ToString();
            // Example: Display the data in a message box
            MessageBox.Show(json);
            byte code = 1;
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
                GolbalClient.ClientName = username;
                MessageBox.Show("Succeed");
                MenuWindow menuWindow = new MenuWindow();
                menuWindow.Show();
                this.Close();

            }
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
