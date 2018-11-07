using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour {

	private float panSpeed;				// The camera movement speed
	private float smoothing;			// The speed with which the camera will be following.
	private Vector4 panLimit;			// Camera limits
	private float panBorderThickness;	// Border size for mouse camera movement

	private Vector3 offset;				// The initial offset from the target.
	private Vector3 movement;			// Current camera velocity
	private Vector3 targetCamPos;		// Camera target position

	private Vector3 dragOrigin;
	private bool enableDrag;

	void Start ()
	{
		this.panSpeed = 20;
		this.smoothing = 10;
		this.panLimit.Set(0, 0, 100, 100);
		this.panBorderThickness = 10;

		this.offset = this.transform.position;
		this.movement.Set(0, 0, 0);
		this.targetCamPos = Vector3.zero;

		this.dragOrigin.Set(0, 0, 0);
		this.enableDrag = false;
	}

	void Update ()
	{
		float dx = Input.GetAxisRaw( "Horizontal" );
		float dy = Input.GetAxisRaw( "Vertical" );

		if (Input.mousePosition.x <= this.panBorderThickness)
			dx -= 1;
		if (Input.mousePosition.x >= Screen.width - this.panBorderThickness)
			dx += 1;
		if (Input.mousePosition.y <= this.panBorderThickness)
			dy -= 1;
		if (Input.mousePosition.y >= Screen.height - this.panBorderThickness)
			dy += 1;

		if (Input.GetMouseButtonDown(1)) {
			this.dragOrigin = GetMousePlanePosition();
			enableDrag = false;
		}
		if (Input.GetMouseButton(1)) {
			
			if ( (this.dragOrigin - GetMousePlanePosition()).magnitude > 0.2 ) {
				enableDrag = true;
			}
			if (this.enableDrag) {
				this.targetCamPos = this.transform.position - this.offset + ( this.dragOrigin - GetMousePlanePosition() );
			}
		}
		else
		{
			// Move the camera around the scene with WASD.
			MoveCamera( dx, dy );
		}

		// Clamp position to limits
		targetCamPos.x = Mathf.Clamp( targetCamPos.x, panLimit.x, panLimit.z );
		targetCamPos.z = Mathf.Clamp( targetCamPos.z, panLimit.y, panLimit.w );

		// Smoothly interpolate between the camera's current position and it's target position.
		transform.position = Vector3.Lerp( transform.position, targetCamPos + offset, smoothing * Time.deltaTime );
	}

	void MoveCamera (float h, float v)
	{
		// Set the movement vector based on the axis input.
		movement.Set( h, 0f, v );
		// Normalise the movement vector and make it proportional to the speed per second.
		movement = movement.normalized * panSpeed * Time.deltaTime;

		// Create a position the camera is aiming for based on the offset from the target.
		targetCamPos += movement;
	}

	Vector3 GetMousePlanePosition() {
		Plane plane = new Plane( Vector3.up, Vector3.right );
		Ray ray = Camera.main.ScreenPointToRay( Input.mousePosition );
		float distance = 0.0f;

		if ( plane.Raycast( ray, out distance ) )
			return ray.GetPoint( distance );
		else
			return Vector3.zero;
	}
}