using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using InCharge.Procedural.Terrain;

namespace InCharge.Logic
{
    /// <summary>
    /// Holds information about an area which is to be dug out
    /// </summary>
    public class DigArea
    {
        public bool HasChanged { get; set; }

        private List<BlockPosition> selectedBlocks;

        public List<BlockPosition> SelectedBlocks
        {
            get { return selectedBlocks; }
            set { selectedBlocks = value; }
        }

        private Dictionary<int, int> selectedMaterial;

        /// <summary>
        /// Offset position of mining area in world space
        /// </summary>
        public Vector3 OffsetPosition { get; set; }

        public DigArea()
        {
            this.selectedBlocks = new List<BlockPosition>();
            this.Initialize();
        }

        protected void Initialize()
        {
            this.selectedMaterial = new Dictionary<int, int>();
            
        }

        public void AddSelectedBlock(BlockPosition blockPos)
        {
            bool hasAdded = false;

            if (!selectedBlocks.Contains(blockPos))
            {
                selectedBlocks.Add(blockPos);
                hasAdded = true;
            }

            if (hasAdded)
            {
                this.HasChanged = true;
                this.CountMaterial();
            }
        }

        public void RemoveSelectedBlock(BlockPosition blockPos)
        {
            bool hasRemoved = false;

            hasRemoved = selectedBlocks.Remove(blockPos);             

            if (hasRemoved)
            {
                this.HasChanged = true;
                this.CountMaterial();
            }
        }

        protected void CountMaterial()
        {
            this.selectedMaterial.Clear();

            foreach (BlockPosition blockPosition in this.selectedBlocks)
            {
                // TODO: change to block pos, get block from regions etc.
                /*int count = 0;
                bool hasValue = this.selectedMaterial.TryGetValue(tb.Type, out count);
                if (hasValue)
                    this.selectedMaterial[tb.Type] = count + 1;
                else
                    this.selectedMaterial.Add(tb.Type, 1);*/
            }
        }

        public Dictionary<int, int> SelectedMaterial
        {
            get { return this.selectedMaterial; }
        }
    }
}
