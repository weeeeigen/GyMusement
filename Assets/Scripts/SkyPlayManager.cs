using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using SocketIO;
using MiniJSON;

public class SkyPlayManager : MonoBehaviour {
	// GUI
	public GameObject mainCamera;
	public Text timeText;
	public Text meterText;
	public Text pointText;
	public Text pointUpText;
	public Text messageText;
	public Text upDownText;
	public Text rhythmText;
	public GameObject backTest;
	public GameObject backTitle;
	public GameObject canvas;

	public GameObject pointer;

	// 妖精
	public GameObject[] normalFairies;
	public GameObject[] rareFairies;
	public GameObject[] superFairies;
	public GameObject selectFairy;


	// 敵
	public GameObject freedom;
	public GameObject enemy1;
	public GameObject enemy2;
	public GameObject[] beams1;
	public GameObject[] beams2;
	public GameObject[] bursts;

	// 酸素，出現エフェクト
	public GameObject explosion;
	public GameObject effect;

	// 出現座標とフラグ
	public Vector3[] fairyPositions;
	public bool[] fairyAppearFlags;
	public bool[] takeBreathFlags;
	public Vector3 fairyPosition;
	public Vector3[] smallExplosionPositions;
	public Vector3[] largeExplosionPositions;

	// プレイ開始までの時間とフラグ
	public int readyTime;
	public float readyTimeCounter;
	public bool startFlag;
	public bool endFlag;

	// プレイ中の距離と制限時間
	public int time;
	public int meter;
	public float timeCounter;
	public int meterCounter;

	public float beamTimeCounter1;
	public float beamTimeCounter2;
	public bool freedomFlag;
	public bool enemyFlag;
	public bool burstFlag;

	// リズムのタイミング，0がUp
	public int[] upDownTime;
	public float upDownTimeCounter;
	public string[] upDownMessage;
	public int upDownNum;

	// リズムのフラグ，trueがup
	public bool upDownFlag;

	// 魚のポイント
	public int[] normalFairiesPoint;
	public int[] rareFairiesPoint;
	public int[] superFairiesPoint;
	public int selectPoint;
	public int point;

	// ランキング用のポイント
	public int point1st;
	public int point2nd;
	public int point3rd;

	// リズム badかnormalかgood
	public string rhythm;
	public float[] speeds;
	public float speed;

	// BGM
	public AudioClip bgm;
	public AudioClip appear;
	public AudioClip disappear;
	public AudioClip landing;
	public AudioClip enemy;
	public AudioClip beam;
	public AudioClip burst;
	public AudioClip smallExplosion;
	public AudioClip largeExplosion;
	public AudioClip up;
	public AudioClip down;
	private AudioSource audioSources;

	// ソケット
	private SocketIOComponent socket;


	// Use this for initialization
	void Start () {
		// SocketIOオブジェクトを読み込む&サーバと接続
		GameObject go = GameObject.Find("SocketIO");
		socket = go.GetComponent<SocketIOComponent>();
		socket.On ("MOVE", OnUserMove);

		// 終了時の遷移ボタンを隠す
		backTitle.SetActive(false);
		backTest.SetActive(false);

		// BGMスタート
		audioSources = gameObject.GetComponent<AudioSource> ();
		audioSources.clip = bgm;
		audioSources.Play ();

		// 開始までの時間，制限時間，ポイント，リズム，フラグを初期化
		readyTime = 4;
		selectPoint = 0;
		rhythm = "good";
		startFlag = false;
		endFlag = false;
		upDownFlag = true;

		// 出現座標を初期化
		for (int i = 0; i < 20; i++) {
			fairyPositions[i] = (new Vector3 (0, 0, 30 * i + 20));
			fairyAppearFlags[i] = true;
			takeBreathFlags[i] = true;
		}
	}

	// sensortgのデータが更新されたら呼び出される
	private void OnUserMove(SocketIOEvent e){
		if (Json.Deserialize (e.data ["id"].ToString ()) as string == PlayerPrefs.GetString ("sensorId")) {
			// UPDOWNの判定
			if (Json.Deserialize (e.data ["status"].ToString ()) as string == "up") {
				upDownFlag = true;
				upDownTime [1] = 6;
			} else if (Json.Deserialize (e.data ["status"].ToString ()) as string == "down") {
				upDownFlag = false;
				upDownTime [0] = 4;
			}

			Debug.Log (e.data ["status"].ToString ());

			rhythm = Json.Deserialize (e.data ["rhythm"].ToString ()) as string;
			Debug.Log (rhythm);
		}
	}


	
	// Update is called once per frame
	void Update () {
		if (socket.IsConnected == false) {
			socket.Connect ();
		}

		if(startFlag == true && endFlag == false){
			// UPDOWNの指示を出す
			UpDownCount();

			// オブジェクトを移動させる
			MoveObjects();

			// 制限時間を減らす
			CountdownTime ();

			// 妖精を出現させる
			FairyAppear ();

			// 息を吐く
			//TakeBreath ();

			// ビーム
			EnemyAttack();

			// ハイマットフルバースト
			Burst();

			// ゲーム終了
			FinishGame();

		}else if(startFlag == false){
			GameReady ();
		}

		test();
	}


	// テスト
	public void test(){
		if(Input.GetKey(KeyCode.LeftArrow)){
			rhythm = "verySlow";
		}else if(Input.GetKey(KeyCode.DownArrow)){
			rhythm = "slow";
		}else if(Input.GetKey(KeyCode.RightArrow)){
			rhythm = "good";
		}

	}


	// ゲーム開始まで
	public void GameReady(){
		timeCounter += Time.deltaTime;
		if (timeCounter > 1 && timeCounter < 2) {
			if (readyTime > 1) {
				readyTime -= 1;
				timeCounter = 0;
				messageText.text = readyTime.ToString ();
			} else {
				startFlag = true;
				messageText.enabled = false;
			}
		}
	}


	// UpDownの指示を出す
	public void UpDownCount(){
		if(time > 1){
			if(upDownFlag == true){
				upDownNum = 0;
			}else if(upDownFlag == false){
				upDownNum = 1;
			}

			// カウントダウン
			upDownTimeCounter += Time.deltaTime;
			if(upDownTimeCounter >= 1){
				upDownTime [upDownNum] -= 1;
				upDownTimeCounter = 0;
				upDownText.text = upDownMessage[upDownNum] + " : " + upDownTime [upDownNum].ToString();

				// 0になったら逆になる
				if(upDownTime[upDownNum] <= 1 && upDownNum == 0){
					upDownFlag = false;
					upDownTime[0] = 4;
				}else if(upDownTime[upDownNum] <= 1 && upDownNum == 1){
					upDownFlag = true;
					upDownTime [1] = 6;
				}
			}
		}
	}


	// 時間を減らす
	public void CountdownTime(){
		timeCounter += Time.deltaTime;
		if (timeCounter >= 1) {
			if(time >= 1){
				time -= 1;
				timeCounter = 0;
				timeText.text = time.ToString ();
			}
		}
	}


	// カメラとキャンバスを移動
	public void MoveObjects(){
		if(rhythm == "veryFast" || rhythm == "verySlow"){
			speed = speeds[0];
		}else if(rhythm == "fast" || rhythm == "slow"){
			speed = speeds[1];
		}else if(rhythm == "good"){
			speed = speeds[2];
		}

		mainCamera.transform.position += new Vector3 (0, 0, speed);
		canvas.transform.position += new Vector3 (0,0,speed);

		rhythmText.text = rhythm;

		meter = Mathf.FloorToInt (mainCamera.transform.position.z) + 10;
		meterText.text = meter.ToString () + " m";
	}


	// 一定の距離間隔で妖精を出現、ポイント加算
	public void FairyAppear(){
		for(int i=0; i<20; i++){
			if(meter == 30*i && fairyAppearFlags[i] == true){

				// 速さに応じて魚とポイントを変える
				int num;

				if (rhythm == "veryFast" || rhythm == "verySlow") {
					num = Random.Range (0, normalFairies.Length);
					selectFairy = normalFairies [num];
					selectPoint = normalFairiesPoint [num];
				} else if (rhythm == "fast" || rhythm == "slow") {
					num = Random.Range (0, rareFairies.Length);
					selectFairy = rareFairies [num];
					selectPoint = normalFairiesPoint [num];
				} else if (rhythm == "good") {
					num = Random.Range (0, superFairies.Length);
					selectFairy = superFairies [num];
					selectPoint = normalFairiesPoint [num];
				}

				//カウントが減るたびに効果音再生
				if(upDownNum == 0){
					audioSources.PlayOneShot (up);
				} else if(upDownNum == 1){
					audioSources.PlayOneShot (down);
				}

				fairyPosition = fairyPositions [i];

				// まずエフェクトが出現
				Instantiate (effect, fairyPositions [i] - new Vector3(0, 3, 0), Quaternion.identity);
				Invoke ("Apa", 1f);
				audioSources.PlayOneShot (appear);

				// ポイント加算
				point += selectPoint;
				pointText.text = point.ToString () + " p";
				pointUpText.text = "+ " + selectPoint.ToString () + " p";
				Invoke ("Ngo", 1.0f);
				fairyAppearFlags [i] = false;
			}
		}
	}


	// エフェクトのあとに妖精だす
	public void Apa(){
		Instantiate (selectFairy, fairyPosition, Quaternion.Euler(0,180,0));
		Invoke ("Tyai", 1);
	}

	public void Tyai(){
		audioSources.PlayOneShot (disappear);
	}


	// ポイントテキストを消す
	public void Ngo(){
		pointUpText.text = "";
	}


	// 一定の距離間隔で息を吐く
	public void TakeBreath(){
		for(int i=0; i<20; i++){
			if(meter == 40*i && takeBreathFlags[i] == true){
				//Instantiate (bubble, mainCamera.transform.position + new Vector3(2,-1.5f,10), Quaternion.identity);
				takeBreathFlags [i] = false;
			}
		}
	}


	// 相手の攻撃
	public void EnemyAttack(){
		if (mainCamera.transform.position.z > 180) {
			beamTimeCounter1 += Time.deltaTime;
			if (beamTimeCounter1 > 4){
				for (int i = 0; i < beams1.Length; i++) {
					Instantiate (beams1 [i]);
				}
				audioSources.PlayOneShot (beam);
				Invoke ("BeamExplosion", 0.5f);
				beamTimeCounter1 = 0;
			}
		}
		if (mainCamera.transform.position.z > 220) {
			if (enemy1.transform.position.y < 0 && enemy2.transform.position.y < 0) {
				enemy1.transform.position += new Vector3 (0, 2, 0);
				enemy2.transform.position += new Vector3 (0, 2, 0);
				if(enemyFlag == false){
					audioSources.PlayOneShot (enemy);
					enemyFlag = true;
				}
			} else {
				beamTimeCounter2 += Time.deltaTime;
				if (beamTimeCounter2 > 4) {
					for (int i = 0; i < beams2.Length; i++) {
						Instantiate (beams2 [i]);
					}
					audioSources.PlayOneShot (beam);
					Invoke ("BeamExplosion", 0.5f);
					beamTimeCounter2 = 0;
				}
			}
		}
	}


	void BeamExplosion(){
		for (int i = 0; i < smallExplosionPositions.Length; i++) {
			Instantiate (explosion, smallExplosionPositions [i], Quaternion.identity);
		}
		audioSources.PlayOneShot (smallExplosion);
	}


	// 反撃
	public void Burst(){
		// 着地
		if(mainCamera.transform.position.z > 260 && freedom.transform.position.y > 0){
			freedom.transform.position -= new Vector3 (0,2,0);
			if(freedomFlag == false){
				audioSources.PlayOneShot (landing);
				freedomFlag = true;
			}

		}
		// 攻撃
		if (mainCamera.transform.position.z > 270 && burstFlag == false) {
			for (int i = 0; i < bursts.Length; i++) {
				Instantiate (bursts [i]);
			}
			audioSources.PlayOneShot (burst);
			Invoke ("BurstExplosion", 0.5f);
			burstFlag = true;
		}
	}
		

	void BurstExplosion(){
		for (int i = 0; i < largeExplosionPositions.Length; i++) {
			Instantiate (explosion, largeExplosionPositions [i],Quaternion.identity);
		}
		audioSources.PlayOneShot (largeExplosion);
	}


	// ゲーム終了時
	public void FinishGame(){
		if(time < 1){
			messageText.enabled = true;

			if(endFlag == false){
				messageText.text = "Finish!!";
				Invoke ("ShowScore", 3.0f);
				endFlag = true;
			}

		}
	}


	// スコアを見せる
	public void ShowScore(){
		messageText.text = "You get " + point.ToString () + "p";

		Invoke ("ShowRanking", 3.0f);
	}


	// スコアを見せた後，ランキングを見せる
	public void ShowRanking(){
		point1st = PlayerPrefs.GetInt ("1st_sky");
		point2nd = PlayerPrefs.GetInt ("2nd_sky");
		point3rd = PlayerPrefs.GetInt ("3rd_sky");

		if(point > point1st){
			PlayerPrefs.SetInt ("1st_sky", point);
			PlayerPrefs.SetInt ("2nd_sky", point1st);
			PlayerPrefs.SetInt ("3rd_sky", point2nd);
		}else if(point > point2nd){
			PlayerPrefs.SetInt ("2nd_sky", point);
			PlayerPrefs.SetInt ("3rd_sky", point2nd);
		}else if(point > point3rd){
			PlayerPrefs.SetInt ("3rd_sky", point);
		}

		messageText.text = "1st:" + PlayerPrefs.GetInt ("1st_sky").ToString () + "\n" + "2nd:" + PlayerPrefs.GetInt ("2nd_sky").ToString() + "    3rd:" + PlayerPrefs.GetInt ("3rd_sky").ToString();
		backTitle.SetActive(true);
		backTest.SetActive(true);
		GameObject obj = Instantiate (pointer, mainCamera.transform.position + new Vector3(0,0,0.44f), Quaternion.identity) as GameObject;
		obj.transform.parent = mainCamera.transform;
	}


	public void BackTest(){
		Debug.Log ("BackTest");
		SceneManager.LoadScene ("Test");
	}

	public void Replay(){
		Debug.Log ("Replay");
		SceneManager.LoadScene ("SkyPlay");

	}

	//終了処理
	private void OnApplicationQuit (){
		socket.Close ();
		PlayerPrefs.DeleteKey ("status");
	}
}
