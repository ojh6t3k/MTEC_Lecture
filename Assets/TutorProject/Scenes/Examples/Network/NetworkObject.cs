using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class NetworkObject : NetworkBehaviour
{
	[SyncVar]
	public int health = 100;

	private float time = 0;

	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		time += Time.deltaTime;
		if(time > 1f)
		{
			if(isServer == true)
				health--;
			
			time = 0;
			Debug.Log(health);
		}
	}

	[ClientRpc]
	void Rpc_DiePlayer()
	{
	}

	[Command]
	public void Cmd_RefilllHealth()
	{
		health = 100;
	}
}
