using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UImethods : MonoBehaviour {

	public void register()
	{
		Application.LoadLevel("Register");
	}
	public void backtologin()
	{
		Application.LoadLevel("LogInScreen");
	}

}
