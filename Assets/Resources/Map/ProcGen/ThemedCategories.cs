using UnityEngine;
using System.Collections.Generic;



public class ThemedCategories {
	public List<Material> Lips = new List<Material>(); // moulding 
	public List<Material> Walls = new List<Material>();
	public List<Material> Floors = new List<Material>();
	public List<Material> Ceilings = new List<Material>();



	public ThemedCategories() {
		Debug.Log("");
		
		var metalFloor = Mats.Get("metal_floor_003");
		var metalGroovedEdges = Mats.Get("metal_plate_005");
		var metalWithRivets = Mats.Get("metal_plate_008");
		var sciFiMat = Mats.Get("sci_fi_003");
		
		Ceilings.Add(Mats.Get("Shutter_01"));
		Ceilings.Add(metalFloor);
		Ceilings.Add(metalGroovedEdges);
		Ceilings.Add(metalWithRivets);
		Ceilings.Add(sciFiMat);

		Walls.Add(sciFiMat);
		Walls.Add(metalGroovedEdges);
		Walls.Add(metalWithRivets);
		
		Floors.Add(metalFloor);
		Floors.Add(metalGroovedEdges);
		Floors.Add(sciFiMat);
		
		Lips.Add(metalFloor);
		Lips.Add(metalWithRivets);
	}
}
