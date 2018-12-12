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
            if (!nodes.Contains(Program.MijnPoort))
                nodes.Add(Program.MijnPoort);
            foreach (KeyValuePair<int, Connection> buur in Program.Buren)
            {
                // send Mydist MijnPoort 0 to buur
                buur.Value.Write.WriteLine("M " + Program.MijnPoort + " " + Program.MijnPoort + " 0");
                if (!nodes.Contains(buur.Key))
                    nodes.Add(buur.Key);
                Console.WriteLine("TEST: " + buur.Key);
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
            if (Program.Du[Destination] != oldDu)
            {
                foreach (KeyValuePair<int, Connection> buur in Program.Buren)
                    buur.Value.Write.WriteLine("M " + Program.MijnPoort + " " + Destination + " " + Program.Du[Destination]);
            }
        }
    }
}
