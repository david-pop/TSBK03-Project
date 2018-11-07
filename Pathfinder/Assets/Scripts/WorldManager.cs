using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour {
	public static WorldManager Instance = null;

	public Material obstacleMaterial;

	public int GridSize = 100;
	public int CellSize = 1;

	private int[,] worldGrid;

	// Use this for initialization
	void Start () {
		if(Instance == null){
			Instance = this;
		}else if(Instance != this){
			Destroy(gameObject);
		}

		int seed = Random.Range(0, 10000);

		worldGrid = new int[GridSize, GridSize];
		int borderSize = 10;

		for (int x = -borderSize; x <= GridSize + borderSize; x += 1) {
			for (int z = -borderSize; z <= GridSize + borderSize; z += 1) {
				float p = Mathf.PerlinNoise( seed + (float)x/10.0f, seed + (float)z/10.0f );

				// Height around borders
				p += Mathf.Max(
					Mathf.Max(0, Mathf.Abs(x - GridSize/2) - GridSize/2 + 4),
					Mathf.Max(0, Mathf.Abs(z - GridSize/2) - GridSize/2 + 4)
				) / 8.0f;

				p = Mathf.Atan((p-0.5f)*2.0f)/2.0f+0.5f;

				float limit = 0.5f;
				if ( p > limit ) {
					addSquareObstacleOfSizeAtPosition( 1, x, z, p-limit );
				}
			}
		}
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
}
