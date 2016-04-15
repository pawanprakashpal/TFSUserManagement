using EnvDTE80;
using Microsoft.VisualStudio.TeamFoundation.VersionControl;
using System;

namespace TFSUserManagement.Common.Helper
{
    /// <summary>
    /// Helper class.
    /// </summary>
    public static class Helpers
    {
        /// <summary>
        /// Get the VersionControlExt extensibility object.
        /// </summary>
        public static VersionControlExt GetVersionControlExt(IServiceProvider serviceProvider)
        {
            if (serviceProvider != null)
            {
                DTE2 dte = serviceProvider.GetService(typeof(Microsoft.VisualStudio.Shell.Interop.SDTE)) as DTE2;
                if (dte != null)
                {
                    return dte.GetObject("Microsoft.VisualStudio.TeamFoundation.VersionControl.VersionControlExt") as VersionControlExt;
                }
            }

            return null;
        }
    }
}
