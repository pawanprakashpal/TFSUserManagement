using Microsoft.VisualStudio.PlatformUI;
using TFSUserManagement.ViewModel;

namespace TFSUserManagement
{
    /// <summary>
    /// Interaction logic for AddUserDialog.xaml
    /// </summary>
    public partial class AddUserDialog : DialogWindow
    {
        public AddUserDialog(TfsUtilityViewModel model)
        {
            InitializeComponent();
            var viewModel = new AddUserViewModel(model);
            this.DataContext = viewModel;
            viewModel.CloseAction = viewModel.CloseAction ?? new System.Action(() => this.Close());            
        }
    }
}
