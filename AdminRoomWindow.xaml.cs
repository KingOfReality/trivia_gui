using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Gui_client
{
    public partial class AdminRoomWindow : Window
    {
        private NetworkStream clientStream;
        private CancellationTokenSource cancellationTokenSource;
        private TimeSpan t;
        private int temp;
        public AdminRoomWindow(string nameRoom, bool isAdmin)
        {
            InitializeComponent();
            clientStream = GolbalClient.ClientStream;
            cancellationTokenSource = new CancellationTokenSource();
            Task.Run(UpdatePlayersList, cancellationTokenSource.Token);

        }

        private async Task UpdatePlayersList()
        {
            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                JObject data = new JObject();
                string json = data.ToString();

                byte code = 21;
                int sizeOfJson = json.Length;
                byte[] size = BitConverter.GetBytes(sizeOfJson);
                Array.Reverse(size);
                byte[] dataBytes = Encoding.ASCII.GetBytes(json);
                byte[] message = new byte[1 + size.Length + dataBytes.Length];
                message[0] = code;
                Buffer.BlockCopy(size, 0, message, 1, size.Length);
                Buffer.BlockCopy(dataBytes, 0, message, 1 + size.Length, dataBytes.Length);
                List<string> lst = SendAndReceiveData(message);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    PlayersListView.ItemsSource = lst;
                });
                await Task.Delay(3000);
            }
        }

        private List<string> SendAndReceiveData(byte[] message)
        {
            clientStream.Write(message, 0, message.Length);
            clientStream.Flush();

            byte[] responseBuffer = new byte[4096];
            int bytesRead = clientStream.Read(responseBuffer, 0, responseBuffer.Length);
            string response = Encoding.UTF8.GetString(responseBuffer, 0, bytesRead);

            int len = Convert.ToInt32(response.Substring(8, 32), 2);
            JObject jsonObject = JObject.Parse(response.Substring(40, len));

            if (jsonObject.ContainsKey("players"))
            {
                JToken playersToken = jsonObject.GetValue("players");
                JArray playersArray = JArray.FromObject(playersToken);
                List<string> playersList = playersArray.ToObject<List<string>>();
                temp = jsonObject["timeOut"].ToObject<int>();

                return playersList;
            }
            return null;
        }
        private void Start_Game(object sender, RoutedEventArgs e)
        {
            cancellationTokenSource.Cancel();
            TriviaWindow t ;

            JObject data = new JObject();
            string json = data.ToString();

            byte code = 22;
            int sizeOfJson = json.Length;
            byte[] size = BitConverter.GetBytes(sizeOfJson);
            Array.Reverse(size);
            byte[] dataBytes = Encoding.ASCII.GetBytes(json);
            byte[] message = new byte[1 + size.Length + dataBytes.Length];
            message[0] = code;
            Buffer.BlockCopy(size, 0, message, 1, size.Length);
            Buffer.BlockCopy(dataBytes, 0, message, 1 + size.Length, dataBytes.Length);
            if (DataStart(message))
            {
                t = new TriviaWindow(temp);
                t.Show();

                this.Close();
            }

        }

        private bool DataStart(byte[] message)
        {
            clientStream.Write(message, 0, message.Length);
            clientStream.Flush();

            byte[] responseBuffer = new byte[4096];
            int bytesRead = clientStream.Read(responseBuffer, 0, responseBuffer.Length);
            string response = Encoding.UTF8.GetString(responseBuffer, 0, bytesRead);

            int len = Convert.ToInt32(response.Substring(8, 32), 2);
            JObject jsonObject = JObject.Parse(response.Substring(40, len));
            if (jsonObject.ContainsKey("status"))
            {
                return true;
            }
            return false;
        }

        private void delete_Click(object sender, RoutedEventArgs e)
        {
            cancellationTokenSource.Cancel(); 
            JObject data = new JObject();
            string json = data.ToString();
            MenuWindow menuWindow;
            byte code = 20;
            int sizeOfJson = json.Length;
            byte[] size = BitConverter.GetBytes(sizeOfJson);
            Array.Reverse(size);
            byte[] dataBytes = Encoding.ASCII.GetBytes(json);
            byte[] message = new byte[1 + size.Length + dataBytes.Length];
            message[0] = code;
            Buffer.BlockCopy(size, 0, message, 1, size.Length);
            Buffer.BlockCopy(dataBytes, 0, message, 1 + size.Length, dataBytes.Length);

            if (Datadelete(dataBytes))
            {
                 menuWindow = new MenuWindow();
                menuWindow.Show();
                this.Close();

            }
        }
        private bool Datadelete(byte[] message)
        {
            clientStream.Write(message, 0, message.Length);
            clientStream.Flush();
            
            byte[] responseBuffer = new byte[4096];
            int bytesRead = clientStream.Read(responseBuffer, 0, responseBuffer.Length);
            string response = Encoding.UTF8.GetString(responseBuffer, 0, bytesRead);

            int len = Convert.ToInt32(response.Substring(8, 32), 2);
            JObject jsonObject = JObject.Parse(response.Substring(40, len));

            if (jsonObject.ContainsKey("status"))
            {
                return true;
            }
            return false;
        }
    }
}
