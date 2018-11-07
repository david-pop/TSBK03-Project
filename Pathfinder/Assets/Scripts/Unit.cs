using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour {
	private float speed = 1.0f;
	private Vector3 goalPosition;


	// Use this for initialization
	void Start () {
		this.goalPosition = this.transform.position;
	}

	// Update is called once per frame
	void Update () {
		if (Random.Range(0.0f, 1.0f) < 0.02) {
			this.goalPosition += new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f));
		}

		Vector3 newPos = Vector3.MoveTowards( this.transform.position, this.goalPosition, this.speed * Time.deltaTime );
		this.transform.position = newPos;
	}

	public void SetGoalPosition(Vector3 goal) {
		this.goalPosition = goal;
	}
}