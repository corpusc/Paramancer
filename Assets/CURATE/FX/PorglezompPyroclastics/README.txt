NOTE: For best results, make sure mipmaps are disabled for all textures on the explosion shader, compression MUST be disabled for the noise texture.

This package contains a pyroclastic noise explosion shader and the associated resources.

USAGE
AUTOMATIC
Drag the Pyroclastic Puff prefab into the scene. Scaling it will change the size of the explosion, and it should move around with the sphere object.

MANUAL
Do not add the material itself onto objects, instead attach the ExplosionMat script, which will create and maintain a copy of the material with all the proper settings.
In most cases it's better to just modify the Pyroclastic Puff prefab for your purposes.