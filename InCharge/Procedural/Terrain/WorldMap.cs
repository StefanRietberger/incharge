using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace InCharge.Procedural.Terrain
{
    public class WorldMap
    {
        public struct WorldCell
        {
            public float Elevation;
            public int Type;
        }
        
        const int worldSizeX = 512;
        const int worldSizeY = 512;

        private Random random;

        private WorldCell[,] map;

        public WorldCell[,] Map
        {
            get { return map; }
            set { map = value; }
        }

        public WorldMap(int seed)
        {
            this.random = new Random(seed);
        }

        public void BuildMap()
        {
            map = new WorldCell[worldSizeX, worldSizeY];

            var points = this.GetBasePoints();
            //var voronoiGraph = Fortune.ComputeVoronoiGraph(points);
            /*foreach (var edge in voronoiGraph.Edges)
            {
                
            }*/
        }

        private IEnumerable<Vector2> GetBasePoints()
        {
            List<Vector2> points = new List<Vector2>();
            int numPoints = (int)Math.Sqrt(worldSizeX * worldSizeY);

            for (int i = 0; i < numPoints; i++)
            {
                points.Add(new Vector2((float)(this.random.NextDouble() * worldSizeX), (float)(this.random.NextDouble() * worldSizeY)));
            }

            return points;
        }
    }
}
