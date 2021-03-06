﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class register : MonoBehaviour {
	public InputField user;//12 characters max
	public InputField pass;//6 min, 12 max
	public Text t;
	bool ok = false;
	public string dbg = "*";
	string tosend="";
	public void submit()
	{
		//check each field
		dbg = "*";
		string u = user.text;
		ok = (u.Length >= 4 && u.Length <=12);//need to check against special characters
		tosend = u + "|";
		if(!ok)
		{
			dbg += "Username length invalid\n";
		}

		u = pass.text;
		ok = (u.Length >= 6 && u.Length <=12);//need to check against special characters
		//hash u(the password)
        tosend += MD5Manager.hashPassword(u);
		if(!ok)
		{
			dbg += "Password length invalid\n";
			tosend = "invalid";
		}
		//Debug.Log(tosend);
		t.text = dbg;
	}
	public string getsendinfo()
	{
		submit();
		return tosend;
	}

	public void goback()
	{
		Application.LoadLevel("LogInScreen");
	}
}
