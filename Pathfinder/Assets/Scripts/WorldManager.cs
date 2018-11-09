using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour {
	public static WorldManager Instance = null;

	public Material obstacleMaterial;

	public int GridSize = 100;
	public int CellSize = 1;

	public int[,] worldGrid;

	public GameObject arrow;

	// Use this for initialization
	void Start () {
		if (Instance == null) {
			Instance = this;
		} else if (Instance != this) {
			Destroy(gameObject);
		}

		CreateGroundPlane();

		float seed = Random.Range(0.0f, 10000.0f);

		worldGrid = new int[GridSize, GridSize];
		int borderSize = 10;

		for (int x = -borderSize; x <= GridSize + borderSize; x += 1) {
			for (int z = -borderSize; z <= GridSize + borderSize; z += 1) {
				float p = Mathf.PerlinNoise( seed + (float)x/10.0f, seed + (float)z/10.0f );

				// Height around borders
				p += Mathf.Max(
					Mathf.Max(0, Mathf.Abs(x - GridSize/2) - GridSize/2 + 4),
					Mathf.Max(0, Mathf.Abs(z - GridSize/2) - GridSize/2 + 4)
				) / 6.0f;

				p = Mathf.Atan((p-0.5f)*2.0f)/2.0f+0.5f;

				float limit = 0.5f;
				if ( p > limit ) {
					addSquareObstacleOfSizeAtPosition( 1, x, z, p-limit );
				}
			}
		}

		//Vector3 goal = GetRandomAccessible();
		//FlowField ff = new FlowField( this.worldGrid, 1, goal.x, goal.z );
		//DebugFlowfield(ff);
	}

	// Update is called once per frame
	void Update () {
	}

	private void addSquareObstacleOfSizeAtPosition(int size, int gridX, int gridZ, float height){
		for (int x = gridX; x < gridX + size; x++) {
			for (int z = gridZ; z < gridZ + size; z++) {
				if (x >= 0 && x < GridSize && z >= 0 && z < GridSize) {
					worldGrid[x, z] = 1;
				}

				GameObject newCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
				newCube.transform.position = new Vector3(
					x * CellSize + CellSize / 2.0f,
					0,
					z * CellSize + CellSize / 2.0f
				);
				newCube.transform.localScale = new Vector3(1.0f, 1+(int)(height*8), 1.0f);
				newCube.GetComponent<MeshRenderer>().material = obstacleMaterial;
			}
		}
	}

	private void CreateGroundPlane() {
		GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
		plane.name = "GroundPlane";

		float scaleFactor = GridSize / 10;
		float translation = GridSize / 2;

		plane.transform.localScale = new Vector3(scaleFactor, 1, scaleFactor);
		plane.transform.position = new Vector3(translation, 0, translation);
	}

	public bool IsAccessible(int x, int z) {
		return (this.worldGrid[x, z] == 0);
	}

	public Vector3 GetRandomAccessible() {
		for (int tries=0; tries<10000; tries++) {
			int x = Random.Range(0, this.GridSize);
			int z = Random.Range(0, this.GridSize);
			if ( IsAccessible(x, z) ) {
				return new Vector3(x, 0, z);
			}
		}
		return Vector3.zero;
	}


	public void DebugFlowfield(FlowField ff) {
		int density = 3;

		for (float x = 0; x <= this.GridSize; x+=1.0f/density) {
			for (float z = 0; z <= this.GridSize; z+=1.0f/density) {
				float offset = 0.5f / density;
				float px = x + offset;
				float pz = z + offset;
				Vector3 dir = ff.GetDirection( px, pz );
				float cost = ff.GetCost( px, pz );

				if (cost < float.MaxValue) {

					/*
					GameObject newArrow = Instantiate(arrow);
					newArrow.transform.position = new Vector3( px, cost/50.0f, pz );

					if (dir.sqrMagnitude > 0) {
						newArrow.transform.forward = dir;
					}
					Vector3 s = newArrow.transform.localScale;
					if (dir.magnitude == Mathf.Infinity)
						dir.Set(0,1,0);
					s.z = 0.1f + dir.magnitude/6.0f;
					s *= 1.0f / density;
					newArrow.transform.localScale = s;
					Renderer[] cs = newArrow.GetComponentsInChildren<Renderer>();
					foreach (Renderer c in cs) {
						float v = ff.IsAccessible( (int)px, (int)pz ) ? 1.0f : 0.3f;
						c.material.color = Color.HSVToRGB( (cost/100.0f)%1, 1, v );
					}
					*/

					GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
					obj.transform.position = new Vector3( px, 0, pz );
					obj.transform.localScale = new Vector3( 0.5f/density, cost/50.0f, 0.5f/density );
					float v = ff.IsAccessible( (int)px, (int)pz ) ? 1.0f : 0.3f;
					obj.GetComponent<Renderer>().material.color = Color.HSVToRGB( (cost/10.0f)%1, 1, v );
				}
			}
		}
	}
}
