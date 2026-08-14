using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Common.Enums
{
    public enum ChatConnectionState
    {
        Disconnected, 
        Connecting, 
        Connected, 
        Reconnecting
    }
}
