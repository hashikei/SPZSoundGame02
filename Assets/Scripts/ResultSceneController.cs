using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ResultSceneController : MonoBehaviour {

	[SerializeField] Text scoreText;

	// Use this for initialization
	void Start () {
		int score = PlayerPrefs.GetInt (NotesManager.SAVE_KEY);
		scoreText.text = "Score:" + score.ToString ("D7");
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Return)) {
			SceneManager.LoadScene ("Title");
		}
	}
}
