using Mastery.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace Mastery.Views
{
    /// <summary>
    /// Interaction logic for DetailsPanel.xaml
    /// </summary>
    public partial class DetailsPanel : Window
    {
        public DetailsPanel(ProjectModel project)
        {
            InitializeComponent();
            ChallengeText.Content = "My " + project.TargetHours.ToString("G") + " hour mastery of " + project.Task;
            Started.Content = "Officially started on " + project.StartDate;
            Hours.Content = project.CurrentHour.ToString();
            Statement.Content = "Hours Spent " + project.Task;

            double total = project.TargetHours * 3600000.0;
            ProgressBar.Value = (project.ElapsedTime / total) * 100;
            ProgressPercentage.Text = ((ProgressBar.Value >= 100) ? 100 : ProgressBar.Value).ToString("F4") + "%";

            Console.WriteLine(ChallengeText);
        }
    }
}
