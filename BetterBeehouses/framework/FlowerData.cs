using Microsoft.Xna.Framework;
using StardewModdingAPI.Framework.Logging;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace BetterBeehouses.framework
{
	public readonly record struct FlowerData
	{
		public readonly string ID;
		public readonly Vector2 Tile;
		public readonly Crop crop;
		public readonly Vector2 SourceTile;
		public readonly string type;
		public readonly bool InPot;

		public FlowerData(string id, Vector2 tile, Vector2 sourceTile, string type, bool inPot)
		{
			ID = id; Tile = tile; SourceTile = sourceTile; this.type = type; InPot = inPot;
			Print();
		}

		public FlowerData(Crop crop)
		{
			this.crop = crop;
			ID = crop.indexOfHarvest.Value;
			Tile = crop.tilePosition;
			type = "Crop";
			SourceTile = Tile;
			InPot = false;
			Print();
		}

		public FlowerData(Crop crop, Vector2 tile)
		{
			this.crop = crop;
			ID = crop.indexOfHarvest.Value;
			Tile = tile;
			type = "Crop";
			SourceTile = Tile;
			InPot = true;
			Print();
		}

		public FlowerData(Vector2 pos, string id, string type, bool inPot)
		{
			ID = id;
			Tile = pos;
			crop = null;
			this.type = type;
			SourceTile = Tile;
			InPot = inPot;
			Print();
		}

		public FlowerData(GiantCrop giant, Vector2 pos, string id)
		{
			ID = id;
			SourceTile = giant.Tile;
			Tile = pos;
			type = "GiantCrop";
			crop = null;
			InPot = false;
			Print();
		}

		private void Print()
		{
			if (ModEntry.monitor.IsVerbose)
				ModEntry.monitor.Log($"Source:\t{SourceTile.X}\t{SourceTile.Y}\tTile:\t{Tile.X}\t{Tile.Y}\tPot: {InPot}\tType:\t{type}\tID:\t{ID}");
		}
	}
}
