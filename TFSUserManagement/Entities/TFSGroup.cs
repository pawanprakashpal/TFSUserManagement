using System.Collections.Generic;

namespace TFSUserManagement.Entities
{
    public class TFSGroup
    {
        private string _groupName;
        private bool _isFavorite;
        private int _memberCount;
        private List<GroupMembers> _listMembers;

        public List<GroupMembers> ListMembers
        {
            get { return _listMembers; }
            set { _listMembers = value; }
        }

        public int MemberCount
        {
            get { return _memberCount; }
            set { _memberCount = value; }
        }

        public bool IsFavorite
        {
            get { return _isFavorite; }
            set
            {
                _isFavorite = value;
            }
        }

        public string GroupName
        {
            get { return _groupName; }
            set { _groupName = value; }
        }
    }

    public class GroupMembers
    {
        public string Name { get; set; }
    }
}

