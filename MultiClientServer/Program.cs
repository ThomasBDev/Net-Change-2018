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
        static Input input = new Input();
        
        static void Main(string[] args)
        {
            //Initialisatie.
            MijnPoort = int.Parse(args[0]);
            Console.Title = "NetChange " +  args[0];
            new Server(MijnPoort);

            for (int t = 1; t < args.Length; t++)
            {
                int anderePoort = int.Parse(args[t]);
                if (MijnPoort < anderePoort)
                {
                    Buren.Add(anderePoort, new Connection(anderePoort));
                    Du.Add(anderePoort, 1);
                    Console.WriteLine("Start verbinding gemaakt naar " + anderePoort);
                }
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
                    int anderePoort = int.Parse(input[1]);

                    switch (messageType)
                    {
                        case "B":
                            string bericht = input[2];
                            if (!Buren.ContainsKey(anderePoort))
                                Console.WriteLine("We hebben geen verbinding naar " + anderePoort + " om een bericht over te kunnen sturen");
                            else
                                Buren[anderePoort].Write.WriteLine("Van " + MijnPoort + " hebben we een bericht gekregen: " + bericht);
                            break;
                        //Verbining wordt nog beide kanten op gemaakt.
                        case "C":
                            if (Buren.ContainsKey(anderePoort))
                                Console.WriteLine("We hebben al een verbinding naar " + anderePoort);
                            else
                            {
                                try
                                {
                                    // Leg verbinding aan (als client)
                                    Buren.Add(anderePoort, new Connection(anderePoort));
                                    Du.Add(anderePoort, 1);
                                    Console.WriteLine("Vebinding gemaakt naar " + anderePoort);
                                }
                                catch(System.Net.Sockets.SocketException)
                                {
                                    Console.WriteLine("De node bestaat niet");
                                }
                            }
                            break;
                        //Verbinding wordt alleen vanaf deze kant verwijderd.
                        case "D":
                            if (!Buren.ContainsKey(anderePoort))
                                Console.WriteLine("We hebben geen verbinding naar deze node om te verbreken");
                            else
                            {
                                Buren.Remove(anderePoort);
                                Console.WriteLine("Verbinding naar " + anderePoort + " verbroken");
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
    }
}
