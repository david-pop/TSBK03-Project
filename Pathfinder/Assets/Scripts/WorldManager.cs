using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour {
	public static WorldManager Instance = null;

	public Material obstacleMaterial;

	public int GridSize = 100;
	public int CellSize = 1;
	public int CellDensity = 3;

	private int ChunkSize = 8;
	private Dictionary<Vector3, List<Matrix4x4>> terrainChunks;
	public int[,] worldGrid;
	private int[,] groupGrid;

	public Mesh wallMesh;
	public Material wallMaterial;

	private int DebugChunkSize = 8;
	public GameObject debugShape;
	public Mesh debugMesh;
	public Material debugMaterial;
	private GameObject[,] debugShapes;
	private Dictionary<Vector3, List<Matrix4x4>> debugChunks;
	private Vector4[] debugColors;
	private int debugMode;


	private MaterialPropertyBlock debugPropertyBlock;



	// Use this for initialization
	void Start () {
		if (Instance == null) {
			Instance = this;
		} else if (Instance != this) {
			Destroy(gameObject);
		}

		CreateGroundPlane();

		Debug.Log("Generating terrain...");
		CreateTerrain();
		Debug.Log("Done!");

		Debug.Log("Separating terrain...");
		CreateTerrainGroups();
		Debug.Log("Done!");

		CreateDebugShapes();

		FlowField.InitUnitField();
		FlowField.InitWallCostField();

		debugMode = 0;
	}


	private void CreateGroundPlane() {
		GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
		plane.name = "GroundPlane";

		float scaleFactor = GridSize / 10;
		float translation = GridSize / 2;

		plane.transform.localScale = new Vector3(scaleFactor, 1, scaleFactor);
		plane.transform.position = new Vector3(translation, 0, translation);
	}

	private void CreateTerrain() {
		worldGrid = new int[GridSize, GridSize];
		terrainChunks = new Dictionary<Vector3, List<Matrix4x4>>();

		float seedX = Random.Range(0.0f, 100000.0f);
		float seedZ = Random.Range(0.0f, 100000.0f);
		int borderSize = 10;

		for (int chunkX = -borderSize; chunkX <= GridSize + borderSize; chunkX += ChunkSize) {
			for (int chunkZ = -borderSize; chunkZ <= GridSize + borderSize; chunkZ += ChunkSize) {
				Vector3 chunkPos = new Vector3(chunkX, 0, chunkZ);
				terrainChunks[chunkPos] = new List<Matrix4x4>();

				for (int x = chunkX; x < chunkX+ChunkSize; x++) {
					for (int z = chunkZ; z < chunkZ+ChunkSize; z++) {
						float p = Mathf.PerlinNoise( seedX + (float)x/10.0f, seedZ + (float)z/10.0f );

						// Height around borders
						p += Mathf.Max(
							Mathf.Max(0, Mathf.Abs(x - GridSize/2) - GridSize/2 + 4),
							Mathf.Max(0, Mathf.Abs(z - GridSize/2) - GridSize/2 + 4)
						) / 6.0f;

						p = Mathf.Atan((p-0.5f)*2.0f)/2.0f+0.5f;

						float limit = 0.5f;
						if ( p > limit ) {
							Matrix4x4 wall = CreateWallTransform( x, z, p-limit );
							terrainChunks[chunkPos].Add( wall );
						}
					}
				}
			}
		}
	}

	private Matrix4x4 CreateWallTransform(int x, int z, float height){
		if (x >= 0 && x < GridSize && z >= 0 && z < GridSize) {
			worldGrid[x, z] = 1;
		}

		Matrix4x4 matrix = new Matrix4x4();
		matrix.SetTRS(
			new Vector3( x + 0.5f, 0, z + 0.5f ), // Position
			Quaternion.Euler( Vector3.zero ), // Rotation
			new Vector3( 1.0f, 1 + (int)(height*8), 1.0f ) // Scale
		);

		return matrix;
	}

	// Separate open terrain into groups, making it easier to see which parts of the map are connected.
	private void CreateTerrainGroups() {
		groupGrid = new int[GridSize, GridSize];
		for (int x = 0; x < GridSize; x++) {
			for (int z = 0; z < GridSize; z++) {
				groupGrid[x, z] = 0;
			}
		}

		int uniqueGroupId = 1;
		for (int x = 0; x < GridSize; x++) {
			for (int z = 0; z < GridSize; z++) {
				if (IsAccessible(x, z) && groupGrid[x, z] == 0) {
					ExpandGroup(x, z, uniqueGroupId++);
				}
			}
		}
	}

	private void ExpandGroup(int x, int z, int id) {
		if (IsAccessible(x, z) && groupGrid[x, z] == 0) {
			groupGrid[x, z] = id;
			ExpandGroup(x+1, z+0, id);
			ExpandGroup(x-1, z+0, id);
			ExpandGroup(x+0, z+1, id);
			ExpandGroup(x+0, z-1, id);
		}
	}

	public bool AreConnected(int x1, int z1, int x2, int z2) {
		if (x1 < 0 || x1 >= GridSize ||
			z1 < 0 || z1 >= GridSize ||
			x2 < 0 || x2 >= GridSize ||
			z2 < 0 || z2 >= GridSize) {
			return false;
		}

		return	groupGrid[x1, z1] != 0 &&
				groupGrid[x2, z2] != 0 &&
				groupGrid[x1, z1] == groupGrid[x2, z2];
	}


	void Update() {
		Vector3 camPos = Camera.main.transform.position;

		foreach (KeyValuePair<Vector3,List<Matrix4x4>> chunkPair in terrainChunks) {
			Vector3 chunkPos = chunkPair.Key;
			//Vector3 point = Camera.main.WorldToViewportPoint( pos );
			//var bounds = Utils.GetViewportBounds( Camera.main, pos, pos + new Vector3(32, 0, 32) );
			//if ( bounds.Contains( Camera.main.WorldToViewportPoint( pos ) ) ) {
			//}

			if (chunkPos.x < camPos.x + 30 &&
				chunkPos.x + ChunkSize > camPos.x - 30 &&
				chunkPos.z < camPos.z + 55-15 &&
				chunkPos.z + ChunkSize > camPos.z - 15+10) {
				Graphics.DrawMeshInstanced(wallMesh, 0, wallMaterial, chunkPair.Value);
			}
		}

		if(debugMode != 0){
			foreach (KeyValuePair<Vector3, List<Matrix4x4>> chunkPair in debugChunks)
			{
				Vector3 chunkPos = chunkPair.Key;

				if (chunkPos.x < camPos.x + 30 &&
					chunkPos.x + DebugChunkSize > camPos.x - 30 &&
					chunkPos.z < camPos.z + 55 - 15 &&
					chunkPos.z + DebugChunkSize > camPos.z - 15 + 10)
				{
					if (FlowField.activeFlowField != null)
					{
						for (int i = 0; i < chunkPair.Value.Count; i++)
						{
							Matrix4x4 m = chunkPair.Value[i];
							Vector3 pos = m.GetColumn(3);
							float px = pos.x * CellDensity;
							float pz = pos.z * CellDensity;
							float cost = 1.0f;

							if(debugMode == 1){
								cost = 1 + FlowField.activeFlowField.GetCost(px, pz);
							}else if(debugMode == 2){
								cost = 1 + FlowField.activeFlowField.GetCost(px, pz) - FlowField.activeFlowField.GetCost(px, pz, false);
							}else if(debugMode == 3){
								cost = FlowField.activeFlowField.GetWallCost(px, pz) * 5 + 0.1f;
							}

							if (cost < float.MaxValue)
							{
								m.SetTRS(
									pos,
									Quaternion.Euler(Vector3.zero),
									new Vector3(0.5f / CellDensity, cost / 5.0f, 0.5f / CellDensity)
								);
								chunkPair.Value[i] = m;
								debugColors[i] = Color.HSVToRGB((cost / 50.0f) % 1, 1, 1);
							}

						}
						debugPropertyBlock.SetVectorArray("_Color", debugColors);
					}

					Graphics.DrawMeshInstanced(mesh: debugMesh, submeshIndex: 0,
											   material: debugMaterial, matrices: chunkPair.Value,
											   properties: debugPropertyBlock,
											   castShadows: UnityEngine.Rendering.ShadowCastingMode.Off,
											   receiveShadows: false);
				}
			}
		}

		if(Input.GetKeyDown("z")){
			debugMode = 0;
		}

		if (Input.GetKeyDown("x"))
		{
			debugMode = 1;
		}

		if (Input.GetKeyDown("c"))
		{
			debugMode = 2;
		}

		if (Input.GetKeyDown("v"))
		{
			debugMode = 3;
		}

	}

	public bool IsAccessible(int x, int z) {
		return (x >= 0 && x < GridSize && z >= 0 && z < GridSize) && (this.worldGrid[x, z] == 0);
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
				//Vector3 dir = ff.GetDirection( px, pz );
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

	private void CreateDebugShapes() {
		debugChunks = new Dictionary<Vector3, List<Matrix4x4>>();
		debugPropertyBlock = new MaterialPropertyBlock();
		debugColors = new Vector4[DebugChunkSize * DebugChunkSize];

		for (int chunkX = 0; chunkX <= GridSize * CellDensity; chunkX += DebugChunkSize)
		{
			for (int chunkZ = 0; chunkZ <= GridSize * CellDensity; chunkZ += DebugChunkSize)
			{
				Vector3 chunkPos = new Vector3((float)chunkX / CellDensity , 0, (float)chunkZ / CellDensity);
				debugChunks[chunkPos] = new List<Matrix4x4>();

				for (int x = chunkX; x < chunkX + DebugChunkSize; x++){
					for (int z = chunkZ; z < chunkZ + DebugChunkSize; z++){
						float px = (x + 0.5f) / CellDensity;
						float pz = (z + 0.5f) / CellDensity;
						Matrix4x4 shape = new Matrix4x4();
						shape.SetTRS(
							new Vector3(px, 0, pz),
							Quaternion.Euler(Vector3.zero),
							new Vector3(0.5f / CellDensity, 0.1f, 0.5f / CellDensity)
						);


						debugChunks[chunkPos].Add(shape);
					}
				}
			}

		}
	}
}
