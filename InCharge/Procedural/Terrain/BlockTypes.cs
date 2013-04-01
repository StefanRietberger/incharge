using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InCharge.Procedural.Terrain
{
    public class BlockTypes
    {
        public const byte None = 0x00;

        public const byte BedRock = 0x01;
        public const byte Rock = 0x02;
        public const byte Clay = 0x03;
        public const byte Sand = 0x04;
        public const byte Soil = 0x05;
                
        public const byte Coal = 0x10;
        public const byte Iron = 0x11;
        public const byte Copper = 0x12;
        public const byte Silver = 0x13;
        public const byte Gold = 0x14;
        public const byte Gem = 0x15;

        public const byte Grass = 0xfe;
        public const byte Water = 0xff;


        public readonly static Dictionary<byte, byte> OverlappingPriority = new Dictionary<byte, byte>();

        static BlockTypes()
        {
            OverlappingPriority.Add(None, 0);
            OverlappingPriority.Add(BedRock, 0);
            OverlappingPriority.Add(Rock, 0);
            OverlappingPriority.Add(Clay, 1);
            OverlappingPriority.Add(Sand, 2);
            OverlappingPriority.Add(Soil, 3);

            OverlappingPriority.Add(Coal, 0);
            OverlappingPriority.Add(Iron, 0);
            OverlappingPriority.Add(Copper, 0);
            OverlappingPriority.Add(Silver, 0);
            OverlappingPriority.Add(Gold, 0);
            OverlappingPriority.Add(Gem, 0);

            OverlappingPriority.Add(Grass, 20);
            OverlappingPriority.Add(Water, 0);
        }
    }
}
