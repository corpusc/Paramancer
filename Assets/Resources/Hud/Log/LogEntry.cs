using UnityEngine;
using System.Collections;

public class LogEntry {
	public string Maker;
	public Color Color;
	public string Text;
	public float TimeAdded;

	public LogEntry() {
		TimeAdded = Time.time;
	}
}
