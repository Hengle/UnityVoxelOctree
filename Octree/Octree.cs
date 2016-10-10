using UnityEngine;
using System.Collections;

namespace PolyOctree {
	
	public class Octree<T> where T : class {

		public int size;
		public int minNodeSize;
		public int maxObjectsPerNode;
		public Vector3 position;

		public OctreeNode<T> root;

		public Octree(int size, Vector3 position, int minNodeSize, int maxObjectsPerNode){
			this.size = size;
			this.position = position;
			this.minNodeSize = minNodeSize;
			this.root = new OctreeNode<T> (size, position, minNodeSize, maxObjectsPerNode, true);
		}

		public bool Add(Vector3 objPosition, T obj){
			return root.Add (objPosition, obj);
		}

		public bool Remove(Vector3 objPosition){
			return root.Remove (objPosition);		
		}
	}	
}