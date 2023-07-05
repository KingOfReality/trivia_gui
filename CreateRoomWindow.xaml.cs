using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
    /// <summary>
    /// Interaction logic for CreateRoomWindow.xaml
    /// </summary>
    public partial class CreateRoomWindow : Window
    {
        private NetworkStream clientStream;
        string name;
        public CreateRoomWindow()
        {
            InitializeComponent();
            clientStream = GolbalClient.ClientStream;

        }
        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            string timeOut = TimeOutTextBox.Text;
            string maxUser = MaxUsersTextBox.Text;
            string count = QuestionCountTextBox.Text;
             name = NameTextBox.Text;
            if (string.IsNullOrEmpty(timeOut) || string.IsNullOrEmpty(maxUser) || string.IsNullOrEmpty(count) || string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Please fill in all the fields.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            int timeOutValue, maxUserValue, countValue;

            // Validate if the timeout is a valid integer
            if (!int.TryParse(timeOut, out timeOutValue))
            {
                MessageBox.Show("Timeout must be a valid integer.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Validate if the maxUser is a valid integer
            if (!int.TryParse(maxUser, out maxUserValue))
            {
                MessageBox.Show("Max Users must be a valid integer.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Validate if the count is a valid integer
            if (!int.TryParse(count, out countValue))
            {
                MessageBox.Show("Question Count must be a valid integer.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if(int.Parse(count) > 25)
            {
                // Display a confirmation dialog
                MessageBox.Show("Max count is 25!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                
                    return;
                
            }
            if (int.Parse(count) < 3)
            {
                // Display a confirmation dialog
                MessageBoxResult result = MessageBox.Show("Are you sure you want to start the game with less than 3 questions?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.No)
                {
                    // User clicked "No", return without starting the game
                    return;
                }
            }
            if(int.Parse(timeOut) < 3 || int.Parse(timeOut) > 100)
            {
                // Display a confirmation dialog
               MessageBox.Show("Min time is 3 seconds and the max time is 100 seconds!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                return;
            }
            if (int.Parse(timeOut) > 30)
            {
                // Display a confirmation dialog
                MessageBoxResult result = MessageBox.Show("Are you sure you want to start the game with more than 30 seconds for every question?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.No)
                {
                    // User clicked "No", return without starting the game
                    return;
                }
            }
            JObject data = new JObject();
            bool flag = true;
            data["timeOut"] = int.Parse(timeOut);
            data["maxUsers"] = int.Parse(maxUser);
            data["questionCount"] = int.Parse(count);
            data["name"] = name;
            string json = data.ToString();
            // Example: Display the data in a message box
            MessageBox.Show(json);
            byte code = 14;
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
                AdminRoomWindow ad = new AdminRoomWindow(name,true);
                
                ad.Show();
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
            if (jsonObject.ContainsKey("status") && jsonObject.ContainsKey("id"))
            {
               
                return true;
            }
            return false;

        }
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            MenuWindow menuWindow = new MenuWindow();
            menuWindow.Show()
;
            this.Close();
        }

    }
}
