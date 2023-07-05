using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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
using System.Windows.Threading;

namespace Gui_client
{
    /// <summary>
    /// Interaction logic for TriviaWindow.xaml
    /// </summary>
    /// 
    public partial class TriviaWindow : Window
    {
        private NetworkStream clientStream;
        private Question currentQuestion;
        private DispatcherTimer questionTimer;
        private int timeRemaining;
        private int seconds;
        public TriviaWindow(int seconds)
        {
            InitializeComponent();
            clientStream = GolbalClient.ClientStream;
            questionTimer = new DispatcherTimer();
            questionTimer.Interval = TimeSpan.FromSeconds(1);
            questionTimer.Tick += QuestionTimer_Tick;
            this.seconds = seconds;
            getQuestion();
            questionLabel.Text = currentQuestion.question;
            option1Button.Content = currentQuestion.answers[0];
            option2Button.Content = currentQuestion.answers[1];
            option3Button.Content = currentQuestion.answers[2];
            option4Button.Content = currentQuestion.answers[3];

            // Start the timer
            StartTimer();
        }

        private void QuestionTimer_Tick(object sender, EventArgs e)
        {
            if (timeRemaining > 0)
            {
                timeRemaining--;
                timerTextBlock.Text = timeRemaining.ToString();
            }
            else
            {
                // Timer expired, show a message or take appropriate action
                TimeOutRequest();
                // Proceed to the next question or end the game
                if (getQuestion())
                {
                    questionLabel.Text = currentQuestion.question;
                    option1Button.Content = currentQuestion.answers[0];
                    option2Button.Content = currentQuestion.answers[1];
                    option3Button.Content = currentQuestion.answers[2];
                    option4Button.Content = currentQuestion.answers[3];

                    // Restart the timer
                    StartTimer();
                }
                else
                {
                    end_game();
                }
            }
        }
        private bool TimeOutRequest()
        {
            JObject data = new JObject();
            bool flag = true;
            data["answer"] = -1;

            string json = data.ToString();
            byte code = 41;
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
            

            return true;
        }
        private void StartTimer()
        {
            timeRemaining = seconds; // Set the initial time remaining
            timerTextBlock.Text = timeRemaining.ToString();

            // Start the timer
            questionTimer.Start();
        }

        private bool getQuestion()
        {
            // Stop the timer before getting the next question
            questionTimer.Stop();

            JObject data = new JObject();
            bool flag = true;
            string json = data.ToString();

            byte code = 40;
            int sizeOfJson = json.Length;
            byte[] size = BitConverter.GetBytes(sizeOfJson);
            Array.Reverse(size);
            byte[] dataBytes = Encoding.ASCII.GetBytes(json);
            byte[] message = new byte[1 + size.Length + dataBytes.Length];
            message[0] = code;
            Buffer.BlockCopy(size, 0, message, 1, size.Length);
            Buffer.BlockCopy(dataBytes, 0, message, 1 + size.Length, dataBytes.Length);
            return SendAndReceiveData(message);
        }
        private bool SendAndReceiveData(byte[] message)
        {
            clientStream.Write(message, 0, message.Length);
            clientStream.Flush();

            byte[] responseBuffer = new byte[4096];
            int bytesRead = clientStream.Read(responseBuffer, 0, responseBuffer.Length);
            string response = Encoding.UTF8.GetString(responseBuffer, 0, bytesRead);
            int len = Convert.ToInt32(response.Substring(8, 32), 2);
            JObject jsonObject = JObject.Parse(response.Substring(40, len));
            if(jsonObject.ContainsKey("message"))
            {
                return false;
            }
            string[] answers = jsonObject["answers"].ToObject<string[]>();
            int id = jsonObject["correct"].ToObject<int>();
            string question = jsonObject["question"].ToObject<string>();
            currentQuestion = new Question(question, answers, id);

            return true;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = (Button)sender;
            int buttonIndex =  int.Parse( clickedButton.Tag.ToString()); // Assuming the button's Tag property is set to the index

            JObject data = new JObject();
            bool flag = true;
            data["answer"] = buttonIndex;
            string json = data.ToString();

            byte code = 41;
            int sizeOfJson = json.Length;
            byte[] size = BitConverter.GetBytes(sizeOfJson);
            Array.Reverse(size);
            byte[] dataBytes = Encoding.ASCII.GetBytes(json);
            byte[] message = new byte[1 + size.Length + dataBytes.Length];
            message[0] = code;
            Buffer.BlockCopy(size, 0, message, 1, size.Length);
            Buffer.BlockCopy(dataBytes, 0, message, 1 + size.Length, dataBytes.Length);
            SubmitAnswer(message,sender);
        }
        private async void SubmitAnswer(byte[] message, object sender)
        {
            Button clickedButton = (Button)sender;
            Brush originalBorderBrush = clickedButton.BorderBrush;

            clientStream.Write(message, 0, message.Length);
            clientStream.Flush();

            byte[] responseBuffer = new byte[4096];
            int bytesRead = clientStream.Read(responseBuffer, 0, responseBuffer.Length);
            string response = Encoding.UTF8.GetString(responseBuffer, 0, bytesRead);
            int len = Convert.ToInt32(response.Substring(8, 32), 2);
            JObject jsonObject = JObject.Parse(response.Substring(40, len));
            int isCorrect = jsonObject["status"].ToObject<int>();

            if (isCorrect == 1)
            {
                // Correct answer
                clickedButton.BorderBrush = Brushes.Green;
            }
            else
            {
                // Incorrect answer
                clickedButton.BorderBrush = Brushes.Red;
            }

            await Task.Delay(1000); // Wait for 1 second

            clickedButton.BorderBrush = originalBorderBrush;
            if (getQuestion())
            {
                questionLabel.Text = currentQuestion.question;

                option1Button.Content = currentQuestion.answers[0];
                option2Button.Content = currentQuestion.answers[1];
                option3Button.Content = currentQuestion.answers[2];
                option4Button.Content = currentQuestion.answers[3];
            }
            else
            {
                end_game();
            }

            // Start the timer
            StartTimer();
        }


        private void end_game()
        {
           
            JObject data = new JObject();
            bool flag = true;
            string json = data.ToString();

            byte code = 42;
            int sizeOfJson = json.Length;
            byte[] size = BitConverter.GetBytes(sizeOfJson);
            Array.Reverse(size);
            byte[] dataBytes = Encoding.ASCII.GetBytes(json);
            byte[] message = new byte[1 + size.Length + dataBytes.Length];
            message[0] = code;
            Buffer.BlockCopy(size, 0, message, 1, size.Length);
            Buffer.BlockCopy(dataBytes, 0, message, 1 + size.Length, dataBytes.Length);
            get_end(message);
        }
        private void get_end(byte[] message)
        {

            clientStream.Write(message, 0, message.Length);
            clientStream.Flush();

            byte[] responseBuffer = new byte[4096];
            int bytesRead = clientStream.Read(responseBuffer, 0, responseBuffer.Length);
            string response = Encoding.UTF8.GetString(responseBuffer, 0, bytesRead);
            int len = Convert.ToInt32(response.Substring(8, 32), 2);
            JObject jsonObject = JObject.Parse(response.Substring(40, len));
           
            int correctCount = jsonObject["correctCount"].ToObject<int>();
            int wrongCount = jsonObject["wrongCount"].ToObject<int>();
            int avg = jsonObject["avg"].ToObject<int>();
            string username = jsonObject["username"].ToObject<string>();
            StatsWindow s = new StatsWindow(correctCount, wrongCount, avg, username);
            s.Show();
            this.Close();
        }
        private void LeaveGameButton_Click(object sender, RoutedEventArgs e)
        {
            JObject data = new JObject();
            bool flag = true;
            string json = data.ToString();

            byte code = 43;
            int sizeOfJson = json.Length;
            byte[] size = BitConverter.GetBytes(sizeOfJson);
            Array.Reverse(size);
            byte[] dataBytes = Encoding.ASCII.GetBytes(json);
            byte[] message = new byte[1 + size.Length + dataBytes.Length];
            message[0] = code;
            Buffer.BlockCopy(size, 0, message, 1, size.Length);
            Buffer.BlockCopy(dataBytes, 0, message, 1 + size.Length, dataBytes.Length);
            leave_data(message);
        }
        private void leave_data(byte[] message)
        {

            clientStream.Write(message, 0, message.Length);
            clientStream.Flush();

            byte[] responseBuffer = new byte[4096];
            int bytesRead = clientStream.Read(responseBuffer, 0, responseBuffer.Length);
            string response = Encoding.UTF8.GetString(responseBuffer, 0, bytesRead);
            int len = Convert.ToInt32(response.Substring(8, 32), 2);
            JObject jsonObject = JObject.Parse(response.Substring(40, len));
            int isCorrect = jsonObject["status"].ToObject<int>();
            
            MenuWindow menuWindow = new MenuWindow();
            menuWindow.Show();
            this.Close();
        }
    }
    public class Question
    {
        public string question { get; }
        public string[] answers { get; }
        public int correct { get; }

        public Question(string text, string[] options, int correctOptionIndex)
        {
            question = text;
            answers = options;
            correct = correctOptionIndex;
        }
    }
}
