using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiClientServer
{
    class Program
    { 
        static public int MijnPoort;
        public static List<int> nodes = new List<int>();
        static public Dictionary<int, Connection> Buren = new Dictionary<int, Connection>();
        static public Dictionary<int, int> Du = new Dictionary<int, int>();
        static public Dictionary<Tuple<int, int>, int> Ndis = new Dictionary<Tuple<int, int>, int>(); 
        static public Dictionary<int, int> Nb = new Dictionary<int, int>();
        static Input input = new Input();

        static void Main(string[] args)
        {
            //Initialisatie.
            MijnPoort = int.Parse(args[0]);
            Console.Title = "NetChange " +  args[0];
            new Server(MijnPoort);
            Console.WriteLine("SERVER GESTART");

            Du.Add(MijnPoort, 0);
            Nb.Add(MijnPoort, MijnPoort);
            nodes.Add(MijnPoort);

            for (int t = 1; t < args.Length; t++)
            {
                int anderePoort = int.Parse(args[t]);
                if (MijnPoort < anderePoort)
                {
                    nodes.Add(anderePoort);
                    Buren.Add(anderePoort, new Connection(anderePoort));
                    Ndis.Add(new Tuple<int, int>(anderePoort, anderePoort), 1);
                    Du.Add(anderePoort, 1);
                    Nb.Add(anderePoort, anderePoort); //pref neighbour; (nb, destination)
                    Console.WriteLine("INITIAL Verbonden: " + anderePoort);
                }
            }

            foreach (KeyValuePair<int, Connection> buur in Buren)
            {
                // send Mydist MijnPoort 0 to buur
                buur.Value.Write.WriteLine("M " + MijnPoort + " " + 0);
            }

            //Na de initialisatie.
            while (true)
            {
                Console.WriteLine("LISTEN FOR USER INPUT IN PROGRAM.CS");
                listenForUserInput();
                Console.WriteLine();
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
                    //Verbinding wordt nog beide kanten op gemaakt.
                    case "C":
                        Console.WriteLine("CREATE CONNECTION");
                        createConnection(anderePoort);
                        break;
                    //Verbinding wordt alleen vanaf deze kant verwijderd.
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
            foreach (KeyValuePair<int, int> d in Du)
            {
                int destination = d.Key;
                int distance = d.Value;
                //int prefNb = Nb[];
                Console.WriteLine(destination + " " + distance + " " + 0);
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
        
        public static void Recompute(int Destination)
        {
            int oldDu = Du[Destination];
            if (MijnPoort == Destination)
            {
                Du[MijnPoort] = 0;
                Nb[MijnPoort] = MijnPoort;
            }
            else
            {
                int smallestNdis = 3;
                int newPrefNb = Nb[Destination];
                foreach (KeyValuePair<int, Connection> buur in Buren)
                    if (Ndis[new Tuple<int, int>(buur.Key, MijnPoort)] < smallestNdis)
                    {
                        smallestNdis = Ndis[new Tuple<int, int>(buur.Key, Destination)];
                        newPrefNb = buur.Key;
                    }
                int distance = 1 + smallestNdis;
                if (distance < 3)
                {
                    Du[Destination] = distance;
                    Nb[Destination] = newPrefNb;
                }
                else
                {
                    Du[Destination] = 3;
                    Nb[Destination] = 0;
                }
            }
            if (Du[Destination] != oldDu)
            {
                foreach (KeyValuePair<int, Connection> buur in Buren)
                    buur.Value.Write.WriteLine("M " + Destination + " " + Du[Destination]);
            }
        }
    }
}