using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComboText : MonoBehaviour {

	private static readonly float DRAW_TIME = 1f;

	private Text text;
	private float startDrawTime;

	void Awake() {
		text = GetComponent<Text> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (!text.IsActive ())
			return;

		if (Time.time - startDrawTime <= DRAW_TIME)
			return;

		text.enabled = false;
	}

	public void SetText(string text) {
		this.text.text = text + "Combo!!";
		this.text.enabled = true;
		startDrawTime = Time.time;
	}
}
