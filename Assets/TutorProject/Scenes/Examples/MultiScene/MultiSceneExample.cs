using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class MultiSceneExample : MonoBehaviour
{
	public string startingScene = "";
	public CanvasGroup fader;

	void Start ()
	{
		fader.alpha = 1f;
		StartCoroutine(Fade (0f));
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}

	public void LoadScene (string sceneName)
	{
		
	}

	private IEnumerator Fade (float alpha)
	{
		float fadeSpeed = Mathf.Abs (fader.alpha - alpha) / 3f;

		while (!Mathf.Approximately(fader.alpha, alpha))
		{
			fader.alpha = Mathf.MoveTowards(fader.alpha, alpha,	fadeSpeed * Time.deltaTime);
			yield return null;
		}
	}
}
