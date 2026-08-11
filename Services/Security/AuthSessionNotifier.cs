using ChatClient.Services.Security.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Services.Security
{
    public class AuthSessionNotifier : IAuthSessionNotifier
    {
        public event EventHandler? SessionExpired;

        public void NotifySessionExpired()=>
                        SessionExpired?.Invoke(this, EventArgs.Empty);
    }
}
