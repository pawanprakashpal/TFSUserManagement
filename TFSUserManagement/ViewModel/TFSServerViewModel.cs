using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using TFSUserManagement.Common;
using TFSUserManagement.Entities;

namespace TFSUserManagement.ViewModel
{
    public class TFSServerViewModel : Base.ViewModelBase
    {
        private IServiceProvider _serviceProvider;
        public TFSServer TfsServer { get; set; } = new TFSServer();
        /// <summary>
        /// To Close the Add User Dialog
        /// </summary>
        public Action CloseAction { get; set; }

        /// <summary>
        /// Command to add ser to TFS Group
        /// </summary>
        public RelayCommand<object> AddServerCommand { get; private set; }

        public TFSServerViewModel(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
            AddServerCommand = new RelayCommand<object>(AddServer);
        }

        /// <summary>
        /// To Save server to json file
        /// </summary>
        /// <param name="obj"></param>
        private void AddServer(object obj)
        {
            string json = JsonConvert.SerializeObject(this.TfsServer, Formatting.Indented);
            File.WriteAllText(Constants.FILENAME, json);
            //Close the Add Server dialog
            CloseAction();
            //Open the Group Dialog 
            var xamlDialog = new GroupDialog(_serviceProvider)
            {
                Title = Constants.GROUPTITLE,
                HasMaximizeButton = false,
                HasMinimizeButton = false
            };
            xamlDialog.ShowModal();
        }

        /// <summary>
        /// To Fetch the saved TFS Server
        /// </summary>
        /// <returns></returns>
        public static string SavedServers()
        {
            var server = string.Empty;
            if (File.Exists(Constants.FILENAME))
            {
                using (var fs = new FileStream(Constants.FILENAME, FileMode.Open, FileAccess.Read))
                {
                    using (var sr = new StreamReader(fs))
                    {
                        using (var reader = new JsonTextReader(sr))
                        {
                            var json = JObject.Load(reader);
                            server = json["TFSUrl"].ToString();
                        }
                    }
                }
            }
            return server;
        }
    }
}
