using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace NetworkPlayServer
{
    internal class Player
    {
        public IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
        private string nickname = "";
        private Vector3 position = Vector3.Zero;

        public Player(IPEndPoint endPoint, string nickname)
        {
            this.endPoint = endPoint;
            this.nickname = nickname;
        }

        public void setTransform(Vector3 position)
        {
            this.position = position;
        }

        public string getNickname() { 
            return nickname;
        }

    }
}
