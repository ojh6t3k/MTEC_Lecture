using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MySelectable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		MeshRenderer renderer = GetComponent<MeshRenderer>();
		renderer.material.color = Color.yellow;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		MeshRenderer renderer = GetComponent<MeshRenderer>();
		renderer.material.color = Color.white;
	}
}
