using UnityEngine;
using System.Collections;

public class Fairy : MonoBehaviour {

	public float timeCounter;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		timeCounter += Time.deltaTime;
		if(timeCounter > 1){
			this.gameObject.transform.position += new Vector3 (0,1,0);

		}
		Destroy (this.gameObject, 5.0f);
	}

}
