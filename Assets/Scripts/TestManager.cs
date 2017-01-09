using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SocketIO;
using MiniJSON;


public class TestManager : MonoBehaviour {
	public GameObject sea;
	public GameObject regImage;
	public GameObject armImage;
	public GameObject point1Button;
	public GameObject point2Button;
	public GameObject point3Button;

	// ボタンのテキストとか
	public Text title;
	public Text point1;
	public Text point2;
	public Text point3;

	// 各フラグ
	public bool point1Flag;
	public bool point2Flag;
	public bool point3Flag;
	public bool testFlag;

	public AudioClip bgm;
	private AudioSource audioSource;

	private SocketIOComponent socket;

	// Use this for initialization
	void Start () {

		GameObject go = GameObject.Find ("SocketIO");
		socket = go.GetComponent<SocketIOComponent> ();
		if (PlayerPrefs.GetString ("status") != "ok") {
			StartCoroutine (ConnectToServer ());
		}
		socket.On ("USER_CONNNECTED", OnUserConnected);
		socket.On ("SENSOR", OnSensorChoose);

		audioSource = gameObject.GetComponent<AudioSource> ();
		audioSource.clip = bgm;
		audioSource.Play ();

		// 画像とボタンを隠す
		regImage.SetActive (false);
		armImage.SetActive (false);
		point2Button.SetActive (false);
		point3Button.SetActive (false);
	
	}

	IEnumerator ConnectToServer() {
		yield return new WaitForSeconds (0.5f);
		Dictionary<string, string> data = new Dictionary<string, string>();
		data["name"] = "Eigen";
		socket.Emit ("PLAY", new JSONObject (data));
	}

	// Server接続時に呼ばれる
	private void OnUserConnected(SocketIOEvent e){
		Debug.Log (e.data + "connected to server");
	}
		
	//センサータグとスマホを紐付ける
	private void OnSensorChoose(SocketIOEvent e){
		PlayerPrefs.SetString ("status", "ok");
		PlayerPrefs.SetString ("sensorId", Json.Deserialize( e.data ["sensorId"].ToString ()) as string);
		ChangeScene(Json.Deserialize( e.data ["game"].ToString ()) as string);
		Debug.Log ("Connected to " + e.data ["sensorId"]);
	}
	
	// Update is called once per frame
	void Update () {

		sea.transform.position += new Vector3 (0,0,-0.05f);

		// 視線のテストが終わった後
		if(point1Flag == true && point2Flag == true && point3Flag == true){
			testFlag = true;
			title.text = "Select Training Type";
			regImage.SetActive (true);
			armImage.SetActive (true);
			point3Button.SetActive (false);
			point1.text = "Leg";
			point2.text = "Arm";
		}
	
	}
		
	public void Point1Enter(){
		if(testFlag == false){
			Debug.Log ("Point1");
			point1.text = "OK";
			point1Flag = true;

			// 次のボタンを見えるようにする
			point2Button.SetActive (true);
		}else if(testFlag == true){
			Dictionary<string,string> data = new Dictionary<string,string>();
			data["game"] = "sea";
			socket.Emit("GAME", new JSONObject(data));
			ChangeScene (data["game"]);
		}
	}

	public void Point2Enter(){
		if (testFlag == false) {
			Debug.Log ("Point2");
			point2.text = "OK";
			point2Flag = true;

			// 次のボタンを見えるようにする
			point3Button.SetActive (true);
		}else if(testFlag == true){
			Dictionary<string,string> data = new Dictionary<string,string>();
			data["game"] = "sky";
			socket.Emit("GAME", new JSONObject(data));
			ChangeScene (data["game"]);
		}
	}

	public void Point3Enter(){
		if (testFlag == false) {
			Debug.Log ("Point3");
			point3.text = "OK";
			point3Flag = true;
		}
	}

	private void ChangeScene(string game){
		Dictionary<string,string> data = new Dictionary<string,string>();
		data["game"] = game;
		data["sensorId"] = PlayerPrefs.GetString ("sensorId");
		if (PlayerPrefs.GetString ("status") == "ok") {
			socket.Emit ("SENSORID", new JSONObject(data));
			if (game == "sea") {
				SceneManager.LoadScene ("SeaTitle");
			} else if (game == "sky") {  
				SceneManager.LoadScene ("SkyTitle");
			}
		}
	}
	//終了処理
	private void OnApplicationQuit (){
		socket.Close ();
		PlayerPrefs.DeleteKey ("status");
	}
}
