using System;
using Stavadlo22.Infrastructure.Enums;


namespace Stavadlo22.Infrastructure
{
    //MH: 
    /// <summary>
    /// Obsahuje udaje o aktualne prihlasenom uzivatelovi;
    /// Pri zmene roly odpali event RoleChangedEvent;
    /// StavadloGlobalData.CurrentUser sa nastavi v LoginWindowViewModel
    /// </summary>
    public class CurrentUser
    {
        public event Action<CurrentUserEventArgs> RoleChangedEvent;

        public CurrentUser()
        {
            Name = "";
            Pass = "";
            Role = USER_ROLE.NONE;
        }

        public CurrentUser(string userName, string userPass, USER_ROLE userRole)
        {
            Name = userName;
            Pass = userPass;
            Role = userRole;
        }

        public string Name { get; set; }
        public string Pass { get; set; }


        /*USER_ROLE:
            NONE,
            ADMIN,
            DISPECER,
            UDRZBA,
            READONLY
         */
        USER_ROLE role;
        public USER_ROLE Role
        {
            get { return role; }
            set
            {
                if (value == role)
                    return;
                role = value;
#if !DEBUG
                RoleChangedEvent?.Invoke(new CurrentUserEventArgs() { currentRole = role, currentUserName = Name });
#endif
            }
        }


        public override string ToString()
        {
            return Name + ";" + Role.ToString();
        }

    }//class CurrentUser

    /// <summary>
    /// MH: 08.11.2013
    /// trieda pre EventArgument
    /// </summary>
    public class CurrentUserEventArgs : EventArgs
    {
        public string currentUserName;
        public USER_ROLE currentRole;
    }
}
