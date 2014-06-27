// =====================================================================
// Copyright 2011-2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;

/// <summary>
/// Class to steady rotate by a certain amount
/// </summary>
public class MBRotateAxis : MBEditorEnabledScript {
    public Vector3 Value;

    Quaternion mInitialRotation;

    protected override void OnStartsPlaying()
    {
        mInitialRotation = transform.rotation;
    }

    protected override void OnStopsPlaying()
    {
        transform.rotation = mInitialRotation;
    }

	protected override void OnPlaying()
    {
        transform.Rotate(Value * DeltaTime);        	
	}

    protected override void OnDisable()
    {
        base.OnDisable();
        transform.rotation = mInitialRotation;
    }
    
}
