﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		//さらに別の行に変更テスト。GITGIT
	}

	public void PushStartButton(){
		SceneManager.LoadScene ("GameScene");
	}

	public void PushHighscoreButton(){
		SceneManager.LoadScene ("HighscoreScene");
	}

	public void PushCreditButton(){
		SceneManager.LoadScene ("CreditScene");
	}

}
