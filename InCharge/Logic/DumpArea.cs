using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InCharge.Procedural.Terrain;

namespace InCharge.Logic
{
    /// <summary>
    /// Stores information about the dump sites of a connected dig order
    /// </summary>
    public class DumpArea
    {
        public struct DumpSite
        {
            public TerrainBlock.IntVector3 Location;
            public TerrainBlock OriginalBlock;
           
            // TODO: let blocks be stored in processing contraptions or storages
        }

        /// <summary>
        /// Digsite reference
        /// </summary>
        private DigArea digArea;

        private Dictionary<TerrainBlock.IntVector3, TerrainBlock> selectedDumpSites;


        public IEnumerable<TerrainBlock.IntVector3> DumpLocations 
        { 
            get 
            {
                return this.selectedDumpSites.Keys;
            } 
        }

        public bool HasChanged { get; set; }

        /// <summary>
        /// Creates a new dump area
        /// </summary>
        /// <param name="digArea"></param>
        public DumpArea(DigArea digArea)
        {
            this.digArea = digArea;
            this.selectedDumpSites = new Dictionary<TerrainBlock.IntVector3,TerrainBlock>();
        }

        public void AddDumpSite(DumpSite dumpsite)
        {
            if (!this.selectedDumpSites.ContainsKey(dumpsite.Location))
            {
                this.selectedDumpSites.Add(dumpsite.Location, dumpsite.OriginalBlock);
            }
        }

        public void RemoveDumpSite(TerrainBlock.IntVector3 location)
        {
            this.selectedDumpSites.Remove(location);
        }
    }
}
