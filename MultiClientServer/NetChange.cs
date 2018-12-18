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
            Console.WriteLine("InitMdis:");

            if (!nodes.Contains(Program.MijnPoort))
            {
                nodes.Add(Program.MijnPoort);
                Console.WriteLine("Voeg MijnPoort " + Program.MijnPoort + " toe aan nodes in NetChange");
            }
            foreach (KeyValuePair<int, Connection> buur in Program.Buren)
            {
                int buurNummer = buur.Key;

                Console.WriteLine("Vraag aan buur " + buurNummer + " wat zijn Du table is");

                buur.Value.Write.WriteLine("RequestDu " + Program.MijnPoort);

                // send Mydist MijnPoort 0 to buur
                //sendMmessageTo(buur, Program.MijnPoort, 0);
                if (!nodes.Contains(buurNummer))
                {
                    nodes.Add(buurNummer);
                    Console.WriteLine("Voeg nieuwe node " + buurNummer + " toe aan nodes in NetChange");
                }
                //Console.WriteLine("TEST: " + buur.Key);
            }
        }

        public static void Recompute(int Destination)
        {
            int oldDu = 1;
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
                if (distance < 3)
                {
                    Program.Du[Destination] = distance;
                    Program.Nb[Destination] = newPrefNb;
                }
                else
                {
                    Program.Du[Destination] = 3;
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
                    Console.WriteLine("Recompute:");
                    sendMmessageTo(buur, Destination, Program.Du[Destination]);
                }
            }
        }

        public static void sendMmessageTo(KeyValuePair<int, Connection> buur, int destination, int distance)
        {
            Console.WriteLine("sendMmessageTo " + buur.Key + " with destination " + destination + " and distance " + distance);
            buur.Value.Write.WriteLine("M " + Program.MijnPoort + " " + destination + " " + distance);
        }

        public static void printDuTable()
        {
            Console.WriteLine("Current Du table:");
            foreach (KeyValuePair<int, int> element in Program.Du)
                Console.WriteLine(element.Key + " " + element.Value);
        }

        public static void printNbTable()
        {
            Console.WriteLine("Current Nb table:");
            foreach (KeyValuePair<int, int> element in Program.Nb)
                Console.WriteLine(element.Key + " " + element.Value);
        }

        public static void printNdisTable()
        {
            Console.WriteLine("Current Ndis table:");
            foreach (KeyValuePair<Tuple<int, int>, int> element in Program.Ndis)
                Console.WriteLine(element.Key + " " + element.Value);
        }
    }
}
