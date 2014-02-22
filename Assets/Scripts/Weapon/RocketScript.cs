using UnityEngine;
using System.Collections;

public class RocketScript : MonoBehaviour {
	public GameObject particle;
	public int MinParticlesPerFrame = 8;
	public int MaxParticlesPerFrame = 20;
	public int ExplosionParticles = 200;
	public float ExplosionSize = 0.4f;
	public NetworkViewID viewID;
	public NetworkViewID shooterID;
	
	private CcNet net;
	private Vector3 lastPos;
	private float life = 20f;
	
	
	
	void Start () {
		net = GameObject.Find("Main Program").GetComponent<CcNet>();
		lastPos = transform.position;
	}
	
	void Update () {
		if (enabled){
			Vector3 moveForward = transform.forward * Time.deltaTime * 600f / (life < 16f ? 1f : Mathf.Pow(life - 15f, 2f));
			transform.position += moveForward;
			int c_pts = Random.Range(MinParticlesPerFrame, MaxParticlesPerFrame);
			for(int i = 0; i < c_pts; i++){
				var np = (GameObject)GameObject.Instantiate(particle);
				float offMagn = moveForward.magnitude / 2f;
				Vector3 offset = new Vector3(Random.Range(-offMagn, offMagn), Random.Range(-offMagn, offMagn), Random.Range(-offMagn, offMagn));
				np.transform.position = transform.position + offset;
			}
			var hitInfo = new RaycastHit();
			int layerMask = (1<<8) | 1;   //(1<<0);
			var rayDirection = (transform.position - lastPos).normalized;
			if (Physics.SphereCast(lastPos, 0.15f, rayDirection, out hitInfo, Vector3.Distance(transform.position, lastPos), layerMask)) {
				//Debug.Log(hitInfo.collider.gameObject.name);
				for(int i = 0; i < ExplosionParticles; i++){
					var np = (GameObject)GameObject.Instantiate(particle);
					np.transform.position = Random.insideUnitSphere * ExplosionSize + lastPos;
				}
				detonateMaybe();
			}
			
			lastPos = transform.position;
			
			life -= Time.deltaTime;
			if (life <= 0f) {
				detonateMaybe();
			}
		}
	}
	
	private void detonateMaybe() {
		enabled = false;
		if (net.isServer)
			net.Detonate(Item.RocketMaybeJustASingle, transform.position, shooterID, viewID);
	}
}
