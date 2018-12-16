using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace MultiClientServer
{
    class Connection
    {
        public StreamReader Read;
        public StreamWriter Write;

        public int poort;

        // Connection heeft 2 constructoren: deze constructor wordt gebruikt als wij CLIENT worden bij een andere SERVER
        public Connection(int port)
        {
            TcpClient client = new TcpClient("localhost", port);
            Read = new StreamReader(client.GetStream());
            Write = new StreamWriter(client.GetStream());
            Write.AutoFlush = true;

            Console.WriteLine("Connection constructor Client -> Server");

            Write.WriteLine("Poort: " + Program.MijnPoort);

            // Start het reader-loopje
            new Thread(ReaderThread).Start();
        }

        // Deze constructor wordt gebruikt als wij SERVER zijn en een CLIENT maakt met ons verbinding
        public Connection(StreamReader read, StreamWriter write, int mijnPoort)
        {
            Read = read; Write = write;

            Console.WriteLine("Connection constructor Server <- Client");
            poort = mijnPoort;

            // Start het reader-loopje
            new Thread(ReaderThread).Start();
        }

        // LET OP: Nadat er verbinding is gelegd, kun je vergeten wie er client/server is (en dat kun je aan het Connection-object dus ook niet zien!)

        // Deze loop leest wat er binnenkomt en print dit
        public void ReaderThread()
        {
            try
            {
                while (true)
                {
                    listenForOtherNodes();
                }
            }
            catch { } // Verbinding is kennelijk verbroken
        }

        public void listenForOtherNodes()
        {
            string[] incomingMessage = Read.ReadLine().Split();
            string command = incomingMessage[0];
            int anderePoort = int.Parse(incomingMessage[2]);

            Console.WriteLine("command " + command);

            switch (command)
            {
                case "B":
                    string message = incomingMessage[2];
                    Console.WriteLine("B bericht binnengekomen = " + command + " " + anderePoort + " " + message);
                    break;
                case "C":
                    Console.WriteLine("C bericht binnengekomen = " + command + " " + anderePoort);
                    break;
                case "D":
                    Console.WriteLine("D bericht binnengekomen = " + command + " " + anderePoort);
                    Program.Buren.Remove(poort);
                    Program.Du.Remove(poort);
                    break;
                case "M":
                    int destination = int.Parse(incomingMessage[3]);
                    int newDist = int.Parse(incomingMessage[4]);

                    Console.WriteLine("M bericht binnengekomen");
                    Console.WriteLine("command     = " + command);
                    Console.WriteLine("anderePoort = " + anderePoort);
                    Console.WriteLine("destination = " + destination);
                    Console.WriteLine("newDist     = " + newDist);

                    Console.WriteLine("OLD Ndis:");
                    foreach (KeyValuePair<Tuple<int, int>, int> element in Program.Ndis)
                        Console.WriteLine(element);

                    Program.Ndis[new Tuple<int, int>(anderePoort, destination)] = newDist;

                    Console.WriteLine("NEW Ndis:");
                    foreach (KeyValuePair<Tuple<int, int>, int> element in Program.Ndis)
                        Console.WriteLine(element);
                    //NetChange.routingTable()

                    NetChange.Recompute(destination);
                    break;
                default:
                    Console.WriteLine("other command = " + command);
                    break;
            }
        }
    }
}
