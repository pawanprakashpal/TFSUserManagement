using Microsoft.TeamFoundation.Client;
using System;

namespace TFSUserManagement
{
    /// <summary>
    /// Interaction logic for GroupDialog.xaml
    /// </summary>
    public partial class GroupDialog : Microsoft.VisualStudio.PlatformUI.DialogWindow
    {
        public GroupDialog(string helpTopic)
            : base(helpTopic)
        {
            InitializeComponent();
        }

        public GroupDialog()
        {
            InitializeComponent();
            var model = new ViewModel.TfsUtilityViewModel();
            this.DataContext = model;
        }

        private void Notify()
        {
            var userCount = 30;//this.AddUserCollection.Where(a => a.IsSelected).Count();
            var nIcon = new System.Windows.Forms.NotifyIcon();
            nIcon.ShowBalloonTip(5000, "User(s) Added", $"{userCount} user(s) added to {123}", System.Windows.Forms.ToolTipIcon.Info);
        }
    }
}
