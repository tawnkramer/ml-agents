using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoystickCarControl : MonoBehaviour 
{
	public GameObject carObj;
	private ICar car;

	public float MaximumSteerAngle = 25.0f; //has to be kept in sync with the car, as that's a private var.
	
	void Awake()
	{
		if(carObj != null)
			car = carObj.GetComponent<ICar>();
	}

    private void OnDisable()
    {
        car.RequestThrottle(0.0f);
		car.RequestHandBrake(1.0f);
		car.RequestFootBrake(1.0f);
    }
	private void FixedUpdate()
	{
		// pass the input to the car!
		float h = Input.GetAxis("Horizontal");
		float v = Input.GetAxis("Vertical");
		float handbrake = Input.GetAxis("Jump");
		car.RequestSteering(h * MaximumSteerAngle);
		car.RequestThrottle(v);
		car.RequestHandBrake(handbrake);
	}
}
