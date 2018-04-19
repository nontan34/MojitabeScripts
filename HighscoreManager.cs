using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HighscoreManager : MonoBehaviour {
	GameObject highscoreText,maxcomboText,longesttimeText;

	// Use this for initialization
	void Start () {
		highscoreText = GameObject.Find ("HighscoreValueText");
		maxcomboText = GameObject.Find ("MaxcomboValueText");
		longesttimeText = GameObject.Find ("LongestTimeValueText");

		//キーの読み込み
		if (PlayerPrefs.HasKey ("HIGHSCORE") == true) {
			highscoreText.GetComponent<Text> ().text = PlayerPrefs.GetInt ("HIGHSCORE") + " てん";
		}
		if (PlayerPrefs.HasKey ("MAXCOMBO") == true) {
			maxcomboText.GetComponent<Text> ().text = PlayerPrefs.GetInt ("MAXCOMBO") + " コンボ";
		}
		if (PlayerPrefs.HasKey ("LONGESTTIME") == true) {
			longesttimeText.GetComponent<Text> ().text = PlayerPrefs.GetFloat ("LONGESTTIME") + " びょう";
		}

	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void PushBackButton(){
		SceneManager.LoadScene ("TitleScene");
	}
}
