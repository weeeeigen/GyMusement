using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using SocketIO;
using MiniJSON;

public class SeaPlayManager :  MonoBehaviour{
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

	// 魚
	public GameObject[] normalFishes;
	public GameObject[] rareFishes;
	public GameObject[] superFishes;
	public GameObject selectFish;

	// 酸素，出現エフェクト
	public GameObject bubble;
	public GameObject effect;

	// 出現座標とフラグ
	public Vector3[] fishPositions;
	public bool[] fishAppearFlags;
	public bool[] takeBreathFlags;
	public Vector3 fishPosition;
	public Vector3 fishScale;
	public bool fishFlag;

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

	// リズムのタイミング，0がUp
	public int[] upDownTime;
	public float upDownTimeCounter;
	public string[] upDownMessage;
	public int upDownNum;

	// リズムのフラグ，trueがup
	public bool upDownFlag;

	// 魚のポイント
	public int[] normalFishesPoint;
	public int[] rareFishesPoint;
	public int[] superFishesPoint;
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
	public AudioClip bubbles;
	public AudioClip up;
	public AudioClip down;
	private AudioSource audioSources;

	// ソケット
	private SocketIOComponent socket;

	// Use this for initialization
	void Start () {
		//SocketIOオブジェクトを読み込む&サーバと接続
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
			fishPositions[i] = (new Vector3 (-6, -1, 30 * i - 80));
			fishAppearFlags[i] = true;
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

			// 魚を出現させる
			FishAppear ();

			// 息を吐く
			TakeBreath ();

			// ゲーム終了
			FinishGame();

		}else if(startFlag == false){
			GameReady ();
		}

		Test ();
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

		meter = Mathf.FloorToInt (mainCamera.transform.position.z) + 115;
		meterText.text = meter.ToString () + " m";
	}


	// 一定の距離間隔で魚を出現、ポイント加算
	public void FishAppear(){
		for(int i=0; i<20; i++){
			if(meter == 30*i && fishAppearFlags[i] == true){

				// 速さに応じて魚とポイントを変える
				int num;

				if (rhythm == "veryFast" || rhythm == "verySlow") {
					num = Random.Range (0, normalFishes.Length);
					selectFish = normalFishes [num];
					selectPoint = normalFishesPoint [num];
					fishScale = new Vector3 (1,1,1);
					fishFlag = false;
				} else if (rhythm == "fast" || rhythm == "slow") {
					num = Random.Range (0, rareFishes.Length);
					selectFish = rareFishes [num];
					selectPoint = normalFishesPoint [num];
					fishScale = new Vector3 (1.3f,1.3f,1.3f);
					fishFlag = false;
				} else if (rhythm == "good") {
					num = Random.Range (0, superFishes.Length);
					selectFish = superFishes [num];
					selectPoint = normalFishesPoint [num];
					fishScale = new Vector3 (1.3f,1.3f,1.3f);
					fishFlag = true;
				}

				//カウントが減るたびに効果音再生
				if(upDownNum == 0){
					audioSources.PlayOneShot (up);
				} else if(upDownNum == 1){
					audioSources.PlayOneShot (down);
				}

				fishPosition = fishPositions [i];

				// まずエフェクトが出現
				Instantiate (effect, fishPositions [i] - new Vector3(-1, 5, 0), Quaternion.identity);
				audioSources.PlayOneShot (appear);
				Invoke ("Apa", 1f);

				// ポイント加算
				point += selectPoint;
				pointText.text = point.ToString () + " p";
				pointUpText.text = "+ " + selectPoint.ToString () + " p";
				Invoke ("Ngo", 1.0f);
				fishAppearFlags [i] = false;
			}
		}
	}


	// エフェクトのあとに魚だす
	public void Apa(){
		GameObject obj = Instantiate (selectFish, fishPosition, Quaternion.Euler(270,0,90)) as GameObject;
		obj.transform.localScale = fishScale;

		if(fishFlag == true){
			Instantiate (selectFish, fishPosition - new Vector3(-3,3,1.3f),Quaternion.Euler(270,0,90));
			Instantiate (selectFish, fishPosition - new Vector3(-5,-2,-1),Quaternion.Euler(270,0,90));
		}
	}


	// ポイントテキストを消すunity
	public void Ngo(){
		pointUpText.text = "";
	}


	// 一定の距離間隔で息を吐く
	public void TakeBreath(){
		for(int i=0; i<20; i++){
			if(meter == 40*i && takeBreathFlags[i] == true){
				Instantiate (bubble, mainCamera.transform.position + new Vector3(2,-1.5f,10), Quaternion.identity);
				takeBreathFlags [i] = false;

				audioSources.PlayOneShot (bubbles);
			}
		}
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
		point1st = PlayerPrefs.GetInt ("1st_sea");
		point2nd = PlayerPrefs.GetInt ("2nd_sea");
		point3rd = PlayerPrefs.GetInt ("3rd_sea");

		if(point > point1st){
			PlayerPrefs.SetInt ("1st_sea", point);
			PlayerPrefs.SetInt ("2nd_sea", point1st);
			PlayerPrefs.SetInt ("3rd_sea", point2nd);
		}else if(point > point2nd){
			PlayerPrefs.SetInt ("2nd_sea", point);
			PlayerPrefs.SetInt ("3rd_sea", point2nd);
		}else if(point > point3rd){
			PlayerPrefs.SetInt ("3rd_sea", point);
		}

		messageText.text = "1st:" + PlayerPrefs.GetInt ("1st_sea").ToString () + "\n" + "2nd:" + PlayerPrefs.GetInt ("2nd_sea").ToString() + "    3rd:" + PlayerPrefs.GetInt ("3rd_sea").ToString();
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
		SceneManager.LoadScene ("SeaPlay");

	}


	public void Test(){
		if(Input.GetKey(KeyCode.LeftArrow)){
			rhythm = "verySlow";
		}else if(Input.GetKey(KeyCode.DownArrow)){
			rhythm = "slow";
		}else if(Input.GetKey(KeyCode.RightArrow)){
			rhythm = "good";
		}
	}
		
	//終了処理
	private void OnApplicationQuit (){
		socket.Close ();
		PlayerPrefs.DeleteKey ("status");
	}
}