using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WerewolfClient.Models;

namespace WerewolfClient.Services
{
    public static class UserManager
    {
        private static UserInfo _currentUser;

        public static UserInfo CurrentUser
        {
            get { return _currentUser; }
            set { _currentUser = value; }
        }

        public static void SetCurrentUser(string id, string email)
        {
            _currentUser = new UserInfo
            {
                Id = id,
                Email = email
            };
        }

        public static void ClearCurrentUser()
        {
            _currentUser = null;
        }
    }
} 