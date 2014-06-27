// =====================================================================
// Copyright 2011-2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

/// <summary>
/// Class that represents a Magical Box event
/// </summary>
public class MBEvent
{
    /// <summary>
    /// Type of this event
    /// </summary>
    public MBEventType Type { get; private set; }
    /// <summary>
    /// The object rasing this event
    /// </summary>
    public MBObject Source { get; private set; }
    /// <summary>
    /// Context/Detail of this event
    /// </summary>
    public object Context { get; private set; }
    /// <summary>
    /// Data of this event
    /// </summary>
    public object Data { get; private set; }

    public MBEvent(MBEventType type, MBParticle PT, object data)
    {
        Type = type;
        Source = PT.Parent;
        Context = PT;
        Data = data;
    }
    public MBEvent(MBEventType type, MBObject source, object context, object data)
    {
        Type = type;
        Source = source;
        Context = context;
        Data = data;
    }

    /// <summary>
    /// For Particle events, this gets the Particle (stored in Context)
    /// </summary>
    public MBParticle Particle { get { return Context as MBParticle; } }
    /// <summary>
    /// For Particle events, this gets the Death Reason (stored in Data) 
    /// </summary>
    public MBDeathReason DeathReason { get { return (MBDeathReason)Data; } }
}

/// <summary>
/// Defines various event types
/// </summary>
public enum MBEventType
{
    /// <summary>
    /// Raised by an emitter when a particle is born
    /// </summary>
    ParticleBirth = 1,
    /// <summary>
    /// Raised by an emitter when a particle die
    /// </summary>
    ParticleDeath = 2,
    /// <summary>
    /// Raised by a parameter
    /// </summary>
    ParameterCallback = 10,

    /// <summary>
    /// Raised by an emitter when stopping
    /// </summary>
    EmitterStops = 21
}

/// <summary>
/// Magical Box event delegate
/// </summary>
public delegate void MBEventHandler(MBEvent e);

/// <summary>
/// Class handling SendMessage-like events
/// </summary>
/// <remarks>
/// \sa \ref eventusage
/// </remarks>
[System.Serializable]
public class MBSendMessageTarget
{
    public GameObject Target;
    public string MethodName = "";

    List<MBMessageObject> mTargets = new List<MBMessageObject>();

    public MBSendMessageTarget() { }

    public MBSendMessageTarget(GameObject target, string methodName)
    {
        Target = target;
        MethodName = methodName;
        Prepare();
    }

    public void Prepare()
    {
        mTargets.Clear();
        if (Target && !string.IsNullOrEmpty(MethodName)) {
            MonoBehaviour[] c = Target.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour o in c) {
                System.Type T = o.GetType();
                MethodInfo mi=T.GetMethod(MethodName,BindingFlags.Instance|BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.IgnoreCase);
                if (mi != null) {
                    mTargets.Add(new MBMessageObject() { Object = o, Method = mi });
                }
            }
        }
    }

    public void Invoke(MBEvent e)
    {
        if (mTargets.Count==0)
            Prepare();
        foreach (MBMessageObject o in mTargets) {
            if (o.Object!=null)
                o.Method.Invoke(o.Object, new object[] { e });
        }
    }
}

/// <summary>
/// Class holding callback infos gathered by reflection
/// </summary>
public class MBMessageObject
{
    public Object Object;
    public MethodInfo Method;
}