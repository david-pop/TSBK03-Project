using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Node : IComparable {
	public int x { get; set; }
	public int z { get; set; }
	public float cost { get; set; }
	public float priority { get; set; }

	public int CompareTo(object obj)
	{
		var other = obj as Node;
		if (null == other) return 1; //null is always less
		return this.priority.CompareTo(other.priority);
	}

	private static Node _min;
	private static Node _max;

	public static Node MinValue
	{
		get
		{
			if (_min == null)
			{
				_min = new Node { x = 0, z = 0, cost = 0.0f, priority = float.MinValue };
			}
			return _min;
		}
	}
	
	public static Node MaxValue
	{
		get
		{
			if (_max == null)
			{
				_max = new Node { x = 0, z = 0, cost = 0.0f, priority = float.MaxValue };
			}
			return _max;
		}
	}
}


public class FlowField {
	public const float SQRT_2 = 1.41421356f;

	public static int  width, height;
	private float[,] integratorField;
	public static float[,] unitField;
	public static float[,] wallCostField;
	private bool[,] visitedField;

	private PriorityQueue<Node> searchQueue;

	private int goalX;
	private int goalZ;


	public FlowField(float goalX, float goalZ):
		this(
			Mathf.FloorToInt(goalX / WorldManager.Instance.CellSize * WorldManager.Instance.CellDensity),
			Mathf.FloorToInt(goalZ / WorldManager.Instance.CellSize * WorldManager.Instance.CellDensity)
		){}

	private FlowField(int goalX, int goalZ) {
		this.goalX = goalX;
		this.goalZ = goalZ;

		width = WorldManager.Instance.GridSize * WorldManager.Instance.CellDensity;
		height = WorldManager.Instance.GridSize * WorldManager.Instance.CellDensity;

		if (unitField == null)
		{
			unitField = new float[width, height];
		}

		this.integratorField = new float[width, height];
		this.visitedField = new bool[width, height];

		for (int x = 0; x < width; x++) {
			for (int z = 0; z < height; z++) {
				this.integratorField[x, z] = float.MaxValue;
				this.visitedField[x, z] = false;
			}
		}

		Generate();

		//integratorField[this.goalX, this.goalZ] = -5.0f;

		//for (int x = 0; x < width; x++) {
		//	for (int z = 0; z < height; z++) {	
		//		WorldManager.Instance.UpdateDebugShape(x, z, this);
		//	}
		//}
	}

	public static void InitUnitField(){
		int width = WorldManager.Instance.GridSize * WorldManager.Instance.CellDensity;
		int height = WorldManager.Instance.GridSize * WorldManager.Instance.CellDensity;
		unitField = new float[width, height];
	}

	private void Generate() {
		searchQueue = new PriorityQueue<Node>(PriorityOrder.Min);
		searchQueue.Enqueue(
			new Node {
				x = this.goalX,
				z = this.goalZ,
				cost = 0.0f,
				priority = 0.0f
			},
			Node.MinValue,
			Node.MaxValue
		);

		int i = 0;
		while (searchQueue.Count > 0) {
			i += 1;
			if (i == 1000000)
				break;

			Node node = searchQueue.Dequeue();
			searchQueue.TrimExcess();
			Integrate(node.x, node.z, node.cost);
		}
		Debug.Log(i);
	}

	private void Integrate(int x, int z, float cost) {
		if (!IsInside(x, z))
			return;

		if (visitedField[x,z])
			return;

		if (IsWall(x, z))
			return;

		visitedField[x,z] = true;
		integratorField[x,z] = cost;

		for (int dx = x-1; dx <= x+1; dx++) {
			for (int dz = z-1; dz <= z+1; dz++) {
				if ( !visitedField[dx,dz] && IsInside(x, z) && !IsWall(x, z) ) {
					float d = (dx == x || dz == z) ? 1 : SQRT_2;
					searchQueue.Enqueue( new Node {
							x = dx,
							z = dz,
							cost = cost + d + wallCostField[dx, dz],
							priority = cost + d + wallCostField[dx, dz]
						}, Node.MinValue, Node.MaxValue
					);
				}
			}
		}

		//searchQueue.Sort((a,b)=>b.priority.CompareTo(a.priority));
	}

	public bool IsAccessible(int x, int z) {
		return IsInside(x, z) && this.visitedField[x, z];
	}

	public bool IsAccessible(float x, float z) {
		return IsAccessible((int)x, (int)z);
	}

	private float Get(int x, int z, bool withUnits=true) {
		if (withUnits)
			return this.integratorField[x, z] + unitField[x, z]/* + wallCostField[x,z]*/;
		else
			return this.integratorField[x, z];
	}

	public float GetCost(float fx, float fz, bool withUnits=true) {
		fx -= 0.5f;
		fz -= 0.5f;
		int ix = Mathf.FloorToInt(fx);
		int iz = Mathf.FloorToInt(fz);

		float lowestCost = float.MaxValue;
		float maxCost = float.MinValue;

		for (int dx = ix; dx <= ix+1; dx++)
			for (int dz = iz; dz <= iz+1; dz++)
				if (IsAccessible(dx, dz)){
					lowestCost = Mathf.Min(lowestCost, Get(dx, dz, withUnits));
					maxCost = Mathf.Max(maxCost, Get(dx, dz, withUnits));
				}


		float obstacleCost = maxCost + 0.0f;
		float A = IsAccessible(ix+0, iz+0) ? Get(ix+0, iz+0, withUnits) : obstacleCost;
		float B = IsAccessible(ix+1, iz+0) ? Get(ix+1, iz+0, withUnits) : obstacleCost;
		float C = IsAccessible(ix+1, iz+1) ? Get(ix+1, iz+1, withUnits) : obstacleCost;
		float D = IsAccessible(ix+0, iz+1) ? Get(ix+0, iz+1, withUnits) : obstacleCost;

		float value = Utils.QuadLerp(A, B, C, D, fx - ix, fz - iz);

		return Mathf.Max( value, 0 );
	}

	public Vector3 GetDirection(float x, float z) {
		x *= WorldManager.Instance.CellDensity;
		z *= WorldManager.Instance.CellDensity;

		//if (IsAccessible(x, z))
		float d = 0.1f;
		float left	= GetCost( x-d, z );
		float right  = GetCost( x+d, z );
		float bottom = GetCost( x, z-d );
		float top	= GetCost( x, z+d );

		return new Vector3(left - right, 0, bottom - top) / d;
	}

	public Vector3 GetDirection(Vector3 pos) {
		return GetDirection(pos.x, pos.z);
	}

	private static bool IsInside(int x, int z) {
		return (x >= 0 && x < width && z >= 0 && z < height);
	}

	private static bool IsWall(int x, int z) {
		return WorldManager.Instance.worldGrid[(int)(x / WorldManager.Instance.CellDensity), (int)(z / WorldManager.Instance.CellDensity)] != 0;
	}

	public static void AddUnit( Vector3 pos, float radius, float factor ) {
		pos *= WorldManager.Instance.CellDensity;
		radius *= WorldManager.Instance.CellDensity;
		float offset = 0.5f;
		pos.Set( pos.x - offset, 0, pos.z - offset );

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

	public static void initWallCostField(){
		int wallCostFieldWidth = WorldManager.Instance.GridSize * WorldManager.Instance.CellDensity;
		int wallCostFieldHeight = WorldManager.Instance.GridSize * WorldManager.Instance.CellDensity;
		int wallCostRadius = 32;
		int wallCostFactor = 5;

		wallCostField = new float[wallCostFieldWidth, wallCostFieldHeight];

		for (int x = 0; x < WorldManager.Instance.GridSize; x++){
			for (int z = 0; z < WorldManager.Instance.GridSize; z++){
				if(WorldManager.Instance.worldGrid[x, z] == 1){
					float centerx = x * WorldManager.Instance.CellDensity + 1;
					float centerz = z * WorldManager.Instance.CellDensity + 1;

					int ix = Mathf.RoundToInt(centerx);
					int iz = Mathf.RoundToInt(centerz);

					int iRad = Mathf.CeilToInt(wallCostRadius);

					for (int dx = ix - iRad; dx <= ix + iRad; dx++)
					{
						for (int dz = iz - iRad; dz <= iz + iRad; dz++)
						{
							if (dx >= 0 && dx < wallCostFieldWidth && dz >= 0 && dz < wallCostFieldHeight)
							{
								Vector3 p = new Vector3(dx, 0, dz);
								Vector3 pos = new Vector3(ix, 0, iz);
								float d = Vector3.Distance(pos, p);
								float value = Mathf.Exp(-(d * d) / 4);
								wallCostField[dx, dz] = Mathf.Max(wallCostField[dx, dz], wallCostFactor * value);
							}
						}
					}
				}
			}
		}
	}
}