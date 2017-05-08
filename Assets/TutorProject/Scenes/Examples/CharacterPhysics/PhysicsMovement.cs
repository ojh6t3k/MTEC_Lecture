using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsMovement : MonoBehaviour
{
	public float multiplier = 3f;

	private Rigidbody2D _rigidBody2D;

	// Use this for initialization
	void Start () 
	{
		_rigidBody2D = GetComponent<Rigidbody2D>();
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		Vector2 axis = Vector2.zero;
		axis.x = Input.GetAxis("Horizontal");
		//axis.y = Input.GetAxis("Vertical");

		axis *= multiplier;
		axis.y = _rigidBody2D.velocity.y;

		_rigidBody2D.velocity = axis;
		if(Input.GetKey(KeyCode.A))
		{
			Vector2 newPosition = transform.position;
			newPosition += new Vector2(0.1f, 0f);
			_rigidBody2D.MovePosition(newPosition);
		}
	}
}
