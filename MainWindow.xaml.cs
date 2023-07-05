using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Windows;
using Newtonsoft.Json.Linq;
using System;
using System.Windows.Markup;

namespace Gui_client
{
    public partial class MainWindow : Window
    {
        private NetworkStream clientStream;

        public MainWindow()
        {

            InitializeComponent();
            TcpClient client = new TcpClient();
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9999);
            client.Connect(serverEndPoint);
            clientStream = client.GetStream();
            GolbalClient.ClientStream = clientStream;
        }

        private void SignupButton_Click(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text;
            string password = txtPassword.Password;
            string username = txtUsername.Text;
            JObject data = new JObject();
            bool flag = true;
            data["username"] = username;
            data["email"] = email;
            data["password"] = password;
            string json = data.ToString();
            // Example: Display the data in a message box
            MessageBox.Show(json);
            byte code = 2;
            int sizeOfJson = json.Length;
            byte[] size = BitConverter.GetBytes(sizeOfJson);
            Array.Reverse(size);
            byte[] dataBytes = Encoding.ASCII.GetBytes(json);
            byte[] message = new byte[1 + size.Length + dataBytes.Length];
            message[0] = code;
            Buffer.BlockCopy(size, 0, message, 1, size.Length);
            Buffer.BlockCopy(dataBytes, 0, message, 1 + size.Length, dataBytes.Length);

            flag = SendAndReceiveData(message);
            if(flag == true)
            {
                GoToLoginButton_Click(sender, e);
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
            int len = Convert.ToInt32(response.Substring(8, 32),2);
            JObject jsonObject = JObject.Parse(response.Substring(40,len));
            MessageBox.Show(jsonObject.ToString());
            if (jsonObject.ContainsKey("status"))
            {
                return true;
            }
            return false;

        }
        private void GoToLoginButton_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}
