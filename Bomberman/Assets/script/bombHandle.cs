using UnityEngine;
using System.Collections;

public class bombHandle : MonoBehaviour {
	public Client client;
	public GameObject bomb;
	void FixedUpdate(){
		string data = client.GetComponent<Client> ().GetBdata ();
		if (data.Contains ("Bomb;")) {
			int found = data.IndexOf ("Bomb;");
			data = data.Substring (found + 5);
			found = data.IndexOf (";");
			float x = float.Parse (data.Substring (0, found));
			data = data.Substring (found + 1);
			found = data.IndexOf (";");
			float z = float.Parse (data.Substring (0, found));
			data = data.Substring (found + 1);
			found = data.IndexOf (";");
			GameObject creation = Instantiate (bomb, new Vector3 (0, .5f, 1), Quaternion.identity)as GameObject;
			creation.GetComponent<bombLogic> ().active = true;
			client.GetComponent<Client> ().deleteBData();
		}
	}
}
