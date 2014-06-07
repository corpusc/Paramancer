#pragma strict

var Speed = 0.25;
function FixedUpdate()
{
var offset = Time.time * (-Speed);
renderer.material.mainTextureOffset = Vector2 (0,offset);
}
