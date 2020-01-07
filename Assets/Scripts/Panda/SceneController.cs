using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
	public static SceneController Instance => _Instance;
	private static SceneController _Instance;

	private void Awake()
	{
		_Instance = GetComponent<SceneController>();
		DontDestroyOnLoad(this);
	}

	private void Start()
	{
		LoadNextScene();
	}

	public void LoadNextScene()
	{
		var nextSceneNumber = SceneManager.GetActiveScene().buildIndex + 1;
		if (nextSceneNumber <= SceneManager.sceneCountInBuildSettings - 1)
		{
			SceneManager.LoadScene(nextSceneNumber);
		}
		else
		{
			Debug.LogWarning("NO MORE SCENE TO LOAD. START NEW ONE");
			SceneManager.LoadScene(1);
		}
	}
}
