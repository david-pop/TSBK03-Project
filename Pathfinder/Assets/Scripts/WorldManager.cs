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

        worldGrid = new int[GridSize, GridSize];
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void addSquareObstacleOfSizeAtPosition(int size, int gridX, int gridZ){
        if(gridX + size > worldGrid.GetLength(0) - 1 ||
           gridZ + size > worldGrid.GetLength(1) - 1){
            return;
        }

        for (int x = gridX; x < gridX + size; x++){
            for (int z = gridZ; z < gridZ + size; z++){
                worldGrid[x, z] = 1;
                GameObject newCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                newCube.transform.position = new Vector3(
                    x * CellSize + CellSize / 2.0f, 0, z * CellSize + CellSize / 2.0f);
                newCube.GetComponent<MeshRenderer>().material = obstacleMaterial;
            }
        }
    }
}
