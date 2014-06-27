// =====================================================================
// Copyright 2011-2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;

/// <summary>
/// Class providing runtime features in the editor
/// </summary>
/// <remarks>See \ref editorscripts</remarks>
[ExecuteInEditMode]
public class MBEditorEnabledScript : MonoBehaviour
{

    #region ### Private fields ###
    MBParticleSystem mParticleSystem;
    MBEmitter mEmitter;
    bool mPlaying;

    

    #endregion

    #region ### Public Properties ###

    /// <summary>
    /// Gets the ParticleSystem this script belongs to
    /// </summary>
    public MBParticleSystem ParticleSystem
    {
        get
        {
            if (!mParticleSystem)
                mParticleSystem = MBUtility.Get<MBParticleSystem>(transform);
            return mParticleSystem;
        }
    }
    /// <summary>
    /// Gets the emitter this script belongs to, if any
    /// </summary>
    public MBEmitter Emitter 
    {
        get
        {
            if (!mEmitter)
                mEmitter = MBUtility.Get<MBEmitter>(transform);
            return mEmitter; 
        }
    }
    /// <summary>
    /// Shortcut to ParticleSystem.GlobalTime
    /// </summary>
    /// <remarks>Use this instead of Time.time, as this works both in editor and at runtime</remarks>
    public float Time { get { return ParticleSystem.GlobalTime; } }
    /// <summary>
    /// Shortcut to ParticleSystem.DeltaTime
    /// </summary>
    /// <remarks>Use this instead of Time.deltaTime, as this works both in editor and at runtime</remarks>
    public float DeltaTime { get { return ParticleSystem.DeltaTime; } }

    /// <summary>
    /// Gets the current mouse position
    /// </summary>
    /// <remarks>Use this instead of Input.mousePosition, as this works both in editor and at runtime</remarks>
    public static Vector3 MousePosition { get; set; }

    #endregion

    #region ### Unity Callbacks ###

    protected virtual void OnEnable()
    {
        if (!Application.isPlaying) {
            ParticleSystem.mbEditorEnabledScripts.Remove(this);
            ParticleSystem.mbEditorEnabledScripts.Add(this);
        }
    }

    protected virtual void OnDisable()
    {
        if (!Application.isPlaying)
            ParticleSystem.mbEditorEnabledScripts.Remove(this);
    }

    protected virtual void Awake()
    {
        if (!ParticleSystem)
            Debug.LogError("Magical Box: This script needs to be a part of a ParticleSystem or a child of it!");
    }

#if !UNITYEDITOR
    void Update()
    {
        if (Application.isPlaying) {
            MousePosition = Input.mousePosition;
            DoUpdate();
        }
    }
#endif

    #endregion

    #region ### Virtual Methods each script wants to override ###

    /// <summary>
    /// Called once when the parent emitter/particlesystem starts playing
    /// </summary>
    protected virtual void OnStartsPlaying()
    {
    }

    /// <summary>
    /// Called once when the parent emitter/particlesystem stops playing
    /// </summary>
    protected virtual void OnStopsPlaying()
    {
    }
    /// <summary>
    /// Called every frame when parent emitter/particlesystem is playing
    /// </summary>
    protected virtual void OnPlaying()
    {
    }

    /// <summary>
    /// Behaves like Update(), but works both in editor and at runtime
    /// </summary>
    protected virtual void OnUpdate()
    {
    }

    #endregion

    #region ### Private Methods ###

    void DoUpdate()
    {
        bool play = (Emitter) ? Emitter.Playing : ParticleSystem.Playing;
        
        if (play) {
            if (!mPlaying)
                OnStartsPlaying();
            OnPlaying();
        }
        else {
            if (mPlaying)
                OnStopsPlaying();
        }
        mPlaying = play;
        OnUpdate();
    }

    #endregion

    #region ### Internal Publics. Don't use them unless you know what you're doing ###
    /*! @name Internal Public
     * Don't use them unless you know what you're doing
     */
    //@{
    
    public void mbEditorUpdate()
    {
        DoUpdate();
    }

    //@}
    #endregion

}
