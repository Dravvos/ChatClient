using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Services.Api
{
    public class ApiSettings
    {
        public string BaseUrl { get; set; } = "http://localhost:5051/"; // ex.: "https://localhost:5443/"
    }
}
