using StardewModdingAPI;
using System;

namespace BetterBeehouses
{
	class Config
	{
		internal enum UsableOptions { Outdoors, Greenhouse, Anywhere }
		internal enum ProduceWhere { Never, Indoors, Always }
		public ProduceWhere ProduceInWinter { get; set; } = ProduceWhere.Indoors;
		public ProduceWhere UsePottedFlowers { get; set; } = ProduceWhere.Always;
		public UsableOptions UsableIn { get; set; } = UsableOptions.Greenhouse;
		public int DaysToProduce { get; set; } = 4;
		public int FlowerRange { get; set; } = 5;
		public bool UseForageFlowers { get; set; } = false;
		public float ValueMultiplier { get; set; } = 1f;
		public bool Particles { get; set; } = true;
		public bool UseQuality { get; set; } = false;
		private float bearBoost = 1f;
		public float BearBoost
		{
			get { return bearBoost; }
			set { bearBoost = Math.Clamp(value, 1f, 3f); }
		}
		public bool UseGiantCrops { get; set; } = true;
		public bool UseFruitTrees { get; set; } = true;
		public bool UseRandomFlower { get; set; } = false;
		public bool UseFlowerBoost { get; set; } = false;
		public int FlowersPerBoost
		{
			get => flowerBoost;
			set => flowerBoost = Math.Max(value, 1);
		}
		private int flowerBoost = 2;
		public bool UseAnyFruitTrees { set; get; } = false;
		public bool BeePaths { set; get; } = true;
		public int ParticleCount { get; set; } = 20;
		public int PathParticleCount { get; set; } = 5;
		public bool AnythingHoney { get; set; } = false;

		private ITranslationHelper i18n => ModEntry.helper.Translation;

		public void ResetToDefault()
		{
			ProduceInWinter = ProduceWhere.Indoors;
			UsePottedFlowers = ProduceWhere.Always;
			UsableIn = UsableOptions.Greenhouse;
			DaysToProduce = 4;
			FlowerRange = 5;
			ValueMultiplier = 1f;
			UseForageFlowers = false;
			Particles = true;
			UseQuality = false;
			BearBoost = 1f;
			UseGiantCrops = true;
			UseFruitTrees = true;
			UseRandomFlower = false;
			UseFlowerBoost = false;
			FlowersPerBoost = 2;
			UseAnyFruitTrees = false;
			BeePaths = true;
			ParticleCount = 20;
			PathParticleCount = 5;
			AnythingHoney = false;
		}

		public void ApplyConfig()
		{
			ModEntry.helper.WriteConfig(this);
			Patch();
		}

		public void Patch()
		{
			ModEntry.helper.GameContent.InvalidateCache("Mods/aedenthorn.ParticleEffects/dict");
			ModEntry.helper.GameContent.InvalidateCache("Data/Machines");
			BeeManager.ApplyConfigCount(ParticleCount, PathParticleCount);
		}

		public void RegisterModConfigMenu(IManifest manifest)
		{
			if (!ModEntry.helper.ModRegistry.IsLoaded("spacechase0.GenericModConfigMenu"))
				return;

			var api = ModEntry.helper.ModRegistry.GetApi<integration.IGMCMAPI>("spacechase0.GenericModConfigMenu");

			api.Register(manifest, ResetToDefault, ApplyConfig);

			//main
			api.AddQuickInt(this, manifest, nameof(DaysToProduce), 1, 7);
			api.AddQuickInt(this, manifest, nameof(FlowerRange), 1, 14);
			api.AddQuickEnum<UsableOptions>(this, manifest, nameof(UsableIn));
			api.AddQuickEnum<ProduceWhere>(this, manifest, nameof(ProduceInWinter));
			api.AddQuickBool(this, manifest, nameof(UseQuality));
			api.AddQuickFloat(this, manifest, nameof(BearBoost), 1f, 3f, .05f);
			api.AddQuickBool(this, manifest, nameof(UseFlowerBoost));
			api.AddQuickInt(this, manifest, nameof(FlowersPerBoost), 1, 8);
			api.AddQuickLink("sources", manifest);
			api.AddQuickLink("visual", manifest);
			api.AddQuickLink("price", manifest);
			api.AddQuickLink("integration", manifest);

			//sources
			api.AddPage(manifest, "sources", () => i18n.Get("config.sources.name"));
			api.AddQuickEnum<ProduceWhere>(this, manifest, nameof(UsePottedFlowers));
			api.AddQuickBool(this, manifest, nameof(UseForageFlowers));
			api.AddQuickBool(this, manifest, nameof(UseRandomFlower));
			api.AddQuickBool(this, manifest, nameof(UseGiantCrops));
			api.AddQuickBool(this, manifest, nameof(UseFruitTrees));
			api.AddQuickBool(this, manifest, nameof(UseAnyFruitTrees));
			api.AddQuickBool(this, manifest, nameof(AnythingHoney));

			//visual
			api.AddPage(manifest, "visual", () => i18n.Get("config.visual.name"));
			if (ModEntry.AeroCore is not null || ModEntry.helper.ModRegistry.IsLoaded("aedenthorn.ParticleEffects"))
			{
				api.AddQuickBool(this, manifest, nameof(Particles));
				if (ModEntry.AeroCore is not null) // not easily supported with Particles mod
					api.AddQuickInt(this, manifest, nameof(ParticleCount), 1, 40);
			}
			else
			{
				api.AddParagraph(manifest, () => i18n.Get("config.noparticles"));
			}
			api.AddQuickBool(this, manifest, nameof(BeePaths));
			api.AddQuickInt(this, manifest, nameof(PathParticleCount), 0, 20);

			//price balancing
			api.AddPage(manifest, "price", () => i18n.Get("config.price.name"));
			api.AddQuickFloat(this, manifest, nameof(ValueMultiplier), .1f, .2f, .1f);
		}
		public Config()
		{
			ResetToDefault();
		}
	}
}
