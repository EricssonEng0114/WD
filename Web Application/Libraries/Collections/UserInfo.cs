using System;
using System.Collections.Generic;
using System.Text;

namespace UBPC.Web.Collections
{
    public class UserInfo
    {
        private String _userID;
        private String _userName;
        private Boolean _forceChangePassword;
        private String _userDesc;
        private int _userGroup;
        private String _systemType;
        private UserRightCollection _userRightsCollection;

        public UserInfo() { }

        public UserInfo(String userID, String userName, Boolean forceChangePassword, String userDesc,
            int userGroup, String systemType)
        {
            _userID = userID;
            _userName = userName;
            _forceChangePassword = forceChangePassword;
            _userDesc = userDesc;
            _userGroup = userGroup;
            _systemType = systemType;
            _userRightsCollection = new UserRightCollection(userGroup);
        }

        public String UserID
        {
            get { return _userID; }
        }

        public String UserName
        {
            get { return _userName; }
        }

        public Boolean ForceChangePassword
        {
            get { return _forceChangePassword; }
        }

        public String UserDesc
        {
            get { return _userDesc; }
        }

        public int UserGroup
        {
            get { return _userGroup; }
        }

        public String SystemType
        {
            get { return _systemType; }
        }

        public UserRightCollection UserAccessRights
        {
            get { return _userRightsCollection; }
        }
    }
}
