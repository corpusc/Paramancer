// =====================================================================
// Copyright 2011-2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;

/// <summary>
/// Class to set transform to match mouse cursor
/// </summary>
public class MBSyncToCursor2D : MBEditorEnabledScript {
    /// <summary>
    /// The z-plane distance from the camera to translate to
    /// </summary>
    public float Distance;

    Vector3 mInitialPos;

    protected override void Awake()
    {
        base.Awake();

        if (Distance == 0 && ParticleSystem.Camera)
                Distance = transform.position.z - ParticleSystem.Camera.transform.position.z;
    }

    protected override void OnStartsPlaying()
    {
        mInitialPos = transform.position;
    }

    protected override void OnStopsPlaying()
    {
        transform.position = mInitialPos;
    }


    protected override void OnPlaying()
    {
        if (ParticleSystem.Camera)
            transform.position = ParticleSystem.Camera.ScreenToWorldPoint(new Vector3(MousePosition.x, MousePosition.y, Distance));
    }

   

    
}
