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

            Du[MijnPoort] = 0;
            Nb[MijnPoort] = MijnPoort;
            //Geen Buren en Ndis.
            //Je hebt geen buren en de informatie van hen nodig om naar jezelf te wijzen.

            for (int t = 1; t < args.Length; t++)
            {
                int anderePoort = int.Parse(args[t]);
                if (MijnPoort < anderePoort)
                {
                    if (!Buren.ContainsKey(anderePoort))
                    {
                        Console.WriteLine("//We vragen om een verbinding met " + anderePoort);

                        try
                        {
                            createConnectionWithNode(anderePoort, new Connection(anderePoort));
                        }
                        catch (System.Net.Sockets.SocketException)
                        {
                            Console.WriteLine("//De node bestaat nog niet");
                        }
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

        static public void createConnectionWithNode(int anderePoort, Connection connectionType)
        {
            Console.WriteLine("//We maken contact met " + anderePoort);

            lock (NetChange.nodes)
            {
                lock (Du)
                {
                    lock (Nb)
                    {
                        lock (Ndis)
                        {
                            NetChange.nodes.Add(anderePoort);

                            //Je weet zeker dat een Buur de beste afstand en keus is voor zichzelf.
                            //Je kan de tabellen dus direct updaten.
                            //Met de Add methodes kan je een "Key al toegevoegd" exception krijgen.
                            Du[anderePoort] = 1;
                            Nb[anderePoort] = anderePoort; //(destination, pref neighbour)
                            Buren.Add(anderePoort, connectionType);
                            Ndis.Add(new Tuple<int, int>(anderePoort, anderePoort), 0);

                            requestDataFromNode(anderePoort);

                            Console.WriteLine("Verbonden: " + anderePoort);

                            NetChange.Recompute(anderePoort);
                        }
                    }
                }
            }
        }

        static public void destroyConnectionWithNode(int anderePoort)
        {
            Console.WriteLine("//We verbreken verbinding met " + anderePoort);

            lock (Ndis)
            {
                Buren.Remove(anderePoort);
                Ndis.Remove(new Tuple<int, int>(anderePoort, anderePoort));

                Console.WriteLine("Verbroken: " + anderePoort);

                NetChange.Recompute(anderePoort);
            }
        }

        static public void requestDataFromNode(int poort)
        {
            Console.WriteLine("//Vraag aan " + poort + " wat zijn Du table is");
            int besteBuur = Nb[poort];
            Buren[besteBuur].Write.WriteLine("RequestDu " + MijnPoort);
        }

        static void listenForUserInput()
        {
            string[] input = Console.ReadLine().Split();
            string messageType = input[0];

            if (messageType == "R")
                printRoutingTable();
            else
            {
                int anderePoort = -1;

                //if voorkomt een out of array exception als je een invalid command geeft.
                if (messageType == "B" || messageType == "C" || messageType == "D" || messageType == "REQ")
                    anderePoort = int.Parse(input[1]);

                switch (messageType)
                {
                    case "B":
                        //We kennen de destination niet op deze node.
                        if (!Nb.ContainsKey(anderePoort))
                            Console.WriteLine("Poort " + anderePoort + " is niet bekend");
                        //We kennen de destination.
                        //Voor wie is het bestemd?
                        else
                            sendMessage(anderePoort, input[2]);
                        break;
                    case "C":
                        if (Buren.ContainsKey(anderePoort))
                            Console.WriteLine("//We hebben al een verbinding naar " + anderePoort);
                        else
                        {
                            createConnectionWithNode(anderePoort, new Connection(anderePoort));
                            //We hebben net de node van anderePoort toegevoegd aan onze Buren.
                            //Deze aanroep zou dus geen "Key not found" exception moeten kunnen produceren.
                            Buren[anderePoort].Write.WriteLine("C " + anderePoort);                                
                        }
                        break;
                    case "D":
                        if (!Buren.ContainsKey(anderePoort))
                            Console.WriteLine("Poort " + anderePoort + " is niet bekend");
                        else
                        {
                            int besteBuur = Nb[anderePoort];
                            Buren[besteBuur].Write.WriteLine("D " + anderePoort);
                            destroyConnectionWithNode(anderePoort);
                        }
                        break;
                    case "REQ":
                        if (anderePoort != MijnPoort)
                            requestDataFromNode(anderePoort);
                        else
                            Console.WriteLine("//Ndis van jezelf is de Du table, je hoeft het dus niet uit te voeren.");
                        break;
                    case "All":
                        NetChange.printNodesTable();
                        NetChange.printDuTable();
                        NetChange.printNbTable();
                        NetChange.printNdisTable();
                        break;
                    case "Nodes":
                        NetChange.printNodesTable();
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
                        Console.WriteLine("//INVALID COMMAND");
                        break;
                }
            }
        }

        static void printRoutingTable()
        {
            Console.WriteLine("//destination --> distance --> preferred neighbour");

            foreach (KeyValuePair<int, int> elem in Nb)
            {
                int destination = elem.Key;
                int distance = Du[destination];
                int prefNeighbour = elem.Value;
                Console.WriteLine(destination + " " + distance + " " + prefNeighbour);
            }
        }

        static public void sendMessage(int anderePoort, string bericht)
        {
            Console.WriteLine("//Send message aangeroepen.");
            //Het bericht is voor ons bestemd.
            //Een beetje raar om een bericht voor jezelf in te typen, maar het kan gebeuren.
            if (MijnPoort == anderePoort)
                Console.WriteLine(bericht);
            //Het bericht is voor een andere node bestemd.
            else
            {
                int besteBuur = Nb[anderePoort];
                Console.WriteLine("Bericht voor " + anderePoort + " doorgestuurd naar " + besteBuur);
                Buren[besteBuur].Write.WriteLine("B " + anderePoort + " " + bericht);
            }
        }
    }
}