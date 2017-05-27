using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class SceneSwitcher : MonoBehaviour
{
	public string sceneName;

	// Use this for initialization
	void Start ()
	{
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}

	void OnTriggerEnter(Collider other)
	{
		StartCoroutine(LoadScene());
	}

	void OnTriggerExit(Collider other)
	{
		StartCoroutine(UnloadScene());
	}

	private IEnumerator LoadScene()
	{
		yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
	}

	private IEnumerator UnloadScene()
	{
		yield return SceneManager.UnloadSceneAsync(sceneName);
	}
}
