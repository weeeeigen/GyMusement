using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SkyTitleManager : MonoBehaviour {
	public Camera mainCamera;

	public GameObject startButton;
	public Text startText;

	public bool flag;

	public AudioClip bgm;
	private AudioSource audioSource;

	// Use this for initialization
	void Start () {
		startButton.SetActive (false);

		audioSource = gameObject.GetComponent<AudioSource> ();
		audioSource.clip = bgm;
		audioSource.Play ();
	}
	
	// Update is called once per frame
	void Update () {
		if (mainCamera.transform.position.y < 600) {
			mainCamera.transform.position += new Vector3 (0, 1, 0);
		}else{
			if (flag == false) {
				Invoke ("ShowGUI",1);
			}
		}
	}

	public void ShowGUI(){
		startButton.SetActive (true);
	}

	public void LoadPlay(){
		startText.text = "Loading...";
		SceneManager.LoadScene ("SkyPlay");
	}
	//終了処理
	private void OnApplicationQuit (){
		PlayerPrefs.DeleteKey ("status");
	}
}
