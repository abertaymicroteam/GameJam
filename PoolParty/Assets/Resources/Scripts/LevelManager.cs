using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelManager : MonoBehaviour {
	
	public float autoLoadNextLevelAfter;
	private SceneManager Scene;
	
	void Start (){
		if (autoLoadNextLevelAfter > 0)
			{
				Invoke ("LoadNextLevel", autoLoadNextLevelAfter);
			}
	}
	public void LoadLevel(string name){
		Debug.Log ("Level load requested for: " + name);
		SceneManager.LoadScene(name);
	
	}
	
	public void QuitRequest(){
		Debug.Log ("I want to quit!");
		Application.Quit();
	}
	
	public void LoadNextLevel(){
		if (SceneManager.GetActiveScene().name == "Splash")
			{
				SceneManager.LoadScene("Movement");
			}
		else 
			{
				SceneManager.LoadScene (SceneManager.GetActiveScene().buildIndex + 1);
			}

	}
}
