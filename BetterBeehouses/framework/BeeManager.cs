using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using BetterBeehouses.integration;
using HarmonyLib;
using BetterBeehouses.patches;
using StardewValley.Mods;

namespace BetterBeehouses.framework
{
	internal class Bee
	{
		internal Vector2 source;
		internal Vector2 target;
		internal double pct;
		internal double rate;
		internal int frame;
		internal Rectangle sourceRect;
		internal double millis = 0;
	}

	internal class BeeManager
	{
		private static readonly PerScreen<List<Bee>> bees = new(() => new());
		private static readonly PerScreen<List<Vector2>> bee_houses = new(() => new());

		private static int pamt = -1;
		private static int bamt = -1;

		internal static void Init()
		{
			ModEntry.helper.Events.World.ObjectListChanged += UpdateObjects;
			ModEntry.helper.Events.Player.Warped += (s, e) => ChangeLocation(e.NewLocation);
			ModEntry.helper.Events.GameLoop.SaveLoaded += (s, e) => ChangeLocation(Game1.currentLocation);
			ModEntry.helper.Events.GameLoop.ReturnedToTitle += Exit;
			ModEntry.helper.Events.Display.RenderingStep += RenderStep;
		}

		private static void RenderStep(object sender, RenderingStepEventArgs e)
		{
			if (e.Step is RenderSteps.World_Sorted)
			{
				var b = e.SpriteBatch;
				DrawBees(b);
				DrawParticles(b);
			}
		}

		internal static void ApplyConfigCount(int amt, int pam)
		{
			if (pamt == pam || pam < 0 || amt == bamt || amt < 0)
				return;

			pamt = pam;
			bamt = amt;
			var houses = bee_houses.Value;
			var beev = bees.Value;
			var targ = pamt * houses.Count;
			if (beev.Count > targ)
				beev.RemoveRange(targ, beev.Count - targ);
			else if (beev.Count < targ)
				for (int i = beev.Count; i < targ; i++)
					beev.Add(new() { pct = Game1.random.NextDouble() * -10.0 });
		}

		private static void UpdateObjects(object _, ObjectListChangedEventArgs ev)
		{
			var houses = bee_houses.Value;
			foreach (var pair in ev.Removed)
			{
				houses.Remove(pair.Key);
			}
			foreach ((var pos, var obj) in ev.Added)
			{
				if (obj.Name is "Bee House")
				{
					houses.Add(pos);
				}
			}
		}

		private static void ChangeLocation(GameLocation where)
		{
			var houses = bee_houses.Value;
			var beev = bees.Value;
			houses.Clear();
			beev.Clear();
			foreach (var obj in where.Objects.Values)
				if (obj.Name is "Bee House")
					houses.Add(obj.TileLocation);
			for (int i = 0; i < houses.Count * pamt; i++)
				beev.Add(new() { pct = Game1.random.NextDouble() * -10.0 });
		}
		private static void Exit(object _, ReturnedToTitleEventArgs ev)
		{
			bee_houses.Value.Clear();
			bees.Value.Clear();
		}
		private static void DrawParticles(SpriteBatch b)
		{
			if (!ModEntry.config.Particles || !ProducingHere())
				return;

			// TODO readd swarms

			return;
		}
		private static void DrawBees(SpriteBatch b)
		{
			var houses = bee_houses.Value;
			if (houses.Count == 0 || !ModEntry.config.BeePaths || !ProducingHere())
				return;

			var beev = bees.Value;
			var tex = ModEntry.BeeTex;
			var time = Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
			var view = new Vector2(Game1.viewport.X, Game1.viewport.Y);
			for (int i = 0; i < beev.Count; i++)
			{
				var bee = beev[i];
				if (bee.pct > 2.0)
					SetupBee(bee, houses);
				else if (bee.pct < 0.0)
					bee.pct = Math.Min(bee.pct + time * .001, 0.0);
				else if (bee.pct == 0.0)
					SetupBee(bee, houses);

				if (bee.pct >= 0.0)
				{
					// draw
					var pos = Vector2.Lerp(bee.target, bee.source, MathF.Abs(1f - (float)bee.pct));
					b.Draw(tex, pos - view, bee.sourceRect, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, (pos.Y + 48f) * .0001f);

					// move
					bee.millis += time;
					if (bee.millis > 50)
					{
						bee.frame = bee.frame > 0 ? 0 : 1;
						bee.sourceRect.X = bee.frame * 8;
						bee.millis %= 50;
					}
					bee.pct += time * bee.rate * .05; // pixels/millisecond speed
				}
			}
		}

		private static bool ProducingHere()
			=>  bee_houses.Value.Count is not 0 && 
				Game1.currentLocation.Objects.TryGetValue(bee_houses.Value[0], out var sobj) && 
				sobj.ShouldTimePassForMachine();

		private static void SetupBee(Bee bee, IList<Vector2> houses)
		{
			var src = houses[Game1.random.Next(houses.Count)];
			bee.source = src * 64f + new Vector2((float)Game1.random.NextDouble() * 32f + 8f, (float)Game1.random.NextDouble() * 32f - 8f);
			bee.target = GetTarget(src) * 64f + new Vector2(Game1.random.Next(32f) + 16f, Game1.random.Next(32f) - 8f);
			bee.rate = 1f / Vector2.Distance(bee.source, bee.target);
			var variant = Game1.random.Next(2);
			bee.sourceRect = new(0, variant * 8, 8, 8);
			bee.pct = 0.0;
		}

		private static Vector2 GetTarget(Vector2 source)
		{
			if (ModEntry.config.UseRandomFlower)
			{
				var items = Utilities.GetAllNearFlowers(Game1.currentLocation, source, ModEntry.config.FlowerRange).ToArray();
				if (items.Length > 0)
					return items[Game1.random.Next(items.Length)].Key;
				else
					return source;
			}
			var enumer = Utilities.GetAllNearFlowers(Game1.currentLocation, source, ModEntry.config.FlowerRange).GetEnumerator();
			if (enumer.MoveNext())
				return enumer.Current.Key;
			return source;
		}
	}
}
