using UnityEngine;

using System.Collections;


public class Feeler : MonoBehaviour {


	public Gem owner;

	void OnTriggerEnter(Collider c){
		if(c.tag == "Gem")
		{
		owner.AddNeighbor(c.GetComponent<Gem>());
		}

		}
	void OnTriggerExit(Collider c){

		if(c.tag == "Gem")
		{
		owner.RemoveNeighbor(c.GetComponent<Gem>());
		}

		

		}


}
