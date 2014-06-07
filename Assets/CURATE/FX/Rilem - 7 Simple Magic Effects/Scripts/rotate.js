var rotationspeed : float = 30;

function Update() {
    
     transform.Rotate(Vector3.up * Time.deltaTime * rotationspeed);
}
