using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiClientServer
{
    class Program
    { 
        static public int MijnPoort;
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

            Du.Add(MijnPoort, 0);
            Nb.Add(MijnPoort, MijnPoort);
            for (int t = 1; t < args.Length; t++)
            {
                int AnderePoort = int.Parse(args[t]);
                if (MijnPoort < AnderePoort)
                {
                    Buren.Add(AnderePoort, new Connection(AnderePoort));
                    Ndis.Add(new Tuple<int, int>(AnderePoort, AnderePoort), 1);
                    Du.Add(AnderePoort, 1);
                    Nb.Add(AnderePoort, AnderePoort); //pref neighbour; (nb, destination)
                    Console.WriteLine("Start verbinding gemaakt naar " + AnderePoort);
                }
            }

            foreach (KeyValuePair<int, Connection> buur in Buren)
            {
                // send Mydist MijnPoort 0 to buur
            }

            //Na de initialisatie.
            while (true)
            {
                string[] input = Console.ReadLine().Split();
                string messageType = input[0];

                if (messageType == "R")
                {
                    //1 = Alle doel nodes bereikbaar vanaf deze node.
                    //2 = Bekendste korste afstanden tot de doel nodes.
                    //3 = De buren die je moet nemen als je de korste route naar die doel node wil nemen.

                    int k = 0;
                    int[] nodes = new int[Buren.Count];

                    // for (int t = 0; t < Buren.Keys.Count, t++)
                    foreach (int key in Buren.Keys)
                    {
                        nodes[k] = key;
                        k++;
                    }

                    for (int t = 0; t < Buren.Count; t++)
                    {
                        int doelNode = nodes[t];
                        int korsteAfstand = Du[doelNode];
                        int besteBuur = 100 - t;

                        Console.WriteLine(doelNode + " " + korsteAfstand + " " + besteBuur);
                    }
                }
                else
                {
                    int AnderePoort = int.Parse(input[1]);

                    switch (messageType)
                    {
                        case "B":
                            string bericht = input[2];
                            if (!Buren.ContainsKey(AnderePoort))
                                Console.WriteLine("We hebben geen verbinding naar " + AnderePoort + " om een bericht over te kunnen sturen");
                            else
                                Buren[AnderePoort].Write.WriteLine("Van " + MijnPoort + " hebben we een bericht gekregen: " + bericht);
                            break;
                        //Verbining wordt nog beide kanten op gemaakt.
                        case "C":
                            if (Buren.ContainsKey(AnderePoort))
                                Console.WriteLine("We hebben al een verbinding naar " + AnderePoort);
                            else
                            {
                                try
                                {
                                    // Leg verbinding aan (als client)
                                    Buren.Add(AnderePoort, new Connection(AnderePoort));
                                    Du.Add(AnderePoort, 1);
                                    Console.WriteLine("Vebinding gemaakt naar " + AnderePoort);
                                }
                                catch(System.Net.Sockets.SocketException)
                                {
                                    Console.WriteLine("De node bestaat niet");
                                }
                            }
                            break;
                        //Verbinding wordt alleen vanaf deze kant verwijderd.
                        case "D":
                            if (!Buren.ContainsKey(AnderePoort))
                                Console.WriteLine("We hebben geen verbinding naar deze node om te verbreken");
                            else
                            {
                                Buren.Remove(AnderePoort);
                                Console.WriteLine("Verbinding naar " + AnderePoort + " verbroken");
                                Console.WriteLine("Buren na verwijderen:");
                                for (int t = 1; t < args.Length; t++)
                                    if (Buren.ContainsKey(int.Parse(args[t])))
                                        Console.WriteLine(int.Parse(args[t]));
                                //Du zou kunnen via andere route.
                            }
                            break;
                        default:
                            Console.WriteLine("Invalid command");
                            break;
                    }
                }
            }
        }
        public void Recompute(int Destination)
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
                //send myDist again with new Du to all nb
                //otherwise just stop
            }
        }
    }
}
