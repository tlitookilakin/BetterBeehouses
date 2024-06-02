using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley.Mods;
using StardewValley.Extensions;

namespace BetterBeehouses.framework
{

	internal class BeeManager
	{
		public class Bee
		{
			public Vector2 source;
			public Vector2 target;
			public double pct;
			public double rate;
			public int frame;
			public Rectangle sourceRect;
			public double millis = 0;
		}

		public class SwarmBee
		{
			public Vector2 pos;
			public float life;
			public float angle;
			public float distance;
			public int frame;
			public int precharge;
			public float maxLife;
			public float direction;

			public SwarmBee()
			{
				Reset();
			}

			public void Reset()
			{
				precharge = 0;
				life = Game1.random.Next(3000f) + 2000f;
				maxLife = life;
				angle = Game1.random.Next(MathF.Tau);
				distance = Game1.random.Next(48f) + 16f;
				frame = Game1.random.NextBool() ? 0 : 8;
				direction = Game1.random.NextBool() ? -1 : 1;
				direction = 1;

				(pos.Y, pos.X) = MathF.SinCos(angle * direction);
				pos *= -distance;
				pos.X += Game1.random.Next(-16, 16);
				pos.Y += Game1.random.Next(-16, 16);
			}
		}

		private static readonly PerScreen<List<Bee>> bees = new(() => new());
		private static readonly PerScreen<List<Vector2>> bee_houses = new(() => new());
		private static readonly PerScreen<List<SwarmBee[]>> bee_swarms = new(() => new());

		private static int pamt = -1;

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

		internal static void ApplyConfigCount(int pam)
		{
			if (pamt == pam || pam < 0)
				return;

			pamt = pam;

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
			var swarms = bee_swarms.Value;
			foreach (var pair in ev.Removed)
			{
				int index = houses.IndexOf(pair.Key);
				if (index is -1)
					continue;

				houses.RemoveAt(index);
				swarms.RemoveAt(index);
			}
			foreach ((var pos, var obj) in ev.Added)
			{
				if (obj.HasContextTag("bee_house"))
				{
					houses.Add(pos);
					swarms.Add(GenerateSwarm());

					// add more bees if needed
					var beev = bees.Value;
					var targ = pamt * houses.Count;
					for (int i = beev.Count; i < targ; i++)
						beev.Add(new() { pct = Game1.random.NextDouble() * -10.0 });
				}
			}
		}

		private static SwarmBee[] GenerateSwarm()
		{
			var swarm = new SwarmBee[32];

			for (int i = 0; i < 32; i++)
				swarm[i] = new(){precharge = Game1.random.Next(2000)};

			return swarm;
		}

		private static void ChangeLocation(GameLocation where)
		{
			var houses = bee_houses.Value;
			var beev = bees.Value;
			var swarms = bee_swarms.Value;
			swarms.Clear();
			houses.Clear();
			beev.Clear();

			foreach (var obj in where.Objects.Values)
			{
				if (obj.HasContextTag("bee_house"))
				{
					houses.Add(obj.TileLocation);
					swarms.Add(GenerateSwarm());
				}
			}

			for (int i = 0; i < houses.Count * pamt; i++)
				beev.Add(new() { pct = Game1.random.NextDouble() * -10.0 });
		}

		private static void Exit(object _, ReturnedToTitleEventArgs ev)
		{
			bee_houses.Value.Clear();
			bees.Value.Clear();
			bee_swarms.Value.Clear();
		}

		private static void DrawParticles(SpriteBatch b)
		{
			if (!Config.config.Particles || !ProducingHere())
				return;

			var houses = bee_houses.Value;
			var swarms = bee_swarms.Value;
			var elapsed = Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
			var tex = ModEntry.BeeTex;

			int count = Math.Min(houses.Count, swarms.Count);
			Vector2 tile_offset = new(32f, -32f);

			for (int i = 0; i < count; i++)
			{
				var source = Game1.GlobalToLocal(Game1.viewport, houses[i] * 64f + tile_offset);
				var base_depth = houses[i].Y * 64f;

				foreach(var bee in swarms[i])
				{
					if (bee.precharge > 0)
					{
						bee.precharge -= (int)elapsed;
						continue;
					}
					else if (bee.life <= 0)
					{
						bee.Reset();
						continue;
					}

					Vector2 position = source + bee.pos;
					var (Sin, Cos) = MathF.SinCos(bee.life * MathF.Tau / bee.maxLife * bee.direction + bee.angle);
					position.X += Cos * bee.distance;
					position.Y += Sin * bee.distance;

					float depth = (base_depth + Sin * bee.distance + 48f) * .0001f;

					b.Draw(
						tex, position,
						new Rectangle((((int)bee.life / 32) & 1) * 8, bee.frame, 8, 8),
						Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, depth
					);

					bee.life -= (float)elapsed;
				}
			}

			return;
		}

		private static void DrawBees(SpriteBatch b)
		{
			var houses = bee_houses.Value;
			if (houses.Count == 0 || !Config.config.BeePaths || !ProducingHere())
				return;

			var beev = bees.Value;
			var tex = ModEntry.BeeTex;
			var time = Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
			var view = new Vector2(Game1.viewport.X, Game1.viewport.Y);
			var max_count = Math.Min(beev.Count, pamt * houses.Count);

			for (int i = 0; i < beev.Count; i++)
			{
				var bee = beev[i];

				if (bee.pct > 2.0 && i < max_count)
					SetupBee(bee, houses);
				else if (bee.pct < 0.0)
					bee.pct = Math.Min(bee.pct + time * .001, 0.0);
				else if (bee.pct == 0.0)
					SetupBee(bee, houses);

				if (bee.pct is >= 0.0 and <= 2.0)
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
			if (Config.config.UseRandomFlower)
			{
				var items = FlowerFinder.GetAllNearFlowers(
					Game1.currentLocation, FlowerFinder.DefaultSearch(source)
				).ToArray();

				if (items.Length > 0)
					return items.SelectFrom(source).Tile;
				else
					return source;
			}
			var enumer = FlowerFinder.GetAllNearFlowers(
				Game1.currentLocation, FlowerFinder.DefaultSearch(source)
			).GetEnumerator();

			if (enumer.MoveNext())
				return enumer.Current.Tile;
			return source;
		}
	}
}
