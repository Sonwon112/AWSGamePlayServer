using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class DTO
{
    public int id { get; set; }
    public string type { get; set; }
    public string msg { get; set; }

    public DTO(int id, string type, string msg)
    {
        this.id = id;
        this.type = type;
        this.msg = msg;
    }
}

enum MessageType
{
    NONE,
    CONNECT,
    INSTANTIATE,
    SEND_PARTICIPANT,
    SEND_TRANSFORM,
    WIN
}
