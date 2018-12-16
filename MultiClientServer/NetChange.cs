using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiClientServer
{
    class NetChange
    {
        public static int maxNetworkSize = 20;
        public static List<int> nodes = new List<int>();
        public static RoutingTable routingTable = new RoutingTable();
        public static Ndis ndis = new Ndis(Program.Buren);
        
        public static void InitMdis()
        {
            routingTable.SetRoute(Program.MijnPoort, Program.MijnPoort, 0);
            nodes.Add(Program.MijnPoort);
            foreach (KeyValuePair<int, Connection> buur in Program.Buren)
            {
                // send Mydist MijnPoort 0 to buur
                Console.WriteLine("InitMdis:");
                sendMmessageTo(buur, Program.MijnPoort, Program.MijnPoort, 0);
                //sendMmessageTo(buur, Program.MijnPoort, 1);
                if (!nodes.Contains(buur.Key))
                    nodes.Add(buur.Key);
                Console.WriteLine("TEST: " + buur.Key);
            }
        }

        public static void Recompute(int Destination)
        {
            int oldDu = Program.Du[Destination];
            if (Program.MijnPoort == Destination)
            {
                Program.Du[Program.MijnPoort] = 0;
                Program.Nb[Program.MijnPoort] = Program.MijnPoort;
                routingTable.SetRoute(Destination, Destination, 0);
            }
            else
            {
                int smallestNdis = maxNetworkSize;
                int newPrefNb = Program.Nb[Destination];
                lock (Program.Buren)
                {
                    foreach (KeyValuePair<int, Connection> buur in Program.Buren)
                        try
                        {
                            lock (ndis)
                            {
                                if (ndis.ndis[buur.Key][Destination] < smallestNdis)
                                {
                                    smallestNdis = ndis.ndis[buur.Key][Destination];
                                    newPrefNb = buur.Key;
                                }
                            }
                        }
                        catch { }
                }

                int distance = 1 + smallestNdis;
                if (distance < maxNetworkSize)
                {
                    Program.Du[Destination] = distance;
                    Program.Nb[Destination] = newPrefNb;
                    routingTable.SetRoute(Destination, newPrefNb, distance);
                }
                else
                {
                    Program.Du[Destination] = maxNetworkSize;
                    Program.Nb[Destination] = -1;
                    routingTable.SetRoute(Destination, -1, maxNetworkSize);
                }
            }

            Console.WriteLine("DISTANCES:");
            Console.WriteLine("Destination = " + Destination);
            Console.WriteLine(Program.Du[Destination] + " " + oldDu);
            if (Program.Du[Destination] != oldDu)
            {
                foreach (KeyValuePair<int, Connection> buur in Program.Buren)
                {
                    Console.WriteLine("Recompute:");
                    sendMmessageTo(buur, Program.MijnPoort, Destination, Program.Du[Destination]);
                }
            }
        }

        public static void UpdateDist(int sender, int destination, int distance)
        {
            if (!nodes.Contains(destination))
                nodes.Add(destination);
            ndis.Update(sender, destination, distance);
            Recompute(destination);
        }

        public static void sendMmessageTo(KeyValuePair<int, Connection> buur, int start, int destination, int distance)
        {
            Console.WriteLine("sendMmessageTo " + buur.Key + " with destination " + destination + " and distance " + distance);
            buur.Value.Write.WriteLine("M " + Program.MijnPoort + " " + destination + " " + distance);
        }
    }
}
