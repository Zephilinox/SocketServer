using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net;

namespace ServerData
{
    [Serializable]
    public class Packet
    {
        public List<string> generalData;
        public int integer;
        public bool boolean;
        public string senderID;
        public PacketType type;

        public Packet(string sID, PacketType t)
        {
            generalData = new List<string>();
            senderID = sID;
            type = t;
        }

        public Packet(byte[] bytes)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream(bytes);

            Packet p = (Packet)bf.Deserialize(ms);
            ms.Close();

            generalData = p.generalData;
            integer = p.integer;
            boolean = p.boolean;
            senderID = p.senderID;
            type = p.type;
        }

        public byte[] ToBytes()
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();

            bf.Serialize(ms, this);
            byte[] bytes = ms.ToArray();
            ms.Close();

            return bytes;
        }

        public static string GetIP4Address()
        {
            IPAddress[] ips = Dns.GetHostAddresses(Dns.GetHostName());
            
            foreach(IPAddress ip in ips)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }

            return "127.0.0.1";
        }
    }

    public enum PacketType
    {
        Registration,
        Chat
    }
}
