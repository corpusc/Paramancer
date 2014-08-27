using UnityEngine;
using System.Collections;

public class Health : MonoBehaviour {
	public float MaxHealth=100;
	public float CurrentHealth;
	public bool Invincible;
	public bool Dead;
	
	
	// Use this for initialization
	void Start () {
		//MAKE THE CURRENT HEALTH THE MAX HEALTH AT START
	CurrentHealth=MaxHealth;
	}
	
	// Update is called once per frame
	void Update () {
	
		//IF INVINCIBLE, HE CANNOT DIE..
		if(Invincible){
		CurrentHealth=MaxHealth;	
		}
		else{
		if(CurrentHealth<=0){
			CurrentHealth=0;
			Dead=true;
		}	
			
		//MAX HEALTH
			if(CurrentHealth>=MaxHealth)CurrentHealth=MaxHealth;
			
			//WHEN DEATH IS UPON HIM
		if(Dead){
				//TELL THE AI SCRIPT HE IS DEAD
			FreeAI AI=(FreeAI)GetComponent("FreeAI");
				if(AI){
			if(AI.IsDead){}
			else AI.IsDead=true;
		}
		}
		}
	
	}
}
