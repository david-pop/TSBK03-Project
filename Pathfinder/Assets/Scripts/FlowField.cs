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

	public static int  width, height;
	private int[,] obstacleField;
	private float[,] integratorField;
	public static float[,] unitField;
	private bool[,] visitedField;

	private Queue<Node> searchQueue;

	private static int valuesPerCell = 3;


	public FlowField(int[,] obstacleField, int cellSize, float goalX, float goalZ):
	this(obstacleField, cellSize,
		 Mathf.FloorToInt(goalX / cellSize * valuesPerCell),
		 Mathf.FloorToInt(goalZ / cellSize * valuesPerCell)){
	}

	private FlowField(int[,] obstacleField, int cellSize, int goalX, int goalZ) {
		width = obstacleField.GetLength(0) * valuesPerCell;
		height = obstacleField.GetLength(1) * valuesPerCell;

		if (unitField == null)
		{
			unitField = new float[width, height];
		}

		this.obstacleField = obstacleField;
		this.integratorField = new float[width, height];
		this.visitedField = new bool[width, height];

		for (int x = 0; x < width; x++) {
			for (int z = 0; z < height; z++) {
				this.integratorField[x, z] = float.MaxValue;
				this.visitedField[x, z] = false;
			}
		}

		Generate(
			(int)(goalX * valuesPerCell),
			(int)(goalZ * valuesPerCell)
		);

		//integratorField[goalX, goalZ] = -5.0f;

		for (int x = 0; x < width; x++) {
			for (int z = 0; z < height; z++) {
				WorldManager.Instance.UpdateDebugShape(x, z, this);
			}
		}
	}

	public static void InitUnitField(){
		int width = WorldManager.Instance.GridSize * valuesPerCell;
		int height = WorldManager.Instance.GridSize * valuesPerCell;
		unitField = new float[width, height];
	}

	private void Generate(int goalX, int goalZ) {
		searchQueue = new Queue<Node>();
		searchQueue.Enqueue( new Node(goalX, goalZ, 0.0f) );

		while (searchQueue.Count > 0) {
			Node node = searchQueue.Dequeue();
			Integrate(node.x, node.z, node.cost);
		}
	}

	private void Integrate(int x, int z, float cost) {
		if (!IsInside(x, z))
			return;

		if (visitedField[x, z])
			return;

		if (obstacleField[(int)(x / valuesPerCell), (int)(z / valuesPerCell)] != 0)
			return;

		visitedField[x, z] = true;
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

	public bool IsAccessible(int x, int z) {
		return IsInside(x, z) && this.visitedField[x, z];
	}

	public bool IsAccessible(float x, float z) {
		return IsAccessible((int)x, (int)z);
	}

	private float Get(int x, int z, bool withUnits=true) {
		if (withUnits)
			return this.integratorField[x, z] + unitField[x, z];
		else
			return this.integratorField[x, z];
	}

	public float GetCost(float fx, float fz, bool withUnits=true) {
		fx -= 0.5f;
		fz -= 0.5f;
		int ix = Mathf.FloorToInt(fx);
		int iz = Mathf.FloorToInt(fz);

		float lowestCost = float.MaxValue;
		for (int dx = ix; dx <= ix+1; dx++)
			for (int dz = iz; dz <= iz+1; dz++)
				if (IsAccessible(dx, dz))
					lowestCost = Mathf.Min( lowestCost, Get(dx, dz, withUnits) );

		float obstacleCost = lowestCost + 4.0f;
		float A = IsAccessible(ix+0, iz+0) ? Get(ix+0, iz+0, withUnits) : obstacleCost;
		float B = IsAccessible(ix+1, iz+0) ? Get(ix+1, iz+0, withUnits) : obstacleCost;
		float C = IsAccessible(ix+1, iz+1) ? Get(ix+1, iz+1, withUnits) : obstacleCost;
		float D = IsAccessible(ix+0, iz+1) ? Get(ix+0, iz+1, withUnits) : obstacleCost;

		float value = Utils.QuadLerp( A, B, C, D, fx - ix, fz - iz );
		return Mathf.Max( value, 0 );
	}

	public Vector3 GetDirection(float x, float z) {
		x *= valuesPerCell;
		z *= valuesPerCell;

		if (IsAccessible(x, z))
		{
			float d = 0.1f;
			float left   = GetCost( x-d, z );
			float right  = GetCost( x+d, z );
			float bottom = GetCost( x, z-d );
			float top    = GetCost( x, z+d );

			return new Vector3(left - right, 0, bottom - top) / d;
		}

		return Vector3.zero;
	}

	public Vector3 GetDirection(Vector3 pos) {
		return GetDirection(pos.x, pos.z);
	}

	private static bool IsInside(int x, int z) {
		return (x >= 0 && x < width && z >= 0 && z < height);
	}

	public static void AddUnit( Vector3 pos, float radius, float factor ) {
		pos *= valuesPerCell;
		radius *= valuesPerCell;
		//float offset = 0.5f;
		//pos.Set( pos.x - offset, 0, pos.z - offset );

		int ix = Mathf.RoundToInt( pos.x );
		int iz = Mathf.RoundToInt( pos.z );

		int iRad = Mathf.CeilToInt( radius );

		for (int dx = ix-iRad; dx <= ix+iRad; dx++) {
			for (int dz = iz-iRad; dz <= iz+iRad; dz++) {
				if(IsInside(dx, dz)){
					Vector3 p = new Vector3(dx, 0, dz);
					float d = Vector3.Distance(pos, p) / radius;
					//Debug.Log(radius + " (" + dx + ", " + dz + ") " + d);

					//float value = Mathf.Sin(12.6f * d) / (12.6f * d);
					float value = Mathf.Exp(-(d * d) / 0.1f);
					//float value = Mathf.Exp( -5*d );

					unitField[dx, dz] += factor * value;
					//this.UpdateDebugCylinder(dx, dz);
				}
			}
		}
	}

	public static void RemoveUnit(Vector3 pos, float radius, float factor)
	{
		FlowField.AddUnit(pos, radius, -factor);
	}
}