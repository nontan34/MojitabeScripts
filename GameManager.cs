using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;

//時間延長アイテム
//同じたべものコンボ
//お気に入りたべもの

//ハイスコア記録日

//周囲の食べ物を消すアイテム
//調整要素、食べ物の重複配置回避

public class GameManager : MonoBehaviour {

	//CSV読み込み用
	private TextAsset csvFile; //データ読み込み用のCSVファイル
	private List<string[]> csvDatas = new List<string[]> (); //CSVの中身を入れるリスト
	private string foodDataPath="CSV/fooddata";
	private string objectiveDataPath="CSV/objectivedata";

	//FoodsData
	Food[] foods = new Food[37];
	GameObject foodPrefab;
	GameObject popfood;
	const float SPAWNTIME = 2.0f;

	//ObjectiveData ゲーム目標系データのこと
	Objective[] obj = new Objective[17];
	public GameObject blockPrefab;

	//Score
	GameObject scoreText,comboText;
	int score=0;
	int comboCount=0,maxcomboCount=0;
	const int COMBOPOINT = 50;
	const int GREATCOMBOCOUNT = 10;
	const int COMBOMAX = 15; //加点として有効なコンボ数上限

	//UI and Effect
	GameObject getFoodText;
	GameObject playerIcon;
	GameObject retryButton,endButton;

	//指示,General
	GameObject instructionText;
	int objNumber; //目標番号
	bool FLAG_gameProgress=false;

	//時間管理
	float delta=0,spawndelta=0;
	float totaltime=0;
	float additionalTime=0;
	float limitTime;
	const float DEF_TIMELIMIT=40.0f;
	GameObject timeText;

	//Audio
	public AudioClip seOK,seNG,seComboOK;
	public AudioClip seTimeUp, seGameStart;
	private AudioSource audioSource;

	//DebugTool
	GameObject debugText;
	string debugLog;

	// Use this for initialization
	void Start () {
		//初期化部分、Sceneの最初だけ
		InitializeDebugTool ();
		InitializeObjects ();

		InitializeFoods ();
		InitializeObjective (); //ゲーム目標の初期化

		//ゲーム開始準備
		PutBlocks ();
		AnnounceObjective ();
		PrepareFoods ();
		audioSource.PlayOneShot(seGameStart);

		//Indevelopment
		//スタートカウントダウン演出いれるならここ

		FLAG_gameProgress = true;
	}

	// Update is called once per frame
	void Update () {

		if (FLAG_gameProgress == true) {
			delta += Time.deltaTime;
			spawndelta += Time.deltaTime;

			scoreText.GetComponent<Text> ().text = score + " てん";
			comboText.GetComponent<Text> ().text = comboCount + " コンボ";
			limitTime = DEF_TIMELIMIT - totaltime + additionalTime;
			timeText.GetComponent<Text> ().text = "のこりじかん " + limitTime;

			if (spawndelta > SPAWNTIME) {
				SpawnMoreFoods ();
				if (comboCount > GREATCOMBOCOUNT) {
					SpawnMoreFoods ();
				}
				spawndelta = 0;
			}

			if (delta > 1.0f) {
				totaltime += 1.0f;
				delta = 0;
			}

			if (totaltime > (DEF_TIMELIMIT + additionalTime) ) {
				audioSource.PlayOneShot (seTimeUp);
				totaltime = 0;

				FLAG_gameProgress = false;
				retryButton.SetActive (true);
				endButton.SetActive (true);

				//スコア集計処理
				SaveRecord();
			}
		}
	}

	//ルール管理
	//-ルールのランダム決定
	void AnnounceObjective(){
		objNumber = Random.Range (0, 9); //わをんは消してある。
		if (objNumber == 7) {
			objNumber = Random.Range (0, 9); //やゆよはもう一回。つまらないから
		}

		instructionText.GetComponent<Text> ().text = "「" + obj[objNumber].name + " 」のたべものをたべよう";
	}

	//マップ生成
	//-Food Prefabをマップ中にランダム生成
	void PrepareFoods(){
		for (int i = 0; i < 36; i++) {
			int spawn = Random.Range(2,6); //いくつ出現するか
			for(int j=0; j< spawn;j++){
				popfood = Instantiate (Resources.Load ("Prefabs/" + foods [i].codename)) as GameObject;
				popfood.transform.position = new Vector3 (Random.Range (-15.0f, 15.0f), Random.Range (-7.0f, 7.0f), 0);
			}
		}
	}

	void SpawnMoreFoods(){
		int k = Random.Range (0, 36);
		popfood = Instantiate (Resources.Load ("Prefabs/" + foods [k].codename)) as GameObject;
		popfood.transform.position = new Vector3(Random.Range (-15, 15), Random.Range (-7, 7), 0);
	}


	//GeneralScript
	//CSVデータの読み込み処理
	void ReadCSVData(string path){
		csvFile = Resources.Load (path) as TextAsset;
		StringReader reader = new StringReader (csvFile.text);

		while (reader.Peek() > -1) {
			string line = reader.ReadLine ();
			csvDatas.Add (line.Split (','));
		}
	}

	//Initialize
	void InitializeObjects (){
		//Find UI 
		instructionText = GameObject.Find ("InstructionText");
		scoreText = GameObject.Find ("ScoreText");
		comboText = GameObject.Find ("ComboText");
		timeText = GameObject.Find("TimeText");
		retryButton  = GameObject.Find("RetryButton");
		retryButton.SetActive (false);
		endButton  = GameObject.Find("EndButton");
		endButton.SetActive (false);
		getFoodText = GameObject.Find ("GetFoodText");
		getFoodText.SetActive (false);

		playerIcon = GameObject.Find ("PlayerIcon");

		audioSource = gameObject.GetComponent<AudioSource> ();
	}

	void InitializeFoods(){
		csvDatas.Clear ();
		ReadCSVData (foodDataPath);
		for (int i = 0; i < 36; i++) {
			foods [i] = new Food (csvDatas [i + 1] [1],csvDatas[i+1][2],csvDatas[i+1][3]);
		}
	}

	void InitializeObjective(){
		csvDatas.Clear ();
		ReadCSVData (objectiveDataPath);
		for (int i = 0; i < 11; i++) {
			obj [i] = new Objective (csvDatas [i + 1] [1],csvDatas [i + 1] [2],csvDatas [i + 1] [3],csvDatas [i + 1] [4],csvDatas [i + 1] [5],csvDatas [i + 1] [6]);
		}
	}

	void PutBlocks(){
		for (int i = 0; i < Random.Range (0, 5); i++) {
			GameObject bl = Instantiate (blockPrefab) as GameObject;
			bl.transform.position = new Vector3 (Random.Range (-15, 15), Random.Range (-7, 7), 0);
		}
	}

	void ResetScene(){
		score=0;
		comboCount=0;
		maxcomboCount = 0;
		delta = 0;
		spawndelta=0;
		totaltime=0;
		additionalTime = 0;

		ClearAllPrefabs(); //タグ探して消せばOK

		//ゲーム開始準備
		PutBlocks ();
		AnnounceObjective ();
		PrepareFoods ();
		audioSource.PlayOneShot(seGameStart);
	}

	void ClearAllPrefabs(){
		GameObject[] clones = GameObject.FindGameObjectsWithTag ("Food");
		foreach (GameObject clone in clones) {
			Destroy (clone);
		}

		clones = GameObject.FindGameObjectsWithTag ("Wall");
		foreach (GameObject clone in clones) {
			Destroy (clone);
		}
	}


	//Food処理
	//Food取得処理
	public void AddFood(string foodname){
		//Prefabだと名前が nasu(clone)などになってしまう
		int rm = foodname.IndexOf("(");
		foodname = foodname.Remove (rm);

		ShowGetFoodEffect ( GetFullname (foodname) ); //たべもの入手演出へ
		score += JudgePoint(foodname);
	}

	//指定されたファイル名と一致するフルネームを返す
	public string GetFullname(string codename){
		for (int i = 0; i < 36; i++) {
			if (foods [i].codename == codename) {
				return foods [i].fullname;
			}
		}
		return "nothing";
	}

	//現在のルールに合わせて得点可否を判断する
	public int JudgePoint(string codename){
		for(int i = 0 ; i<36;i++){
			if(foods [i].codename == codename) {
				for (int j = 0; j < 5; j++) {
					//Debug.Log ("比較その" + j + " = " + foods [i].SearchMoji (obj [objNumber].word[j]) );
					if (foods [i].SearchMoji (obj [objNumber].word[j]) != -1) {
						//ルールで指定された文字と一致する文字があったら得点

						//連続正解コンボ点計算
						comboCount++;
						additionalTime += 1.0f;
						//OK効果音
						if (comboCount < GREATCOMBOCOUNT) {
							audioSource.PlayOneShot (seOK);
						} else {
							//スーパーコンボの歓声演出
							audioSource.PlayOneShot(seComboOK);
						}

						//コンボ最高数更新
						if (comboCount > maxcomboCount) {
							maxcomboCount = comboCount;
						}

						//得点加算
						int point;
						if (comboCount < COMBOMAX) {
							//加点コンボ上限に配慮
							point = 50 + comboCount * COMBOPOINT;
						} else {
							point = 50 + COMBOMAX * COMBOPOINT;
						}

						return point;
					}
				}
			}
		}

		//１文字も一致しなかったらマイナス点

		//コンボ消失処理
		comboCount=0;
		//NG効果音
		audioSource.PlayOneShot(seNG);

		return -50;
	}


	//ユーザデータ（ハイスコアなど）の記録
	void SaveRecord(){
		//キーの存在を確認
		if (PlayerPrefs.HasKey ("HIGHSCORE") == true) {
			int kiroku = PlayerPrefs.GetInt ("HIGHSCORE");
			if (score > kiroku) {
				//新記録だ!
				PlayerPrefs.SetInt("HIGHSCORE",score);
			}
		} else {
			//キーが無い（初記録）
			PlayerPrefs.SetInt("HIGHSCORE",score);
		}

		if (PlayerPrefs.HasKey ("MAXCOMBO") == true) {
			int kiroku = PlayerPrefs.GetInt ("MAXCOMBO");
			if (maxcomboCount > kiroku) {
				PlayerPrefs.SetInt("MAXCOMBO",maxcomboCount);
			}
		} else {
			PlayerPrefs.SetInt("MAXCOMBO",maxcomboCount);
		}

		if (PlayerPrefs.HasKey ("LONGESTTIME") == true) {
			float kiroku = PlayerPrefs.GetFloat ("LONGESTTIME");
			if ((additionalTime+DEF_TIMELIMIT) > kiroku) {
				PlayerPrefs.SetFloat("LONGESTTIME",(additionalTime+DEF_TIMELIMIT));
			}
		} else {
			PlayerPrefs.SetFloat("LONGESTTIME",(additionalTime+DEF_TIMELIMIT));
		}

		//スコア書き込み
		PlayerPrefs.Save();
	}


	//UI演出系
	//Food獲得時の文字表示 徐々に上昇して2秒後に消える
	//コルーチンとセット
	void ShowGetFoodEffect(string foodname){
		getFoodText.SetActive (true);
		getFoodText.GetComponent<Text>().text = foodname;
		StartCoroutine ("FloatingEffect");
	}
	private IEnumerator FloatingEffect(){
		float x, y;
		x = playerIcon.transform.position.x;
		y = playerIcon.transform.position.y;

		for (int i = 0; i < 50; i++) {
			getFoodText.transform.position = new Vector3 (x, y + 0.7f + i*0.04f, 0);
			yield return null;
		}

		getFoodText.SetActive (false);
		getFoodText.transform.position = new Vector3 (x, y, 0);
	}

	//Score加算演出
	void ShowGetScoreEffect(){
	}


	/*Pushボタン周り*/
	public void PushRetryButton(){
		ResetScene ();
		retryButton.SetActive (false);
		endButton.SetActive (false);
		FLAG_gameProgress = true;
	}

	public void PushEndButton(){
		SceneManager.LoadScene ("TitleScene");
	}


	/* Indebug */
	void InitializeDebugTool(){
		debugText = GameObject.Find ("DebugText");
	}

	void DebugCheck_Position(){
		
		float pPosX = PlayerController.pPosX;
		float pPosY = PlayerController.pPosY;
		float tPosX = PlayerController.tPosX;
		float tPosY = PlayerController.tPosY;

		debugLog = "(pPosX,pPoxY)=( " + pPosX + " , " + pPosY + " )\n" + "(tPosX,tPoxY)=( " + tPosX + " , " + tPosY + " )\n";
		debugLog = debugLog + "pAngle=" + PlayerController.pAngle + " // " + "tAngle=" + PlayerController.tAngle + "\n" ;
		debugLog = debugLog + "(distance X , Y) = " + Mathf.Abs (pPosX - tPosX) + " , " + Mathf.Abs (pPosY - tPosY);
		debugLog = debugLog + "(Speed X , Y) = " + PlayerController.speedX + " , " + PlayerController.speedY;

		debugText.GetComponent<Text> ().text = debugLog;
	}

}

public class Food
{
	public string codename;
	public string fullname;
	public string checkname;

	public Food(string n,string m,string c){
		this.codename = n;
		this.fullname = m;
		this.checkname = c;
	}

	//指定された文字を含むか調べて個数を返す
	public int SearchMoji(string moji){
		int n;
		n = checkname.IndexOf (moji);
		//Debug.Log ("「" + checkname + "」から、『" + moji + "』を探し、結果は－＞" + n );
		return n;
	}

}

public class Objective
{
	public string name;
	public string[] word = new string[5];

	public Objective(string n,string s1,string s2,string s3,string s4,string s5){
		this.name = n;
		this.word [0] = s1;
		this.word [1] = s2;
		this.word [2] = s3;
		this.word [3] = s4;
		this.word [4] = s5;

	}
}