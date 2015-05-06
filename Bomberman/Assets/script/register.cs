using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class register : MonoBehaviour {
	public InputField user;//12 characters max
	public InputField pass;//6 min, 12 max
	public InputField fullname;
	public InputField pin;//4digits
	public Text t;
	bool ok = false;
	string dbg = "*";
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
		tosend += u + "|";
		if(!ok)
		{
			dbg += "Password length invalid\n";
		}
		u = fullname.text;
		ok = (u.Length != 0);//need to check against special characters
		tosend += u + "|";
		if(!ok)
		{
			dbg += "Name is blank\n";
		}
		u = pin.text;
		ok = (u.Length == 4);//need to check for numbers only
		tosend += u + "|";
		if(!ok)
		{
			dbg += "Invalid pin\n";
			tosend = "b";
		}

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
