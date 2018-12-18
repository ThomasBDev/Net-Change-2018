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

            if (Program.test)
                Console.WriteLine("Connection constructor Client -> Server");

            Write.WriteLine("Poort: " + Program.MijnPoort);

            // Start het reader-loopje
            new Thread(ReaderThread).Start();
        }

        // Deze constructor wordt gebruikt als wij SERVER zijn en een CLIENT maakt met ons verbinding
        public Connection(StreamReader read, StreamWriter write, int mijnPoort)
        {
            Read = read; Write = write;

            if (Program.test)
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
                    if (Program.test)
                    {
                        Console.WriteLine("===============================================");

                        NetChange.printDuTable();
                        NetChange.printNbTable();
                        NetChange.printNdisTable();

                        Console.WriteLine("===============================================");
                    }

                    listenForOtherNodes();
                }
            }
            catch { } // Verbinding is kennelijk verbroken
        }

        public void listenForOtherNodes()
        {
            string[] incomingMessage = Read.ReadLine().Split();
            string command = incomingMessage[0];
            int anderePoort = int.Parse(incomingMessage[1]);

            //Console.WriteLine("command " + command);

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
                    int destination = int.Parse(incomingMessage[2]);
                    int newDist = int.Parse(incomingMessage[3]);

                    Console.WriteLine("M bericht binnengekomen");
                    Console.WriteLine("command     = " + command);
                    Console.WriteLine("anderePoort = " + anderePoort);
                    Console.WriteLine("destination = " + destination);
                    Console.WriteLine("newDist     = " + newDist);

                    NetChange.printNdisTable();

                    Program.Ndis[new Tuple<int, int>(anderePoort, destination)] = newDist;

                    NetChange.printNdisTable();

                    NetChange.Recompute(destination);
                    break;
                case "RequestDu":
                    Console.WriteLine("REQUEST");
                    Console.WriteLine(anderePoort + " wil weten wat onze Du table is");

                    int besteBuur = Program.Nb[anderePoort];
                    int DuLength = Program.Du.Count;

                    string reply = "ReplyDu " + Program.MijnPoort + " " + DuLength;
                    foreach (KeyValuePair<int, int> dist in Program.Du)
                        reply += " " + dist.Key + " " + dist.Value;
                    Program.Buren[besteBuur].Write.WriteLine(reply);

                    Console.WriteLine("REQUEST");
                    break;
                case "ReplyDu":
                    Console.WriteLine("REPLY");
                    Console.WriteLine("We hebben een " + command + " gekregen van " + anderePoort);

                    int length = int.Parse(incomingMessage[2]);

                    //NetChange.printDuTable();
                    //NetChange.printNdisTable();

                    for (int t = 0; t < length; t++)
                    {
                        int index = 3 + (2 * t);
                        int destinationPort = int.Parse(incomingMessage[index]);
                        int distance = int.Parse(incomingMessage[index + 1]);

                        Console.WriteLine("destinationPort distance = " + destinationPort + " " + distance);

                        Tuple<int, int> viaBuurNaarDestination = new Tuple<int, int>(anderePoort, destinationPort);

                        if (!Program.Ndis.Keys.Contains(viaBuurNaarDestination) && (Program.MijnPoort != destinationPort))
                        {
                            Console.WriteLine("Voeg " + destinationPort + " " + distance + " toe aan onze Du table");
                            //distance komt van de nDis table, dus voor ons is dat +1.
                            Program.Du.Add(destinationPort, distance + 1);

                            Console.WriteLine("Voeg " + destinationPort + " " + anderePoort + " toe aan onze Nb table");
                            Program.Nb.Add(destinationPort, anderePoort);

                            Console.WriteLine("Voeg " + anderePoort + " " + destinationPort + " toe aan onze Ndis table");
                            Program.Ndis.Add(viaBuurNaarDestination, distance);
                        }
                        else
                            Console.WriteLine(anderePoort + " " + destinationPort + " staat al in onze Ndis table OF wij zijn de destination");
                    }

                    //NetChange.printDuTable();
                    //NetChange.printNdisTable();

                    Console.WriteLine("REPLY");
                    break;
                default:
                    Console.WriteLine("other command = " + command);
                    break;
            }
        }
    }
}
