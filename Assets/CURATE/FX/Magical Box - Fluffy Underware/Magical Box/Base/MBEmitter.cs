// =====================================================================
// Copyright 2011-2013 FluffyUnderware
// All rights reserved
// =====================================================================

// UNCOMMENT THIS IF YOU WANT YOUR PARTICLES TO GENERATE NORMALS. YOU'LL NEED TO UNCOMMENT THE SAME IN MBLayer.cs
//#define PARTICLES_USE_NORMALS

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Emitter class
/// </summary>
/// <remarks>
/// An Emitter is used to control a particle effect.
/// \par
/// Use Play() to start playing, Stop() or Halt() to stop the emitter. LaunchParticle() will instantly launch a particle while
/// Clear() immediately kill all particles.
/// \par
/// MBEmitter needs a MBEmitterType class attached to the same GameObject and a MBParticleSystem in
/// its parent hierarchy. While the MBEmitterType controls new particle's initial position and heading,
/// the MBParticleSystem handles the rendering of the existing particles.
/// 
/// \sa \ref emref
/// 
/// </remarks>
[ExecuteInEditMode]
public class MBEmitter : MBObject
{
    #region ### Inspector Fields ###
    /// <summary>
    /// Does this emitter renders particles?
    /// </summary>
    /// <remarks>
    /// Unlike muted, particles will be spawned and calculated and only the rendering will be suppressed.
    /// </remarks>
    public bool Invisible;
    /// <summary>
    /// Delay in seconds from ParticleSystem's startime till this emitter starts emitting
    /// </summary>
    public float Delay;
    /// <summary>
    /// Duration in seconds this emitter will emit
    /// </summary>
    public float Duration=1;
    /// <summary>
    /// Does this emitter start automatically on ParticleSystem's start?
    /// </summary>
    public bool AutoPlay=true;
    /// <summary>
    /// Does this emitter repeat playing?
    /// </summary>
    /// <remarks>This ist most useful if you've set the ParticleSystem to AutoRepeat, too</remarks>
    public bool AutoRepeat = false;
    /// <summary>
    /// Gets whether the launchrate uses a curve
    /// </summary>
    public bool LaunchRateAnimated;
    /// <summary>
    /// The emitting rate in particles per second.
    /// </summary>
    public float LaunchRate = 10;
    /// <summary>
    /// Emitting rate randomness
    /// </summary>
    public float LaunchRandom = 0;
    /// <summary>
    /// Animated emitting rate curve
    /// </summary>
    public AnimationCurve LaunchRateCurve;
    /// <summary>
    /// Particles are spawned once LaunchRate fills the buffer
    /// </summary>
    public int LaunchBuffer = 0;
    /// <summary>
    /// The number of particles that will spawn instantly on Play()
    /// </summary>
    public int InstantLaunch;
    /// <summary>
    /// Whether InstantLaunch should be called continously when AutoRepeat is enabled
    /// </summary>
    public bool InstantLaunchRepeat=true;
    /// <summary>
    /// If true, particles will spawn at the parent emitter's position
    /// </summary>
    public bool IsTrail;
    /// <summary>
    /// The pivot/handle of the particle quad.
    /// </summary>
    public Vector2 ImagePivot = Vector2.zero;
    /// <summary>
    /// Textures used by this emitter
    /// </summary>
    /// <remarks>Call Layer.GenerateTextureMap(false) after changing textures</remarks>
    public List<Texture2D> Textures = new List<Texture2D>();
    /// <summary>
    /// Add a useful description to your emitter
    /// </summary>
    public string Description;

    /// <summary>
    /// The assigned material layer
    /// </summary>
    /// <remarks>Call ParticleSystem.GenerateTextureMap after changing the layer</remarks>
    public MBLayer Layer;
    /// <summary>
    /// This is used to auto-assign a matching layer when importing prefabbed emitters
    /// </summary>
    /// <remarks>This will be set by the emitter's inspector class</remarks>
    [HideInInspector]
    public string LayerShaderName;

    #endregion
  
    #region #### Public Properties ###
    /// <summary>
    /// The age of this emitter in seconds (0..Duration)
    /// </summary>
    public float Age { get; private set; }
    /// <summary>
    /// The age of this emitter in percent (0..1)
    /// </summary>
    public float AgePercent { get; private set; }

    /// <summary>
    /// Gets or sets Billboarding mode
    /// </summary>
    public MBBillboard Billboard
    {
        get { return mBillboard; }
        set
        {
            mBillboard = value;
            mbInitializeQuad();
        }
    }
    /// <summary>
    /// Gets how many particles died due their age exceeds
    /// </summary>
    public int DeathByAge { get { return mDeathReason[(int)MBDeathReason.Age]; } }
    /// <summary>
    /// Gets how many particles died due their alpha falls below 0
    /// </summary>
    public int DeathByColor { get { return mDeathReason[(int)MBDeathReason.Color]; } }
    /// <summary>
    /// Gets how many particles died due their size falls below 0
    /// </summary>
    public int DeathBySize { get { return mDeathReason[(int)MBDeathReason.Size]; } }
    /// <summary>
    /// Gets how many particles died by a zone parameter
    /// </summary>
    public int DeathByZone { get { return mDeathReason[(int)MBDeathReason.Zone]; } }
    /// <summary>
    /// Gets how many particles died by a user command
    /// </summary>
    public int DeathByUser { get { return mDeathReason[(int)MBDeathReason.User]; } }
    /// <summary>
    /// Gets a list of all (smart) child emitters
    /// </summary>
    /// <remarks>This list is cached, use mbReload() to reload the cache</remarks>
    public List<MBEmitter> Emitter
    {
        get
        {
            if (mEmitters == null) {
                mEmitters = new List<MBEmitter>(MBUtility.GetChildren<MBEmitter>(Transform, true, true));
                mEmitters.Sort();
            } return mEmitters;
        }
    }
    /// <summary>
    /// Gets the EmitterType of this emitter
    /// </summary>
    public MBEmitterType EmitterType { get; private set; }
    /// <summary>
    /// Gets whether this emitter is currently emitting particles
    /// </summary>
    public bool Emitting { get; private set; }
    /// <summary>
    /// Gets the uv rects of all texture frames
    /// </summary>
    public Rect[] FramesUV { get; set; }
    /// <summary>
    /// Gets whether this emitter is managed a an anchor's pooling manager
    /// </summary>
    public bool IsPooled
    {
        get { return mbIsPooled; }
    }

    /// <summary>
    /// Gets all active parameters of this emitter
    /// </summary>
    public List<MBParameter> Parameters { get; protected set; }
    // Gets a list of active particles
    public List<MBParticle> Particles { get; protected set; }
    /// <summary>
    /// Gets the number of active particles managed by this emitter
    /// </summary>
    public int ParticleCount { 
        get
        {
            return Particles.Count;
        }
    }

    /// <summary>
    /// Gets whether this emitter is currently playing
    /// </summary>
    public bool Playing { get; private set; }
    /// <summary>
    /// Gets whether this emitter is stopping
    /// </summary>
    public bool Stopping { get; private set; }

    #endregion

    #region ### Private Fields ###

    float mToLaunch;
    float mStartTime;
    
    Vector3[] mVertex = new Vector3[4];
    Vector2[] mUV = new Vector2[4];
    Color[] mColor = new Color[4];
    #if PARTICLES_USE_NORMALS
    Vector3[] mNormal = new Vector3[4];
    #endif
    
    Vector3[] mQuad;
    int[] mDeathReason = new int[System.Enum.GetValues(typeof(MBDeathReason)).Length];
    [SerializeField, HideInInspector]
    MBBillboard mBillboard = MBBillboard.Vertical2D;// Billboarding mode
    List<MBParameter> mAnimatedParameters = new List<MBParameter>();// List of animated parameters (=will be applied every frame)
    List<MBParticle> mExhausted = new List<MBParticle>();// List of particles to be deleted at the end of the update loop
    List<MBEmitter> mEmitters;// Child emitters
    List<string> mParticleUserData = new List<string>();
    MBObject mControllingParent;

    Matrix4x4 mMatrixToParticleSystem;

    #endregion

    #region ### Events ###

    /// <summary>
    /// Event is raised when a particle is spawned
    /// </summary>
    /// <remarks>See \ref events</remarks>
    public event MBEventHandler ParticleBirth;
    /// <summary>
    /// SendMessage like event linking
    /// </summary>
    /// <remarks>See \ref events</remarks>
    public MBSendMessageTarget ParticleBirthSM;
    /// <summary>
    /// Event is raised when a particle dies
    /// </summary>
    /// <remarks>See \ref events</remarks>
    public event MBEventHandler ParticleDeath;
    /// <summary>
    /// SendMessage like event linking
    /// </summary>
    /// <remarks>See \ref events</remarks>
    public MBSendMessageTarget ParticleDeathSM;
    /// <summary>
    /// Event is raised when this emitter stops playing
    /// </summary>
    /// <remarks>See \ref events</remarks>
    public event MBEventHandler EmitterStopsPlaying;
    /// <summary>
    /// SendMessage like event linking
    /// </summary>
    /// <remarks>See \ref events</remarks>
    public MBSendMessageTarget EmitterStopsPlayingSM;

    #endregion

    #region ### Unity Callbacks ###

    void Awake()
    {
        mbDebugRate = 0.1f;
        mbDebugSizeCol = Color.white;
        mbDebugOrientationCol = Color.white;
        mbDebugSpeedCol = new Color(1f, 0.6f, 0f);
        mbDebugAgeCol = new Color(0f, 0.4f, 1f);
        mbDebugForcesCol = new Color(0.9f, 0.1f, 0.8f);
        mbDebugHeadingCol = new Color(0f, 0.5f, 0f);
        mbDebugZonesCol = new Color(1, 0, 0);
        mbDebugSize = true;
        mbDebugOrientation = true;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        Parameters = new List<MBParameter>();
        Particles = new List<MBParticle>();
        EmitterType = GetComponent<MBEmitterType>();
        if (!EmitterType)
            SetEmitterType<MBCircleEmitter>();
        mbInitializeQuad();
    }

    protected override void OnDisable()
    {
        if (this.enabled)
            Halt(false);

        base.OnDisable();
    }

    void Start()
    {
        // auto-assign layer if neccessary
        if (!Layer && ParticleSystem)
            Layer = ParticleSystem.FindLayer(LayerShaderName);
    }

    protected override void OnDrawGizmos()
    {
        if (ShouldDrawGizmos)
            DoGizmos();
    }

    protected override void OnDrawGizmosSelected()
    {
        if (ShouldDrawGizmosSelected)
            DoGizmos();
    }

    #endregion

    #region ### Public Methods ###

    /// <summary>
    /// Adds a parameter to this emitter
    /// </summary>
    /// <typeparam name="T">Parameter's type</typeparam>
    /// <returns>The new parameter</returns>
    public T AddParameter<T>() where T : MBParameter
    {
        return AddParameter(typeof(T).Name) as T;
    }

    /// <summary>
    /// Adds a parameter to this emitter
    /// </summary>
    /// <param name="className">Parameter's class name</param>
    /// <returns>The new parameter</returns>
    public MBParameter AddParameter(string className)
    {
        GameObject go = new GameObject(className);
        go.transform.parent = Transform;
        MBParameter param = go.AddComponent(className) as MBParameter;
        param.Reset();
        Parameters.Add(param);
        mbSortParameters();
        return param;
    }

    /// <summary>
    /// Adds a parameter to this emitter
    /// </summary>
    /// <typeparam name="T">Parameter's type</typeparam>
    /// <returns>The new parameter</returns>
    public MBParameter AddParameter(System.Type type)
    {
        return AddParameter(type.Name);
    }

    /// <summary>
    /// Clears all existing particles.
    /// </summary>
    /// <remarks>
    /// The emitter's playing state is kept intact. ParticleDeath event is called with MBDeathReason.User.
    /// </remarks>
    public void Clear()
    {
        foreach (MBParticle PT in Particles)
            OnParticleDeath(PT, MBDeathReason.User);
        mbDebugParticles.Clear();
        // When prefabbing, we loose parent
        if (ParticleSystem)
            ParticleSystem.mbIdleParticle(Particles);
        Particles.Clear();
    }

    /// <summary>
    /// Gets the emitter or the particlesystem this emitter is controlled by (i.e. in its Emitter list)
    /// </summary>
    /// <returns>Particlesystem or Emitter</returns>
    public MBObject ControllingParent
    {
        get
        {
            if (!mControllingParent)
                mControllingParent = (MBObject)MBUtility.GetParent(Transform, typeof(MBEmitter), typeof(MBParticleSystem));
            return mControllingParent;
        }
    }

    public override void Destroy()
    {
        Halt();
        UnregisterFromControllingParent();
        base.Destroy();
    }


    /// <summary>
    /// Duplicates this emitter
    /// </summary>
    /// <returns>the new emitter</returns>
    public MBEmitter Duplicate()
    {
        return ParticleSystem.AddEmitter(this, Parent);
    }

    /// <summary>
    /// Lets a particle die immediately 
    /// </summary>
    /// <param name="PT">The particle to remove</param>
    /// <param name="reason">For statistical purpose only</param>
    public void KillParticle(MBParticle PT, MBDeathReason reason)
    {
        OnParticleDeath(PT, reason);
        PT.UserData = null;
        mExhausted.Add(PT);
        mDeathReason[(int)reason]++;
    }

   
    /// <summary>
    /// Gets the first active parameter of a given type
    /// </summary>
    /// <typeparam name="T">a Type derived from MBParameter</typeparam>
    /// <returns>the resulting parameter or null</returns>
    public T FindParameter<T>() where T:MBParameter
    {
        foreach (MBParameter param in Parameters) 
            if (param is T)
                return (T)param;

        return null;
    }
    /// <summary>
    /// Gets all active parameters of a given type
    /// </summary>
    /// <typeparam name="T">a Type derived from MBParameter</typeparam>
    /// <returns>a list of resulting parameters or an empty list</returns>
    public List<T> FindParameters<T>() where T:MBParameter
    {
        List<T> res = new List<T>();
        foreach (MBParameter param in Parameters)
            if (param is T)
                res.Add((T)param);
        return res;
    }

    /// <summary>
    /// Immediately stops and clears the emitter and all child emitters
    /// </summary>
    public void Halt() { Halt(true); }
    /// <summary>
    /// Immediately stops and clears the emitter and all child emitters
    /// </summary>
    /// <param name="includeChildren">false to only halt this emitter</param>
    public void Halt(bool includeChildren)
    {
        Stop();
        Clear();
        Playing = false;
        
        if (includeChildren) {
            foreach (MBEmitter em in Emitter)
                em.Halt(includeChildren);
        }
    }

    /// <summary>
    /// Immediately launch an amount of particles
    /// </summary>
    public void LaunchParticle(int count)
    {
        for (int i = 0; i < count; i++)
            LaunchParticle();
    }

    /// <summary>
    /// Immediately launch a single particle
    /// </summary>
    /// <returns></returns>
    public MBParticle LaunchParticle()
    {
        if (!EmitterType) return null;

        MBParticle PT = ParticleSystem.mbGetParticle();
        PT.Parent = this;
        // Set initial position by tranforming emitter space to particlesystem space
        PT.Position = (IsTrail) ? ParticleSystem.Transform.InverseTransformPoint(Transform.TransformPoint(EmitterType.GetPosition(PT))) : TransformPointToParticleSystem(EmitterType.GetPosition(PT));
        //BEFORE: PT.Position = ParticleSystem.Transform.InverseTransformPoint(Transform.TransformPoint(EmitterType.GetPosition(PT)));
        PT.Heading = EmitterType.GetHeading(PT);
        PT.Lifetime = Duration;
        PT.Age = 0;
        // User Data
        if (mParticleUserData.Count > 0)
            PT.UserData = new object[mParticleUserData.Count];

        Particles.Add(PT);
        for (int i = 0; i < Parameters.Count; i++) {
            MBParameter param = Parameters[i];
            if (!param.Muted)
                param.OnBirth(PT);
        }
        // Debugging
        if (mbDebugging) {
            mbDebugAccu += mbDebugRate;
            if (mbDebugAccu >= 1) {
                mbDebugAccu -= 1;
                mbDebugParticles.Add(PT);
            }
        }

        // Birth event
        OnParticleBirth(PT);
        return PT;
    }
    
    /// <summary>
    /// Starts this emitter and all child emitters
    /// </summary>
    public void Play() { Play(true); }

    /// <summary>
    /// Starts this emitter and all child emitters
    /// </summary>
    /// <param name="includeChildren">whether child emitters start as well</param>
    public void Play(bool includeChildren)
    {
        mDeathReason = new int[System.Enum.GetValues(typeof(MBDeathReason)).Length];
        bool repeating = Playing;
        if (Muted) return;
        // Keep user data slots while playing
        if (!Playing) {
            mParticleUserData.Clear();
            // warn if layer is missing
            if (!Layer)
                Debug.Log(string.Format("Magical Box: Emitter '{0}' has no layer assigned!", name));
        }
        if (EmitterType)
            EmitterType.OnPlay();
        Playing = true;
        Stopping = false;
        mStartTime = ParticleSystem.GlobalTime + Delay;
        Emitting = false;
        if (includeChildren) {
            foreach (MBEmitter em in Emitter)
                em.Play(includeChildren);
        }
        //mToLaunch = 0;
        float rate=(LaunchRateAnimated) ? LaunchRateCurve.Evaluate(0) : LaunchRate;
        if (InstantLaunch > 0 && (InstantLaunchRepeat || !repeating))
            mToLaunch = InstantLaunch;
        else if (mToLaunch == 0 && rate > 0)
            mToLaunch = Mathf.Max(0, 1 - rate);
            //mToLaunch = Mathf.Max(0, rate);
        
        // Initialize parameters
        foreach (MBParameter param in Parameters)
            param.OnPlay();

    }
    /// <summary>
    /// Gets a user data slot for this emitter's particles. If the slot doesn't exist, it will be created
    /// </summary>
    /// <param name="name">name of the slot</param>
    /// <returns>the assigned slot ID</returns>
    /// <remarks>\sa \ref particleuserdata</remarks>
    public int RegisterParticleUserData(string name)
    {

        int id = GetParticleUserDataID(name.ToLower());
        if (id == -1) {
            mParticleUserData.Add(name.ToLower());
            id=mParticleUserData.Count - 1;
        }
        return id;
    }

    /// <summary>
    /// Gets the slot ID assigned to a slot name
    /// </summary>
    /// <param name="name">name of the slot</param>
    /// <returns>the assigned slot ID or -1</returns>
    /// <remarks>\sa \ref particleuserdata</remarks>
    public int GetParticleUserDataID(string name)
    {
        return mParticleUserData.IndexOf(name.ToLower());
    }


    
    /// <summary>
    /// Removes a parameter from this emitter
    /// </summary>
    /// <param name="param">the parameter to be removed</param>
    /// <remarks>
    /// This is equivalent to calling MBParameter.Destroy()
    /// </remarks>
    public void RemoveParameter(MBParameter param)
    {
        Parameters.Remove(param);
        GameObject.DestroyImmediate(param.gameObject);
        mbSortParameters();
    }

    /// <summary>
    /// Sets the emitter type. An existing emitter type will be replaced.
    /// </summary>
    /// <typeparam name="T">Type of new emitter</typeparam>
    public T SetEmitterType<T>() where T : MBEmitterType
    {
        return SetEmitterType(typeof(T)) as T;
    }
    /// <summary>
    /// Sets the emitter type. An existing emitter type will be replaced
    /// </summary>
    /// <typeparam name="T">Type of new emitter</typeparam>
    public MBEmitterType SetEmitterType(System.Type type)
    {
        if (EmitterType) {
            if (Application.isPlaying)
                GameObject.Destroy(EmitterType);
            else
                GameObject.DestroyImmediate(EmitterType, true);
        }
        if (type == null)
            EmitterType = null;
        else
            EmitterType=(MBEmitterType)gameObject.AddComponent(type);
        return EmitterType;
    }

    public override void SetParent(MBObject parent)
    {
        if (parent is MBParameter) {
            Debug.LogError(string.Format("Magical Box: '{0}' ({1}) can't be a child of '{2}' ({3}) !", name, GetType(), parent.name, parent.GetType()));
            return;
        }
        // Unregister our controller
        UnregisterFromControllingParent();
        
        MBParticleSystem oldSys = mParticleSystem;

        base.SetParent(parent);

        // Register at our new controller
        RegisterAtControllingParent();

        // Has our ParticleSystem changed?
        if (oldSys != ParticleSystem) {
            
            if (oldSys) {
                oldSys.mbCalculateRuntime();
                oldSys.GenerateTextureMap();

            }
            if (ParticleSystem){
                // when using the editor to copy&paste, this emitter has it's layer already set to a layer from this particleSystem
                // if our layer isn't found in this particlesystem, set a default one or null
                if (Layer) {
                    if (!ParticleSystem.Layer.Contains(Layer) && ParticleSystem.DefaultLayer)
                            Layer = ParticleSystem.DefaultLayer;
                }
                ParticleSystem.GenerateTextureMap();
                ParticleSystem.mbCalculateRuntime();
            }
            
        }
            
    }
    /// <summary>
    /// Sets the texture used by this emitter
    /// </summary>
    /// <param name="texture">a texture with its ReadWrite flag set to true</param>
    public void SetTexture(Texture2D texture)
    {
        Textures.Clear();
        if (texture)
            Textures.Add(texture);

        ParticleSystem.GenerateTextureMap();
    }

    /// <summary>
    /// Sets the textures used by this emitter
    /// </summary>
    /// <param name="textures">an array of textures with their ReadWrite flag set to true</param>
    public void SetTexture(Texture2D[] textures)
    {
        Textures.Clear();
        if (textures.Length>0)
            Textures.AddRange(textures);

        ParticleSystem.GenerateTextureMap();
    }

    /// <summary>
    /// Stops the emitter and all child emitters, leaving existing particles intact
    /// </summary>
    public void Stop() { Stop(true); }
    /// <summary>
    /// Stops the emitter and all child emitters, leaving existing particles intact
    /// </summary>
    /// <param name="includeChildren">whether child emitters should stop as well</param>
    public void Stop(bool includeChildren)
    {
        Emitting = false;
        Stopping = true;
        mParticleUserData.Clear();
        mToLaunch = 0;
        if (includeChildren) {
            foreach (MBEmitter em in Emitter)
                em.Stop(includeChildren);
        }
    }

    /// <summary>
    /// Transforms a position from emitter space to ParticleSystem space
    /// </summary>
    /// <param name="emitterSpace">position in emitter space</param>
    /// <returns>position in ParticleSystem space</returns>
    public Vector3 TransformPointToParticleSystem(Vector3 emitterSpace)
    {
        return mMatrixToParticleSystem.MultiplyPoint3x4(emitterSpace);
    }

    #endregion

    #region ### Internal Publics. Don't use them unless you know what you're doing ###
    /*! @name Internal Public
     * Don't use them unless you know what you're doing
     */
    //@{
    
    //Debugging properties
    public bool mbDebugging { get; set; }
    public float mbDebugRate { get; set; }
    public bool mbDebugHeading { get; set; }
    public bool mbDebugSize { get; set; }
    public bool mbDebugSpeed { get; set; }
    public bool mbDebugAge { get; set; }
    public bool mbDebugForces { get; set; }
    public bool mbDebugZones { get; set; }
    public bool mbDebugOrientation { get; set; }
    public Color mbDebugHeadingCol { get; set; }
    public Color mbDebugSizeCol { get; set; }
    public Color mbDebugSpeedCol { get; set; }
    public Color mbDebugAgeCol { get; set; }
    public Color mbDebugForcesCol { get; set; }
    public Color mbDebugOrientationCol { get; set; }
    public Color mbDebugZonesCol { get; set; }
    public bool mbIsFreezed { get; set; }
    public bool mbIsPooled { get; set; }
    float mbDebugAccu;
    List<MBParticle> mbDebugParticles = new List<MBParticle>();


    /// <summary>
    /// Fetch UV's
    /// </summary>
    public void mbInitializeTextures()
    {
        //if (Layer) {
        //    Layer.UpdateMaterial();
        //}
        FramesUV = new Rect[Textures.Count];
        for (int i = 0; i < Textures.Count; i++)
            FramesUV[i] = ParticleSystem.GetTextureUV(Textures[i]);

        if (FramesUV.Length == 0)
            FramesUV = new Rect[1] { new Rect() };

        foreach (MBEmitter child in Emitter)
            child.mbInitializeTextures();
    }

    /// <summary>
    /// Precalculate the quad vertices taking Image-Handle and Billboard-Mode into account
    /// </summary>
    public void mbInitializeQuad()
    {
        switch (Billboard) {
            case MBBillboard.Billboard:
            case MBBillboard.Vertical2D:
                mQuad = new Vector3[4] { new Vector3(-1-ImagePivot.x, -1-ImagePivot.y, 0),
                                       new Vector3(-1-ImagePivot.x, 1-ImagePivot.y, 0),
                                       new Vector3(1-ImagePivot.x, 1-ImagePivot.y, 0), 
                                       new Vector3(1-ImagePivot.x, -1-ImagePivot.y, 0) };
#if PARTICLES_USE_NORMALS
                mNormal = new Vector3[4] { new Vector3(0,0,-1),
                                           new Vector3(0,0,-1),
                                           new Vector3(0,0,-1),
                                           new Vector3(0,0,-1) };
#endif
                break;
            case MBBillboard.Horizontal2D:
                mQuad = new Vector3[4] { new Vector3(-1-ImagePivot.x, 0,-1-ImagePivot.y),
                                         new Vector3(-1-ImagePivot.x, 0,1-ImagePivot.y),
                                         new Vector3(1-ImagePivot.x, 0,1-ImagePivot.y), 
                                         new Vector3(1-ImagePivot.x, 0,-1-ImagePivot.y) };
#if PARTICLES_USE_NORMALS
                mNormal = new Vector3[4] { new Vector3(0,1,0),
                                           new Vector3(0,1,0),
                                           new Vector3(0,1,0),
                                           new Vector3(0,1,0) };
#endif
                break;
        }
    }

    /// <summary>
    /// Update cached hierarchy values
    /// </summary>
    public override void mbReloadHierarchy()
    {
        base.mbReloadHierarchy();

        mControllingParent = null;
        mEmitters = null;
        // cache/refetch child emitter now:
        if (Emitter==null) {}
        
        // reload parameters
        Parameters.Clear();
        Parameters.AddRange(MBUtility.GetChildren<MBParameter>(Transform,false,true));
        mbSortParameters();
    }

    /// <summary>
    /// Sorts parameters by their order and type (Animated vs. Static)
    /// </summary>
    public void mbSortParameters()
    {
        Parameters.Sort();
        mAnimatedParameters.Clear();
        foreach (MBParameter param in Parameters) {
            if (param.AnimatedLife)
                mAnimatedParameters.Add(param);
        }
    }

    
    /// <summary>
    /// Main update function
    /// </summary>
    /// <returns># of rendered particles</returns>
    public int mbUpdateParticles(float delta, bool render)
    {
        if (!Layer || !Layer.Material || Muted || !ParticleSystem.ActiveCamera  || mbIsFreezed) {
            mbIsFreezed = false;
            return 0;
        }
        
        int ptcount = 0;
        Vector3 oldPos = Vector3.zero;
        bool ptAlive = true;
        Matrix4x4 mMatrix;
        Matrix4x4 mBBMatrix = Matrix4x4.identity;
        Quaternion mRotation = Quaternion.identity;
        Vector3 PTorient = Vector3.zero;
        Vector3 look = Vector3.zero;
        Vector3 scale = Vector3.one;

        mMatrixToParticleSystem = ParticleSystem.Transform.worldToLocalMatrix * Transform.localToWorldMatrix;
       
        // render children
        foreach (MBEmitter em in Emitter) {
            ptcount += em.mbUpdateParticles(delta, render);
        }

        if (Playing) {
            float dur = Mathf.Max(Mathf.Epsilon, Duration);
            Age = ParticleSystem.GlobalTime - mStartTime;
            AgePercent = Age / dur;

            if (AgePercent > 1) {
                Emitting = false;
                if (AutoRepeat && !Stopping)
                    Play();
                else
                    Playing = ParticleCount > 0;
                if (!Playing)
                    OnEmitterStopsPlaying();
            }
            else if (!Emitting && !Stopping && AgePercent >= 0) {
                Emitting = true;
            }
            if (Emitting) {
                AgePercent = Mathf.Clamp01(AgePercent);
                // Accumulate launch rate 
                mToLaunch += delta * ((LaunchRateAnimated) ? LaunchRateCurve.Evaluate(AgePercent) : LaunchRate + LaunchRate * LaunchRandom * MBUtility.RandomSign());
                
                // need to launch particles?
                if (mToLaunch>=1 && mToLaunch >= LaunchBuffer) {
                    if (IsTrail) {
                        float accu = 0;
                        int ppc = ((MBEmitter)Parent).Particles.Count;
                        for (int pp = 0; pp < ppc; pp++) {
                            MBParticle parentParticle = ((MBEmitter)Parent).Particles[pp];
                            accu = mToLaunch;
                            Position = parentParticle.Position;

                            switch (EmitterType.Heading) {
                                case MBEmitterTypeHeading.TrailVelocity:
                                    EmitterType.FixedHeading = parentParticle.Velocity;
                                    break;
                                case MBEmitterTypeHeading.TrailOrientation:
                                    EmitterType.FixedHeading = new Vector3(-Mathf.Sin(-parentParticle.Orientation * Mathf.Deg2Rad), Mathf.Cos(-parentParticle.Orientation * Mathf.Deg2Rad), 0);
                                    break;
                            }
                            if (accu > LaunchBuffer) {
                                LaunchParticle((int)accu);
                            }
                        } // for each parent particle

                    }
                    else { // No Trail
                        LaunchParticle((int)mToLaunch);
                    }
                    mToLaunch = mToLaunch - (int)mToLaunch; // keep only time fragments <1 (.xx)
                } // if Need To Launch
                
            } // if Emitting

            Transform camTransform = ParticleSystem.ActiveCamera.transform;
            // UPDATE Particles
            
            for (int pi = 0; pi < Particles.Count; pi++) {

                MBParticle PT = Particles[pi];

                // Set Pre-Parameter variables
                PT.Age += delta;
                PT.AgePercent = PT.Age / PT.Lifetime;
                // Early Death: Duration exceedded?
                if (PT.AgePercent > 1) {
                    KillParticle(PT, MBDeathReason.Age);
                }
                else {
                    ptAlive = true;
                    if (PT.Age > 0) {
                        // RESET ACCUMULATED VALUES
                        PT.Force = Vector3.zero;
                        oldPos = PT.Position;

                        // ANIMATE
                        int apc = mAnimatedParameters.Count;
                        for (int i=0;i<apc;i++) {
                            MBParameter param = mAnimatedParameters[i];
                            if (!param.Muted)
                                if (!param.OnLifetime(PT)) {
                                    KillParticle(PT, param.DeathReason);
                                    ptAlive = false;
                                    break;
                                }
                        }
                    }
                    if (ptAlive) {
                        // APPLY VALUES
                        PT.Mass = Mathf.Max(1f, PT.Mass);
                        Vector3 F = ((PT.Acceleration * PT.Mass * PT.Heading) + PT.Force);
                        PT.Velocity = (PT.Velocity + (F / PT.Mass) * delta) * Mathf.Clamp01((1 - PT.Friction * delta));
                        PT.Position += PT.Velocity * delta;

                        PT.AbsVelocity = PT.Position - oldPos;

                        // Values used for rendering: PT.Position, PT.Orientation, PT.Scale, PT.Color, PT.ImageFrameIndex
                        if (render && !Invisible) {
                            // RENDER
                            switch (Billboard) {
                                case MBBillboard.Billboard:
                                    PTorient.x = 0;PTorient.y=0;PTorient.z=-PT.Orientation;
                                    mRotation.eulerAngles = PTorient;
                                    look.x = PT.Position.x - camTransform.position.x;
                                    look.y = PT.Position.y - camTransform.position.y;
                                    look.z = PT.Position.z - camTransform.position.z;
                                    look.Normalize();
                                    Vector3 right = Vector3.Cross(camTransform.up, look);
                                    Vector3 up = Vector3.Cross(look, right);

                                    mBBMatrix.SetColumn(0, new Vector4(right.x, right.y, right.z, 0));
                                    mBBMatrix.SetColumn(1, new Vector4(up.x, up.y, up.z, 0));
                                    mBBMatrix.SetColumn(2, new Vector4(look.x, look.y, look.z, 0));
                                    mBBMatrix.SetColumn(3, new Vector4(PT.Position.x, PT.Position.y, PT.Position.z, 1));
                                    scale.x = PT.Scale.x * Transform.lossyScale.x;
                                    scale.y = PT.Scale.y * Transform.lossyScale.y;
                                    scale.z = PT.Scale.z * Transform.lossyScale.z;
                                    mMatrix = mBBMatrix * Matrix4x4.TRS(Vector3.zero, mRotation, scale);
                                    mVertex[0] = mMatrix.MultiplyPoint3x4(mQuad[0]);
                                    mVertex[1] = mMatrix.MultiplyPoint3x4(mQuad[1]);
                                    mVertex[2] = mMatrix.MultiplyPoint3x4(mQuad[2]);
                                    mVertex[3] = mMatrix.MultiplyPoint3x4(mQuad[3]);
#if PARTICLES_USE_NORMALS
                                    mNormal[0] = -look;
                                    mNormal[1] = mNormal[0];
                                    mNormal[2] = mNormal[0];
                                    mNormal[3] = mNormal[0];
#endif
                                    break;
                                case MBBillboard.Horizontal2D:
                                    PTorient.x = 0;PTorient.y=-PT.Orientation;PTorient.z=0;
                                    mRotation.eulerAngles = PTorient;
                                    scale.x = PT.Scale.x * Transform.lossyScale.x;
                                    scale.y = PT.Scale.y * Transform.lossyScale.y;
                                    scale.z = 1;
                                    
                                    mMatrix = Matrix4x4.TRS(PT.Position, mRotation, scale);
                                    mVertex[0] = mMatrix.MultiplyPoint3x4(mQuad[0]);
                                    mVertex[1] = mMatrix.MultiplyPoint3x4(mQuad[1]);
                                    mVertex[2] = mMatrix.MultiplyPoint3x4(mQuad[2]);
                                    mVertex[3] = mMatrix.MultiplyPoint3x4(mQuad[3]);
                                    break;
                                case MBBillboard.Vertical2D:
                                    mRotation = Quaternion.Euler(0, 0, -PT.Orientation);
                                    PTorient.x = 0;PTorient.y=0;PTorient.z=-PT.Orientation;
                                    mRotation.eulerAngles = PTorient;
                                    scale = new Vector3(PT.Scale.x * Transform.lossyScale.x,
                                                        PT.Scale.y * Transform.lossyScale.y,
                                                        PT.Scale.z * Transform.lossyScale.z);
                                    mMatrix = Matrix4x4.TRS(PT.Position, mRotation, scale);
                                    mVertex[0] = mMatrix.MultiplyPoint3x4(mQuad[0]);
                                    mVertex[1] = mMatrix.MultiplyPoint3x4(mQuad[1]);
                                    mVertex[2] = mMatrix.MultiplyPoint3x4(mQuad[2]);
                                    mVertex[3] = mMatrix.MultiplyPoint3x4(mQuad[3]);
                                    break;
                            }

                            mColor[0] = PT.Color;
                            mColor[1] = PT.Color;
                            mColor[2] = PT.Color;
                            mColor[3] = PT.Color;

                            PT.ImageFrameIndex = Mathf.Clamp(PT.ImageFrameIndex, 0, FramesUV.Length - 1);

                            mUV[0].x=FramesUV[PT.ImageFrameIndex].xMin;mUV[0].y=FramesUV[PT.ImageFrameIndex].yMin;
                            mUV[1].x=FramesUV[PT.ImageFrameIndex].xMin;mUV[1].y=FramesUV[PT.ImageFrameIndex].yMax;
                            mUV[2].x=FramesUV[PT.ImageFrameIndex].xMax;mUV[2].y=FramesUV[PT.ImageFrameIndex].yMax;
                            mUV[3].x = FramesUV[PT.ImageFrameIndex].xMax;mUV[3].y=FramesUV[PT.ImageFrameIndex].yMin;
                            #if PARTICLES_USE_NORMALS
                                Layer.mbAddToRenderBuffer(ref mVertex, ref mUV, ref mColor, ref mNormal);
                            #else
                                Layer.mbAddToRenderBuffer(ref mVertex, ref mUV, ref mColor);
                            #endif
                        } // render
                    } // alive
                } // Age-alive
            } // for all particles
            for (int ei=0;ei<mExhausted.Count;ei++){
                MBParticle p = mExhausted[ei];
                Particles.Remove(p);
                ParticleSystem.mbIdleParticle(p);
                if (mbDebugging)
                    mbDebugParticles.Remove(p);
            }
            mExhausted.Clear();

        }  // playing
        
        return ptcount + ParticleCount;
    }
    //@}
    #endregion

    #region ### Privates ###

    /// <summary>
    /// Gizmo drawing method, called by OnDrawGizmos and OnDrawGizmosSelected.
    /// </summary>
    /// <remarks>Gizmo-Visibility will be set by the user in the editor</remarks>
    protected virtual void DoGizmos()
    {
        Gizmos.DrawIcon(Transform.position, GetType().Name,false);
        if (mbDebugging) {
            foreach (MBParticle PT in mbDebugParticles) {
                Vector3 scale = new Vector3(PT.Scale.x * Transform.lossyScale.x,
                                            PT.Scale.y * Transform.lossyScale.y,
                                            PT.Scale.z * Transform.lossyScale.z);
                Gizmos.matrix = Matrix4x4.TRS(PT.WorldPosition, ParticleSystem.Transform.rotation * Quaternion.Euler(0, 0, -PT.Orientation), scale);
                // Size
                if (mbDebugSize) {
                    Gizmos.color = mbDebugSizeCol;
                    MBUtility.DrawGizmoRect(new Rect(-1, -1, 2, 2));
                }

                if (mbDebugOrientation) {
                    Gizmos.color = mbDebugOrientationCol;
                    MBUtility.DrawGizmoArrow(Vector3.zero, Vector3.up,1.25f);
                }

                if (mbDebugAge) {
                    Gizmos.color = mbDebugAgeCol;
                    float size = (1-PT.AgePercent);
                    Gizmos.DrawCube(new Vector3(-1,-1+size,0),new Vector3(0.1f,size*2,0.1f));
                }

                Gizmos.matrix = Matrix4x4.TRS(PT.WorldPosition, ParticleSystem.Transform.rotation, scale);
                // Heading
                if (mbDebugHeading) {
                    Gizmos.color = mbDebugHeadingCol;
                    Gizmos.DrawRay(Vector3.zero, PT.Heading);
                }
                if (mbDebugForces) {
                    Gizmos.color = mbDebugForcesCol;
                    Gizmos.DrawRay(Vector3.zero, PT.Force);
                }
                if (mbDebugSpeed) {
                    Gizmos.color = mbDebugSpeedCol;
                    Gizmos.DrawRay(Vector3.zero, PT.Velocity);
                }
                // Unscaled
                Gizmos.matrix = Matrix4x4.TRS(PT.WorldPosition, ParticleSystem.Transform.rotation, Vector3.one);
                if (mbDebugZones && PT.AffectedByZones!=null) {
                    Gizmos.color = mbDebugZonesCol;
                    Gizmos.DrawCube(Vector3.zero, new Vector3(0.1f * PT.AffectedByZones.Count, 0.1f * PT.AffectedByZones.Count, 0.1f * PT.AffectedByZones.Count));
                }
            }
        }
    }

    void OnParticleBirth(MBParticle PT)
    {
        if (ParticleBirth!=null) 
            ParticleBirth(new MBEvent(MBEventType.ParticleBirth, this, PT, null));
        else if (ParticleBirthSM != null && ParticleBirthSM.Target && !string.IsNullOrEmpty(ParticleBirthSM.MethodName)) 
            ParticleBirthSM.Invoke(new MBEvent(MBEventType.ParticleBirth, this, PT, null));
    }

    void OnParticleDeath(MBParticle PT, MBDeathReason reason)
    {
        if (ParticleDeath != null)
            ParticleDeath(new MBEvent(MBEventType.ParticleDeath, this, PT, reason));
        else if (ParticleDeathSM != null && ParticleDeathSM.Target && !string.IsNullOrEmpty(ParticleDeathSM.MethodName))
            ParticleDeathSM.Invoke(new MBEvent(MBEventType.ParticleDeath, this, PT, reason));
    }

    void OnEmitterStopsPlaying()
    {
        if (EmitterStopsPlaying != null)
            EmitterStopsPlaying(new MBEvent(MBEventType.EmitterStops, this, this, null));
        else if (EmitterStopsPlayingSM != null && EmitterStopsPlayingSM.Target && !string.IsNullOrEmpty(EmitterStopsPlayingSM.MethodName))
            EmitterStopsPlayingSM.Invoke(new MBEvent(MBEventType.EmitterStops, this, this, null));
    }

    void UnregisterFromControllingParent()
    {
        if (!mParticleSystem) return;
        if (ControllingParent == ParticleSystem)
            ParticleSystem.Emitter.Remove(this);
        else
            ((MBEmitter)ControllingParent).Emitter.Remove(this);
        mControllingParent = null;
    }

    void RegisterAtControllingParent()
    {
        if (ControllingParent is MBEmitter)
            ((MBEmitter)ControllingParent).Emitter.Add(this);
        else {
            ((MBParticleSystem)ControllingParent).Emitter.Add(this);
            IsTrail = false;
        }

    }
    #endregion


    [System.Obsolete("MBEmitter.AddEmitter is obsolete. Use MBParticleSystem.AddEmitter (parent) instead!")]
    public MBEmitter AddEmitter()
    {
        return ParticleSystem.AddEmitter(this);
    }

    [System.Obsolete("MBEmitter.AddEmitter is obsolete. Use MBParticleSystem.AddEmitter (source,parent) instead!")]
    public MBEmitter AddEmitter(MBEmitter source)
    {
        return ParticleSystem.AddEmitter(source, this);
    }
}

/// <summary>
/// Defines death reasons.
/// </summary>
/// <remarks>This statistical values help you to optimize your ParticleSystems</remarks>
public enum MBDeathReason
{
    /// <summary>
    /// Age > 1
    /// </summary>
    Age = 0,
    /// <summary>
    /// Size <= 0
    /// </summary>
    Size = 1,
    /// <summary>
    /// Alpha <= 0
    /// </summary>
    Color = 2,
    /// <summary>
    /// By one of the zone parameters
    /// </summary>
    Zone = 3,
    /// <summary>
    /// The rest
    /// </summary>
    User = 4
}

/// <summary>
/// Defines billboarding methods
/// </summary>
public enum MBBillboard
{
    /// <summary>
    /// Full billboarding, particles will always face the camera (Slowest)
    /// </summary>
    Billboard=0,
    /// <summary>
    /// Particles won't turn and keep on the X/Z-plane (Fast)
    /// </summary>
    /// <remarks>Use this if your camera won't turn</remarks>
    Horizontal2D=1,
    /// <summary>
    /// Particles won't turn and keep on the X/Y-plane (Fast)
    /// </summary>
    /// <remarks>Use this if your camera won't turn</remarks>
    Vertical2D=2
}