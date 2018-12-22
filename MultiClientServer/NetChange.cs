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
        
        public static void InitMdis()
        {
            Console.WriteLine("InitMdis START");

            if (!nodes.Contains(Program.MijnPoort))
            {
                nodes.Add(Program.MijnPoort);
                //Console.WriteLine("Voeg MijnPoort " + Program.MijnPoort + " toe aan nodes in NetChange");
            }

            //else
            //    Console.WriteLine("MijnPoort " + Program.MijnPoort + " zit al in nodes in NetChange");

            foreach (KeyValuePair<int, Connection> buur in Program.Buren)
            {
                //Vraag data van je directe Buren.
                int buurNummer = buur.Key;
                Program.requestDataFromNode(buurNummer);

                // send Mydist MijnPoort 0 to buur
                //sendMmessageTo(buur, Program.MijnPoort, 0);
                if (!nodes.Contains(buurNummer))
                {
                    nodes.Add(buurNummer);
                    //Console.WriteLine("Voeg nieuwe node " + buurNummer + " toe aan nodes in NetChange");
                }

                //else
                //    Console.WriteLine("buurNummer " + buurNummer + " zit al in nodes in NetChange");
                ////Console.WriteLine("TEST: " + buur.Key);
            }

            //Console.WriteLine();
            //printNodesTable();
            //printNbTable();
            //Console.WriteLine();

            Console.WriteLine("InitMdis END");
        }

        public static void Recompute(int Destination)
        {
            Console.WriteLine("--------------------------------------------Recompute START");

            int oldDu = Program.Du[Destination];
            if (Program.MijnPoort == Destination)
            {
                Program.Du[Program.MijnPoort] = 0;
                Program.Nb[Program.MijnPoort] = Program.MijnPoort;
            }
            else
            {
                int smallestNdis = maxNetworkSize;
                int newPrefNb = Program.Nb[Destination];
                foreach (KeyValuePair<int, Connection> buur in Program.Buren)
                    try
                    {
                        if (Program.Ndis[new Tuple<int, int>(buur.Key, Destination)] < smallestNdis)
                        {
                            lock (Program.Buren)
                            {
                                lock (Program.Ndis)
                                    smallestNdis = Program.Ndis[new Tuple<int, int>(buur.Key, Destination)];
                                newPrefNb = buur.Key;
                            }

                        }
                    }
                    catch { }
                int distance = 1 + smallestNdis;
                if (distance < maxNetworkSize)
                {
                    Program.Du[Destination] = distance;
                    Program.Nb[Destination] = newPrefNb;
                }
                else
                {
                    Program.Du[Destination] = maxNetworkSize;
                    Program.Nb[Destination] = 0;
                }
            }

            Console.WriteLine("DISTANCES:");
            Console.WriteLine("Destination = " + Destination);
            Console.WriteLine(Program.Du[Destination] + " " + oldDu);

            if (Program.Du[Destination] != oldDu)
            {
                foreach (KeyValuePair<int, Connection> buur in Program.Buren)
                {
                    sendMmessageTo(buur, Destination, Program.Du[Destination]);
                }
            }

            Console.WriteLine("--------------------------------------------Recompute END");
        }

        public static void sendMmessageTo(KeyValuePair<int, Connection> buur, int destination, int distance)
        {
            Console.WriteLine("sendMmessageTo " + buur.Key + " with destination " + destination + " and distance " + distance);
            buur.Value.Write.WriteLine("M " + Program.MijnPoort + " " + destination + " " + distance);
        }

        public static void printNodesTable()
        {
            Console.WriteLine("Current Nodes table:");
            foreach (int node in nodes)
                Console.WriteLine(node);
        }

        public static void printDuTable()
        {
            Console.WriteLine("Current Du table:");
            Console.WriteLine("destination --> distance");
            foreach (KeyValuePair<int, int> element in Program.Du)
                Console.WriteLine(element.Key + " " + element.Value);
        }

        public static void printNbTable()
        {
            Console.WriteLine("Current Nb table:");
            Console.WriteLine("destination --> pref neighbour");
            foreach (KeyValuePair<int, int> element in Program.Nb)
                Console.WriteLine(element.Key + " " + element.Value);
        }

        public static void printNdisTable()
        {
            Console.WriteLine("Current Ndis table:");
            Console.WriteLine("from neighbour --> to destination --> distance");
            foreach (KeyValuePair<Tuple<int, int>, int> element in Program.Ndis)
                Console.WriteLine(element.Key + " " + element.Value);
        }
    }
}
