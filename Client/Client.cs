using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerData;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace Client
{
    class Client
    {
        public static Socket masterSocket;
        public static string name;
        public static string id;

        static void Main(string[] args)
        {
            Console.Write("Enter your name: ");
            name = Console.ReadLine();

            Console.Write("Enter host IP Address: ");
            string ip = Console.ReadLine();

            masterSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(ip), 8888);

            try
            {
                masterSocket.Connect(ipe);
            }
            catch
            {
                Console.WriteLine("Could not connect to host IP");
            }

            Thread t = new Thread(DataIn);
            t.Start();

            while (true)
            {
                try
                {
                    string input = Console.ReadLine();

                    Packet p = new Packet(id, PacketType.Chat);
                    p.data.Add(name);
                    p.data.Add(input);
                    masterSocket.Send(p.ToBytes());
                }
                catch (SocketException ex)
                {
                    Console.WriteLine("The server has disconnected!");
                    Console.ReadLine();
                    Environment.Exit(1);
                }
            }
        }
        static void DataIn()
        {
            byte[] buffer;
            int readBytes;

            while (true)
            {
                try
                {
                    buffer = new byte[masterSocket.SendBufferSize];
                    readBytes = masterSocket.Receive(buffer);

                    if (readBytes > 0)
                    {
                        DataManager(new Packet(buffer));
                    }
                }
                catch (SocketException ex)
                {
                    Console.WriteLine("The server has disconnected!");
                    Console.ReadLine();
                    Environment.Exit(1);
                }
            }
        }

        static void DataManager(Packet p)
        {
            switch(p.type)
            {
                case PacketType.Registration:
                    id = p.data[0];
                    break;
                
                case PacketType.Chat:
                    ConsoleColor c = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Cyan;

                    Console.WriteLine(p.data[0] + ": " + p.data[1]);

                    Console.ForegroundColor = c;
                    break;
            }
        }
    }
}
