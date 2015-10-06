using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Mastery.Views
{
    /// <summary>
    /// Interaction logic for Popup.xaml
    /// </summary>
    public partial class SystemTrayPopup : UserControl
    {
        public string PopupText { get; set; }

        public SystemTrayPopup()
        {
            InitializeComponent();
            DataContext = this;
            TaskbarIcon.AddBalloonClosingHandler(this, OnBalloonClosing);
        }

        /// <summary>
        /// By subscribing to the <see cref="TaskbarIcon.BalloonClosingEvent"/>
        /// and setting the "Handled" property to true, we suppress the popup
        /// from being closed in order to display the custom fade-out animation.
        /// </summary>
        private void OnBalloonClosing(object sender, RoutedEventArgs e)
        {
            e.Handled = true; //suppresses the popup from being closed immediately
        }

        /// <summary>
        /// Closes the popup once the fade-out animation completed.
        /// The animation was triggered in XAML through the attached
        /// BalloonClosing event.
        /// </summary>
        private void OnFadeOutCompleted(object sender, EventArgs e)
        {
            Popup pp = (Popup)Parent;
            pp.IsOpen = false;
        }
    }
}
