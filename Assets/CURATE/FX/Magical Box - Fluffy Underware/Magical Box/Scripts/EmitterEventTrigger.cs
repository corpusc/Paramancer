// =====================================================================
// Copyright 2011-2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;

/// <summary>
/// Simple trigger class that registers to events and sends a message
/// </summary>
public class EmitterEventTrigger : MonoBehaviour {
    /// <summary>
    /// The type of event you want to watch
    /// </summary>
    public MBEventType Event;
    /// <summary>
    /// The SendMessage target
    /// </summary>
    public MonoBehaviour Target;
    /// <summary>
    /// The SendMessage method to call
    /// </summary>
    public string Method;

    void OnEnable()
    {
        MBEmitter em = GetComponent<MBEmitter>();
        if (em) {
            switch (Event) {
                case MBEventType.EmitterStops: em.EmitterStopsPlaying += new MBEventHandler(emEvent); break;
                case MBEventType.ParticleBirth: em.EmitterStopsPlaying += new MBEventHandler(emEvent); break;
                case MBEventType.ParticleDeath: em.ParticleDeath += new MBEventHandler(emEvent); break;
            }
        }
    }

    void emEvent(MBEvent e)
    {
        if (Target)
            Target.SendMessage(Method,e);
    }
}
