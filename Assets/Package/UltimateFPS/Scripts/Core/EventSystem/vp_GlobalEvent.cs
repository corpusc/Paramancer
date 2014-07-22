/////////////////////////////////////////////////////////////////////////////////
//
//	vp_GlobalEvent.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	This class allows the sending of generic events to and from
//					any class with generic listeners which register and unregister
//					from the events. Events can have 0-3 arguments.
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public delegate void vp_GlobalCallback();									// 0 Arguments
public delegate void vp_GlobalCallback<T>(T arg1);							// 1 Argument
public delegate void vp_GlobalCallback<T, U>(T arg1, U arg2);				// 2 Arguments
public delegate void vp_GlobalCallback<T, U, V>(T arg1, U arg2, V arg3);	// 3 Arguments


// Event with no arguments
public static class vp_GlobalEvent
{

	private static Hashtable m_Callbacks = new Hashtable();
	
	/// <summary>
	/// Registers the event specified by name
	/// </summary>
	public static void Register(string name, vp_GlobalCallback callback)
	{
	
		if(string.IsNullOrEmpty(name))
			throw new ArgumentNullException(@"name");
			
    	if(callback == null)
        	throw new ArgumentNullException("callback");
        
    	List<vp_GlobalCallback> callbacks = (List<vp_GlobalCallback>)m_Callbacks[name];
    	if(callbacks == null)
    	{
        	callbacks = new List<vp_GlobalCallback>();
        	m_Callbacks.Add(name, callbacks);
    	}
   		callbacks.Add(callback);
   		
	}
	
	/// <summary>
	/// Unregisters the event specified by name
	/// </summary>
	public static void Unregister(string name, vp_GlobalCallback callback)
	{
	
	    if(string.IsNullOrEmpty(name))
	        throw new ArgumentNullException(@"name");
	        
	    if(callback == null)
	        throw new ArgumentNullException("callback");
	        
	    List<vp_GlobalCallback> callbacks = (List<vp_GlobalCallback>)m_Callbacks[name];
	    if(callbacks != null)
	        callbacks.Remove(callback);
	    
	}
	
	
	/// <summary>
	/// sends an event
	/// </summary>
	public static void Send(string name)
	{
	
	    if(string.IsNullOrEmpty(name))
	        throw new ArgumentNullException(@"name");
	        
	    List<vp_GlobalCallback> callbacks = (List<vp_GlobalCallback>)m_Callbacks[name];
	    if(callbacks != null)
	        foreach(vp_GlobalCallback c in callbacks)
	            c();
	
	}
	
}


// Accepts 1 Argument
public static class vp_GlobalEvent<T>
{

	private static Hashtable m_Callbacks = new Hashtable();
	
	/// <summary>
	/// Registers the event specified by name
	/// </summary>
	public static void Register(string name, vp_GlobalCallback<T> callback)
	{
	
		if(string.IsNullOrEmpty(name))
			throw new ArgumentNullException(@"name");
			
    	if(callback == null)
        	throw new ArgumentNullException("callback");
        
    	List<vp_GlobalCallback<T>> callbacks = (List<vp_GlobalCallback<T>>)m_Callbacks[name];
    	if(callbacks == null)
    	{
        	callbacks = new List<vp_GlobalCallback<T>>();
        	m_Callbacks.Add(name, callbacks);
    	}
   		callbacks.Add(callback);
   		
	}
	
	/// <summary>
	/// Unregisters the event specified by name
	/// </summary>
	public static void Unregister(string name, vp_GlobalCallback<T> callback)
	{
	
	    if(string.IsNullOrEmpty(name))
	        throw new ArgumentNullException(@"name");
	        
	    if(callback == null)
	        throw new ArgumentNullException("callback");
	        
	    List<vp_GlobalCallback<T>> callbacks = (List<vp_GlobalCallback<T>>)m_Callbacks[name];
	    if(callbacks != null)
	        callbacks.Remove(callback);
	    
	}
	
	
	/// <summary>
	/// sends an event with 1 argument
	/// </summary>
	public static void Send(string name, T arg1)
	{
	
	    if(string.IsNullOrEmpty(name))
	        throw new ArgumentNullException(@"name");
	        
	    if(arg1 == null)
	        throw new ArgumentNullException("arg1");
	        
	    List<vp_GlobalCallback<T>> callbacks = (List<vp_GlobalCallback<T>>)m_Callbacks[name];
	    if(callbacks != null)
	        foreach(vp_GlobalCallback<T> c in callbacks)
	            c(arg1);
	
	}
	
}


// Accepts 2 arguments
public static class vp_GlobalEvent<T, U>
{

	private static Hashtable m_Callbacks = new Hashtable();
	
	/// <summary>
	/// Registers the event specified by name
	/// </summary>
	public static void Register(string name, vp_GlobalCallback<T, U> callback)
	{
	
		if(string.IsNullOrEmpty(name))
			throw new ArgumentNullException(@"name");
			
    	if(callback == null)
        	throw new ArgumentNullException("callback");
        
    	List<vp_GlobalCallback<T, U>> callbacks = (List<vp_GlobalCallback<T, U>>)m_Callbacks[name];
    	if(callbacks == null)
    	{
        	callbacks = new List<vp_GlobalCallback<T, U>>();
        	m_Callbacks.Add(name, callbacks);
    	}
   		callbacks.Add(callback);
   		
	}
	
	/// <summary>
	/// Unregisters the event specified by name
	/// </summary>
	public static void Unregister(string name, vp_GlobalCallback<T, U> callback)
	{
	
	    if(string.IsNullOrEmpty(name))
	        throw new ArgumentNullException(@"name");
	        
	    if(callback == null)
	        throw new ArgumentNullException("callback");
	        
	    List<vp_GlobalCallback<T, U>> callbacks = (List<vp_GlobalCallback<T, U>>)m_Callbacks[name];
	    if(callbacks != null)
	        callbacks.Remove(callback);
	    
	}
	
	
	/// <summary>
	/// sends an event with 2 arguments
	/// </summary>
	public static void Send(string name, T arg1, U arg2)
	{
	
	    if(string.IsNullOrEmpty(name))
	        throw new ArgumentNullException(@"name");
	        
	    if(arg1 == null)
	        throw new ArgumentNullException("arg1");
	        
		if(arg2 == null)
	        throw new ArgumentNullException("arg2");
	        
	    List<vp_GlobalCallback<T, U>> callbacks = (List<vp_GlobalCallback<T, U>>)m_Callbacks[name];
	    if(callbacks != null)
	        foreach(vp_GlobalCallback<T, U> c in callbacks)
	            c(arg1, arg2);
	
	}
	
}


// Accepts 3 Arguments
public static class vp_GlobalEvent<T, U, V>
{

	private static Hashtable m_Callbacks = new Hashtable();
	
	/// <summary>
	/// Registers the event specified by name
	/// </summary>
	public static void Register(string name, vp_GlobalCallback<T, U, V> callback)
	{
	
		if(string.IsNullOrEmpty(name))
			throw new ArgumentNullException(@"name");
			
    	if(callback == null)
        	throw new ArgumentNullException("callback");
        
    	List<vp_GlobalCallback<T, U, V>> callbacks = (List<vp_GlobalCallback<T, U, V>>)m_Callbacks[name];
    	if(callbacks == null)
    	{
        	callbacks = new List<vp_GlobalCallback<T, U, V>>();
        	m_Callbacks.Add(name, callbacks);
    	}
   		callbacks.Add(callback);
   		
	}
	
	/// <summary>
	/// Unregisters the event specified by name
	/// </summary>
	public static void Unregister(string name, vp_GlobalCallback<T, U, V> callback)
	{
	
	    if(string.IsNullOrEmpty(name))
	        throw new ArgumentNullException(@"name");
	        
	    if(callback == null)
	        throw new ArgumentNullException("callback");
	        
	    List<vp_GlobalCallback<T, U, V>> callbacks = (List<vp_GlobalCallback<T, U, V>>)m_Callbacks[name];
	    if(callbacks != null)
	        callbacks.Remove(callback);
	    
	}
	
	
	/// <summary>
	/// sends an event with 3 arguments
	/// </summary>
	public static void Send(string name, T arg1, U arg2, V arg3)
	{
	
	    if(string.IsNullOrEmpty(name))
	        throw new ArgumentNullException(@"name");
	        
	    if(arg1 == null)
	        throw new ArgumentNullException("arg1");
	        
		if(arg2 == null)
	        throw new ArgumentNullException("arg2");
	        
		if(arg3 == null)
	        throw new ArgumentNullException("arg3");
	        
	    List<vp_GlobalCallback<T, U, V>> callbacks = (List<vp_GlobalCallback<T, U, V>>)m_Callbacks[name];
	    if(callbacks != null)
	        foreach(vp_GlobalCallback<T, U, V> c in callbacks)
	            c(arg1, arg2, arg3);
	
	}
	
}