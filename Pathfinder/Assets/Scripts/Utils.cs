using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class Utils
{
	static Texture2D _whiteTexture;
	public static Texture2D WhiteTexture
	{
		get
		{
			if( _whiteTexture == null )
			{
				_whiteTexture = new Texture2D( 1, 1 );
				_whiteTexture.SetPixel( 0, 0, Color.white );
				_whiteTexture.Apply();
			}
 
			return _whiteTexture;
		}
	}
 
	public static void DrawScreenRect( Rect rect, Color color )
	{
		GUI.color = color;
		GUI.DrawTexture( rect, WhiteTexture );
		GUI.color = Color.white;
	}

	public static void DrawScreenRectBorder( Rect rect, float thickness, Color color )
	{
		// Top
		Utils.DrawScreenRect( new Rect( rect.xMin, rect.yMin, rect.width, thickness ), color );
		// Left
		Utils.DrawScreenRect( new Rect( rect.xMin, rect.yMin, thickness, rect.height ), color );
		// Right
		Utils.DrawScreenRect( new Rect( rect.xMax - thickness, rect.yMin, thickness, rect.height ), color);
		// Bottom
		Utils.DrawScreenRect( new Rect( rect.xMin, rect.yMax - thickness, rect.width, thickness ), color );
	}

	public static Vector3 GetMousePlanePosition() {
		return GetPlanePosition( Input.mousePosition );
	}

	public static Vector3 GetPlanePosition( Vector3 position ) {
		Plane plane = new Plane( Vector3.up, Vector3.right );
		Ray ray = Camera.main.ScreenPointToRay( position );
		float distance = 0.0f;

		if ( plane.Raycast( ray, out distance ) )
			return ray.GetPoint( distance );
		else
			return Vector3.zero;
	}

	public static Rect GetScreenRect( Vector3 screenPosition1, Vector3 screenPosition2 )
	{
		// Move origin from bottom left to top left
		screenPosition1.y = Screen.height - screenPosition1.y;
		screenPosition2.y = Screen.height - screenPosition2.y;
		// Calculate corners
		var topLeft = Vector3.Min( screenPosition1, screenPosition2 );
		var bottomRight = Vector3.Max( screenPosition1, screenPosition2 );
		// Create Rect
		return Rect.MinMaxRect( topLeft.x, topLeft.y, bottomRight.x, bottomRight.y );
	}

	public static Bounds GetViewportBounds( Camera camera, Vector3 screenPosition1, Vector3 screenPosition2 )
	{
		var v1 = Camera.main.ScreenToViewportPoint( screenPosition1 );
		var v2 = Camera.main.ScreenToViewportPoint( screenPosition2 );
		var min = Vector3.Min( v1, v2 );
		var max = Vector3.Max( v1, v2 );
		min.z = camera.nearClipPlane;
		max.z = camera.farClipPlane;
	 
		var bounds = new Bounds();
		bounds.SetMinMax( min, max );
		return bounds;
	}

	// Given a (u,v) coordinate that defines a 2D local position inside a planar quadrilateral, find the
	// absolute 3D (x,y,z) coordinate at that location.
	//
	//  0 <----u----> 1
	//  a ----------- b    0
	//  |             |   /|\
	//  |             |    |
	//  |             |    v
	//  |  *(u,v)     |    |
	//  |             |   \|/
	//  d------------ c    1
	//
	// a, b, c, and d are the vertices of the quadrilateral. They are assumed to exist in the
	// same plane in 3D space, but this function will allow for some non-planar error.
	//
	// Variables u and v are the two-dimensional local coordinates inside the quadrilateral.
	// To find a point that is inside the quadrilateral, both u and v must be between 0 and 1 inclusive.  
	// For example, if you send this function u=0, v=0, then it will return coordinate "a".  
	// Similarly, coordinate u=1, v=1 will return vector "c". Any values between 0 and 1
	// will return a coordinate that is bi-linearly interpolated between the four vertices.
	public static float QuadLerp(float a, float b, float c, float d, float u, float v)
	{
		float abu = Mathf.Lerp(a, b, u);
		float dcu = Mathf.Lerp(d, c, u);
		return Mathf.Lerp(abu, dcu, v);
	}
}