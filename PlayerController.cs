using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	//GameObjects
	GameObject playerIcon;
	Rigidbody2D rigid2D;
	GameObject mainCamera;
	GameObject particle;
	GameObject gm;

	//PlayerStatus
	public static float pPosX,pPosY; //プレイヤー座標
	float moveForce=20.0f; //加速力
	public static float speedX,speedY;
	float maxSpeed = 5.0f;
	float minSpeed = 0.8f;
	public static float pAngle; //プレイヤーの向き

	//InputStatus
	Vector3 mousePosScreen,mousePosWorld; //マウス座標。スクリーン座標とワールド座標の２つ。
	public static float tPosX,tPosY; //タップ座標
	public static float tAngle; //マウスのタップ方向

	//Other
	float delta=0;

	// Use this for initialization
	void Start () {
		InitializePlayer ();
	}
	
	// Update is called once per frame
	void Update () {
		delta += Time.deltaTime;

		//現在の移動速度の取得
		speedX = Mathf.Abs(this.rigid2D.velocity.x);
		speedY = Mathf.Abs (this.rigid2D.velocity.y);
		if (speedX > maxSpeed || speedY > maxSpeed) {
			particle.SetActive (true);
		}
		if (speedX < minSpeed && speedY < minSpeed) {
			particle.SetActive (false);
		}

		//タップされていれば移動＆方向判定を常に実施
		if (Input.GetMouseButton (0)) {
			//プレイヤー座標とタップ座標の比較準備
			//-プレイヤー座標取得
			pPosX = playerIcon.transform.position.x;
			pPosY = playerIcon.transform.position.y;
			//-タップ座標取得
			mousePosScreen = Input.mousePosition;
			mousePosWorld = Camera.main.ScreenToWorldPoint (mousePosScreen); //スクリーン座標からワールド座標へ変換
			tPosX = mousePosWorld.x;
			tPosY = mousePosWorld.y;

			//プレイヤー座標との距離で速度を決定
			float distanceX = Mathf.Abs(pPosX - tPosX);
			float distanceY = Mathf.Abs(pPosY - tPosY);
			if (distanceX < 0.4f && distanceY < 0.4f) {
				//プレイヤーに近ければ移動速度はゼロ
				maxSpeed = 0.0f;
				this.rigid2D.velocity = Vector2.zero;
			} else {
				//タップ箇所がプレイヤーから十分に遠ければ制限速度が距離に従って決定される
				//（参考）画面端は8.0fぐらいになる
				maxSpeed = (distanceX + distanceY) / 2.0f;
			}

			//プレイヤーの向きを決定
			//現在のプレイヤーの向きを取得
			pAngle = playerIcon.transform.eulerAngles.z;
			//タップ座標の向きを取得して方向転換させる（方向転換の時間は無し。一瞬。）
			float anglePosX = tPosX - pPosX;
			float anglePosY = tPosY - pPosY;
			tAngle = Mathf.Atan2 (anglePosY, anglePosX) * Mathf.Rad2Deg;
			playerIcon.transform.eulerAngles = new Vector3 (0, 0, tAngle-90);

			//速度制限と移動。最高速度はタップ位置が離れているほど上がる。
			this.rigid2D.AddForce (transform.up * this.moveForce);

			//速度制御の調整が必要 InExpansion
		}

		//カメラの追従
		mainCamera.transform.position = new Vector3 (playerIcon.transform.position.x, playerIcon.transform.position.y, -10);
	}

	//Initialize
	void InitializePlayer(){
		playerIcon = GameObject.Find ("PlayerIcon");
		this.rigid2D = GetComponent<Rigidbody2D> ();

		mainCamera = GameObject.Find ("Main Camera");
		particle = GameObject.Find ("PlayerParticle");

		gm = GameObject.Find ("GameManager");
	}

	//食べ物との当たり判定、当たったアイテム名をGameManagerに伝える
	void OnTriggerEnter2D(Collider2D other){
		gm.GetComponent<GameManager> ().AddFood (other.gameObject.name);
		Destroy (other.gameObject);
	}

}
