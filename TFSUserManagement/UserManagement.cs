﻿//------------------------------------------------------------------------------
// <copyright file="UserManagement.cs" company="Cofunds Ltd">
//     Copyright (c) Cofunds Ltd.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Globalization;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace TFSUserManagement
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class UserManagement
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("1f48791a-550b-47fc-84ac-f7b83ae0d82c");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserManagement"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private UserManagement(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static UserManagement Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        public IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new UserManagement(package);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            dynamic xamlDialog;
            if (string.IsNullOrEmpty(ViewModel.TFSServerViewModel.SavedServers()))
            {
                xamlDialog = new ServerDialog(ServiceProvider)
                {
                    Title = Common.Constants.ADDSERVERTITLE,
                    HasMaximizeButton = false,
                    HasMinimizeButton = false
                };
            }
            else
            {
                xamlDialog = new GroupDialog(ServiceProvider)
                {
                    Title = Common.Constants.GROUPTITLE,
                    HasMaximizeButton = false,
                    HasMinimizeButton = false
                };
            }
            xamlDialog.ShowModal();
        }
    }
}
