using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Xml.Linq;

namespace Gui_client
{
    /// <summary>
    /// Interaction logic for ButtonListWindow.xaml
    /// </summary>
    public partial class ButtonListWindow : Window
    {
        private NetworkStream clientStream;
        private CancellationTokenSource cancellationTokenSource;
        JObject response;
        public ButtonListWindow()
        {
            InitializeComponent();
            clientStream = GolbalClient.ClientStream;
            cancellationTokenSource = new CancellationTokenSource();

            // Start the background thread
            Task.Run(UpdateButtonListAsync, cancellationTokenSource.Token);
        }

        private async Task UpdateButtonListAsync()
        {
            // Initial delay to allow UI initialization
            await Task.Delay(500);

            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                JObject data = new JObject();
                string json = data.ToString();

                byte code = 11;
                int sizeOfJson = json.Length;
                byte[] size = BitConverter.GetBytes(sizeOfJson);
                Array.Reverse(size);
                byte[] dataBytes = Encoding.ASCII.GetBytes(json);
                byte[] message = new byte[1 + size.Length + dataBytes.Length];
                message[0] = code;
                Buffer.BlockCopy(size, 0, message, 1, size.Length);
                Buffer.BlockCopy(dataBytes, 0, message, 1 + size.Length, dataBytes.Length);

                response = await GetRoomsAsync(message);
                MessageBox.Show(response.ToString());
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (response != null && response.HasValues)
                    {
                        List<string> keysList = response.Properties().Select(p => p.Name).ToList();
                        ButtonListView.ItemsSource = keysList;
                    }
                    else
                    {
                        // Handle empty response here, such as displaying a message or clearing the list
                        ButtonListView.ItemsSource = null;
                        // Alternatively, you can display a message box
                        MessageBox.Show("No data available.");
                        MenuWindow menuWindow = new MenuWindow();
                        menuWindow.Show();
                        this.Close();
                    }
                });


                // Check if the cancellation is requested and the list is empty
                if (cancellationTokenSource.Token.IsCancellationRequested && response.Count == 0)
                {
                    // Delay the loop without updating the UI
                    await Task.Delay(3000);
                    continue; // Continue the loop without canceling the thread
                }

                // Wait for half a second before sending the next request
                await Task.Delay(3000);
            }
        }



        private async Task<JObject> GetRoomsAsync(byte[] message)
        {
            clientStream.Write(message, 0, message.Length);
            clientStream.Flush();

            byte[] responseBuffer = new byte[4096];
            int bytesRead = await clientStream.ReadAsync(responseBuffer, 0, responseBuffer.Length);
            string response = Encoding.UTF8.GetString(responseBuffer, 0, bytesRead);
            int len = Convert.ToInt32(response.Substring(8, 32), 2);
            JObject jsonObject = JObject.Parse(response.Substring(40, len));
            return jsonObject;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Handle button click event here
            cancellationTokenSource.Cancel();   
            Button clickedButton = (Button)sender;
            string buttonText = clickedButton.Content.ToString();
            JObject data = new JObject();
            bool flag = true;
            MessageBox.Show(response.ToString());
            data["roomId"] = response[buttonText];
            string json = data.ToString();
            MessageBox.Show(json);
            byte code = 13;
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
                MemberWindow ad = new MemberWindow(buttonText);

                ad.Show();
                this.Close();
            }


            this.Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // Cancel the background thread when the window is closing
            cancellationTokenSource.Cancel();
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
