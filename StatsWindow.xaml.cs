using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Interaction logic for StatsWindow.xaml
    /// </summary>
    public partial class StatsWindow : Window
    {
        public StatsWindow(int correctCount, int wrongCount, double averageTime, string username)
        {
            InitializeComponent();

            correctLabel.Content = $"Correct: {correctCount}";
            wrongLabel.Content = $"Wrong: {wrongCount}";
            avgTimeLabel.Content = $"Average Time: {averageTime:F2}";
            usernameLabel.Content = $"Username: {username}";
        }
    }
}
