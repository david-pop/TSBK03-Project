using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct Node {
	public int x;
	public int z;
	public float cost;

	public Node(int x, int z, float cost) {
		this.x = x;
		this.z = z;
		this.cost = cost;
	}
}

public class FlowField {
	public const bool DEBUG = false;
	public const float SQRT_2 = 1.41421356f;

	private int width, height;
	private float[,] integratorField;
	private int[,] obstacleField;
	private bool[,] visited;
	private GameObject[,] debugCylinders;

	private int cellSize;
	private Queue<Node> searchQueue;


	public FlowField(int[,] obstacleField, int cellSize, float goalX, float goalZ):
	this(obstacleField, cellSize,
		 Mathf.FloorToInt(goalX / cellSize),
		 Mathf.FloorToInt(goalZ / cellSize)){
	}

	public FlowField(int[,] obstacleField, int cellSize, int goalX, int goalZ) {
		width = obstacleField.GetLength(0);
		height = obstacleField.GetLength(1);
		this.cellSize = cellSize;

		integratorField = new float[width, height];
		visited = new bool[width, height];
		debugCylinders = new GameObject[width, height];

		for (int x = 0; x < width; x++) {
			for (int z = 0; z < height; z++) {
				integratorField[x, z] = float.MaxValue;
				visited[x, z] = false;

				if (DEBUG) {
					float px = x + 0.5f;
					float pz = z + 0.5f;
					GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
					debugCylinders[x, z] = obj;
					obj.transform.position = new Vector3( px, 0, pz );

					obj.transform.localScale = new Vector3( 0.5f, 0.0f, 0.5f );
					obj.GetComponent<Renderer>().material.color = Color.black;
				}
			}
		}

		this.obstacleField = obstacleField;

		searchQueue = new Queue<Node>();
		searchQueue.Enqueue( new Node(goalX, goalZ, 0.0f) );

		while (searchQueue.Count > 0) {
			Node node = searchQueue.Dequeue();
			integrate(node.x, node.z, node.cost);
		}
		//integratorField[goalX, goalZ] = -5.0f;

		for (int x = 0; x < width; x++) {
			for (int z = 0; z < height; z++) {
				UpdateDebugCylinder(x, z);
			}
		}
	}

	private void integrate(int x, int z, float cost) {
		if (!isInside(x, z)) {
			return;
		}

		if (integratorField[x, z] != float.MaxValue) {
			return;
		}

		if (obstacleField[x, z] != 0) {
			return;
		}

		visited[x, z] = true;
		integratorField[x,z] = cost;

		searchQueue.Enqueue( new Node(x + 1, z, cost + 1) );
		searchQueue.Enqueue( new Node(x - 1, z, cost + 1) );
		searchQueue.Enqueue( new Node(x, z + 1, cost + 1) );
		searchQueue.Enqueue( new Node(x, z - 1, cost + 1) );
		searchQueue.Enqueue( new Node(x + 1, z + 1, cost + SQRT_2) );
		searchQueue.Enqueue( new Node(x - 1, z + 1, cost + SQRT_2) );
		searchQueue.Enqueue( new Node(x - 1, z - 1, cost + SQRT_2) );
		searchQueue.Enqueue( new Node(x + 1, z - 1, cost + SQRT_2) );
	}

	public bool isAccessible(int x, int z) {
		return isInside(x, z) && this.visited[x, z];
	}

	public bool isAccessible(float x, float z) {
		return isAccessible((int)x, (int)z);
	}

	public float getCost(float fx, float fz) {
		fx -= 0.5f;
		fz -= 0.5f;
		int ix = Mathf.FloorToInt(fx);
		int iz = Mathf.FloorToInt(fz);

		float lowestCost = float.MaxValue;
		for (int dx = ix; dx <= ix+1; dx++)
			for (int dz = iz; dz <= iz+1; dz++)
				if (isAccessible(dx, dz))
					lowestCost = Mathf.Min( lowestCost, this.integratorField[dx, dz] );

		float obstacleCost = lowestCost + 10.0f;
		float A = isAccessible(ix+0, iz+0) ? this.integratorField[ix+0, iz+0] : obstacleCost;
		float B = isAccessible(ix+1, iz+0) ? this.integratorField[ix+1, iz+0] : obstacleCost;
		float C = isAccessible(ix+1, iz+1) ? this.integratorField[ix+1, iz+1] : obstacleCost;
		float D = isAccessible(ix+0, iz+1) ? this.integratorField[ix+0, iz+1] : obstacleCost;

		float value = Utils.QuadLerp( A, B, C, D, fx - ix, fz - iz );
		return value;
	}

	public Vector3 getDirection(float x, float z) {
		if (isAccessible(x, z))
		{
			float d = 0.1f;
			float left   = getCost( x-d, z );
			float right  = getCost( x+d, z );
			float bottom = getCost( x, z-d );
			float top    = getCost( x, z+d );

			return new Vector3(left - right, 0, bottom - top).normalized;
		}

		return Vector3.zero;
	}

	public Vector3 getDirection(Vector3 pos) {
		return getDirection(pos.x, pos.z);
	}

	private bool isInside(int x, int z) {
		return (x >= 0 && x < width && z >= 0 && z < height);
	}

	private void UpdateDebugCylinder( int x, int z ) {
		if (DEBUG) {
			GameObject obj = debugCylinders[x, z];
			float px = x + 0.5f;
			float pz = z + 0.5f;
			float cost = getCost( px, pz );
			if (cost < float.MaxValue) {
				obj.transform.localScale = new Vector3( 0.5f, cost/50.0f, 0.5f );
				float v = isAccessible( (int)px, (int)pz ) ? 1.0f : 0.3f;
				obj.GetComponent<Renderer>().material.color = Color.HSVToRGB( (cost/50.0f)%1, 1, v );
			}
		}
	}
}
