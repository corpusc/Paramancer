using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FreeAI : MonoBehaviour {
	//THE CHARACTER COLLISION LAYER FOR TARGETS
	public Transform AICharacter;
	
	public int CharacterCollisionLayer=15;
	//ENABLES MELEE COMBAT
	public bool EnableCombat=true;
	//THE TARGET WHICH HE FOLLOWS AND ATTACKS
	public Transform Target;
	//THE VECTOR OF THE TARGET
	private Vector3 CurrentTarget;
	//TARGET VISIBIL BOOL
	private bool TargetVisible;
	private bool MoveToTarget;
	//SPEED WHICH THE AI TURNS
	public float turnspeed=5;
	//SPEED WHICH AI RUNS
	public float runspeed=4;
	public float Damage=10;
	public float AttackSpeed=1;
	public float AttackRange=5;
	
	//WHEN THE DAMAGE HAS BEEN DEALT
	private bool damdealt;
	

	//ANIMATIONS
	public AnimationClip RunAnimation;
	public AnimationClip IdleAnimation;
	public AnimationClip AttackAnimation;
	public AnimationClip DeathAnimation;
	private bool stop;
	private bool Swing;
	public bool IsDead;
	private bool DeadPlayed;
	
	
	private float Atimer;
	private bool startfollow;
	//PATHFINDING STUFF
	public bool EnableFollowNodePathFinding;
	public bool DebugShowPath;
	public float DistanceNodeChange=1.5f;
	public List<Vector3> Follownodes;
	private int curf;
	
	
	// Use this for initialization
	void Start () {
		if(AICharacter){}
		else AICharacter=transform;

		
	}
	
	// Update is called once per frame
	void Update () {
		
		
		
	if(IsDead){
			if(DeathAnimation){
			if(DeadPlayed){}
            else AICharacter.animation.CrossFade( DeathAnimation.name, 0.1f);	
			DeadPlayed=true;
			}
		}
		else{
		
		//COMBAT BEHAVE
		if(Target){
		float Tdist=Vector3.Distance(Target.position, transform.position);	
		if(Tdist<=AttackRange){
				if(TargetVisible)stop=true;
			}
			else stop=false;
			
		//RAYCAST VISION SYSTEM	
		RaycastHit hit = new RaycastHit();	
		LayerMask lay=CharacterCollisionLayer;
		Vector3 pdir = (Target.transform.position - transform.position).normalized;
		float playerdirection = Vector3.Dot(pdir, transform.forward);
		if(Physics.Linecast(transform.position, Target.position, out hit, lay)){
			TargetVisible=false;	
			}
			else{
			if(playerdirection > 0){
						startfollow=true;
						TargetVisible=true;	
					}
				//TargetVisible=false;	
			}

		}
		//IF THE TARGET IS VISIBLE
		if(TargetVisible){
			CurrentTarget=Target.position;
				MoveToTarget=true;
		}
		
		
		
		
		//MOVES/RUNS TO TARGET
		if(MoveToTarget){
			if(stop){}
			else{
		transform.position += transform.forward * +runspeed * Time.deltaTime;
			}
			if(RunAnimation){
				if(stop){
						//COMBAT!
						if(EnableCombat){
							Health hp=(Health)Target.transform.GetComponent("Health");	
									if(hp.CurrentHealth>0){
						Atimer+=Time.deltaTime;	
					AICharacter.animation[AttackAnimation.name].speed = AICharacter.animation[AttackAnimation.name].length / AttackSpeed;
					AICharacter.animation.CrossFade( AttackAnimation.name, 0.12f);	
						if(damdealt){}
							else{
						if(Atimer>=AttackSpeed*0.35&Atimer<=AttackSpeed*0.45){
							//LETS DO SOME DAMAGE!
								if(hp){
								hp.CurrentHealth=hp.CurrentHealth-Damage;
										damdealt=true;
								}
							}
							}
							
							if(Atimer>=AttackSpeed){
									damdealt=false;
									Atimer=0;
								}
								
							}
							else AICharacter.animation.CrossFade( IdleAnimation.name, 0.12f);
					
				}
					else AICharacter.animation.CrossFade( IdleAnimation.name, 0.12f);
				}
			else{
						Atimer=0;
			AICharacter.animation.CrossFade( RunAnimation.name, 0.12f);
				}
			}
			}
		else{
			if(IdleAnimation){
			AICharacter.animation.CrossFade( IdleAnimation.name, 0.12f);
			}
		}
		
		
		//FOLLOW PATHFINDING
		if(TargetVisible){}
		else{
			if(EnableFollowNodePathFinding&startfollow){
			if(Follownodes.Count<=0)Follownodes.Add(CurrentTarget);
		
				RaycastHit hit = new RaycastHit();	
		LayerMask lay=CharacterCollisionLayer;
					
			if(Physics.Linecast(Follownodes[Follownodes.Count-1], Target.position, out hit, lay)){	
						Follownodes.Add(Target.position);
					
				}
				

				float dist=Vector3.Distance(transform.position, Follownodes[0]);
					if(dist<DistanceNodeChange){
						 Follownodes.Remove(Follownodes[0]);
					}
					}
			}
		}
		
		
		if(TargetVisible&Follownodes.Count>0){
				Follownodes.Clear();
				}
		
		if(DebugShowPath){
				if(Follownodes.Count>0){
				int listsize=Follownodes.Count;
				Debug.DrawLine(Follownodes[0], transform.position, Color.green);
			for (int i = 0; i < listsize; i++)
					if(i<Follownodes.Count-1){
					{
					Debug.DrawLine(Follownodes[i], Follownodes[i+1], Color.green);
				
					}
					
				}
				
			}
		
	
			
		
		//POINT AT TARGET
		if(MoveToTarget){
			if(Follownodes.Count>0){
			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Follownodes[0] - transform.position), turnspeed * Time.deltaTime);	
			}
			else{
			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(CurrentTarget - transform.position), turnspeed * Time.deltaTime);	
				
			}
	
		}
			
		transform.eulerAngles = new Vector3(0,transform.eulerAngles.y,0);
	}
	}
}
