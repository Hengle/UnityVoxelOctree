using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PolyOctree {

	public class GizmosGenerator<T> where T : class 
	{
		Octree<T> octree;

		public GizmosGenerator(Octree<T> octree){
			this.octree = octree;
		}

		public void DrawGizmos() {

			// Root Gizmo
			Gizmos.color = Color.cyan;
			Gizmos.DrawWireCube (octree.position, Vector3.one * octree.size + (Vector3.one * 0.2f));

			Gizmos.color = Color.black;

			OctreeNode<T>[] childNodes = octree.root.childNodes;
			DrawChildNodes (childNodes);
		}

		void DrawChildNodes(OctreeNode<T>[] childNodes){

			for (int i = 0; i < 8; i++) {
				OctreeNode<T> newNode = childNodes [i];
				if (newNode != null) {
					DrawNode (newNode);
				}

				OctreeNode<T>[] DeeperChildNodes = newNode.childNodes;
				if (DeeperChildNodes == null) {
					continue;
				}

				DrawChildNodes (DeeperChildNodes);
			}
		}

		void DrawNode(OctreeNode<T> node){
			Gizmos.DrawWireCube (node.position, (Vector3.one * node.size));

			UnityEditor.Handles.color = Color.cyan;


			int nodeObjectsCounts = node.objects.Count;
			if(nodeObjectsCounts > 0){
				UnityEditor.Handles.Label (node.position, node.objects.Count.ToString());
			}
		}
	}
}

