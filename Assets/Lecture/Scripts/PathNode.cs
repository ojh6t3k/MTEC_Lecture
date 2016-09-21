using UnityEngine;
using System.Collections;

public class PathNode : MonoBehaviour
{
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;

		Vector3 forwardVec = transform.forward;
		Vector3 fromVec = transform.position + forwardVec * 1f;
		Vector3 toVec = transform.position + forwardVec * -1f;

		Gizmos.DrawLine(fromVec, toVec);
	}
}
