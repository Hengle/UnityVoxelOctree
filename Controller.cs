using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using PolyOctree;

public class Controller : MonoBehaviour {

	public int size = 16;
	public int minNodeSize = 2;
	public int maxObjectsPerNode = 2;
	public bool removeObjects;
	public bool colorObjects;


	Octree<GameObject> octree;
	GizmosGenerator<GameObject> gizmosGenerator;
	Dictionary<Vector3, GameObject> gameObjects;

	void Start () {
		octree = new Octree<GameObject> (size, transform.position, minNodeSize, maxObjectsPerNode);
		gizmosGenerator = new GizmosGenerator<GameObject> (octree);

		int halfSize = size / 2;
		gameObjects = new Dictionary<Vector3, GameObject> ();

		Vector3[] cubePositions = new Vector3[] {
			new Vector3 (6, 7, 6),
			new Vector3 (6, 6, 5),
			new Vector3 (6, 6, 6),
			new Vector3 (6, 6, 7),
			new Vector3 (6, 5, 6),
			new Vector3 (6, 4, 6),
			new Vector3 (6, 2, 6),
			new Vector3 (6, 2, 2),
			new Vector3 (6, -6, 5),
			new Vector3 (-4, 2, -3),
		};

		for (int i = 0; i < cubePositions.Length; i++) {
			GameObject cube = CreateCube (cubePositions[i]);
			octree.Add (cube.transform.position, cube);

			gameObjects.Add (cubePositions[i], cube);
		}

//
//		for (int i = 0; i < 5; i++) {
//			Vector3 randPosition = new Vector3 (Random.Range (5, 7), Random.Range (0, 7), Random.Range (5, 7));
//
//			if (gameObjects.ContainsKey(randPosition)) {
//				continue;
//			}
//
//			GameObject cube = CreateCube (randPosition);
//			octree.Add (cube.transform.position, cube);
//
//			gameObjects.Add (randPosition, cube);
//		}

		print ("done");
	}

	void Update(){
		if (colorObjects) {
			ColorChildNodeObjects (octree.root);
			colorObjects = false;
		}

		if (removeObjects) {
			foreach (KeyValuePair<Vector3, GameObject> entry in gameObjects) {
				octree.Remove (entry.Key);
				Destroy (entry.Value);
			}
			removeObjects = false;
		}
	}

	void OnDrawGizmos(){
		if (gizmosGenerator != null) {
			gizmosGenerator.DrawGizmos ();
		}
	}

	GameObject CreateCube(Vector3 position){
		GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
		cube.transform.position = position;
		cube.tag = "Cube";
		cube.transform.parent = transform;

		return cube;
	}

	void ColorChildNodeObjects(OctreeNode<GameObject> node){
		if (node.childNodes == null) {
			return;
		}
		foreach (OctreeNode<GameObject> childNode in node.childNodes) {
			Color objectsColor = new Color (Random.Range (0, 0.6f), Random.Range (0, 0.6f), Random.Range (0, 0.6f));

			foreach (KeyValuePair<Vector3, GameObject> entry in childNode.objects) {
				GameObject obj = (GameObject)entry.Value;
				obj.GetComponent<Renderer> ().material.color = objectsColor;
			}

			if (childNode.childNodes != null) {
				ColorChildNodeObjects (childNode);
			}
		}
	}

}
