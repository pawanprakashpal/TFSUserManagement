using Microsoft.VisualStudio.PlatformUI;
using System;
using TFSUserManagement.ViewModel;

namespace TFSUserManagement
{
    /// <summary>
    /// Interaction logic for ServerDialog.xaml
    /// </summary>
    public partial class ServerDialog : DialogWindow
    {
        public ServerDialog(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            var viewModel = new TFSServerViewModel(serviceProvider);
            this.DataContext = viewModel;
            viewModel.CloseAction = viewModel.CloseAction ?? new Action(() => this.Close());
            CenterWindowOnScreen();
        }

        private void CenterWindowOnScreen()
        {
            double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
            double windowWidth = this.Width;
            double windowHeight = this.Height;
            this.Left = (screenWidth / 2) - (windowWidth / 2);
            this.Top = (screenHeight / 3) - (windowHeight / 3);
        }
    }
}
