using UnityEngine;

using System.Collections;


public class Feeler : MonoBehaviour {


	public Gem owner;

	void OnTriggerEnter(Collider c){
		if(c.tag == "Gem") //prevents Feelers and other objects from being added
		{
            Gem gemToAdd = c.GetComponent<Gem>();
            if(gemToAdd != null) //just-in-case testing
                owner.AddNeighbor(gemToAdd);
        }

		}
	void OnTriggerExit(Collider c){

		if(c.tag == "Gem")
		{
            Gem gemToRemove = c.GetComponent<Gem>();
            if(gemToRemove != null)
                owner.RemoveNeighbor(gemToRemove);

        }

		

		}


}
