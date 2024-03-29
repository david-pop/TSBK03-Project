﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour {

	public Material selectedUnitMaterial;
	public Material defaultUnitMaterial;

	public GameObject unitPrefab;

	private List<GameObject> units;
	private List<GameObject> selectedUnits;
	private bool isSelecting = false;
	private Vector3 mousePosition1;

    private const int NUM_CONTROL_GROUPS = 10;
    private List<GameObject>[] controlGroups = 
        new List<GameObject>[NUM_CONTROL_GROUPS];

	// Use this for initialization
	void Start () {
		units = new List<GameObject>();
		selectedUnits = new List<GameObject>();

		int unitCount = 500;

		bool[,] placedUnits = new bool[WorldManager.Instance.GridSize, WorldManager.Instance.GridSize];
		int failCount = 0;

		while (unitCount > 0) {
			int x = Random.Range(0, WorldManager.Instance.GridSize);
			int z = Random.Range(0, WorldManager.Instance.GridSize);
			if ( WorldManager.Instance.IsAccessible(x, z) && !placedUnits[x,z] ) {
				createUnitAtPosition( new Vector3(x+0.5f, 0, z+0.5f) );
				placedUnits[x,z] = true;
				unitCount -= 1;
				failCount = 0;
			} else {
				failCount += 1;
				if (failCount > 1000) {
					break;
				}
			}
		}
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(0)) {
			isSelecting = true;
			mousePosition1 = Input.mousePosition;
		}

		if (Input.GetMouseButtonUp(0)) {
			isSelecting = false;
			selectUnits(mousePosition1, Input.mousePosition);
		}

		if(Input.GetMouseButtonDown(1)){
			Vector3 pos = Utils.GetMousePlanePosition();

			if (selectedUnits.Count > 0) {
				FlowField ff = new FlowField(pos.x, pos.z);

				selectedUnits.Sort(delegate(GameObject a, GameObject b) {
					return Vector3.Distance(pos, a.transform.position).CompareTo(
						Vector3.Distance(pos, b.transform.position)
					);
				});
				selectedUnits.Reverse();

				foreach (GameObject unit in selectedUnits) {
					Vector3 p = unit.transform.position;
					ff.Generate((int)p.x, (int)p.z);

					Unit obj = unit.GetComponent<Unit>();
					obj.setFlowField(ff);
				}
			}
		}

		if (Input.GetKeyDown("space")) {
			Debug.Log("Selected all");
            selectUnits(this.units);
		}
    }

	void OnGUI() {
		if( isSelecting )
		{
			// Create a rect from both mouse positions
			var rect = Utils.GetScreenRect( mousePosition1, Input.mousePosition );
			Utils.DrawScreenRect( rect, new Color( 0.8f, 0.1f, 0.0f, 0.1f ) );
			Utils.DrawScreenRectBorder( rect, 2, new Color( 0.8f, 0.1f, 0.0f, 1.0f ) );
		}

        KeyCode pressedKey = Event.current.keyCode;
        bool isKeyDown = Event.current.type == EventType.KeyDown;
        bool isControlDown = Event.current.control;

        if (isKeyDown && 
           pressedKey >= KeyCode.Alpha0 && 
           pressedKey <= KeyCode.Alpha9){

            int groupIndex = pressedKey - KeyCode.Alpha0;

            if(isControlDown){
                controlGroups[groupIndex] = new List<GameObject>(selectedUnits);
            }else{
                selectUnits(controlGroups[groupIndex]);
            }
        }
    }

	private void createUnitAtPosition(Vector3 pos){
		GameObject newUnit = Instantiate(unitPrefab);
		newUnit.transform.position = pos;
		units.Add(newUnit);
	}

	private void selectUnits (Vector3 mousePosition1, Vector3 mousePosition2) {
		foreach (GameObject unit in selectedUnits) {
			unit.GetComponent<MeshRenderer>().material = defaultUnitMaterial;
		}
		selectedUnits.Clear();

		var viewportBounds = Utils.GetViewportBounds( Camera.main, mousePosition1, mousePosition2 );

		foreach ( GameObject unit in units ) {
			Vector3 p = unit.transform.position;
			if ( viewportBounds.Contains( Camera.main.WorldToViewportPoint( p ) ) ) {
				selectedUnits.Add( unit );
				unit.GetComponent<MeshRenderer>().material = selectedUnitMaterial;
			}
		}
	}

    private void selectUnits(List<GameObject> unitsToSelect){
        foreach (GameObject unit in selectedUnits)
        {
            unit.GetComponent<MeshRenderer>().material = defaultUnitMaterial;
        }
        selectedUnits.Clear();
        selectedUnits = new List<GameObject>(unitsToSelect);

        foreach (GameObject unit in unitsToSelect)
        {
            selectedUnits.Add(unit);
            unit.GetComponent<MeshRenderer>().material = selectedUnitMaterial;
        }
    }
}
