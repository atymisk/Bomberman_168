using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class chat : MonoBehaviour 
{
	public GameObject panel;
	public Text t;
	// Use this for initialization
	void Start () 
	{
		if(!Client.connected)
		{
			//t.gameObject.SetActive(false);
		//	panel.SetActive(false);
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}
}
