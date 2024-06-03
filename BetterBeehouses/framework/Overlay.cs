using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;

namespace BetterBeehouses.framework
{
	internal class Overlay
	{
		public static bool Enabled
		{
			get => enabled.Value; 
			set => enabled.Value = value;
		}
		private static readonly PerScreen<bool> enabled = new();

		public static void Init(IModHelper helper)
		{
			helper.Events.Display.RenderedWorld += Draw;
			helper.Events.Input.ButtonPressed += OnKey;
		}

		private static void OnKey(object sender, ButtonPressedEventArgs e)
		{
			if (Config.config.DebugKey.JustPressed())
				Enabled = !Enabled;
		}

		private static void Draw(object sender, RenderedWorldEventArgs e)
		{
			if (!Enabled)
				return;

			var mouse = Game1.getMousePositionRaw();
			Vector2 offset = new(Game1.viewport.Location.X, Game1.viewport.Location.Y);
			Vector2 mouseTile = new(MathF.Floor((mouse.X + offset.X) / Game1.tileSize), MathF.Floor((mouse.Y + offset.Y) / Game1.tileSize));

			if (!Game1.currentLocation.Objects.TryGetValue(mouseTile, out var obj))
				return;

			if (obj.QualifiedItemId != "(BC)10" && (!Config.config.ModifyCustomBeehouses || !obj.HasContextTag("bee_house")))
				return;

			var b = e.SpriteBatch;
			var tint = Utility.GetPrismaticColor() * .3f;

			foreach (var data in FlowerFinder.GetAllNearFlowers(Game1.currentLocation, FlowerFinder.DefaultSearch(mouseTile)))
			{
				Rectangle localTile = 
					new(
						(int)data.Tile.X * Game1.tileSize - (int)offset.X, 
						(int)data.Tile.Y * Game1.tileSize - (int)offset.Y, 
						Game1.tileSize, Game1.tileSize
					);

				var item = ItemRegistry.GetDataOrErrorItem(data.ID);

				b.Draw(Game1.staminaRect, localTile, Color.Black * .2f);
				b.Draw(Game1.staminaRect, localTile, tint);
				b.Draw(item.GetTexture(), localTile, item.GetSourceRect(), Color.White);
			}
		}
	}
}
