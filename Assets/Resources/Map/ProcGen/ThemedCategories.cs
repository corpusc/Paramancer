using UnityEngine;
using System.Collections.Generic;



// surfaces will belong to categories,   and be different for each theme 
public class ThemedCategories {
	public List<Material> Lips = new List<Material>(); // moulding 
	public List<Material> Walls = new List<Material>();
	public List<Material> Floors = new List<Material>();
	public List<Material> Ceilings = new List<Material>();



	public void Init(Theme theme = Theme.Fantasy) {
		Mats.Init();

		Debug.Log("Constructing ThemedCategories");

		// setup surfaces palette 
		switch (theme) {
			case Theme.Fantasy:
				fantasy();
				break;
			case Theme.SciFi:
				sciFi();
				break;
		}
	}

	public Surfaces GetRandomSurfaces()	{
		var sur = new Surfaces();

		sur.Floor = Floors[Random.Range(0, Floors.Count)];
		sur.Ceiling = Ceilings[Random.Range(0, Ceilings.Count)];
		sur.Lip = Lips[Random.Range(0, Lips.Count)];
		sur.Walls = Walls[Random.Range(0, Walls.Count)];
		
		while (sur.Walls == sur.Lip)
			sur.Walls = Walls[Random.Range(0, Walls.Count)];

		return sur;
	}
	
	void fantasy() {
		var sand = Mats.Get("Desert_Sand_01");
		var pave = Mats.Get("Pavement_01");
		var roof = Mats.Get("roofing_007");
		var ston = Mats.Get("Stones_01");
		var wood = Mats.Get("Wood_Planks_01");

		Ceilings.Add(sand);
		Ceilings.Add(pave);
		Ceilings.Add(roof);
		Ceilings.Add(ston);
		Ceilings.Add(wood);

		Floors.Add(sand);
		Floors.Add(pave);
		Floors.Add(wood);

		Walls.Add(sand);
		Walls.Add(pave);
		Walls.Add(roof);
		Walls.Add(ston);
		Walls.Add(wood);

		Lips.Add(roof);
		Lips.Add(sand);
		Lips.Add(pave);
		Lips.Add(ston);
		Lips.Add(wood);
	}

	void sciFi() {
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
