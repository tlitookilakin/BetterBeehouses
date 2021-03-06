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
        public bool PatchAutomate { get; set; } = true;
        public bool PatchPFM { get; set; } = true;
        public int CapFactor { get; set; } = 700;
        private float capCurve = 0f;
        public float CapCurve
        {
            get { return capCurve; }
            set { capCurve = Math.Clamp(value, 0f, 1f); }
        }
        private float bearBoost = 1f;
        public float BearBoost
        {
            get { return bearBoost; }
            set { bearBoost = Math.Clamp(value, 1f, 3f); }
        }

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
            PatchAutomate = true;
            PatchPFM = true;
            CapFactor = 700;
            CapCurve = 0f;
            BearBoost = 1f;
        }

        public void ApplyConfig()
        {
            ModEntry.helper.WriteConfig(this);
            ModEntry.helper.GameContent.InvalidateCache("Mods/aedenthorn.ParticleEffects/dict");
            integration.AutomatePatch.Setup();
            integration.PFMPatch.Setup();
            integration.PFMAutomatePatch.Setup();
        }

        public void RegisterModConfigMenu(IManifest manifest)
        {
            if (!ModEntry.helper.ModRegistry.IsLoaded("spacechase0.GenericModConfigMenu"))
                return;

            var api = ModEntry.helper.ModRegistry.GetApi<integration.IGMCMAPI>("spacechase0.GenericModConfigMenu");

            api.Register(manifest, ResetToDefault, ApplyConfig);

            api.AddNumberOption(manifest,
                () => DaysToProduce,
                (n) => DaysToProduce = n,
                () => i18n.Get("config.daysToProduce.name"),
                () => i18n.Get("config.daysToProduce.desc"),
                1, 7
            );
            api.AddNumberOption(manifest,
                () => FlowerRange,
                (n) => FlowerRange = n,
                () => i18n.Get("config.flowerRange.name"),
                () => i18n.Get("config.flowerRange.desc"),
                1, 14
            );
            api.AddTextOption(manifest,
                () => UsableIn.ToString(),
                (s) => UsableIn = (UsableOptions)Enum.Parse(typeof(UsableOptions), s),
                () => i18n.Get("config.usableIn.name"),
                () => i18n.Get("config.usableIn.desc"),
                Enum.GetNames(typeof(UsableOptions)),
                (s) => TranslatedOption("usableOptions",s)
            ); 
            api.AddTextOption(manifest,
                () => ProduceInWinter.ToString(),
                (s) => ProduceInWinter = (ProduceWhere)Enum.Parse(typeof(ProduceWhere), s),
                () => i18n.Get("config.produceInWinter.name"),
                () => i18n.Get("config.produceInWinter.desc"),
                Enum.GetNames(typeof(ProduceWhere)),
                (s) => TranslatedOption("produceWhere", s)
            );
            api.AddTextOption(manifest,
                () => UsePottedFlowers.ToString(),
                (s) => UsePottedFlowers = (ProduceWhere)Enum.Parse(typeof(ProduceWhere), s),
                () => i18n.Get("config.usePottedFlowers.name"),
                () => i18n.Get("config.usePottedFlowers.desc"),
                Enum.GetNames(typeof(ProduceWhere)),
                (s) => TranslatedOption("produceWhere", s)
            );
            api.AddBoolOption(manifest,
                () => UseForageFlowers,
                (b) => UseForageFlowers = b,
                () => i18n.Get("config.useForage.name"),
                () => i18n.Get("config.useForage.desc")
            );
            api.AddBoolOption(manifest,
                () => UseQuality,
                (b) => UseQuality = b,
                () => i18n.Get("config.useQuality.name"),
                () => i18n.Get("config.useQuality.desc")
            );
            api.AddNumberOption(manifest,
                () => BearBoost,
                (n) => BearBoost = n,
                () => i18n.Get("config.bearBoost.name"),
                () => i18n.Get("config.bearBoost.desc"),
                1f, 3f, .05f
            );
            api.AddPageLink(manifest, "price", () => i18n.Get("config.price.name"), () => i18n.Get("config.price.desc"));
            api.AddPageLink(manifest, "integration", () => i18n.Get("config.integration.name"), () => i18n.Get("config.integration.desc"));

            //integration
            api.AddPage(manifest, "integration", () => i18n.Get("config.integration.name"));
            if (ModEntry.helper.ModRegistry.IsLoaded("aedenthorn.ParticleEffects"))
                api.AddBoolOption(manifest,
                    () => Particles,
                    (b) => Particles = b,
                    () => i18n.Get("config.particles.name"),
                    () => i18n.Get("config.particles.desc")
                );
            api.AddBoolOption(manifest,
                () => PatchAutomate,
                (b) => PatchAutomate = b,
                () => i18n.Get("config.patchAutomate.name"),
                () => i18n.Get("config.patchAutomate.desc")
            );
            api.AddBoolOption(manifest,
                () => PatchPFM,
                (b) => PatchPFM = b,
                () => i18n.Get("config.patchPFM.name"),
                () => i18n.Get("config.patchPFM.desc")
            );

            //price balancing
            api.AddPage(manifest, "price", () => i18n.Get("config.price.name"));
            api.AddNumberOption(manifest,
                () => ValueMultiplier,
                (n) => ValueMultiplier = n,
                () => i18n.Get("config.valueMultiplier.name"),
                () => i18n.Get("config.valueMultiplier.desc"),
                .1f, 2f, .1f
            );
            api.AddNumberOption(manifest,
                () => CapFactor,
                (v) => CapFactor = v,
                () => i18n.Get("config.capFactor.name"),
                () => i18n.Get("config.capFactor.desc"),
                100
            );
            api.AddNumberOption(manifest,
                () => CapCurve,
                (v) => CapCurve = v,
                () => i18n.Get("config.capCurve.name"),
                () => i18n.Get("config.capCurve.desc"),
                0f, 1f, .01f
            );
        }
        public string TranslatedOption(string enumName, string value)
            => i18n.Get($"config.{enumName}.{value}");
        public Config()
        {
            ResetToDefault();
        }
    }
}
