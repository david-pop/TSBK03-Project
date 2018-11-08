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
	public const float SQRT_2 = 1.41421356f;

	private float[,] integratorField;
	private int width, height;
	private int[,] obstacleField;
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

		for (int x = 0; x < width; x++) {
			for (int z = 0; z < height; z++) {
				integratorField[x, z] = float.MaxValue;
			}
		}

		this.obstacleField = obstacleField;

		searchQueue = new Queue<Node>();
		searchQueue.Enqueue( new Node(goalX, goalZ, 0.0f) );

		while (searchQueue.Count > 0) {
			Node node = searchQueue.Dequeue();
			integrate(node.x, node.z, node.cost);
		}
	}

	private void integrate(int x, int z, float cost) {
		//Debug.Log("(" + x + ", " + z + ")" + " " + cost);

		if (!isInside(x, z)) {
			return;
		}

		if (integratorField[x, z] != float.MaxValue) {
			return;
		}

		if (obstacleField[x, z] != 0) {
			return;
		}

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
		return this.integratorField[x, z] != float.MaxValue;
	}

	public float getCost(int x, int z) {
		return this.integratorField[x, z];
	}

	public Vector3 getDirection(int x, int z) {
		if (isInside(x, z) && isAccessible(x, z))
		{
			float left = ((x - 1) < 0 || obstacleField[x - 1, z] != 0) ? 
				integratorField[x, z] : integratorField[x - 1, z];

			float right = ((x + 1) >= width || obstacleField[x + 1, z] != 0) ?
				integratorField[x, z] : integratorField[x + 1, z];

			float bottom = ((z - 1) < 0 || obstacleField[x, z - 1] != 0) ?
				integratorField[x, z] : integratorField[x, z - 1];

			float top = ((z + 1) >= height || obstacleField[x, z + 1] != 0) ?
				integratorField[x, z] : integratorField[x, z + 1];

			return new Vector3(left - right, 0, bottom - top).normalized;
		}

		return Vector3.zero;
	}

	public Vector3 getDirection(Vector3 pos) {
		return getDirection(
			Mathf.FloorToInt(pos.x / cellSize), Mathf.FloorToInt(pos.z / cellSize)
		);
	}

	private bool isInside(int x, int z) {
		return (x >= 0 && x < width && z >= 0 && z < height);
	}
}
