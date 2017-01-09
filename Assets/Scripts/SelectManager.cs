using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SelectManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void SelectReg(){
		SceneManager.LoadScene ("SeaTitle");
	}

	public void SelectArm(){
		SceneManager.LoadScene ("SkyTitle");
	}
}
