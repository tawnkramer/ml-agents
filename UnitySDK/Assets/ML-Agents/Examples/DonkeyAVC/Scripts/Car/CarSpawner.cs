using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSpawner : MonoBehaviour {

	public GameObject carPrefab;
	public Transform startTm;
    public bool EnableTrainingManager = false;

	public delegate void OnNewCar(GameObject carObj);

	public OnNewCar OnNewCarCB;	

	void Start()
	{
		Spawn();
	}

	static public GameObject getChildGameObject(GameObject fromGameObject, string withName) {
         //Author: Isaac Dart, June-13.
         Transform[] ts = fromGameObject.transform.GetComponentsInChildren<Transform>(true);
         foreach (Transform t in ts) if (t.gameObject.name == withName) return t.gameObject;

        Debug.LogError("couldn't find: " + withName);
         return null;
     }

	public void Spawn () 
	{
        //Create a car object, and also hook up all the connections
        //to various places in game that need to hook into the car.
		GameObject go = GameObject.Instantiate(carPrefab) as GameObject;

		go.transform.rotation = startTm.rotation;
		go.transform.position = startTm.position;
        go.GetComponent<Car>().SavePosRot();


        if (OnNewCarCB != null)
			OnNewCarCB.Invoke(go);

        ///////////////////////////////////////////////
        //Search scene to find these.
        CameraFollow cameraFollow = GameObject.FindObjectOfType<CameraFollow>();


        //set camera target follow tm
        if (cameraFollow != null)
			cameraFollow.target = getChildGameObject(go, "CameraFollowTm").transform;
        ///////////////////////////////////////////////

    }

}
