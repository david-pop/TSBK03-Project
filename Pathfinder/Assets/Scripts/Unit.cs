using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour {
	private float speed = 2.0f;
	private Vector3 goalPosition;
	private Vector3 direction;


	// Use this for initialization
	void Start () {
		Vector3 groundPos = this.transform.position;
		groundPos.y = this.transform.localScale.y;
		this.transform.position = groundPos;

		this.goalPosition = this.transform.position;
		this.direction.Set(1, 0, 0);
	}

	// Update is called once per frame
	void Update () {
		if (this.goalPosition != this.transform.position) {

			Vector3 newPos = Vector3.MoveTowards( this.transform.position, this.goalPosition, this.speed * Time.deltaTime );
			this.transform.position = newPos;
		}
		else if (Random.Range(0.0f, 1.0f) < 0.02) {
			this.goalPosition += new Vector3(Random.Range(-1, 2), 0, Random.Range(-1, 2));
			this.direction = this.goalPosition - this.transform.position;
		}

		this.transform.forward = Vector3.Lerp(
			this.transform.forward,
			this.direction,
			10 * Time.deltaTime
		);
	}

	public void SetGoalPosition(Vector3 goal) {
		goal.y = this.transform.localScale.y;
		this.goalPosition = goal;
		this.direction = this.goalPosition - this.transform.position;
	}
}