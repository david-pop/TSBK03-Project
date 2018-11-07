using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour {

    public Material selectedUnitMaterial;
    public Material defaultUnitMaterial;

    public GameObject unitPrefab;

    private List<GameObject> units;
    private List<GameObject> selectedUnits;

	// Use this for initialization
	void Start () {
        units = new List<GameObject>();
        selectedUnits = new List<GameObject>();

        createUnitAtPosition( new Vector3(10, 1, 10) );
        createUnitAtPosition( new Vector3(15, 1, 10) );
	}

	// Update is called once per frame
	void Update () {
        if(Input.GetMouseButtonDown(0)){
            clearSelection();

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if(Physics.Raycast(ray, out hit)){
                if(hit.collider != null){
                    GameObject hitObject = hit.collider.gameObject;
                    if(units.Contains(hitObject)){
                        selectUnit(hitObject);
                    }
                }
            }
        }

        if(Input.GetMouseButtonDown(1)){
            Vector3 pos = GetMousePlanePosition();

            foreach(GameObject unit in selectedUnits){
                Unit obj = unit.GetComponent<Unit>();
                obj.SetGoalPosition(pos);
            }
        }
	}

    private void createUnitAtPosition(Vector3 pos){
        GameObject newUnit = Instantiate(unitPrefab);
        newUnit.transform.position = pos;
        units.Add(newUnit);
    }

    private void selectUnit(GameObject unit){
        selectedUnits.Add(unit);
        unit.GetComponent<MeshRenderer>().material = selectedUnitMaterial;
    }

    private void deselectUnit(GameObject unit){
        unit.GetComponent<MeshRenderer>().material = defaultUnitMaterial;
    }

    private void clearSelection(){
        foreach(GameObject unit in selectedUnits){
            deselectUnit(unit);
        }

        selectedUnits.Clear();
    }

    private Vector3 GetMousePlanePosition() {
        Plane plane = new Plane( Vector3.up, Vector3.right );
        Ray ray = Camera.main.ScreenPointToRay( Input.mousePosition );
        float distance = 0.0f;

        if ( plane.Raycast( ray, out distance ) )
            return ray.GetPoint( distance );
        else
            return Vector3.zero;
    }
}
