// global general purpose singleton

using UnityEngine;
using System;
using System.Globalization;



static public class S {
	static public int GetInt(string v) {
		return Convert.ToInt32(v.Trim(), new CultureInfo("en-US"));
	}
}
