using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerData;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Net;

namespace Server
{
    class Server
    {
        static Socket listenerSocket;
        static List<ClientData> clients;

        static void Main(string[] args)
        {
            Console.WriteLine("Starting server on " + Packet.GetIP4Address());
            listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clients = new List<ClientData>();

            IPEndPoint ip = new IPEndPoint(IPAddress.Parse(Packet.GetIP4Address()), 8888);
            listenerSocket.Bind(ip);

            Thread listenerThread = new Thread(ListenThread);
            listenerThread.Start();
        }

        static void ListenThread()
        {
            while (true)
            {
                listenerSocket.Listen(0);
                clients.Add(new ClientData(listenerSocket.Accept()));
            }
        }

        public static void DataIn(object cSocket)
        {
            Socket clientSocket = (Socket)cSocket;
            byte[] buffer;
            int readBytes;

            while (true)
            {
                try
                {
                    buffer = new byte[clientSocket.SendBufferSize];
                   
                    readBytes = clientSocket.Receive(buffer);
                    if (readBytes > 0)
                    {
                        Packet p = new Packet(buffer);
                        DataManager(p);
                    }
                }
                catch (SocketException ex)
                {
                    Console.WriteLine("A client disconnected!");
                }
            }
        }

        static public void DataManager(Packet p)
        {
            switch (p.type)
            {
                case PacketType.Chat:
                    foreach(ClientData c in clients)
                    {
                        if (c.id != p.senderID)
                        {
                            c.clientSocket.Send(p.ToBytes());
                        }
                    }

                    break;
            }
        }
    }

    class ClientData
    {
        public Socket clientSocket;
        public Thread clientThread;
        public string id;

        public ClientData()
        {
            id = Guid.NewGuid().ToString();

            clientThread = new Thread(Server.DataIn);
            clientThread.Start(clientSocket);

            SendRegistrationPacket();
        }

        public ClientData(Socket cSocket)
        {
            id = Guid.NewGuid().ToString();
            clientSocket = cSocket;

            clientThread = new Thread(Server.DataIn);
            clientThread.Start(clientSocket);

            SendRegistrationPacket();
        }

        public void SendRegistrationPacket()
        {
            Packet p = new Packet("Server", PacketType.Registration);
            p.data.Add(id);
            clientSocket.Send(p.ToBytes());
        }
    }
}
