using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SeaTitleManager : MonoBehaviour {
	public Camera mainCamera;

	public GameObject bubble;

	public GameObject whale;
	public GameObject fish;

	public Text startText;

	public float time;

	public AudioClip bgm;
	private AudioSource audioSource;

	// Use this for initialization
	void Start () {
		audioSource = gameObject.GetComponent<AudioSource> ();
		audioSource.clip = bgm;
		audioSource.Play ();
	}
	
	// Update is called once per frame
	void Update () {
		time += Time.deltaTime;
		if(time > 4){
			Instantiate (bubble, mainCamera.transform.position + new Vector3(2,-3,3), Quaternion.identity);
			time = 0;
		}
		if(mainCamera.transform.position.y > -1){
			mainCamera.transform.position -= new Vector3 (0, 0.05f, 0);
		}

		fish.transform.position += new Vector3(0.03f, 0, 0);
		whale.transform.position -= new Vector3(0.05f, 0, 0.05f);
	}

	public void LoadPlay(){
		startText.text = "Loading...";
		SceneManager.LoadScene ("SeaPlay");
	}
	//終了処理
	private void OnApplicationQuit (){
		PlayerPrefs.DeleteKey ("status");
	}
}