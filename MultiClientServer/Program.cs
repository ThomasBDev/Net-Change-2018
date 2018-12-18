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

        public static bool test = false;

        static void Main(string[] args)
        {
            //Initialisatie.
            MijnPoort = int.Parse(args[0]);
            Console.Title = "NetChange " +  args[0];
            new Server(MijnPoort);

            Du.Add(MijnPoort, 0);
            Nb.Add(MijnPoort, MijnPoort);
            //Ndis.Add(new Tuple<int, int>(MijnPoort, MijnPoort), 0);

            for (int t = 1; t < args.Length; t++)
            {
                int anderePoort = int.Parse(args[t]);
                if (MijnPoort < anderePoort)
                {
                    if (!Buren.ContainsKey(anderePoort))
                    {
                        Du.Add(anderePoort, 1);
                        Nb.Add(anderePoort, anderePoort); //(destination, pref neighbour)
                        Buren.Add(anderePoort, new Connection(anderePoort));
                        Ndis.Add(new Tuple<int, int>(anderePoort, anderePoort), 0);
                        Console.WriteLine("INITIAL Verbonden: " + anderePoort);
                        //NetChange.Recompute(anderePoort);
                    }
                }
            }

            if (test)
            {
                Console.WriteLine("===============================================");

                NetChange.printDuTable();
                NetChange.printNdisTable();
                NetChange.printNbTable();

                Console.WriteLine("===============================================");
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
                if (messageType == "B" || messageType == "C" || messageType == "D" || messageType == "REQ")
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
                    case "REQ":
                        Buren[anderePoort].Write.WriteLine("RequestDu " + MijnPoort);
                        break;
                    case "All":
                        NetChange.printDuTable();
                        NetChange.printNbTable();
                        NetChange.printNdisTable();
                        break;
                    case "Du":
                        NetChange.printDuTable();
                        break;
                    case "Nb":
                        NetChange.printNbTable();
                        break;
                    case "Ndis":
                        NetChange.printNdisTable();
                        break;
                    default:
                        Console.WriteLine("INVALID COMMAND");
                        break;
                }
            }
        }

        static void printRoutingTable()
        {
            foreach (KeyValuePair<Tuple<int,int>,int> kvp in Ndis)
            {
                int prefNb = kvp.Key.Item1;
                int destination = kvp.Key.Item2;
                int distance = kvp.Value;
                //int prefNb = Nb[];
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