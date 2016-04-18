using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TFSUserManagement.Entities;
using TFSUserManagement.ViewModel;

namespace TFSUserManagement.TFSData
{
    /// <summary>
    /// Class to work with TFS Groups
    /// </summary>
    public sealed class TfsCollection
    {
        private static readonly TfsCollection instance = new TfsCollection();
        private TfsCollection()
        {
        }

        /// <summary>
        /// Static Instance of the Data Model
        /// </summary>
        public static TfsCollection Instance
        {
            get
            {
                return instance;
            }
        }

        /// <summary>
        /// TFS URI
        /// </summary>
        public static Uri TFSUri
        {
            get
            {
                return new Uri(GetTFSUrl().TFSUrl);
            }
        }

        /// <summary>
        /// TfsTeamProjectCollection Instance
        /// </summary>
        public static TfsTeamProjectCollection ProjectCollection
        {
            get
            {
                return TfsTeamProjectCollectionFactory.GetTeamProjectCollection(TFSUri); ;
            }
        }

        /// <summary>
        /// IIdentityManagementService
        /// </summary>
        public IIdentityManagementService IMS
        {
            get
            {
                return ProjectCollection.GetService<IIdentityManagementService>();
            }
        }

        /// <summary>
        /// TeamFoundationIdentity Collection
        /// </summary>
        public TeamFoundationIdentity[] TeamIdentities
        {
            get
            {
                return IMS.ListApplicationGroups("Cofunds", ReadIdentityOptions.TrueSid);
            }
        }

        /// <summary>
        /// Function to remove the users from TFS Group
        /// </summary>
        /// <param name="user"></param>
        /// <param name="group"></param>
        public void RemoveUsers(string user, string group)
        {
            var tfsGroupIdentity = IMS.ReadIdentity(IdentitySearchFactor.AccountName,
                       group,
                       MembershipQuery.None,
                       ReadIdentityOptions.IncludeReadFromSource);
            var userIdentity = IMS.ReadIdentity(IdentitySearchFactor.AccountName,
                user,
                MembershipQuery.None,
                ReadIdentityOptions.IncludeReadFromSource);
            IMS.RemoveMemberFromApplicationGroup(tfsGroupIdentity.Descriptor,
                userIdentity.Descriptor);
        }

        /// <summary>
        /// To add the user to TFS Group
        /// </summary>
        /// <param name="user"></param>
        /// <param name="group"></param>
        public void AddUsers(string user, string group)
        {
            var tfsGroupIdentity = IMS.ReadIdentity(IdentitySearchFactor.AccountName,
                       group,
                       MembershipQuery.None,
                       ReadIdentityOptions.IncludeReadFromSource);

            var userIdentity = IMS.ReadIdentity(IdentitySearchFactor.AccountName,
                user,
                MembershipQuery.None,
                ReadIdentityOptions.IncludeReadFromSource);

            IMS.AddMemberToApplicationGroup(tfsGroupIdentity.Descriptor,
                userIdentity.Descriptor);
        }

        /// <summary>
        /// Function to fetch TFS Groups from TFS with their members
        /// </summary>
        /// <returns></returns>
        public async Task<List<TFSGroup>> GetTFSGroups()
        {
            var grpNames = new List<string>();
            var groupList = new List<TFSGroup>();
            await Task.Run(() =>
            {
                TeamFoundationIdentity[] projectGroups = TeamIdentities;

                _identityCollection = new Dictionary<IdentityDescriptor, TeamFoundationIdentity>(IdentityDescriptorComparer.Instance);
                _tfsGroups = new List<TeamFoundationIdentity>();

                var descSet = new Dictionary<IdentityDescriptor, object>(IdentityDescriptorComparer.Instance);
                foreach (TeamFoundationIdentity projectGroup in projectGroups)
                {
                    grpNames.Add(projectGroup.DisplayName);
                    descSet[projectGroup.Descriptor] = projectGroup.Descriptor;
                }

                // Expanded membership of project groups
                projectGroups = IMS.ReadIdentities(descSet.Keys.ToArray(), MembershipQuery.Expanded, ReadIdentityOptions.None);

                // Collect all descriptors
                foreach (TeamFoundationIdentity projectGroup in projectGroups)
                {
                    foreach (IdentityDescriptor identityDescriptor in projectGroup.Members)
                    {
                        descSet[identityDescriptor] = identityDescriptor;
                    }
                }

                FetchIdentities(descSet.Keys.ToArray());

                // Now output groups and their members.
                foreach (TeamFoundationIdentity identity in _tfsGroups)
                {
                    var tfsGroup = new TFSGroup
                    {
                        GroupName = identity.DisplayName,
                        ListMembers = new List<GroupMembers>()
                    };
                    foreach (IdentityDescriptor memDesc in identity.Members)
                    {
                        tfsGroup.ListMembers.Add(
                            new GroupMembers
                            {
                                Name = _identityCollection.ContainsKey(memDesc) ? _identityCollection[memDesc].UniqueName : string.Empty
                            });
                        tfsGroup.MemberCount = tfsGroup.ListMembers.Count;
                    }
                    groupList.Add(tfsGroup);
                }
            });
            return groupList.Where(g => grpNames.Contains(g.GroupName)).ToList();
        }

        /// <summary>
        /// To Fetch users from the AD
        /// </summary>
        /// <returns></returns>
        public async Task<List<TFSUser>> FetchUsers()
        {
            var users = new List<TFSUser>();
            await Task.Run(() =>
            {
                var validUsers = IMS.ReadIdentities(
                    IdentitySearchFactor.AccountName,
                    new[] { "Project Collection Valid Users" },
                    MembershipQuery.Expanded,
                    ReadIdentityOptions.None)[0][0].Members;

                IMS.ReadIdentities(
                    validUsers,
                    MembershipQuery.None,
                    ReadIdentityOptions.TrueSid)
                    .Where(x => !x.IsContainer && x.IsActive)
                    .ToList()
                    .ForEach(user =>
                    {
                        users.Add(new TFSUser
                        {
                            FullName = user.DisplayName,
                            UserName = user.UniqueName
                        });
                    });
            });
            return users.OrderBy(a => a.FullName).ToList();
        }

        private Dictionary<IdentityDescriptor, TeamFoundationIdentity> _identityCollection;
        private List<TeamFoundationIdentity> _tfsGroups;

        /// <summary>
        /// Function to fetch identities from provided IdentityDescriptor Collection
        /// </summary>
        /// <param name="descriptors"></param>
        private void FetchIdentities(IdentityDescriptor[] descriptors)
        {
            TeamFoundationIdentity[] identities;

            // If total membership exceeds batch size limit for Read, break it up
            int batchSizeLimit = 100000;

            if (descriptors.Length > batchSizeLimit)
            {
                int batchNum = 0;
                int remainder = descriptors.Length;
                IdentityDescriptor[] batchDescriptors = new IdentityDescriptor[batchSizeLimit];

                while (remainder > 0)
                {
                    int startAt = batchNum * batchSizeLimit;
                    int length = batchSizeLimit;
                    if (length > remainder)
                    {
                        length = remainder;
                        batchDescriptors = new IdentityDescriptor[length];
                    }

                    Array.Copy(descriptors, startAt, batchDescriptors, 0, length);
                    identities = IMS.ReadIdentities(batchDescriptors, MembershipQuery.Direct, ReadIdentityOptions.None);
                    SortIdentities(identities);
                    remainder -= length;
                }
            }
            else
            {
                identities = IMS.ReadIdentities(descriptors, MembershipQuery.Direct, ReadIdentityOptions.None);
                SortIdentities(identities);
            }
        }

        /// <summary>
        /// Function to sort Identities from provided TeamFoundationIdentity Collection
        /// </summary>
        /// <param name="identities"></param>
        private void SortIdentities(TeamFoundationIdentity[] identities)
        {
            foreach (TeamFoundationIdentity identity in identities)
            {
                _identityCollection[identity.Descriptor] = identity;

                if (identity.IsContainer)
                {
                    _tfsGroups.Add(identity);
                }
            }
        }

        /// <summary>
        /// Function to fetch the TFS Server URL
        /// </summary>
        /// <returns></returns>
        private static TFSServer GetTFSUrl()
        {                      
            return new TFSServer
            {
                TFSUrl = TFSServerViewModel.SavedServers(),
                IsDefault = true
            };
        }
    }
}

