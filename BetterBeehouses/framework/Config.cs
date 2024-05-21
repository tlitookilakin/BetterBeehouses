using StardewModdingAPI;
using StardewValley;
using System;
using xTile.Dimensions;

namespace BetterBeehouses.framework
{
    class Config
    {
        public static Config config => inst ??= ModEntry.helper.ReadConfig<Config>();
        private static Config inst;

        internal enum UsableOptions { Outdoors, Greenhouse, Anywhere }
        internal enum ProduceWhere { Never, Indoors, Always }
        public ProduceWhere ProduceInWinter { get; set; }
        public ProduceWhere UsePottedFlowers { get; set; }
        public UsableOptions UsableIn { get; set; }
        public int DaysToProduce { get; set; }
        public int FlowerRange { get; set; }
        public bool UseForageFlowers { get; set; }
        public float ValueMultiplier { get; set; }
        public bool Particles { get; set; }
        public bool UseQuality { get; set; }
        private float bearBoost = 1f;
        public float BearBoost
        {
            get { return bearBoost; }
            set { bearBoost = Math.Clamp(value, 1f, 3f); }
        }
        public bool UseGiantCrops { get; set; }
        public bool UseFruitTrees { get; set; }
        public bool UseRandomFlower { get; set; }
        public bool UseFlowerBoost { get; set; }
        public int FlowersPerBoost
        {
            get => flowerBoost;
            set => flowerBoost = Math.Max(value, 1);
        }
        private int flowerBoost = 2;
        public bool UseAnyFruitTrees { set; get; }
        public bool BeePaths { set; get; }
        public int PathParticleCount { get; set; }
        public bool AnythingHoney { get; set; }
        public bool UseBushes { get; set; }
        public bool ModifyCustomBeehouses { get; set; }

        private static ITranslationHelper i18n => ModEntry.helper.Translation;

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
            PathParticleCount = 5;
            AnythingHoney = false;
            UseBushes = true;
            ModifyCustomBeehouses = true;
        }

        public void ApplyConfig()
        {
            ModEntry.helper.WriteConfig(this);
            Patch();
        }

        public void Patch()
        {
            ModEntry.helper.GameContent.InvalidateCache("Data/Machines");
            BeeManager.ApplyConfigCount(PathParticleCount);
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
            api.AddQuickBool(this, manifest, nameof(ModifyCustomBeehouses));
            api.AddQuickLink("sources", manifest);
            api.AddQuickLink("visual", manifest);
            api.AddQuickLink("price", manifest);

            //sources
            api.AddPage(manifest, "sources", () => i18n.Get("config.sources.name"));
            api.AddQuickEnum<ProduceWhere>(this, manifest, nameof(UsePottedFlowers));
            api.AddQuickBool(this, manifest, nameof(UseForageFlowers));
            api.AddQuickBool(this, manifest, nameof(UseRandomFlower));
            api.AddQuickBool(this, manifest, nameof(UseGiantCrops));
            api.AddQuickBool(this, manifest, nameof(UseBushes));
            api.AddQuickBool(this, manifest, nameof(UseFruitTrees));
            api.AddQuickBool(this, manifest, nameof(UseAnyFruitTrees));
            api.AddQuickBool(this, manifest, nameof(AnythingHoney));

            //visual
            api.AddPage(manifest, "visual", () => i18n.Get("config.visual.name"));
            api.AddQuickBool(this, manifest, nameof(BeePaths));
            api.AddQuickBool(this, manifest, nameof(Particles));
            api.AddQuickInt(this, manifest, nameof(PathParticleCount), 0, 20);

            //price balancing
            api.AddPage(manifest, "price", () => i18n.Get("config.price.name"));
            api.AddQuickFloat(this, manifest, nameof(ValueMultiplier), .1f, 2f, .1f);
        }
        public Config()
        {
            ResetToDefault();
        }

        public bool UsingFlowerRules(GameLocation where)
            => UseFruitTrees || UseGiantCrops || UseForageFlowers || UseBushes || AnythingHoney || 
            Utils.GetProduceHere(where, UsePottedFlowers);

	}
}
