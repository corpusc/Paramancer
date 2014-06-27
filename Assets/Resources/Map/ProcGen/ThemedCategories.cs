using UnityEngine;
using System.Collections;



public class ThemedCategories {
	public List<Material> lips = new List<Material>();
	public List<Material> walls = new List<Material>();
	public List<Material> floors = new List<Material>();
	public List<Material> ceilings = new List<Material>();



	public ThemedCategories() {
		Debug.Log("");
		
		var MetalFloor = Mats.Get("metal_floor_003");
		var MetalGroovedEdges = Mats.Get("metal_plate_005");
		var MetalWithRivets = Mats.Get("metal_plate_008");
		var SciFiMat = Mats.Get("sci_fi_003");
		
		ceilings.Add(Mats.Get("Shutter_01"));
		ceilings.Add(MetalFloor);
		ceilings.Add(MetalGroovedEdges);
		ceilings.Add(MetalWithRivets);
		ceilings.Add(SciFiMat);

		walls.Add(SciFiMat);
		walls.Add(MetalGroovedEdges);
		walls.Add(MetalWithRivets);
		
		floors.Add(MetalFloor);
		floors.Add(MetalGroovedEdges);
		floors.Add(SciFiMat);
		
		lips.Add(MetalFloor);
		lips.Add(MetalWithRivets);
	}
}
