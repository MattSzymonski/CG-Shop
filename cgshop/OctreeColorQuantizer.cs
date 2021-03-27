using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Media;


// Add pixels as last nodes (leaves) at lowest level (8) by their bits of colors
// We can add until number of leaves is less than max number
// If it is greather then we need to trim node (not leaf), possibly on lowest level and the one that has largest or smallest number of children or random node
// Then we add together all r g b values from all its leaves and sum number of children in them and set that node removing all these children, and mark this node as trimmed (leaf)
// Sometimes there is only one leaf in trimmed node to we in fact remove it but create another leaf from that node. This means that this procedure has to be repeated until there are less leaf than max color
// Then we can add more pixels

// Finally after last pixel has been added, palette will be all leaves

namespace cgshop
{
    class OctreeNode
    {
        private static readonly Byte[] bitMask = new Byte[] { 0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01 };

        public OctreeNode parentNode;
        public int treeLevel;
        public OctreeNode[] children;


        public int red;
        public int green;
        public int blue;
        public int pixelCount;

        public bool trimmed; // true if this node had children but they was removed by reducting procedure

        public OctreeColorQuantizer octreeColorQuantizer;


        public OctreeNode(OctreeNode parentNode, int treeLevel, OctreeColorQuantizer octreeColorQuantizer)
        {
            this.parentNode = parentNode;
            this.treeLevel = treeLevel;
            this.octreeColorQuantizer = octreeColorQuantizer;

            children = new OctreeNode[8];
            octreeColorQuantizer.leafCount++;

            octreeColorQuantizer.treeLevels[treeLevel].Add(this); // Register this node in list of nodes for each level in octreeColorQuantizer
            //if (treeLevel < 7)
            //{
                
            //}

        }

        int GetLeafIndexAtLevel(int b, int g, int r, int level)
        {
            // Binary add bits for provided level
            return ((b & bitMask[level]) == bitMask[level] ? 4 : 0) | ((g & bitMask[level]) == bitMask[level] ? 2 : 0) | ((r & bitMask[level]) == bitMask[level] ? 1 : 0);
        }

        public void AddPixelColor(int b, int g, int r) // level - depth level
        {
            // if this node is a leaf or was trimmed, then increase a color amount
            if (treeLevel == 8 || trimmed)
            {
                blue += b;
                red += r;
                green += g;
                
                pixelCount++;
            }
            else if (treeLevel < 8) // otherwise goes one level deeper
            {
                // calculates an index for the next leaf
                int leafIndex = GetLeafIndexAtLevel(b, g, r, treeLevel);  

                // Create leaf if it doesnt exist
                if (children[leafIndex] == null)
                {
                    children[leafIndex] = new OctreeNode(this, treeLevel + 1, octreeColorQuantizer);
                    
                }
                    

                // Add pixel color to that leaf
                children[leafIndex].AddPixelColor(b, g, r);
            }
        }

        public void Trim() // Get data from leaves and delete them
        {
            for (int i = 0; i < children.Length; i++)
            {
                if(children[i] != null)
                {
                    red += children[i].red;
                    green += children[i].green;
                    blue += children[i].blue;
                    pixelCount += children[i].pixelCount;

                    octreeColorQuantizer.treeLevels[treeLevel + 1].Remove(children[i]);
                    children[i] = null;
                    octreeColorQuantizer.leafCount--;
                }    
            }

            octreeColorQuantizer.leafCount++;
            trimmed = true;
        }

        public int GetChildrenCount()
        {
            int result = 0;
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i] != null)
                {
                    result++;
                }
            }
            return result;
        }

        public (int b, int g, int r) GetNearestColor(int b, int g, int r)
        {
            if (children[GetLeafIndexAtLevel(b, g, r, treeLevel)] != null)
            {
                return GetNearestColor(b, g, r); // Child exists so check for final color in it
            }
            else
            {
                return (b, g, r); // No child so this node is final color
            }
        }
    }



    class OctreeColorQuantizer
    {
        OctreeNode root;
        int maxColors;
        public int leafCount = 0;

        public List<OctreeNode>[] treeLevels = new List<OctreeNode>[8];

        public OctreeColorQuantizer(int maxColors)
        {
            this.maxColors = maxColors;
            root = new OctreeNode(null, 0, this);
        }

        public void AddPixelColor(int b, int g, int r)
        {
            if (leafCount < maxColors)
            {
                root.AddPixelColor(b, g, r);
            }
            else
            {
                TrimLeaves();
            }  
        }

        public void TrimLeaves()
        {
            for (int l = treeLevels.Length - 2; l >= 0; l--) // For each level (without lowest level)
            {
                for (int n = 0; n < treeLevels[l].Count; n++) // For each node
                {
                    OctreeNode selectedNode = treeLevels[l][n];
                    if (selectedNode.trimmed == false && selectedNode.GetChildrenCount() > 0)
                    {
                        selectedNode.Trim();
                        if (leafCount < maxColors) // It may happen that node has only one leaf so if we delete it then node itself becomes a leaf
                        {
                            return;
                        }
                    }
                }
            }

            throw new Exception("Cannot reduce color palette, not enough leaves");
        }

        public (int b, int g, int r) GetNearestPaletteColor(int b, int g, int r)
        {
            return root.GetNearestColor(b, g, r);
        }
    }
}
