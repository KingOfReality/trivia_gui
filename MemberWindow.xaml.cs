using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
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
using System.Windows.Shapes;

namespace Gui_client
{
    /// <summary>
    /// Interaction logic for MemberWindow.xaml
    /// </summary>
    public partial class MemberWindow : Window
    {
        private NetworkStream clientStream;
        private CancellationTokenSource cancellationTokenSource;
        private Mutex mutex;
        private bool gameBegun;

        public MemberWindow(string buttonText)
        {
            InitializeComponent();
            clientStream = GolbalClient.ClientStream;
            cancellationTokenSource = new CancellationTokenSource();
            mutex = new Mutex();
            gameBegun = false;

            Task.Run(UpdatePlayerListAsync);
        }

        private async Task UpdatePlayerListAsync()
        {
            while (true)
            {
                JObject data = new JObject();
                string json = data.ToString();

                byte code = 30;
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
                bool check = jsonObject["gameBegun"].ToObject<bool>();
                if (check)
                {
                    cancellationTokenSource.Cancel();
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        TriviaWindow triviaWindow = new TriviaWindow(jsonObject["timeOut"].ToObject<int>());
                        triviaWindow.Show();
                        this.Close();
                    });
                    mutex.ReleaseMutex(); // Move this line outside the Dispatcher.Invoke block
                }


                return playersList;
            }

            if (jsonObject.ContainsKey("message"))
            {
                cancellationTokenSource.Cancel();
                MessageBox.Show("room closed!");
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MenuWindow menuWindow = new MenuWindow();
                    menuWindow.Show();
                    this.Close();
                });
            }

            return null;
        }

        private void Leave_Room(object sender, RoutedEventArgs e)
        {
            cancellationTokenSource.Cancel();
            JObject data = new JObject();
            string json = data.ToString();

            byte code = 31;
            int sizeOfJson = json.Length;
            byte[] size = BitConverter.GetBytes(sizeOfJson);
            Array.Reverse(size);
            byte[] dataBytes = Encoding.ASCII.GetBytes(json);
            byte[] message = new byte[1 + size.Length + dataBytes.Length];
            message[0] = code;
            Buffer.BlockCopy(size, 0, message, 1, size.Length);
            Buffer.BlockCopy(dataBytes, 0, message, 1 + size.Length, dataBytes.Length);
            clientStream.Write(message, 0, message.Length);
            clientStream.Flush();

            byte[] responseBuffer = new byte[4096];
            int bytesRead = clientStream.Read(responseBuffer, 0, responseBuffer.Length);
            string response = Encoding.UTF8.GetString(responseBuffer, 0, bytesRead);

            int len = Convert.ToInt32(response.Substring(8, 32), 2);
            JObject jsonObject = JObject.Parse(response.Substring(40, len));
            if (jsonObject.ContainsKey("status"))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MenuWindow menuWindow = new MenuWindow();
                    menuWindow.Show();
                    this.Close();
                });
            }
        }
    }
}
