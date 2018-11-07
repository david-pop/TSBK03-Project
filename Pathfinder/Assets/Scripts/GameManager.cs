using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	public float groundPlaneSize = 100.0f;

	// Use this for initialization
	void Start () {
		createGroundPlane();
	}

	// Update is called once per frame
	void Update () {

	}

	private void createGroundPlane(){
		GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
		plane.name = "GroundPlane";

		float scaleFactor = groundPlaneSize / 10;
		float translation = groundPlaneSize / 2;

		plane.transform.localScale = new Vector3(scaleFactor, 1, scaleFactor);
		plane.transform.position = new Vector3(translation, 0, translation);
	}
}
