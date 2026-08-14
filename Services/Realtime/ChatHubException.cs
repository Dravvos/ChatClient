using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Services.Realtime
{
    public class ChatHubException(string message) : Exception(message)
    {
    }
}
