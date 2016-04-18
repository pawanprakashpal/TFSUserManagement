namespace TFSUserManagement.Entities
{
    public class TFSServer : ViewModel.Base.ViewModelBase
    {
        private string _tfsUrl;
        private bool _isDefault;
        private bool _isEnabled;

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                OnPropertyChanged();
            }
        }
        public bool IsDefault
        {
            get { return _isDefault; }
            set
            {
                _isDefault = value;
                OnPropertyChanged();
            }
        }
        public string TFSUrl
        {
            get { return _tfsUrl; }
            set
            {
                _tfsUrl = value;
                OnPropertyChanged();
                if (!string.IsNullOrEmpty(_tfsUrl))
                    IsEnabled = true;
            }
        }
    }
}
