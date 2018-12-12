using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour {
	private float speed = 4.0f;
	private float separationRadius = 2.0f;
	private float separationFactor = 4.0f;
	//private float cohesionRadius = 10.0f;
	//private float cohesionFactor = -0.1f;

	private FlowField flowField = null;
	private Vector3 velocity;
    private float unitRadius = 0.25f;


	// Use this for initialization
	void Start () {
		Vector3 groundPos = this.transform.position;
		groundPos.y = this.transform.localScale.y;
		this.transform.position = groundPos;

		this.velocity.Set(0, 0, 0);
	}

	// Update is called once per frame
	void Update () {
		//else if (Random.Range(0.0f, 1.0f) < 0.02) {
		//	this.goalPosition += new Vector3(Random.Range(-1, 2), 0, Random.Range(-1, 2));
		//}

		if (this.flowField != null) {
            FlowField.RemoveUnit(this.transform.position, this.separationRadius, this.separationFactor);
            

            //this.flowField.AddSeparation( this.transform.position, this.separationRadius, -this.separationFactor );
			//this.flowField.AddSeparation( this.transform.position, this.cohesionRadius, -this.cohesionFactor );

			Vector3 dir = this.flowField.GetDirection(this.transform.position);
			this.velocity += 0.5f * dir;
			this.velocity = Mathf.Min( this.velocity.magnitude*0.9f, this.speed ) * this.velocity.normalized;

            /*
			this.transform.position = Vector3.MoveTowards(
				this.transform.position,
				this.transform.position + this.velocity.normalized,
				this.velocity.magnitude * Time.deltaTime
			);
			*/
            float unitX = this.transform.position.x;
            float unitZ = this.transform.position.z;

            float newUnitX = unitX + this.velocity.x * Time.deltaTime;
            float newUnitZ = unitZ + this.velocity.z * Time.deltaTime;

            Vector3 radVec = this.velocity.normalized * unitRadius;

            float testUnitX = newUnitX + radVec.x;
            float testUnitZ = newUnitZ + radVec.z;




            if (WorldManager.Instance.worldGrid[(int)testUnitX, (int)unitZ] != 0){
                newUnitX = Mathf.Max((int)testUnitX, (int)unitX) - radVec.x;
            }

            if (WorldManager.Instance.worldGrid[(int)unitX, (int)testUnitZ] != 0){
                newUnitZ = Mathf.Max((int)testUnitZ, (int)unitZ) - radVec.z;
            }

            //else if (WorldManager.Instance.worldGrid[(int)testUnitX, (int)testUnitZ] != 0)
            //{
            //    newUnitX = Mathf.Max((int)testUnitX, (int)unitX) - radVec.x;
            //    newUnitZ = Mathf.Max((int)testUnitZ, (int)unitZ) - radVec.z;
            //}

            this.transform.position = new Vector3(newUnitX, this.transform.position.y, newUnitZ);


            FlowField.AddUnit(this.transform.position, this.separationRadius, this.separationFactor);
            
            //this.flowField.AddSeparation( this.transform.position, this.separationRadius, this.separationFactor );
			//this.flowField.AddSeparation( this.transform.position, this.cohesionRadius, this.cohesionFactor );

			if (this.velocity.sqrMagnitude > 0.001) {
				this.transform.forward = this.velocity;
			} else {
				/*
				this.flowField.AddSeparation( this.transform.position, this.separationRadius, -this.separationFactor );
				this.flowField.AddSeparation( this.transform.position, this.cohesionRadius, -this.cohesionFactor );
				this.flowField = null;
				*/
			}
		}
	}

	public void setFlowField(FlowField flowField){
        if(this.flowField == null){
            FlowField.AddUnit(this.transform.position, this.separationRadius, this.separationFactor);
        }

        this.flowField = flowField;
        

        //this.flowField.AddSeparation( this.transform.position, this.separationRadius, this.separationFactor );
		//this.flowField.AddSeparation( this.transform.position, this.cohesionRadius, this.cohesionFactor );
	}
}