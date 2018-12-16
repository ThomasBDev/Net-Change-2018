using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiClientServer
{
    class Ndis
    {
        public Dictionary<int, Dictionary<int, int>> ndis = new Dictionary<int, Dictionary<int, int>>();

        public Ndis(Dictionary<int, Connection> buren)
        {
            foreach (KeyValuePair<int, Connection> buur in buren)
                ndis.Add(buur.Key, new Dictionary<int, int>());
        }

        public void Update(int buur, int destination, int distance)
        {
            if (!ndis.ContainsKey(buur))
                ndis.Add(buur, new Dictionary<int, int>());
            if (!ndis[buur].ContainsKey(destination))
                ndis[buur].Add(destination, distance);
            else
            {
                lock (ndis[buur])
                    ndis[buur][destination] = distance;
            }
        }
    }
}
