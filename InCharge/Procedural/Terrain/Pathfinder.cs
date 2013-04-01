using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InCharge.Logic.Character;
using Microsoft.Xna.Framework;
using InCharge.Util;

namespace InCharge.Procedural.Terrain
{
    public static class Pathfinder
    {
        private static Random random = new Random();

        /// <summary>
        /// A node in the path, representing a passable block
        /// </summary>
        public class PathNode
        {
            public BlockPosition Block;
            public PathNode Previous;
            public int fCost = 0;
            public int gCost = 0;
            public int hCost = 0;

            public class Comparer : IComparer<PathNode>
            {
                public int Compare(PathNode x, PathNode y)
                {
                    return x.fCost - y.fCost;
                }
            }            
        }

        public static List<BlockPosition> FindCharacterPathToDestination(Character character, TerrainMap map, BlockPosition destination)
        {
            var path = new List<BlockPosition>();
            var startRegion = map.GetRegionForWorldCoordinates(character.Position);
            var endRegion = map.GetRegionForWorldCoordinates(destination.WorldPosition);            

            if (startRegion != null && endRegion != null)
            {
                var currentPosition = map.GetBlockPositionForWorldCoordinates(character.Position);
                var currentNode = new PathNode()
                {
                    Block = startRegion.AccessibleBlocks[currentPosition.X, currentPosition.Y, currentPosition.Z],
                    Previous = null
                };

                var destNode = new PathNode()
                {
                    Block = endRegion.AccessibleBlocks[destination.X, destination.Y, destination.Z],
                    Previous = null
                };

                Pathfinder.CalculateCosts(currentNode, destNode);

                List<PathNode> closedSet = new List<PathNode>();
                BinaryHeap<PathNode> openSet = new BinaryHeap<PathNode>(new PathNode.Comparer());

                // START
                openSet.Insert(currentNode);

                bool pathFound = false;                
                Region currentRegion = startRegion;
                while (openSet.Count > 0)
                {
                    // set the current node to the first element in the f cost sorted open set
                    currentNode = openSet.RemoveRoot();
                    // switch current node from open set to closed set 
                    closedSet.Add(currentNode);

                    // check if destination was reached (heuristic cost of 0)
                    if (currentNode.hCost == 0)
                    {
                        pathFound = true;
                        break;
                    }

                    // get walkable adjacent nodes
                    var neighbors = Pathfinder.GetPassableNeighborBlocks(currentNode, currentRegion, map, character);
                    // add eligible nodes to open set (must not be in either open or closed set)
                    foreach (var node in neighbors)
                    {
                        // ignore nodes already on the closed set
                        if (closedSet.Exists(n => node.Block.WorldPosition.Equals(n.Block.WorldPosition)))
                        {
                            continue;
                        }

                        // check nodes for open set
                        var openSetNode = openSet.FirstOrDefault(n => node.Block.WorldPosition.Equals(n.Block.WorldPosition));
                        if (openSetNode == null && currentNode.hCost != 0)
                        {
                            node.Previous = currentNode;
                            // calculate h and f costs, g cost was set when finding neighbors
                            Pathfinder.CalculateCosts(node, destNode);
                            //openSet.Add(node);
                            openSet.Insert(node);
                        }
                        else // node already in open set, compare G cost and update path
                        {
                            if (node.gCost < openSetNode.gCost)
                            {
                                // update path
                                openSetNode.Previous = currentNode;
                                // update gCost to lower (more optimal) value
                                openSetNode.gCost = node.gCost;
                                // recalculate h and f costs
                                Pathfinder.CalculateCosts(openSetNode, destNode);
                                // re-order openset!
                                openSet.Insert(openSet.RemoveRoot());
                            }
                        }
                    }                    
                  
                    // sort open set by f cost
                    openSet.OrderBy(node => node.fCost);

                    // may need to update current region:
                    currentRegion = map.GetRegionForWorldCoordinates(currentNode.Block.WorldPosition);
                }

                if (pathFound && closedSet.Count > 0)
                {
                    // retrace path
                    var node = closedSet.Last();
                    do
                    {
                        path.Add(node.Block);
                        node = node.Previous;
                    } while (node != null);
                    path.Reverse();
                }
            }

            return path;
        }

        /// <summary>
        /// Find neighboring blocks which are passable by the given character
        /// </summary>
        /// <param name="map"></param>
        /// <param name="character"></param>
        /// <returns></returns>
        public static IEnumerable<PathNode> GetPassableNeighborBlocksForCharacter(TerrainMap map, Character character)
        {
            var node = new PathNode()
            {
                Block = map.GetBlockPositionForWorldCoordinates(character.Position)
            };
            var region = map.GetRegionForWorldCoordinates(character.Position);
            return Pathfinder.GetPassableNeighborBlocks(node, region, map, character);
        }

        /// <summary>
        /// Find neighboring blocks which are passable by the given character
        /// </summary>
        /// <param name="currentNode"></param>
        /// <param name="currentRegion"></param>
        /// <param name="map"></param>
        /// <param name="character"></param>
        /// <returns></returns>
        private static IEnumerable<PathNode> GetPassableNeighborBlocks(PathNode currentNode, 
            Region currentRegion, TerrainMap map, Character character)
        {
            var blocks = new List<PathNode>();       
            
            for (int x = -1; x < 2; x++)
            {
                for (int z = -1; z < 2; z++)
                {
                    if (x == 0 && z == 0) continue;

                    // each neighboring block might be in another region
                    var region = map.GetRegionForWorldCoordinates(currentNode.Block.WorldPosition + new Vector3(x, 0, z));
                    if (region == null) continue;

                    // get region relative x and z coords
                    var relX = (currentNode.Block.X + x) % Region.RegionWidth;
                    var relZ = (currentNode.Block.Z + z) % Region.RegionLength;
                    if (relX < 0) relX += Region.RegionWidth;
                    if (relZ < 0) relZ += Region.RegionLength;

                    var isSolid = region.GetBlock(relX, currentNode.Block.Y, relZ) != BlockTypes.None;

                    if (isSolid)
                    {
                        // check upwards if the character could jump/climb
                        for (int y = 0; y < character.JumpHeight; y++)
                        {
                            // check block above target (hence +1)
                            if (region.GetBlock(relX, currentNode.Block.Y + y + 1, relZ) == BlockTypes.None)
                            {
                                if (Pathfinder.CheckCharacterFitsOnBlock(region, relX, currentNode.Block.Y, relZ, character))
                                {
                                    blocks.Add(new PathNode()
                                        {
                                            Previous = currentNode,
                                            gCost = currentNode.gCost + Pathfinder.GetMovementCostByOffset(x, y, z),
                                            Block = region.AccessibleBlocks[relX, currentNode.Block.Y + y, relZ],                                            
                                        });
                                    break;
                                }
                            }
                        }
                    }
                    else // neighbor block is free
                    {                     
                        if (!Pathfinder.CheckCharacterFitsOnBlock(region, relX, currentNode.Block.Y, relZ, character))
                            continue;

                        // check downwards, either jump / climb down or go laterally
                        var y = -1;
                        while (region.GetBlock(relX, currentNode.Block.Y + y, relZ) == BlockTypes.None)
                        {
                            if (-y > character.JumpHeight * 2) break; // characters can fall twice as far as they jump
                            y--;
                        }

                        blocks.Add(new PathNode()
                            {
                                Previous = currentNode,
                                gCost = currentNode.gCost + Pathfinder.GetMovementCostByOffset(x, y, z),
                                Block = region.AccessibleBlocks[relX, currentNode.Block.Y + y, relZ]
                            });
                    }
                }
            }

            return blocks;
        }

        private static int GetMovementCostByOffset(int offX, int offY, int offZ)
        {
            var cost = 0;

            var manhattanOffset = Math.Abs(offX) + Math.Abs(offZ);
            cost = manhattanOffset > 1 ? 14 : 10; // diagonal or straight
            cost += offY * 2;

            return cost;
        }

        private static bool CheckCharacterFitsOnBlock(Region region, int x, int y, int z, Character character)
        {
            bool fits = true;
            for (int i = 1; i < character.BoundHeight; i++)
            {
                fits &= region.GetBlock(x, y + i, z) == BlockTypes.None;
            }
            return fits;
        }

        /// <summary>
        /// Calculates costs (heuristic 'H' and total 'F') on a pathnode to the destination node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="dst"></param>
        private static void CalculateCosts(PathNode node, PathNode dst)
        {
            // calculate Manhattan distance
            var hCost = (int)(Math.Abs(dst.Block.WorldPosition.X - node.Block.WorldPosition.X) + 
                Math.Abs(dst.Block.WorldPosition.Y - node.Block.WorldPosition.Y) +
                Math.Abs(dst.Block.WorldPosition.Z - node.Block.WorldPosition.Z)) * 10; // straight movement has a cost of 10
            //var hCost = (int)(Vector3.Distance(node.Block.WorldPosition, dst.Block.WorldPosition) * 10);
            var fCost = node.gCost + hCost;
            node.hCost = hCost;
            node.fCost = fCost;
        }
    }
}
