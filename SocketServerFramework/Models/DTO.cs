using System;
using SocketServerFramework.Utils;

namespace SocketServerFramework.Models
{
    class DTO
    {
        private string timestamp;
        public string data;

        public DTO()
        {
            this.timestamp = TimeStamp.GetTimeStampString();
        }
    }
}
