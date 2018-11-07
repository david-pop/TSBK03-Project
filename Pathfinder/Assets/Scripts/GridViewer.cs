using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class GridViewer : MonoBehaviour
{
	public int GridSize;
	public Color GridColor;

	void Awake()
	{
		MeshFilter filter = gameObject.GetComponent<MeshFilter>();
		var mesh = new Mesh();
		var verticies = new List<Vector3>();

		var indicies = new List<int>();
		for (int i = 0; i <= this.GridSize; i++)
		{
			verticies.Add(new Vector3(i, 0, 0));
			verticies.Add(new Vector3(i, 0, this.GridSize));

			indicies.Add(4 * i + 0);
			indicies.Add(4 * i + 1);

			verticies.Add(new Vector3(0, 0, i));
			verticies.Add(new Vector3(this.GridSize, 0, i));

			indicies.Add(4 * i + 2);
			indicies.Add(4 * i + 3);
		}

		mesh.vertices = verticies.ToArray(); 
		mesh.SetIndices(indicies.ToArray(), MeshTopology.Lines, 0);
		filter.mesh = mesh;

		MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
		meshRenderer.material = new Material(Shader.Find("Sprites/Default"));
		meshRenderer.material.color = this.GridColor;
	}
}