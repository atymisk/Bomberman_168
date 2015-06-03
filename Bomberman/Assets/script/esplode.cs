using UnityEngine;
using System.Collections;

public class esplode : MonoBehaviour {

	public bool active = false;
	private int timer;
	// Use this for initialization
	void Awake () 
	{
		active = false;
		timer = 1;
		Debug.Log ("Make me");
		//this.gameObject.SetActive(false);
	}

	// Update is called once per frame
	void FixedUpdate () 
	{
		//Debug.Log (Time.deltaTime);
		//Debug.Log(timer % 700 == 0 && active);
		if(active)
		{
			if(timer % 85 == 0)
			{
				Debug.Log("meep");
				timer = 0;
				active = false;
				Destroy (this.gameObject);//destroy the effect after
			}
			timer++;
		}

	}
}
