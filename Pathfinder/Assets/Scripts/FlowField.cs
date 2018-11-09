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
	private int[,] obstacleField;
	private float[,] integratorField;
	private float[,] unitField;
	private bool[,] visitedField;
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

		this.obstacleField = obstacleField;
		this.integratorField = new float[width, height];
		this.unitField = new float[width, height];
		this.visitedField = new bool[width, height];
		this.debugCylinders = new GameObject[width, height];

		for (int x = 0; x < width; x++) {
			for (int z = 0; z < height; z++) {
				this.integratorField[x, z] = float.MaxValue;
				this.unitField[x, z] = 0;
				this.visitedField[x, z] = false;

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

		Generate(goalX, goalZ);

		//integratorField[goalX, goalZ] = -5.0f;

		for (int x = 0; x < width; x++) {
			for (int z = 0; z < height; z++) {
				UpdateDebugCylinder(x, z);
			}
		}
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

		if (obstacleField[x, z] != 0)
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
			return this.integratorField[x, z] + this.unitField[x, z];
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

	private bool IsInside(int x, int z) {
		return (x >= 0 && x < width && z >= 0 && z < height);
	}

	public void AddSeparation( Vector3 pos, float radius, float factor ) {
		pos.Set( pos.x - 0.5f, 0, pos.z - 0.5f );
		int ix = Mathf.RoundToInt( pos.x );
		int iz = Mathf.RoundToInt( pos.z );

		int iRad = Mathf.CeilToInt( radius );

		for (int dx = ix-iRad; dx <= ix+iRad; dx++) {
			for (int dz = iz-iRad; dz <= iz+iRad; dz++) {
				if (IsAccessible(dx, dz)) {
					Vector3 p = new Vector3(dx, 0, dz);
					float d = Vector3.Distance( pos, p ) / radius;
					//Debug.Log(radius + " (" + dx + ", " + dz + ") " + d);

					//float value = Mathf.Sin(12.6f * d) / (12.6f * d);
					float value = Mathf.Exp( -(d*d) / 0.1f );
					//float value = Mathf.Exp( -5*d );

					this.unitField[dx, dz] += factor * value;
					this.UpdateDebugCylinder(dx, dz);
				}
			}
		}
	}

	private void UpdateDebugCylinder( int x, int z ) {
		if (DEBUG) {
			GameObject obj = debugCylinders[x, z];
			float px = x + 0.5f;
			float pz = z + 0.5f;
			float cost =  GetCost( px, pz );
			//float cost = 1 + GetCost( px, pz ) - GetCost( px, pz, false );

			if (cost < float.MaxValue) {
				obj.transform.localScale = new Vector3( 0.5f, cost/50.0f, 0.5f );
				float v = IsAccessible( (int)px, (int)pz ) ? 1.0f : 0.3f;
				obj.GetComponent<Renderer>().material.color = Color.HSVToRGB( (cost/50.0f)%1, 1, v );
			}
		}
	}
}
