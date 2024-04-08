using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace BetterBeehouses.framework
{
	public struct FlowerData
	{
		public string ID;
		public Vector2 Tile;
		public Crop crop;
		public Vector2 SourceTile;
		public string type;
		public bool InPot;

		public FlowerData()
		{
			ID = "";
			Tile = default;
			crop = null;
			SourceTile = default;
			type = null;
			InPot = false;
		}

		public FlowerData(Crop crop)
		{
			this.crop = crop;
			ID = crop.indexOfHarvest.Value;
			Tile = crop.tilePosition;
			type = null;
			SourceTile = Tile;
			InPot = false;
		}

		public FlowerData(Vector2 pos, string id, string type)
		{
			ID = id;
			Tile = pos;
			crop = null;
			this.type = type;
			SourceTile = Tile;
			InPot = false;
		}

		public FlowerData(GiantCrop giant, Vector2 pos, string id)
		{
			ID = id;
			SourceTile = giant.Tile;
			Tile = pos;
			type = "GiantCrop";
			crop = null;
			InPot = false;
		}
	}
}
