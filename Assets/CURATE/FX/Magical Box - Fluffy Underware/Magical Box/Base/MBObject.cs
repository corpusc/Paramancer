// =====================================================================
// Copyright 2011-2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Base class that all Magical Box components (except particles) inherit from
/// </summary>
public class MBObject : MonoBehaviour, System.IComparable
{
    #region ### Inspector Fields ###
    /// <summary>
    /// Whether this object is muted or not
    /// </summary>
    /// <remarks>Muted objects won't be processed and rendered</remarks>
    public bool Muted;
    /// <summary>
    /// Gets or sets object's scale
    /// </summary>
    public Vector3 Scale;

    

    #endregion

    #region ### Public Properties ###
   
    /// <summary>
    /// Gets a list of (enabled) children.
    /// </summary>
    public List<MBObject> Children
    {
        get
        {
            if (mChildren == null) {
                mChildren = MBUtility.GetChildren<MBObject>(Transform, true, true);
                mChildren.Sort();
            }
            return mChildren;
        }
    }
    /// <summary>
    /// Gets the number of (enabled) children
    /// </summary>
    /// <remarks>This is just a shortcut to Children.Count</remarks>
    public int Count { get { return Children.Count; } }

    /// <summary>
    /// Gets the parent
    /// </summary>
    public virtual MBObject Parent
    {
        get
        {
            if (!mParent)
                mParent = MBUtility.GetParent<MBObject>(transform);
            return mParent;
        }
    }

    /// <summary>
    /// Gets the parent ParticleSystem
    /// </summary>
    /// <remarks>This may be the direct parent or the next ParticleSystem (if any) up the hierarchy</remarks>
    public virtual MBParticleSystem ParticleSystem
    {
        get
        {
            if (!mParticleSystem)
                mParticleSystem = MBUtility.GetParent<MBParticleSystem>(transform);
            return mParticleSystem;
        }
    }
    /// <summary>
    /// Gets or sets the position in ParticleSystem's space
    /// </summary>
    /// <remarks>
    /// This involves a TransformPoint calculation, so use it wisely!
    /// </remarks>
    public Vector3 Position
    {
        get { return ParticleSystem.Transform.InverseTransformPoint(Transform.position); }
        set { Transform.position = ParticleSystem.Transform.TransformPoint(value); }
    }

    /// <summary>
    /// Gets whether this object should draw gizmos
    /// </summary>
    /// <remarks>
    /// This is set by the editor
    /// </remarks>
    public bool ShouldDrawGizmos
    {
        get
        {
            System.Type T = GetType();
            System.Type Tc;
            // handle inherited classes
            if (T.IsSubclassOf(typeof(MBEmitterType)))
                Tc = typeof(MBEmitter);
            else if (T.IsSubclassOf(typeof(MBParameter)))
                Tc = typeof(MBParameter);
            else
                Tc = T;
            try {
                MBGizmoFlags fl = (MBGizmoFlags)System.Enum.Parse(typeof(MBGizmoFlags), Tc.Name);
                return (ParticleSystem.mbDrawGizmos & fl) == fl;
            }
            catch {
                return false;
            }
        }
    }
    /// <summary>
    /// Gets whether this object should draw gizmos when selected
    /// </summary>
    /// <remarks>
    /// This is set by the editor
    /// </remarks>
    public bool ShouldDrawGizmosSelected
    {
        get
        {
            System.Type T = GetType();
            System.Type Tc;
            // handle inherited classes
            if (T.IsSubclassOf(typeof(MBEmitterType)))
                Tc = typeof(MBEmitter);
            else if (T.IsSubclassOf(typeof(MBParameter)))
                Tc = typeof(MBParameter);
            else
                Tc = T;
            try {
                MBGizmoFlags fl = (MBGizmoFlags)System.Enum.Parse(typeof(MBGizmoFlags), Tc.Name);
                return (ParticleSystem.mbDrawGizmosSelected & fl) == fl;
            }
            catch {
                return false;
            }
        }
    }

    /// <summary>
    /// Gets a child by its index
    /// </summary>
    /// <param name="emitterIndex">index of a child</param>
    /// <returns>the resulting child or null</returns>
    public MBObject this[int childIndex]
    {
        get
        {
            return (Children!=null && childIndex >= 0 && childIndex < Children.Count) ? Children[childIndex] : null;
        }
    }
    /// <summary>
    /// Gets a child by its name
    /// </summary>
    /// <param name="emitterIndex">name of a child</param>
    /// <returns>the resulting child or null</returns>
    public MBObject this[string childName]
    {
        get
        {
            if (Children != null) {
                foreach (MBObject ch in Children)
                    if (ch.name.Equals(childName, System.StringComparison.CurrentCultureIgnoreCase))
                        return ch;
            }
            return null;
        }
    }

    /// <summary>
    /// Gets the transform of this object
    /// </summary>
    /// <remarks>Transform is cached due performance reasons</remarks>
    public Transform Transform
    {
        get
        {
            if (!mTransform)
                mTransform = transform;
            return mTransform;
        }
    }
    #endregion

    #region ### Private Fields ###

    // caching hierarchy
    protected MBParticleSystem mParticleSystem;
    protected MBObject mParent;
    protected List<MBObject> mChildren;
    protected Transform mTransform;

    #endregion

    #region ### Unity Callbacks ###

    /// <summary>
    /// By default draw the corresponding gizmo icon (with name of TypeName)
    /// </summary>
    /// <remarks>ShouldDrawGizmos returns whether gizmos should be drawn or not, depending of the current editor settings</remarks>
    protected virtual void OnDrawGizmos()
    {
        if (ShouldDrawGizmos)
            Gizmos.DrawIcon(Transform.position, GetType().Name,false);
        
    }
    /// <summary>
    /// By default draw the corresponding gizmo icon (with name of TypeName)
    /// </summary>
    /// /// <remarks>ShouldDrawGizmos returns whether gizmos should be drawn or not, depending of the current editor settings</remarks>
    protected virtual void OnDrawGizmosSelected()
    {
        if (ShouldDrawGizmosSelected)
            Gizmos.DrawIcon(Transform.position, GetType().Name,false);
    }

    protected virtual void OnEnable()
    {
        //Transform.localScale = Vector3.one;
    }

    protected virtual void OnDisable()
    {
    }

    public virtual void Reset()
    {
    }

    #endregion

    #region ### Public Methods ###

    

    /// <summary>
    /// Destroys this object
    /// </summary>
    /// <remarks>Because all Magical Box objects are part of a cached particle system hierarchy, you need to call this instead of just deleting the GameObject </remarks>
    public virtual void Destroy()
    {
        SetParent(null);
        GameObject.Destroy(gameObject);
    }
    
    /// <summary>
    /// Gets whether class is of one of the given types
    /// </summary>
    /// <param name="types">one or more types</param>
    public bool Matches(params System.Type[] types)
    {
        foreach (System.Type T in types)
            if (GetType() == T || GetType().IsSubclassOf(T))
                return true;
        return false;
    }

    /// <summary>
    /// Recursively clear resources. Useful for parameters that load textures etc...
    /// </summary>
    /// <remarks>This is called by the editor when you export to a prefabs and on Reset()</remarks>
    public virtual void Purge()
    {
        foreach (MBObject obj in MBUtility.GetChildren<MBObject>(transform,true,true))
            obj.Purge();
    }

    /// <summary>
    /// Sets a new parent for this object
    /// </summary>
    public virtual void SetParent(MBObject parent)
    {
        // Valid target?

        if ((parent is MBParameter) ||
             (this is MBParameter && !(parent is MBEmitter))) {
            Debug.LogError(string.Format("Magical Box: '{0}' ({1}) can't be a child of '{2}' ({3}) !", name, GetType(), parent.name, parent.GetType()));
            return;
        }
        
        mTransform = null;
        if (mParent) 
            mParent.Children.Remove(this);
        
        mParent = parent;
        if (parent) {
            transform.parent = parent.Transform;
            mParent.Children.Add(this);
            mParticleSystem = parent.ParticleSystem;
        }
    }
    

    #endregion

    #region ### Internal Publics. Don't use them unless you know what you're doing ###
    /*! @name Internal Public
     * Don't use them unless you know what you're doing
     */
    //@{

    /// <summary>
    /// Force this object and all child objects to refetch parent and reload/recache their stuff
    /// </summary>
    /// <remarks>This is used to update the cached hierarchy after changes were made</remarks>
    public virtual void mbReloadHierarchy()
    {
        mParticleSystem = null;
        mParent = null;
        mChildren = null;
        // cache/refetch now
        if (ParticleSystem) { }
        if (Parent) { }
        // Reload children
        foreach (MBObject obj in Children)
            obj.mbReloadHierarchy();
    }
    //@}
    

    #endregion

    #region IComparable Member

    public virtual int CompareTo(object obj)
    {
        if (this is MBLayer) {
            if (obj is MBLayer)
                return -((MBObject)obj).name.CompareTo(name);
            else
                return -1;
        }
        else if (this is MBAnchor) {
            if (obj is MBAnchor)
                return -((MBObject)obj).name.CompareTo(name);
            else if (obj is MBLayer)
                return 1;
            else
                return -1;
        }
        else if (this is MBEmitter) {
            if (obj is MBLayer || obj is MBAnchor)
                return 1;
            else
                return -((MBObject)obj).name.CompareTo(name);
        } else
            return -((MBObject)obj).name.CompareTo(name);
    }

    #endregion
}

/// <summary>
/// Flags that define which objects should draw gizmos
/// </summary>
[System.Flags]
public enum MBGizmoFlags
{
    None=0,
    MBParticleSystem=1,
    MBAnchor=2,
    MBEmitter=4,
    MBParameter=8,
    AllExceptParameters=7
}

