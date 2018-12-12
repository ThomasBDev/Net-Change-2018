using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiClientServer
{
    class RoutingTable
    {
        public Dictionary<int, Tuple<int, int>> routingTable = new Dictionary<int, Tuple<int, int>>();

        public void SetRoute(int destination, int prefBuur, int distance)
        {
            if (!routingTable.Keys.Contains(destination))
                routingTable.Add(destination, new Tuple<int, int>(prefBuur, distance));
            else
                lock (routingTable)
                    routingTable[destination] = new Tuple<int, int>(prefBuur, distance);
        }
    }
}
