using UnityEngine;
using System.Collections;

public class SimplePistolBullet : MonoBehaviour {
	
	
	private LineRenderer lr;
	
	private Color col = new Color(1,1,1,0.5f);
	public Vector3 start;
	public Vector3 end;
	
	
	// Use this for initialization
	void Start () {
		lr = GetComponent<LineRenderer>();
		
		lr.SetPosition(0,start);
		lr.SetPosition(1,end);
		
		lr.SetColors(col,col);
	}
	
	// Update is called once per frame
	void Update () {
		
		lr.SetColors(col,col);
		
		col.a -= Time.deltaTime;
		if (col.a<=0f){
			Destroy(gameObject);
		}
	}
}
