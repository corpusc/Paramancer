using UnityEngine;
using System.Collections;

public class SpawnOnFire : MonoBehaviour {
    public MBAnchor Pool;

	// Use this for initialization
	void Awake () {
        if (!Pool)
            Pool = GetComponent<MBAnchor>();
	}
	
	// Update is called once per frame
	void Update () {
        // Fire emitter?
        if (Pool && Input.GetButtonDown("Fire1")) {
            Ray r = Camera.mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Debug.DrawRay(r.origin, r.direction);
            if (Physics.Raycast(r, out hit, Mathf.Infinity)) {
                MBEmitter em = Pool.Spawn();
                if (em) {
                    em.Position = hit.point;
                    em.Play();
                }
            }
        }
	}
}
