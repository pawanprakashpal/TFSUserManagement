using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using TFSUserManagement.Common;

namespace TFSUserManagement.ViewModel
{
    public class TFSServerViewModel : Base.ViewModelBase
    {
        private string _tfsUrl;
        private bool _isDefault;
        private bool _isEnabled;

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { _isEnabled = value; OnPropertyChanged(); }
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
            }
        }

        /// <summary>
        /// Command to add ser to TFS Group
        /// </summary>
        public RelayCommand<object> AddServerCommand { get; private set; }

        public TFSServerViewModel()
        {
            AddServerCommand = new RelayCommand<object>(AddServer);
        }

        /// <summary>
        /// To Save 
        /// </summary>
        /// <param name="obj"></param>
        private void AddServer(object obj)
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(@"c:\user.json", json);
        }

        private void SavedServers()
        {
            using (var fs = new FileStream(@"c:\user.json", FileMode.Open, FileAccess.Read))
            {
                using (var sr = new StreamReader(fs))
                {
                    using (var reader = new JsonTextReader(sr))
                    {
                        if(reader.TokenType==JsonToken.StartObject)
                        {
                            var obj = JObject.Load(reader);
                        }
                    }
                }
            }
        }
    }
}
