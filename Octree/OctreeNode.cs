using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PolyOctree {
	
	public class OctreeNode<T> where T : class {
		
		public int size;
		public int minNodeSize;
		public int maxObjectsPerNode;
		public Vector3 position;
		public OctreeNode<T>[] childNodes;
		public Dictionary<Vector3, T> objects;

		Bounds bounds;
		bool isRootNode;

		public OctreeNode(int size, Vector3 position, int minNodeSize, int maxObjectsPerNode, bool isRootNode = false){
			this.size = size;
			this.position = position;
			this.minNodeSize = minNodeSize;
			this.maxObjectsPerNode = maxObjectsPerNode;

			this.bounds = new Bounds (position, Vector3.one * size);
			this.objects = new Dictionary<Vector3, T> ();
			this.isRootNode = isRootNode;
		}

		public bool Add(Vector3 objPosition, T obj){
			if (! bounds.Contains (objPosition)) {
				return false;
			}

			if (!isRootNode && childNodes == null && (objects.Count < maxObjectsPerNode || size / 2 < minNodeSize)) {
				objects.Add (objPosition, obj);

				return true;
			}

			if (childNodes == null) {
				InstantiateChildNodes ();
				SplitObjectsByChildren ();
			}

			OctreeNode<T> closestChild = FindClosestChild (objPosition, this);
			closestChild.Add (objPosition, obj);

			return true;
		}

		public bool Remove(Vector3 objPosition){
			OctreeNode<T> objectsNode = null;
			if (!TryGetObjectsNode (objPosition, this, out objectsNode)) {
				return false;
			}

			objectsNode.objects.Remove (objPosition);

			if (objectsNode.childNodes != null) {
				TryMergeChildNodes (objectsNode);
			}

			return true;
		}

		private void InstantiateChildNodes(){
			int childSize = size / 2;
			int nodeQuarterSize = size / 4;

			childNodes = new OctreeNode<T>[8];
			childNodes [0] = new OctreeNode<T> (childSize, position + new Vector3(-nodeQuarterSize, nodeQuarterSize, -nodeQuarterSize), minNodeSize, maxObjectsPerNode);
			childNodes [1] = new OctreeNode<T> (childSize, position + new Vector3(nodeQuarterSize, nodeQuarterSize, -nodeQuarterSize), minNodeSize, maxObjectsPerNode);
			childNodes [2] = new OctreeNode<T> (childSize, position + new Vector3(-nodeQuarterSize, nodeQuarterSize, nodeQuarterSize), minNodeSize, maxObjectsPerNode);
			childNodes [3] = new OctreeNode<T> (childSize, position + new Vector3(nodeQuarterSize, nodeQuarterSize, nodeQuarterSize), minNodeSize, maxObjectsPerNode);
			childNodes [4] = new OctreeNode<T> (childSize, position + new Vector3(-nodeQuarterSize, -nodeQuarterSize, -nodeQuarterSize), minNodeSize, maxObjectsPerNode);
			childNodes [5] = new OctreeNode<T> (childSize, position + new Vector3(nodeQuarterSize, -nodeQuarterSize, -nodeQuarterSize), minNodeSize, maxObjectsPerNode);
			childNodes [6] = new OctreeNode<T> (childSize, position + new Vector3(-nodeQuarterSize, -nodeQuarterSize, nodeQuarterSize), minNodeSize, maxObjectsPerNode);
			childNodes [7] = new OctreeNode<T> (childSize, position + new Vector3(nodeQuarterSize, -nodeQuarterSize, nodeQuarterSize), minNodeSize, maxObjectsPerNode);
		}

		private void SplitObjectsByChildren(){

			List<Vector3> keys = new List<Vector3> (objects.Keys);
			foreach (Vector3 objPosition in keys) {
				T obj = null;
				if (!objects.TryGetValue (objPosition, out obj)) {
					continue;
				}

				OctreeNode<T> closestChild = FindClosestChild (objPosition, this);
				closestChild.Add (objPosition, obj);
				objects.Remove (objPosition);
			}
		}

		private OctreeNode<T> FindClosestChild(Vector3 objPosition, OctreeNode<T> node){
			Vector3 nodePosition = node.position;
			OctreeNode<T>[] childNodes = node.childNodes;

			int childIndex = (objPosition.x <= nodePosition.x ? 0 : 1) + (objPosition.y >= nodePosition.y ? 0 : 4) + (objPosition.z <= nodePosition.z ? 0 : 2);

			return childNodes [childIndex];
		}

		private bool TryGetObjectsNode(Vector3 objPosition, OctreeNode<T> node, out OctreeNode<T> objectsNode){

			objectsNode = null;
			OctreeNode<T> closestChild = FindClosestChild (objPosition, node);
			if (closestChild == null) {
				return false;
			}
			if (closestChild.objects.ContainsKey (objPosition)) {
				objectsNode = closestChild;
				return  true;
			}

			if (closestChild.childNodes == null) {
				return false;
			}

			TryGetObjectsNode (objPosition, closestChild, out objectsNode);

			return true;
		}

		private bool TryMergeChildNodes(OctreeNode<T> node){
			int nodeObjectsCount = node.objects.Count;
			if (node.childNodes == null && nodeObjectsCount > node.maxObjectsPerNode) {
				return false;
			}

			if (node.childNodes != null) {
				foreach (OctreeNode<T> childNode in node.childNodes) {

					// If the childNode has children
					// the parent node cannot be merged
					if (childNode.childNodes != null) {
						return false;
					}

					// Add all objects of all childNodes
					// to the parent node Count for checking if they can be merged
					nodeObjectsCount += childNode.objects.Count;

					// If it already has more than the maxObjectsPerNode
					// The merge will not be possible, so the function can end
					if (nodeObjectsCount >= node.maxObjectsPerNode) {
						return false;
					}
				}
			}

			// The merge cannot happend, there are more objects to be
			// added to the parent node than the maxObjectsPerNode permits
			if (nodeObjectsCount > node.maxObjectsPerNode) {
				return false;
			}

			foreach (OctreeNode<T> childNode in node.childNodes) {

				foreach (KeyValuePair<Vector3, T> entry in childNode.objects) {
					node.objects.Add (entry.Key, entry.Value);
				}
			}

			node.childNodes = null;
			
			return true;
		}
	}
}