var myCheck : boolean = true;
var camAnim : Animation;

function OnMouseDown () {
	if(myCheck == true){
	camAnim.animation.Stop ();
	myCheck = false;
	return myCheck;}
	if(myCheck == false){
	camAnim.animation.Play ();
	myCheck = true;
	return myCheck;}
	}

