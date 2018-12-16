using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace MultiClientServer
{
    class Server
    {
        public Server(int port)
        {
            // Luister op de opgegeven poort naar verbindingen
            TcpListener server = new TcpListener(IPAddress.Any, port);
            server.Start();

            // Start een aparte thread op die verbindingen aanneemt
            new Thread(() => AcceptLoop(server)).Start();
        }

        //Deze loop regelt verbindingen?
        //Je kan dit niet in Program.cs stoppen, omdat een verbinding terug clientIn en clientOut nodig heeft.
        private void AcceptLoop(TcpListener handle)
        {
            while (true)
            {
                TcpClient client = handle.AcceptTcpClient();
                StreamReader clientIn = new StreamReader(client.GetStream());
                StreamWriter clientOut = new StreamWriter(client.GetStream());
                clientOut.AutoFlush = true;

                string[] messageFromClient = clientIn.ReadLine().Split();
                string command = messageFromClient[0];
                int anderePoort = int.Parse(messageFromClient[1]);

                //Je hebt een verbinding heen en terug tussen A en B.
                //Als A de verbinding verbreekt, dan blijft de verbinding van B naar A intact.
                //Als A dan weer een verbinding naar B wil maken, dan moet er dus gecheckt worden.
                //Anders wil het programma een 2de verbinding van B naar A maken en dat kan niet/is niet nodig.
                if (Program.Buren.ContainsKey(anderePoort))
                {
                    Console.WriteLine("We hebben al een verbinding van " + Program.MijnPoort + " naar " + anderePoort);
                }
                else
                {
                    Console.WriteLine("We maken een nieuwe verbinding van " + Program.MijnPoort + " naar " + anderePoort);
                    // Zet de nieuwe verbinding in de verbindingslijst
                    Program.Buren.Add(anderePoort, new Connection(clientIn, clientOut, anderePoort));
                    Program.Ndis.Add(new Tuple<int, int>(anderePoort, anderePoort), 1);
                    Program.Du.Add(anderePoort, 1);
                    Program.Nb.Add(anderePoort, anderePoort); //pref neighbour; (nb, destination)
                    //NetChange.routingTable.SetRoute(anderePoort, anderePoort, 1);
                    NetChange.Recompute(anderePoort);
                }
                Console.WriteLine();
            }
        }
    }
}
