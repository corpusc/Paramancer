using UnityEngine;
using System.Collections;



//  === RibbonScript.cs by Sophie Houlden - http://sophiehoulden.com - @S0phieH===
// Version 1.0

/*
Instructions:
place this script on the root of a bone chain, all children become part of the ribbon.
this is handy for ribbon/hair/scarf/other flowy effects.

the root bone will not be any more dynamic, the root bone direction is what determines
the influence over the rest (if any).

 ~ ~ ~
if you are looking to mod the behaviour of the ribbon beyond what the public properties do,
the ActRibbonPiece() function is what moves the ribbon pieces, calculates their forces etc,
that's the one you will want to mod :)
*/



public class RibbonScript: MonoBehaviour {
	public float linkGravity = 0.015f; // how much gravity effects links 
	public float maxForceStrength = 0.8f;// ...a link can have 
	public float drag = 0.9f;// force reduced by this 

	// force from root 
	public Vector3 rootForceDirection = new Vector3(0,0,1);
	public float rootStrength = 0.05f;
	public float rootInfluence = 0.4f;
	
	// for ribbons nested in ribbons,  when calculating positions, we need to start with the 'topmost' ribbon in the hierachy. 
	// this property lets us know if this is the script to start the process, or if another will get it going. 
	[HideInInspector]
	public bool isCompleteRoot = true;



	// private 
	bool initialised = false;
	RibbonPiece[] ribbon;
	Vector3 worldUp = Vector3.up; // make this public if gravity is not standard, or to tweak it at runtime for a wind effect 



	void Start() {
		//normalise the direction of the root force, otherwise it's strengh will get effected
		rootForceDirection.Normalize();
	
		//find out how many links we need (for our array lengths)
		int numLinks = CountBones(transform);
		
		//make our ribbon array
		ribbon = new RibbonPiece[numLinks];
		
		//place the child bones in our ribbon array
		PopulateRibbonArray(transform, 0);
		
		//before we start to change the hierachy, lets make sure which links know what links are their children
		for (int i=0; i<ribbon.Length; i++) {
			foreach (Transform child in ribbon[i].bone.transform) {
				if (child.GetComponent<UnRibbon>() == null && child.GetComponent<RibbonScript>() == null && child.gameObject.name!= "RIBBONROOT"){
					//child is a bone in the ribbon, let's reference it
					for (int j=0; j<ribbon.Length; j++) {
						if (ribbon[j].bone.transform == child) {
							ribbon[i].AddChild(j);
							ribbon[j].parentIndex = i;
						}
					}
				}else if (child.GetComponent<RibbonScript>() != null) {
					//child is another ribbon, let's reference it
					ribbon[i].AddChildRibbon(child.GetComponent<RibbonScript>());
					child.GetComponent<RibbonScript>().isCompleteRoot = false;
				}else if (child.gameObject.name== "RIBBONROOT") {
					//child is another (instantiated) ribbon, let's reference it
					foreach (Transform childB in child) {
						if (childB.GetComponent<RibbonScript>() != null){
							ribbon[i].AddChildRibbon(childB.GetComponent<RibbonScript>());
							childB.GetComponent<RibbonScript>().isCompleteRoot = false;
						}
					}
				}
			}
		}
		
		//make holder objects so we can move bones and whatever crazy orientation they had to start with doesent matter
		//especially important since different 3D tools may orient bones differently.
		ribbon[0].boneHolder = new GameObject();
		ribbon[0].boneHolder.name = "RIBBONROOT";
		ribbon[0].boneHolder.transform.position = ribbon[0].bone.transform.position;
		//make it look the right way
		ribbon[0].boneHolder.transform.LookAt(ribbon[1].bone.transform.position);
		ribbon[0].boneHolder.transform.LookAt(ribbon[0].boneHolder.transform.position - ribbon[0].boneHolder.transform.forward);
		//make sure the bone is a child of the holder, also that the root holder is in the same place on the armature
		ribbon[0].boneHolder.transform.parent = transform.parent;
		ribbon[0].bone.transform.parent = ribbon[0].boneHolder.transform;
		
		//do it for the rest of the ribbon
		for (int i=1; i<ribbon.Length; i++) {
			ribbon[i].boneHolder = new GameObject();
			ribbon[i].boneHolder.name = "RIBBON_" + i.ToString() + "_" + ribbon[0].bone.name + "& " + ribbon[i].parentIndex.ToString();
			ribbon[i].boneHolder.transform.position = ribbon[i].bone.transform.position;
			ribbon[i].boneHolder.transform.LookAt(ribbon[ribbon[i].parentIndex].bone.transform.position);
			ribbon[i].boneHolder.transform.parent = ribbon[0].boneHolder.transform;
			ribbon[i].bone.transform.parent = ribbon[i].boneHolder.transform;
		}
		
		//find out how long each link is.
		for (int i=1; i<ribbon.Length; i++) {
			ribbon[i].linkLength = Vector3.Distance(ribbon[i].boneHolder.transform.position,ribbon[ribbon[i].parentIndex].boneHolder.transform.position);
		}
		
		initialised = true;
		
	}
	
	int CountBones(Transform parentLink) {
		int runningTotal = 1;
		foreach (Transform child in parentLink) {
			// if not another ribbon or ribbon blocker 
			if (child.GetComponent<UnRibbon>() == null && 
			    child.GetComponent<RibbonScript>() == null && 
			    child.gameObject.name!= "RIBBONROOT"
		    ) {
				runningTotal += CountBones(child); // will be part of our array, count it's children 
			}
		}

		return runningTotal;
	}
	
	int PopulateRibbonArray(Transform parentlink, int ribbonIndex) {
		ribbon[ribbonIndex].bone = parentlink.gameObject;
		ribbon[ribbonIndex].position = ribbon[ribbonIndex].bone.transform.position;
		ribbonIndex++;

		foreach (Transform child in parentlink){
			// if not another ribbon or ribbon blocker 
			if (child.GetComponent<UnRibbon>() == null && 
			    child.GetComponent<RibbonScript>() == null && 
			    child.gameObject.name!= "RIBBONROOT"
		    ) {
				ribbonIndex = PopulateRibbonArray(child, ribbonIndex); // add it and it's children to the array 
			}
		}

		return ribbonIndex;
	}
	
	void FixedUpdate() {
		// if topmost ribbon in this hierachy 
		if (isCompleteRoot) {
			RibbonAction();
		}
	}
	
	public void RibbonAction() {
		ActRibbonPiece(0,rootStrength);
	}
	
	// recursively called to do the actual work of the ribbonScript
	void ActRibbonPiece(int ribbonIndex, float currentRootInfluence) {
		if (ribbonIndex > 0) {//don't do this to the root
			
			// move base on forces (yes, I do this first =p)
			ribbon[ribbonIndex].position+= ribbon[ribbonIndex].forces;
			
			// our ribbons aren't stretchy, make sure the distance between links isnt too great 
			if (
				Vector3.Distance(
					ribbon[ribbon[ribbonIndex].parentIndex].position,
					ribbon[ribbonIndex].position
				)

				>

				ribbon[ribbonIndex].linkLength
		    ) {
				//gone too far, place it back
				Vector3 oldPos = ribbon[ribbonIndex].position;
				Vector3 offset = ribbon[ribbonIndex].position-ribbon[ribbon[ribbonIndex].parentIndex].position;
				offset.Normalize();
				ribbon[ribbonIndex].position= ribbon[ribbon[ribbonIndex].parentIndex].position  + (offset * ribbon[ribbonIndex].linkLength);
				
				//since it's been pulled towards it's parent, lets add some of that force to this ribbon piece
				ribbon[ribbonIndex].forces+= (ribbon[ribbonIndex].position-oldPos)*0.8f;
			}
			
			// drag 
			ribbon[ribbonIndex].forces *= drag;
			
			// gravity 
			ribbon[ribbonIndex].forces+= worldUp * -linkGravity;
			
			// root influence 
			Vector3 influenceDirection = (-ribbon[0].boneHolder.transform.right*rootForceDirection.x) + (-ribbon[0].boneHolder.transform.up*rootForceDirection.y) + (-ribbon[0].boneHolder.transform.forward*rootForceDirection.z);
			influenceDirection.Normalize();
			ribbon[ribbonIndex].forces += influenceDirection * currentRootInfluence;
			
			// terminal velocity required, or shit gets crazy when moving fast 
			if (ribbon[ribbonIndex].forces.magnitude>maxForceStrength){
				ribbon[ribbonIndex].forces.Normalize();
				ribbon[ribbonIndex].forces*=maxForceStrength;
			}
		}else{
			// this is the root ribbon piece, since it's movement is what everything else is determined by, 
			// we should keep track when it changes position :) 
			ribbon[0].position = ribbon[0].boneHolder.transform.position;
		}
		
		if (ribbon[ribbonIndex].childIndeces != null) {
			for (int i=0; i<ribbon[ribbonIndex].childIndeces.Length; i++) {
				// act on the children of this ribbon piece 
				ActRibbonPiece(ribbon[ribbonIndex].childIndeces[i], currentRootInfluence*rootInfluence);
			}
		}
		
		if (ribbon[ribbonIndex].childRibbons != null) {
			for (int i=0; i<ribbon[ribbonIndex].childRibbons.Length; i++) {
				// act on the children RibbonScripts of this ribbon piece 
				ribbon[ribbonIndex].childRibbons[i].RibbonAction();
			}
		}
		
	}
	
	
	// all gameObjects are positioned in LateUpdate 
	void LateUpdate() {
		if (isCompleteRoot) {
			Vector3 intervalOffset = ribbon[0].boneHolder.transform.position - ribbon[0].position;
			RibbonPlacement(intervalOffset);
		}
	}
	
	public void RibbonPlacement(Vector3 intervalOffset) {
		PlaceRibbonPiece(1, intervalOffset);
	}
	
	// this function is called recursively to position each piece of the ribbon and make it's orientation suitable 
	void PlaceRibbonPiece(int ribbonIndex, Vector3 intervalOffset) {
		ribbon[ribbonIndex].boneHolder.transform.position = ribbon[ribbonIndex].position + intervalOffset;
		ribbon[ribbonIndex].boneHolder.transform.LookAt(ribbon[ribbon[ribbonIndex].parentIndex].position + intervalOffset, ribbon[ribbon[ribbonIndex].parentIndex].boneHolder.transform.up);
		
		if (ribbon[ribbonIndex].childIndeces!=null) {
			for (int i=0; i<ribbon[ribbonIndex].childIndeces.Length; i++) {
				//act on the children of this ribbon piece
				PlaceRibbonPiece(ribbon[ribbonIndex].childIndeces[i], intervalOffset);
			}
		}
		
		if (ribbon[ribbonIndex].childRibbons!=null) {
			for (int i=0; i<ribbon[ribbonIndex].childRibbons.Length; i++) {
				//act on the children of this ribbon piece
				ribbon[ribbonIndex].childRibbons[i].RibbonPlacement(intervalOffset);
			}
		}
	}
	
	// the pattern we use when describing a piece of a ribbon 
	struct RibbonPiece {
		public GameObject bone; // original game object of this piece 
		public GameObject boneHolder; //the object we create to place the original object in
		public Vector3 position; //where the piece is
		public Vector3 forces; //how fast it's moving along xyz
		public float linkLength; //how far away is the next piece
		
		public int parentIndex; //the index of the parent ribbonPiece
		public int[] childIndeces; //the indeces of children ribbon pieces 
		public RibbonScript[] childRibbons; //references to other ribbon scripts that are children of this ribbon piece
		
		//functions to manage adding child RibbonPieces and child RibbonScripts
		public void AddChild(int childInt){
			if (childIndeces == null) childIndeces = new int[0];
			int[] tempChilds = new int[childIndeces.Length+1];
			for (int i=0; i<childIndeces.Length; i++) tempChilds[i] = childIndeces[i];
			tempChilds[childIndeces.Length] = childInt;
			childIndeces = new int[tempChilds.Length];
			for (int i=0; i<childIndeces.Length; i++) childIndeces[i] = tempChilds[i];
		}

		public void AddChildRibbon(RibbonScript childScript){
			if (childRibbons == null) childRibbons = new RibbonScript[0];
			RibbonScript[] tempChilds = new RibbonScript[childRibbons.Length+1];
			for (int i=0; i<childRibbons.Length; i++) tempChilds[i] = childRibbons[i];
			tempChilds[childRibbons.Length] = childScript;
			childRibbons = new RibbonScript[tempChilds.Length];
			for (int i=0; i<childRibbons.Length; i++) childRibbons[i] = tempChilds[i];
		}
	}
	
	
	
	////////////////////// ~ Display Ribbon Gizmo ~ //////////////////////
	
	// the ribbon will still work fine if you delete everything below this point, 
	// so if you don't want to see the gizmos or want to hyper-optimise, go ahead :) 
	
	void OnDrawGizmosSelected() {
		debuRootInfluence = 1;
		if (!initialised) {
			DrawHierachy(transform);
		}else{
			DrawRibbon(0, debuRootInfluence);
		}
	}
	
	void DrawHierachy(Transform targetTransform) {
		foreach (Transform child in targetTransform.transform) {
			if (child.GetComponent<SkinnedMeshRenderer>() == null && child.GetComponent<RibbonScript>() == null) {
				DrawBone(targetTransform.position, child.position, debuRootInfluence);
				debuRootInfluence *= rootInfluence;
				DrawHierachy(child);
			}
		}
	}
	
	void DrawRibbon(int ribbonIndex, float influenceIndicator) {
		if (ribbon[ribbonIndex].childIndeces!=null) {
			for (int k=0; k<ribbon[ribbonIndex].childIndeces.Length; k++) {
				DrawBone(ribbon[ribbonIndex].position, ribbon[ribbon[ribbonIndex].childIndeces[k]].position, influenceIndicator);
				DrawRibbon(ribbon[ribbonIndex].childIndeces[k], influenceIndicator * rootInfluence);
			}
		}
	}
	
	// private 
	float boneRadius = 0.2f;
	float endBoneLength = 0.3f;
	
	Vector3 lineDirection;
	Vector3 bumpOffset;
	Vector3 crossingVector;
	Vector3 theCross;
	Vector3 theCrossB;
	float lineLength;
	float boneRadiusnuss;
	float debuRootInfluence;
	Color boneColor = new Color();
	
	void DrawBone(Vector3 source, Vector3 child, float strengthness) {
		lineDirection = child-source;
		lineLength = lineDirection.magnitude;
		lineDirection.Normalize();
		
		crossingVector = new Vector3(lineDirection.z,lineDirection.x,lineDirection.y);
		theCross = Vector3.Cross(lineDirection, crossingVector);
		theCrossB = Vector3.Cross(lineDirection, theCross);
		
		bumpOffset = source + (lineDirection*(lineLength*0.2f));
		
		boneRadiusnuss = lineLength * boneRadius;
		boneColor.a =1;
		boneColor.r =1;
		boneColor.g =strengthness;
		boneColor.b =(strengthness*-1)+1;
		
		Debug.DrawLine(source,bumpOffset+(theCross*boneRadiusnuss), boneColor);
		Debug.DrawLine(source,bumpOffset+(theCrossB*boneRadiusnuss), boneColor);
		Debug.DrawLine(source,bumpOffset-(theCross*boneRadiusnuss), boneColor);
		Debug.DrawLine(source,bumpOffset-(theCrossB*boneRadiusnuss), boneColor);
		
		Debug.DrawLine(bumpOffset+(theCross*boneRadiusnuss), child, boneColor);
		Debug.DrawLine(bumpOffset+(theCrossB*boneRadiusnuss), child, boneColor);
		Debug.DrawLine(bumpOffset-(theCross*boneRadiusnuss), child, boneColor);
		Debug.DrawLine(bumpOffset-(theCrossB*boneRadiusnuss), child, boneColor);
	}
}
