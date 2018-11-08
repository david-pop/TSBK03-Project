using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour {
	private float speed = 2.0f;
	private Vector3 goalPosition;
    private FlowField ff = null;


	// Use this for initialization
	void Start () {
		Vector3 groundPos = this.transform.position;
		groundPos.y = this.transform.localScale.y;
		this.transform.position = groundPos;

		this.goalPosition = this.transform.position;
	}

	// Update is called once per frame
	void Update () {

		//else if (Random.Range(0.0f, 1.0f) < 0.02) {
		//	this.goalPosition += new Vector3(Random.Range(-1, 2), 0, Random.Range(-1, 2));
		//}

        if(ff != null){
            this.transform.forward = this.goalPosition - this.transform.position;

            Vector3 newPos = Vector3.MoveTowards(this.transform.position, this.goalPosition, this.speed * Time.deltaTime);
            this.transform.position = newPos;
            goalPosition = this.transform.position +
                this.ff.getDirection(this.transform.position);
        }
	}

	public void SetGoalPosition(Vector3 goal) {
		goal.y = this.transform.localScale.y;
		this.goalPosition = goal;
	}

    public void setFlowField(FlowField ff){
        this.ff = ff;
        SetGoalPosition(this.transform.position);
    }
}