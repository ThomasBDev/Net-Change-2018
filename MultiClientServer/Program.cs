using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiClientServer
{
    class Program
    { 
        static public int MijnPoort;
        public static Dictionary<int, Connection> Buren = new Dictionary<int, Connection>();
        static public Dictionary<int, int> Du = new Dictionary<int, int>();
        static public Dictionary<Tuple<int, int>, int> Ndis = new Dictionary<Tuple<int, int>, int>(); 
        static public Dictionary<int, int> Nb = new Dictionary<int, int>();
        public static RoutingTable routingTable = new RoutingTable();
        static Input input = new Input();

        static void Main(string[] args)
        {
            //Initialisatie.
            MijnPoort = int.Parse(args[0]);
            Console.Title = "NetChange " +  args[0];
            new Server(MijnPoort);

            Du.Add(MijnPoort, 0);
            Ndis.Add(new Tuple<int, int>(MijnPoort, MijnPoort), 0);
            Nb.Add(MijnPoort, MijnPoort);
            routingTable.SetRoute(MijnPoort, MijnPoort, 0);

            for (int t = 1; t < args.Length; t++)
            {
                int anderePoort = int.Parse(args[t]);
                if (MijnPoort < anderePoort)
                {
                    if (!Buren.ContainsKey(anderePoort))
                    {
                        while (!Buren.ContainsKey(anderePoort))
                            Buren.Add(anderePoort, new Connection(anderePoort));
                        Ndis.Add(new Tuple<int, int>(anderePoort, anderePoort), 1);
                        Du.Add(anderePoort, 1);
                        Nb.Add(anderePoort, anderePoort); //pref neighbour; (nb, destination)
                        routingTable.SetRoute(anderePoort, anderePoort, 1);
                        NetChange.UpdateDist(anderePoort, MijnPoort, 1);
                        Console.WriteLine("INITIAL Verbonden: " + anderePoort);
                        //NetChange.Recompute(anderePoort);
                    }

                }
            }
            NetChange.InitMdis();

            //Na de initialisatie.
            while (true)
            {
                listenForUserInput();
            }
        }

        static void listenForUserInput()
        {
            string[] input = Console.ReadLine().Split();
            string messageType = input[0];

            if (messageType == "R")
            {
                Console.WriteLine("PRINT ROUTINGTABLE");
                printRoutingTable();
            }
            else
            {
                int anderePoort = -1;

                //if voorkomt een out of array exception als je een invalid command geeft.
                if (messageType == "B" || messageType == "C" || messageType == "D")
                    anderePoort = int.Parse(input[1]);

                switch (messageType)
                {
                    case "B":
                        Console.WriteLine("SEND MESSAGE");
                        sendMessage(anderePoort, input[2]);
                        break;
                    case "C":
                        Console.WriteLine("CREATE CONNECTION");
                        createConnection(anderePoort);
                        break;
                    case "D":
                        Console.WriteLine("DESTROY CONNECTION");
                        destroyConnection(anderePoort);
                        break;
                    default:
                        Console.WriteLine("INVALID COMMAND");
                        break;
                }
            }
        }

        static void printRoutingTable()
        {
            foreach (KeyValuePair<int, Tuple<int,int>> kvp in NetChange.routingTable.routingTable)
            {
                int destination = kvp.Key;
                int prefNb = kvp.Value.Item1;
                int distance = kvp.Value.Item2;
                if (distance == 0)
                {
                    Console.WriteLine(destination + " " + 0 + " local");
                    continue;
                }
                if (distance > NetChange.maxNetworkSize)
                    continue;
                Console.WriteLine(destination + " " + distance + " " + prefNb);
            }
        }

        static void sendMessage(int anderePoort, string bericht)
        {
            if (!Buren.ContainsKey(anderePoort))
                Console.WriteLine("Poort " + anderePoort + " is niet bekend");
            else if (MijnPoort != anderePoort)
            {
                int besteBuur;
                Console.WriteLine("Bericht voor " + anderePoort + " doorgestuurd naar " + "besteBuur");
                Buren[anderePoort].Write.WriteLine("B " + anderePoort + " " + bericht);
            }
            else
            {
                Console.WriteLine("Bericht is voor ons bestemd.");
                Buren[anderePoort].Write.WriteLine(bericht);
            }
        }

        static void createConnection(int anderePoort)
        {
            if (Buren.ContainsKey(anderePoort))
                Console.WriteLine("We hebben al een verbinding naar " + anderePoort);
            else
            {
                try
                {
                    Buren.Add(anderePoort, new Connection(anderePoort));
                    Du.Add(anderePoort, 1);
                    Buren[anderePoort].Write.WriteLine("C " + anderePoort);

                    Console.WriteLine("Verbonden: " + anderePoort);

                    //Console.WriteLine("Foreach in Program.cs");
                    //foreach (KeyValuePair<int, Connection> Buur in Program.Buren)
                    //    Console.WriteLine("Buur = " + Buur.Key);
                    /*foreach (int node in NetChange.nodes)
                    {
                        Ndis[new Tuple<int, int>(node, anderePoort)] = 20;
                        Program.Buren[anderePoort].Write.WriteLine("M " + Program.MijnPoort + " " + node + " " + Du[node]);
                    }*/
                }
                catch (System.Net.Sockets.SocketException)
                {
                    Console.WriteLine("De node bestaat niet");
                }
            }
        }

        static void destroyConnection(int anderePoort)
        {
            if (!Buren.ContainsKey(anderePoort))
                Console.WriteLine("Poort " + anderePoort + " is niet bekend");
            else
            {
                Buren[anderePoort].Write.WriteLine("D " + anderePoort);
                Buren.Remove(anderePoort);
                Du.Remove(anderePoort);

                Console.WriteLine("Verbroken: " + anderePoort);
            }
        }
    }
}