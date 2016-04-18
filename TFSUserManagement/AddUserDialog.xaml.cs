using Microsoft.VisualStudio.PlatformUI;
using System;
using TFSUserManagement.ViewModel;

namespace TFSUserManagement
{
    /// <summary>
    /// Interaction logic for AddUserDialog.xaml
    /// </summary>
    public partial class AddUserDialog : DialogWindow
    {
        public AddUserDialog(TfsUtilityViewModel model, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            var viewModel = new AddUserViewModel(model, serviceProvider);
            this.DataContext = viewModel;
            viewModel.CloseAction = viewModel.CloseAction ?? new System.Action(() => this.Close());
        }
    }
}
