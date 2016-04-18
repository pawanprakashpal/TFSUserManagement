using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TFSUserManagement.Common;
using TFSUserManagement.Common.Extensions;
using TFSUserManagement.Entities;
using TFSUserManagement.TFSData;

namespace TFSUserManagement.ViewModel
{
    public class AddUserViewModel : Base.ViewModelBase
    {
        #region Private Variables

        private bool _isBusy;
        private string _busyMessage;
        private string _userInput;
        private string _criteria;
        private IServiceProvider _iServiceProvider;
        private ObservableCollection<TFSUser> _addUserCollection;

        #endregion

        #region Public Variables
        public string Criteria
        {
            get { return _criteria; }
            set
            {
                _criteria = value;
                OnPropertyChanged();
                AddUserCollection = string.IsNullOrEmpty(Criteria)
                    ? CachedUserCollection
                    : AddUserCollection
                    .Where(a => a.FullName.ToUpper().Contains(Criteria.ToUpper()))
                    .OrderBy(a => a.FullName)
                    .ToList().ToObservableCollection();
            }
        }
        public string UserInput
        {
            get { return _userInput; }
            set { _userInput = value; OnPropertyChanged(); }
        }
        public ObservableCollection<TFSUser> CachedUserCollection { get; set; }
        public ObservableCollection<TFSUser> AddUserCollection
        {
            get { return _addUserCollection; }
            set
            {
                _addUserCollection = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Busy Message
        /// </summary>
        public string BusyMessage
        {
            get { return _busyMessage; }
            set
            {
                _busyMessage = value;
                OnPropertyChanged("BusyMessage");
            }
        }

        /// <summary>
        /// It will be true until the async function returns the data
        /// </summary>
        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
                OnPropertyChanged("IsBusy");
            }
        }
        #endregion

        /// <summary>
        /// To Close the Add User Dialog
        /// </summary>
        public Action CloseAction { get; set; }

        /// <summary>
        /// Command to add user(s) to TFS Group
        /// </summary>
        public RelayCommand<object> AddUserCommand { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="model"></param>
        public AddUserViewModel(TfsUtilityViewModel model, IServiceProvider serviceProvider)
        {
            this._iServiceProvider = serviceProvider;
            AddUserCommand = new RelayCommand<object>(param => this.AddUser(param, model));
            this.FetchUser();
        }

        /// <summary>
        /// To get users names from TFS
        /// </summary>
        private async void FetchUser()
        {
            IsBusy = true;
            BusyMessage = "Please wait..";
            AddUserCollection = new ObservableCollection<TFSUser>();
            var users = await TfsCollection.Instance.FetchUsers();
            users.ForEach(user =>
            {
                System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(() =>
                {
                    AddUserCollection.Add(user);
                });
            });
            IsBusy = false;
            CachedUserCollection = AddUserCollection;
        }

        /// <summary>
        /// To add user(s) to a TFS group
        /// </summary>
        /// <param name="param"></param>
        private void AddUser(object param, TfsUtilityViewModel model)
        {
            string[] stringSeparators = new string[] { ",", ";", Environment.NewLine };
            var usersToAdd = new List<string>();

            AddUserCollection.Where(a => a.IsSelected)
                .ToList()
                .ForEach(user => usersToAdd.Add(user.UserName));

            if (!string.IsNullOrEmpty(UserInput))
            {
                UserInput.Split(stringSeparators, StringSplitOptions.None)
                    .ToList()
                    .ForEach(user =>
                    {
                        if (!usersToAdd.Contains(user))
                            usersToAdd.Add(user);
                    });
            }
            if (this.Confirm(usersToAdd.Count) == 6)
            {
                usersToAdd.ForEach(user => TfsCollection.Instance.AddUsers(user, model.SelectedItem.GroupName));
                model.PreviousState = model.SelectedItem;
                //Reload the Group
                model.Refresh();
                //Close the Add user dialog
                CloseAction();
            }
        }

        /// <summary>
        /// To confirm before adding user from TFS Group
        /// </summary>
        /// <param name="numberOfUsers"></param>
        /// <returns></returns>
        private int Confirm(int numberOfUsers)
        {
            string message = $"Are you sure you want to add {numberOfUsers} user(s).";
            string title = "Add User";
            return VsShellUtilities.ShowMessageBox(
                   this._iServiceProvider,
                   message,
                   title,
                   OLEMSGICON.OLEMSGICON_WARNING,
                   OLEMSGBUTTON.OLEMSGBUTTON_YESNO,
                   OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_SECOND);
        }
    }
}
