using Microsoft.TeamFoundation.Client;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using TFSUserManagement.Common;
using TFSUserManagement.Entities;
using TFSUserManagement.TFSData;

namespace TFSUserManagement.ViewModel
{
    public class TfsUtilityViewModel : Base.ViewModelBase
    {
        #region private variables

        private bool _isBusy;
        private bool _isMemberVisible;
        private bool _isAddEnabled;
        private string _busyMessage;
        private IServiceProvider _iServiceProvider;
        private TFSGroup _previousState;
        private TFSGroup _seletedItem;
        private ObservableCollection<TFSGroup> _tfsGroups;
        private ObservableCollection<GroupMembers> _members;

        #endregion

        #region Commands
        public RelayCommand<object> FavoriteCommand { get; private set; }
        public RelayCommand<object> OpenAddUserWindowCommand { get; private set; }
        public RelayCommand<object> RemoveUserCommand { get; private set; }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public TfsUtilityViewModel()
        {            
            FavoriteCommand = new RelayCommand<object>(this.Favorite);
            RemoveUserCommand = new RelayCommand<object>(this.RemoveUser);
            OpenAddUserWindowCommand = new RelayCommand<object>(this.OpenDialog);
            this.LoadGroups();
        }

        /// <summary>
        /// If TFS group has members then True else False
        /// </summary>
        public bool IsMemberVisible
        {
            get { return _isMemberVisible; }
            set
            {
                _isMemberVisible = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Message to show during Async processing
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
        /// True till Async processing is not completes
        /// </summary>
        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
                OnPropertyChanged("IsBusy");
                if (SelectedItem == null && PreviousState != null && !_isBusy)
                {
                    SelectedItem = PreviousState;
                    ShowGroupMembers(SelectedItem);
                }
            }
        }

        /// <summary>
        /// Flag to Enable/Disable the Add user button 
        /// </summary>
        public bool IsAddEnabled
        {
            get { return _isAddEnabled; }
            set
            {
                _isAddEnabled = value;
                OnPropertyChanged();
            }
        }

        public IServiceProvider ServiceProvider
        {
            get
            {
                return _iServiceProvider;
            }
            set
            {
                _iServiceProvider = value;
            }
        }

        /// <summary>
        /// Selected Group of Grid
        /// </summary>
        public TFSGroup SelectedItem
        {
            get { return _seletedItem; }
            set
            {
                _seletedItem = value;
                OnPropertyChanged();
                IsAddEnabled = (_seletedItem != null);
                ShowGroupMembers(_seletedItem);
            }
        }

        /// <summary>
        /// Selected Group of Grid
        /// </summary>
        public TFSGroup PreviousState
        {
            get { return _previousState; }
            set
            {
                _previousState = value;
            }
        }

        /// <summary>
        /// TFS Groups
        /// </summary>
        public ObservableCollection<TFSGroup> TfsGroup
        {
            get { return _tfsGroups; }
            set
            {
                _tfsGroups = value;
                OnPropertyChanged("TfsGroup");
            }
        }

        /// <summary>
        /// TFS Group Members
        /// </summary>
        public ObservableCollection<GroupMembers> Members
        {
            get { return _members; }
            set
            {
                _members = value;
                OnPropertyChanged("Members");
            }
        }

        /// <summary>
        /// To fetch data from TFS Groups with the Members 
        /// </summary>
        public async void LoadGroups()
        {
            IsBusy = true;
            BusyMessage = "Please wait while groups are loaded..";
            TfsGroup = new ObservableCollection<TFSGroup>();
            var groupList = await TfsCollection.Instance.GetTFSGroups();
            groupList.OrderBy(a => a.GroupName).ToList()
                .ForEach(grp =>
                {
                    System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(() =>
                    {
                        TfsGroup.Add(grp);
                    });
                });
            IsBusy = false;
        }

        /// <summary>
        /// Function to Open Dialog to add users to the selected Group
        /// </summary>
        /// <param name="param"></param>
        private void OpenDialog(object param)
        {
            AddUserDialog xamlDialog = new AddUserDialog(this)
            {
                Title = "Manage TFS Groups",
                HasMinimizeButton = false,
                HasMaximizeButton = false
            };
            xamlDialog.ShowModal();
        }

        /// <summary>
        /// This function gets called by Relay Command to Remove user(s)
        /// </summary>
        /// <param name="param"></param>
        private void RemoveUser(object param)
        {
            var result = this.Confirm(param != null ? 1 : this.Members.Count);
            if (result == 6)
            {
                if (param != null)
                {
                    var groupMember = ((FrameworkElement)param).DataContext as GroupMembers;
                    this.Remove(groupMember.Name);
                }
                else
                {
                    this.Members.ToList().ForEach(user =>
                    {
                        this.Remove(user.Name);
                    });
                }
                PreviousState = this.SelectedItem;
                this.Refresh();
            }
        }

        /// <summary>
        /// Function called by Favorite Command to favorite/Unfavorite the selected group on click of toggle button from UI
        /// </summary>
        /// <param name="param"></param>
        private void Favorite(object param)
        {
            var tfsGroup = (TFSGroup)((FrameworkElement)((System.Windows.Controls.Primitives.ButtonBase)param).CommandParameter).DataContext;
            TfsGroup.Remove(tfsGroup);
            TfsGroup.Insert(TfsGroup.Count, tfsGroup);
        }

        /// <summary>
        /// Function to List group members
        /// </summary>
        /// <param name="viewmodel"></param>
        private void ShowGroupMembers(TFSGroup selectedGroup)
        {
            if (selectedGroup != null)
            {
                IsMemberVisible = selectedGroup.ListMembers.Count > 0;
                Members = new ObservableCollection<GroupMembers>();
                selectedGroup.ListMembers.ForEach(member =>
                {
                    System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(() =>
                    {
                        Members.Add(member);
                    });
                });
            }
        }

        /// <summary>
        ///  To Remove user(s) from the selected group
        /// </summary>
        /// <param name="user"></param>
        private void Remove(string user)
        {
            TfsCollection.Instance.RemoveUsers(user, this.SelectedItem.GroupName);
        }

        /// <summary>
        /// To reload the refesh data from TFS
        /// </summary>
        public void Refresh()
        {
            this.LoadGroups();
        }

        /// <summary>
        /// To confirm before removing user from TFS Group
        /// </summary>
        /// <param name="numberOfUsers"></param>
        /// <returns></returns>
        private int Confirm(int numberOfUsers)
        {
            string message = $"Are you sure you want to remove {numberOfUsers} user(s).";
            string title = "Remove User";
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

