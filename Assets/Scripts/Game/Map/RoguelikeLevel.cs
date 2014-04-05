using UnityEngine;
using System.Collections;

public struct Vec2i {
	public int x;
	public int y;
};

public class RoguelikeLevel : MonoBehaviour {
	public bool[,] Block; //2d array
	public Vec2i MapSize;
	public int Forms; //the amount of rooms/hallways to create
	public float MaxOverride = 0.1f; //only create a form if there aren't too many things already in there
	public int MinFormWidth = 16;
	public int MaxFormWidth = 500;
	public int MaxArea = 10000; //limits the creation of extremely large rooms

	int safetyLimit = 2000; //the limit of tries Build() can do before surrendering

	// Use this for initialization
	void Start () {
		Block = new bool[MapSize.x, MapSize.y];
		EmptyMap();
		Build();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void Build () {
		preBuild();
		int formsMade = 0;
		for (int i = 0; i < safetyLimit && formsMade < Forms; i++) {
			Vec2i t;
			t.x = Random.Range(0, MapSize.x);
			t.y = Random.Range(0, MapSize.y);

			Vec2i u;
			u.x = Random.Range(0, MapSize.x);
			u.y = Random.Range(0, MapSize.y); //this is last-exclusive, hence all the <= and ...+ 1 later on
			
			Vec2i start;
			Vec2i end;
			start.x = Mathf.Min(t.x, u.x);
			start.y = Mathf.Min(t.y, u.y);
			end.x = Mathf.Max(t.x, u.x);
			end.y = Mathf.Max(t.y, u.y);
			if (containsBlocks(start, end)) {
				if ((end.x >= start.x + MinFormWidth) && (end.y >= start.y + MinFormWidth))
				if ((end.x <= start.x + MaxFormWidth) && (end.y <= start.y + MaxFormWidth))
				if ((end.x - start.x + 1) * (end.y - start.y + 1) <= MaxArea)
					if (blocksInRect(start, end) < (end.x - start.x + 1) * (end.y - start.y + 1) * MaxOverride) {
						fillRect(start, end);
					formsMade++;
				}
			}
		}
	}

	void preBuild() {
		Vec2i a;
		a.x = Random.Range(0, MapSize.x - MaxFormWidth);
		a.y = Random.Range(0, MapSize.y - MaxFormWidth);
		Vec2i b;
		b.x = Random.Range(a.x + MinFormWidth, a.x + MaxFormWidth);
		b.y = Random.Range(a.y + MinFormWidth, a.y + MaxFormWidth);
		fillRect(a, b);
	}

	bool containsBlocks (Vec2i start, Vec2i end) {
		for (int i = start.x; i <= end.x; i++)
			for (int j = start.y; j <= end.y; j++)
				if (Block[i, j]) return true;

		return false;
	}

	int blocksInRect (Vec2i start, Vec2i end) {
		int c = 0;
		for (int i = start.x; i <= end.x; i++)
			for (int j = start.y; j <= end.y; j++)
				if (Block[i, j]) c++;

		return c;
	}

	void fillRect (Vec2i start, Vec2i end) {
		for (int i = start.x; i <= end.x; i++)
			for (int j = start.y; j <= end.y; j++)
				Block[i, j] = true;
	}

	public void EmptyMap () {
		for (int i = 0; i < MapSize.x; i++)
			for (int j = 0; j < MapSize.y; j++)
				Block[i, j] = false;
	}
}
