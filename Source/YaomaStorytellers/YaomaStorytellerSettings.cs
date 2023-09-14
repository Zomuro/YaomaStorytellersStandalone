using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace YaomaStorytellers
{
    public class YaomaStorytellerSettings : ModSettings
    {
        // Deathless Daji Settings
        public bool DajiRessurectMechsDisable = false;

        public bool DajiRetrieveWeaponsDisable = false;

        public float DajiCrimsonSeverityGain = 0.3f;

        public bool DajiBloodyPortrait = false;

        public bool DajiMurderSanity = false;

        public float DajiMurderSanitySevReduce = 0.01f;

        public bool DajiLifestealMelee = false;

        public float DajiLifestealMeleePercent = 0.25f;

        // Farseer Fan Settings

        public float FarseerFanGracePeriodFactor = 1f;

        public bool FarseerFanPredictionDetail = false;

        public bool FarseerFanPredictAlt = false;

        public bool FarseerFanPredictDefer = false;

        // Kaiyi the Karmic Settings

        public float KaiyiKarmicKarma = 0f;

        public float KaiyiKarmicKarmaMax = 500f; // hidden settings put here for convienence

        public float KaiyiKarmicKarmaMin = -500f; // hidden settings put here for convienence

        public float KaiyiKarmicBasePriceFactor = 1f;

        public bool KaiyiKarmicKarmaPointScaling = false;

        public float KaiyiKarmicKarmaPointScalingFactor = 1f;

        public int KaiyiKarmicTradeDays = 7;

        public int KaiyiKarmicMaxChoices = 10;

        public bool KaiyiKarmicRerollIncidents = false;

        public float KaiyiKarmicRerollBaseCost = 10f;

        public bool KaiyiKarmicSavePersist = false;

        // Jianghu Jin Settings

        public int JianghuJinTerraformDays = 15;

        public float JianghuJinDecayProbRooms = 0.5f;

        public int JianghuJinDecayRoomsInt = 3; // RoomDecaySetting.Augmented

        public float JianghuJinDecayProbTerrain = 0.05f;

        public int JianghuJinDecayTerrainInt = 2; // RoomDecaySetting.Adjacent

        public float JianghuJinCleanupRocksProb = 0.3f; // unuse for now

        public bool JianghuJinBiomeChange = false;

        public bool JianghuJinBiomeChangeUnlocked = false;

        public bool JianghuJinHillinessChange = false;

        public float JianghuJinConstructBoost = 1.25f;

        public float JianghuJinMiningBoost = 1.25f;

        public bool JianghuJinSavePersist = false;


        public override void ExposeData()
        {
            // Deathless Daji Settings
            Scribe_Values.Look(ref DajiRessurectMechsDisable, "DajiRessurectMechsDisable", false);
            Scribe_Values.Look(ref DajiRetrieveWeaponsDisable, "DajiRetrieveWeaponsDisable", false);
            Scribe_Values.Look(ref DajiCrimsonSeverityGain, "DajiCrimsonSeverityGain", 0.3f);
            Scribe_Values.Look(ref DajiBloodyPortrait, "DajiBloodyPortrait", false);
            Scribe_Values.Look(ref DajiMurderSanity, "DajiMurderSanity", false);
            Scribe_Values.Look(ref DajiMurderSanitySevReduce, "DajiMurderSanitySevReduce", 0.01f);
            Scribe_Values.Look(ref DajiLifestealMelee, "DajiLifestealMelee", false);
            Scribe_Values.Look(ref DajiLifestealMeleePercent, "DajiLifestealMeleePercent", 0.25f);

            // Farseer Fan Settings
            Scribe_Values.Look(ref FarseerFanGracePeriodFactor, "FarseerFanGracePeriodFactor", 1f);
            Scribe_Values.Look(ref FarseerFanPredictionDetail, "FarseerFanPredictionDetail", false);
            Scribe_Values.Look(ref FarseerFanPredictAlt, "FarseerFanPredictAlt", false);
            Scribe_Values.Look(ref FarseerFanPredictDefer, "FarseerFanPredictDefer", false);

            // Kaiyi the Karmic Settings
            Scribe_Values.Look(ref KaiyiKarmicKarma, "KaiyiKarmicKarma", 0f);
            Scribe_Values.Look(ref KaiyiKarmicKarmaMax, "KaiyiKarmicKarmaMax", 500f);
            Scribe_Values.Look(ref KaiyiKarmicKarmaMin, "KaiyiKarmicKarmaMin", -500f);
            Scribe_Values.Look(ref KaiyiKarmicBasePriceFactor, "KaiyiKarmicBasePriceFactor", 1f);
            Scribe_Values.Look(ref KaiyiKarmicKarmaPointScaling, "KaiyiKarmicKarmaPointScaling", false);
            Scribe_Values.Look(ref KaiyiKarmicKarmaPointScalingFactor, "KaiyiKarmicKarmaPointScalingFactor", 1f);
            Scribe_Values.Look(ref KaiyiKarmicTradeDays, "KaiyiKarmicTradeDays", 7);
            Scribe_Values.Look(ref KaiyiKarmicMaxChoices, "KaiyiKarmicMaxChoices", 10);
            Scribe_Values.Look(ref KaiyiKarmicRerollIncidents, "KaiyiKarmicRerollIncidents", false);
            Scribe_Values.Look(ref KaiyiKarmicRerollBaseCost, "KaiyiKarmicRerollBaseCost", 10f);
            Scribe_Values.Look(ref KaiyiKarmicSavePersist, "KaiyiKarmicSavePersist", false);

            // Kaiyi the Karmic Settings
            Scribe_Values.Look(ref JianghuJinTerraformDays, "JianghuJinTerraformDays", 15);
            Scribe_Values.Look(ref JianghuJinDecayProbRooms, "JianghuJinDecayProbRocks", 0.1f);
            Scribe_Values.Look(ref JianghuJinDecayRoomsInt, "JianghuJinDecayRockInt", 3);
            Scribe_Values.Look(ref JianghuJinDecayProbTerrain, "JianghuJinDecayProbTerrain", 0.05f);
            Scribe_Values.Look(ref JianghuJinDecayTerrainInt, "JianghuJinDecayTerrainInt", 2);
            Scribe_Values.Look(ref JianghuJinBiomeChange, "JianghuJinBiomeChange", false);
            Scribe_Values.Look(ref JianghuJinBiomeChangeUnlocked, "JianghuJinBiomeChangeUnlocked", false);
            Scribe_Values.Look(ref JianghuJinHillinessChange, "JianghuJinHillinessChange", false);
            Scribe_Values.Look(ref JianghuJinConstructBoost, "JianghuJinConstructBoost", 1.25f);
            Scribe_Values.Look(ref JianghuJinMiningBoost, "JianghuJinMiningBoost", 1.25f);
            Scribe_Values.Look(ref JianghuJinSavePersist, "JianghuJinSavePersist", false);

            base.ExposeData();
        }

        public Dictionary<int, RoomDecaySetting> JianghuJinRoomIntSetting = new Dictionary<int, RoomDecaySetting>()
        {
            {1, RoomDecaySetting.Absolute},
            {2, RoomDecaySetting.Adjacent},
            {3, RoomDecaySetting.Augmented},
        };

        public Dictionary<RoomDecaySetting, float> JianghuJinRoomSettingDefault = new Dictionary<RoomDecaySetting, float>()
        {
            {RoomDecaySetting.Absolute, 0.2f},
            {RoomDecaySetting.Adjacent, 0.2f},
            {RoomDecaySetting.Augmented, 0.3f},
        };

        public Dictionary<RoomDecaySetting, float> JianghuJinTerrainSettingDefault = new Dictionary<RoomDecaySetting, float>()
        {
            {RoomDecaySetting.Absolute, 0.05f},
            {RoomDecaySetting.Adjacent, 0.05f},
            {RoomDecaySetting.Augmented, 0.1f},
        };
    }

    public class YaomaStorytellerMod : Mod
    {
        YaomaStorytellerSettings settings;

        public YaomaStorytellerMod(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<YaomaStorytellerSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            tabsList.Clear();
            tabsList.Add(new TabRecord("YS_SettingsFarseerFan".Translate(), delegate ()
            {
                this.tab = Tab.FarseerFan;
            },  this.tab == Tab.FarseerFan));
            tabsList.Add(new TabRecord("YS_SettingsKaiyiKarmic".Translate(), delegate ()
            {
                this.tab = Tab.KaiyiKarmic;
            },  this.tab == Tab.KaiyiKarmic));
            tabsList.Add(new TabRecord("YS_SettingsDeathlessDaji".Translate(), delegate ()
            {
                this.tab = Tab.DeathlessDaji;
            },  this.tab == Tab.DeathlessDaji));
            tabsList.Add(new TabRecord("YS_SettingsJianghuJin".Translate(), delegate ()
            {
                this.tab = Tab.JianghuJin;
            }, this.tab == Tab.JianghuJin));
            Rect tabRect = new Rect(inRect);
            tabRect.yMin = 80;
            TabDrawer.DrawTabs<TabRecord>(tabRect, tabsList, 200);

            Rect leftThird = new Rect(tabRect);
            leftThird.width = inRect.width / 3;
            Rect otherTwoThird = new Rect(tabRect);
            otherTwoThird.xMin += tabRect.width/3;

            switch (this.tab)
            {
                case Tab.FarseerFan:
                    Widgets.DrawTextureFitted(otherTwoThird, StorytellerDefOf.FarseerFan_Yaoma.portraitLargeTex, 0.9f);
                    break;

                case Tab.KaiyiKarmic:
                    Widgets.DrawTextureFitted(otherTwoThird, StorytellerDefOf.KaiyiKarmic_Yaoma.portraitLargeTex, 0.9f);
                    break;

                case Tab.DeathlessDaji:
                    Widgets.DrawTextureFitted(otherTwoThird, StorytellerDefOf.DeathlessDaji_Yaoma.portraitLargeTex, 0.9f);
                    break;

                case Tab.JianghuJin:
                    Widgets.DrawTextureFitted(otherTwoThird, StorytellerDefOf.JianghuJin_Yaoma.portraitLargeTex, 0.9f);
                    break;

                default: break;
            }

            var listing = new Listing_Standard();
            listing.Begin(leftThird);
            listing.Gap(16f);

            switch (tab)
            {
                case Tab.FarseerFan:
                    FarseerFanSettings(ref listing);
                    
                    break;

                case Tab.KaiyiKarmic:
                    KaiyiKarmicSettings(ref listing);
                    break;

                case Tab.DeathlessDaji:
                    DeathlessDajiSettings(ref listing);
                    break;
                case Tab.JianghuJin:
                    JianghuJinSettings(ref listing);
                    break;
                default: break;
            }

            // Reset to default
            listing.Gap(8f);
            if(listing.ButtonText("Reset to global default"))
            {
                DeathlessDajiDefault();
                FarseerFanDefault();
                KaiyiKarmicDefault();
                JianghuJinDefault();
            }
            listing.End();
            
            base.DoSettingsWindowContents(inRect);
        }

        public void DeathlessDajiDefault()
        {
            settings.DajiRessurectMechsDisable = false;
            settings.DajiRetrieveWeaponsDisable = false;
            settings.DajiCrimsonSeverityGain = 0.3f;
            settings.DajiBloodyPortrait = false;
            StorytellerDefOf.DeathlessDaji_Yaoma.ResolveReferences();
            settings.DajiMurderSanity = false;
            settings.DajiMurderSanitySevReduce = 0.01f;
            settings.DajiLifestealMelee = false;
            settings.DajiLifestealMeleePercent = 0.25f;
        }

        public void FarseerFanDefault()
        {
            settings.FarseerFanGracePeriodFactor = 1f;
            settings.FarseerFanPredictionDetail = false;
            settings.FarseerFanPredictAlt = false;
            settings.FarseerFanPredictDefer = false;
        }

        public void KaiyiKarmicDefault()
        {
            settings.KaiyiKarmicKarma = 0f;
            settings.KaiyiKarmicBasePriceFactor = 1f;

            settings.KaiyiKarmicTradeDays = 7;
            settings.KaiyiKarmicMaxChoices = 15;

            settings.KaiyiKarmicKarmaPointScaling = false;
            settings.KaiyiKarmicKarmaPointScalingFactor = 1f;
            settings.KaiyiKarmicRerollIncidents = false;
            settings.KaiyiKarmicRerollBaseCost = 10f;

            settings.KaiyiKarmicSavePersist = false;
        }

        public void JianghuJinDefault()
        {
            settings.JianghuJinTerraformDays = 15;
            settings.JianghuJinDecayRoomsInt = 3; // RoomDecaySetting.Augmented
            settings.JianghuJinDecayProbRooms = settings.JianghuJinRoomSettingDefault[settings.JianghuJinRoomIntSetting[settings.JianghuJinDecayRoomsInt]];
            settings.JianghuJinDecayTerrainInt = 2; // RoomDecaySetting.Adjacent
            settings.JianghuJinDecayProbTerrain = settings.JianghuJinTerrainSettingDefault[settings.JianghuJinRoomIntSetting[settings.JianghuJinDecayTerrainInt]];
            settings.JianghuJinBiomeChange = false;
            settings.JianghuJinBiomeChangeUnlocked = false;
            settings.JianghuJinHillinessChange = false;
            settings.JianghuJinConstructBoost = 1.25f;
            settings.JianghuJinMiningBoost = 1.25f;

            settings.JianghuJinSavePersist = false;
        }

        public void FarseerFanSettings(ref Listing_Standard listing)
        {
            // Farseer Fan
            Text.Font = GameFont.Medium;
            listing.Label("Farseer Fan");
            Text.Font = GameFont.Small;
            listing.GapLine();
            float minDays = FarseerFan_RandomMain != null ? FarseerFan_RandomMain.minDaysPassed : 0.95f;
            string days = GenDate.ToStringTicksToDays((int)(minDays * 60000 * settings.FarseerFanGracePeriodFactor), "F2");
            listing.Label("YS_SettingsFanGrace".Translate(days), -1,
                "YS_SettingsFanGraceTooltip".Translate());
            settings.FarseerFanGracePeriodFactor = listing.Slider((float)settings.FarseerFanGracePeriodFactor, 0f, 5f);
            listing.CheckboxLabeled("YS_SettingsFanPredictDetail".Translate(settings.FarseerFanPredictionDetail.ToString()),
                ref settings.FarseerFanPredictionDetail, "YS_SettingsFanPredictDetailTooltip".Translate());

            listing.GapLine();
            listing.CheckboxLabeled("YS_SettingsFanPredictAlt".Translate(settings.FarseerFanPredictAlt.ToString()),
                ref settings.FarseerFanPredictAlt, "YS_SettingsFanPredictAltTooltip".Translate());
            listing.CheckboxLabeled("YS_SettingsFanPredictDefer".Translate(settings.FarseerFanPredictDefer.ToString()),
                ref settings.FarseerFanPredictDefer, "YS_SettingsFanPredictDeferTooltip".Translate());

            listing.Gap(8f);
            if (listing.ButtonText("Reset to default"))
            {
                FarseerFanDefault();
            }
        }

        public void KaiyiKarmicSettings(ref Listing_Standard listing)
        {
            // Kaiyi the Karmic
            Text.Font = GameFont.Medium;
            listing.Label("Kaiyi the Karmic");
            Text.Font = GameFont.Small;
            listing.GapLine();
            listing.Label("YS_SettingsKaiyiStart".Translate(settings.KaiyiKarmicKarma.ToString("F2")), -1,
                "YS_SettingsKaiyiStartTooltip".Translate());
            settings.KaiyiKarmicKarma = listing.Slider((float)settings.KaiyiKarmicKarma, settings.KaiyiKarmicKarmaMin, settings.KaiyiKarmicKarmaMax);
            listing.Label("YS_SettingsKaiyiCostFactor".Translate(settings.KaiyiKarmicBasePriceFactor.ToString("F2")), -1,
                "YS_SettingsKaiyiCostFactorTooltip".Translate());
            settings.KaiyiKarmicBasePriceFactor = listing.Slider((float)settings.KaiyiKarmicBasePriceFactor, 0f, 5f);

            listing.Label("YS_SettingsKaiyiTradeDays".Translate(settings.KaiyiKarmicTradeDays.ToString("F0")), -1,
                "YS_SettingsKaiyiTradeDaysTooltip".Translate());
            settings.KaiyiKarmicTradeDays = (int) listing.Slider(settings.KaiyiKarmicTradeDays, 0f, 14f);

            listing.Label("YS_SettingsKaiyiMaxSelectable".Translate(settings.KaiyiKarmicMaxChoices.ToString("F0")), -1,
                "YS_SettingsKaiyiMaxSelectableTooltip".Translate());
            settings.KaiyiKarmicMaxChoices = (int)listing.Slider(settings.KaiyiKarmicMaxChoices, 10f, 30f);

            listing.CheckboxLabeled("YS_SettingsKaiyiSavePersist".Translate(settings.KaiyiKarmicSavePersist.ToString()),
                ref settings.KaiyiKarmicSavePersist, "YS_SettingsKaiyiSavePersistTooltip".Translate());

            listing.GapLine();
            listing.CheckboxLabeled("YS_SettingsKaiyiPointScaling".Translate(settings.KaiyiKarmicKarmaPointScaling.ToString()),
                ref settings.KaiyiKarmicKarmaPointScaling, "YS_SettingsKaiyiPointScalingTooltip".Translate());
            if (settings.KaiyiKarmicKarmaPointScaling)
            {
                listing.Label("YS_SettingsKaiyiPointFactor".Translate(settings.KaiyiKarmicKarmaPointScalingFactor.ToString("F2")), -1,
                "YS_SettingsKaiyiPointFactorTooltip".Translate());
                settings.KaiyiKarmicKarmaPointScalingFactor = listing.Slider((float)settings.KaiyiKarmicKarmaPointScalingFactor, 0f, 5f);
            }

            listing.CheckboxLabeled("YS_SettingsKaiyiReroll".Translate(settings.KaiyiKarmicRerollIncidents.ToString()),
                ref settings.KaiyiKarmicRerollIncidents, "YS_SettingsKaiyiRerollTooltip".Translate());
            if (settings.KaiyiKarmicRerollIncidents)
            {
                listing.Label("YS_SettingsKaiyiRerollBaseCost".Translate(settings.KaiyiKarmicRerollBaseCost.ToString("F2")), -1,
                    "YS_SettingsKaiyiRerollBaseCostTooltip".Translate());
                settings.KaiyiKarmicRerollBaseCost = listing.Slider((float)settings.KaiyiKarmicRerollBaseCost, 0f, 25f);
            }

            listing.Gap(8f);
            if (listing.ButtonText("Reset to default"))
            {
                KaiyiKarmicDefault();
            }
        }

        public void DeathlessDajiSettings(ref Listing_Standard listing)
        {
            // Deathless Daji
            Text.Font = GameFont.Medium;
            listing.Label("Deathless Daji");
            Text.Font = GameFont.Small;
            listing.GapLine();
            listing.CheckboxLabeled("YS_SettingsDajiResurrectMechs".Translate(settings.DajiRessurectMechsDisable.ToString()),
                ref settings.DajiRessurectMechsDisable, "YS_SettingsDajiResurrectMechsTooltip".Translate());
            listing.CheckboxLabeled("YS_SettingsDajiResurrectWeapons".Translate(settings.DajiRetrieveWeaponsDisable.ToString()),
                ref settings.DajiRetrieveWeaponsDisable, "YS_SettingsDajiResurrectWeaponsTooltip".Translate());
            listing.Label("YS_SettingsDajiCrimsonSeverityGain".Translate(settings.DajiCrimsonSeverityGain.ToString("F2")), -1,
                "YS_SettingsDajiCrimsonSeverityGainTooltip".Translate());
            settings.DajiCrimsonSeverityGain = listing.Slider((float)settings.DajiCrimsonSeverityGain, 0.1f, 1f);

            bool orgToggle = settings.DajiBloodyPortrait;
            listing.CheckboxLabeled("YS_SettingsDajiBloodyPortrait".Translate(settings.DajiBloodyPortrait.ToString()),
                ref settings.DajiBloodyPortrait, "YS_SettingsDajiBloodyPortraitTooltip".Translate());

            listing.GapLine();
            listing.CheckboxLabeled("YS_SettingsDajiMurderSanity".Translate(settings.DajiMurderSanity.ToString()),
                ref settings.DajiMurderSanity, "YS_SettingsDajiMurderSanityTooltip".Translate());
            if (settings.DajiMurderSanity)
            {
                listing.Label("YS_SettingsDajiMurderSanitySev".Translate(settings.DajiMurderSanitySevReduce.ToString("F3")), -1,
                "YS_SettingsDajiMurderSanitySevTooltip".Translate());
                settings.DajiMurderSanitySevReduce = listing.Slider((float)settings.DajiMurderSanitySevReduce, 0f, 0.05f);
            }
            listing.CheckboxLabeled("YS_SettingsDajiLifestealMelee".Translate(settings.DajiLifestealMelee.ToString()),
                ref settings.DajiLifestealMelee, "YS_SettingsDajiLifestealMeleeTooltip".Translate());
            if (settings.DajiLifestealMelee)
            {
                listing.Label("YS_SettingsDajiLifestealMeleePercent".Translate(settings.DajiLifestealMeleePercent.ToString("P0")), -1,
                "YS_SettingsDajiLifestealMeleePercentTooltip".Translate());
                settings.DajiLifestealMeleePercent = listing.Slider((float)settings.DajiLifestealMeleePercent, 0f, 1f);
            }

            if (orgToggle != settings.DajiBloodyPortrait || !initalizedDaji)
            {
                StorytellerDefOf.DeathlessDaji_Yaoma.ResolveReferences();
                initalizedDaji = true;
            }

            listing.Gap(8f);
            if (listing.ButtonText("Reset to default"))
            {
                DeathlessDajiDefault();
            }
        }

        public void JianghuJinSettings(ref Listing_Standard listing)
        {
            Text.Font = GameFont.Medium;
            listing.Label("Jianghu Jin");
            Text.Font = GameFont.Small;
            listing.GapLine();

            listing.Label("YS_SettingsJianghuJinTerraformDays".Translate(settings.JianghuJinTerraformDays), -1,
                "YS_SettingsJianghuJinTerraformDaysTooltip".Translate());
            settings.JianghuJinTerraformDays = (int)listing.Slider(settings.JianghuJinTerraformDays, 3f, 30f);
            listing.Label("YS_SettingsJianghuJinDecayProbRoom".Translate(settings.JianghuJinDecayProbRooms.ToStringPercent()), -1,
                "YS_SettingsJianghuJinDecayProbRoomTooltip".Translate());
            settings.JianghuJinDecayProbRooms = listing.Slider((float)settings.JianghuJinDecayProbRooms, 0f, 1f);
            if (listing.ButtonTextLabeledPct("YS_SettingsJianghuJinDecayProbRoomSetting".Translate(),
                settings.JianghuJinRoomIntSetting[settings.JianghuJinDecayRoomsInt].ToString(), 0.6f, TextAnchor.MiddleLeft, 
                null, "YS_SettingsJianghuJinDecayProbRoomSettingTooltip".Translate()))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                foreach(var setting in settings.JianghuJinRoomIntSetting)
                {
                    options.Add(new FloatMenuOption(setting.Value.ToString(), delegate ()
                    {
                        settings.JianghuJinDecayRoomsInt = setting.Key;
                        settings.JianghuJinDecayProbRooms = settings.JianghuJinRoomSettingDefault[setting.Value];
                    }, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0));
                }
                Find.WindowStack.Add(new FloatMenu(options));
            }
            listing.Label("YS_SettingsJianghuJinDecayProbTerrain".Translate(settings.JianghuJinDecayProbTerrain.ToStringPercent()), -1,
                "YS_SettingsJianghuJinDecayProbTerrainTooltip".Translate());
            settings.JianghuJinDecayProbTerrain = listing.Slider((float)settings.JianghuJinDecayProbTerrain, 0f, 1f);
            if (listing.ButtonTextLabeledPct("YS_SettingsJianghuJinDecayProbTerrainSetting".Translate(),
                settings.JianghuJinRoomIntSetting[settings.JianghuJinDecayTerrainInt].ToString(), 0.6f, TextAnchor.MiddleLeft,
                null, "YS_SettingsJianghuJinDecayProbTerrainSettingTooltip".Translate()))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                foreach (var setting in settings.JianghuJinRoomIntSetting)
                {
                    options.Add(new FloatMenuOption(setting.Value.ToString(), delegate ()
                    {
                        settings.JianghuJinDecayTerrainInt = setting.Key;
                        settings.JianghuJinDecayProbTerrain = settings.JianghuJinTerrainSettingDefault[setting.Value];
                    }, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0));
                }
                Find.WindowStack.Add(new FloatMenu(options));
            }
            listing.Label("YS_SettingsJianghuJinConstructBoost".Translate(settings.JianghuJinConstructBoost.ToString("F2")), -1,
                "YS_SettingsJianghuJinConstructBoostTooltip".Translate());
            settings.JianghuJinConstructBoost = listing.Slider((float)settings.JianghuJinConstructBoost, 0.5f, 2f);
            listing.Label("YS_SettingsJianghuJinMiningBoost".Translate(settings.JianghuJinMiningBoost.ToString("F2")), -1,
                "YS_SettingsJianghuJinMiningBoostTooltip".Translate());
            settings.JianghuJinMiningBoost = listing.Slider((float)settings.JianghuJinMiningBoost, 0.5f, 2f);

            /*listing.CheckboxLabeled("YS_SettingsJianghuJinSavePersist".Translate(settings.JianghuJinSavePersist.ToString()),
                ref settings.JianghuJinSavePersist, "YS_SettingsJianghuJinSavePersistTooltip".Translate());*/

            listing.GapLine();
            listing.CheckboxLabeled("YS_SettingsJianghuBiome".Translate(settings.JianghuJinBiomeChange.ToString()),
                ref settings.JianghuJinBiomeChange, "YS_SettingsJianghuBiomeTooltip".Translate());
            if (settings.JianghuJinBiomeChange)
            {
                listing.CheckboxLabeled("YS_SettingsJianghuBiomeUnlock".Translate(settings.JianghuJinBiomeChangeUnlocked.ToString()),
                ref settings.JianghuJinBiomeChangeUnlocked, "YS_SettingsJianghuBiomeUnlockTooltip".Translate());
            }
            listing.CheckboxLabeled("YS_SettingsJianghuHilliness".Translate(settings.JianghuJinHillinessChange.ToString()),
                ref settings.JianghuJinHillinessChange, "YS_SettingsJianghuHillinessTooltip".Translate());

            listing.Gap(8f);
            if (listing.ButtonText("Reset to default"))
            {
                JianghuJinDefault();
            }
        }

        

        public override string SettingsCategory()
        {
            return "YaomaStorytellersSettings".Translate();
        }

        // get StorytellerComp_RandomMain of Farseer Fan
        public StorytellerCompProperties_RandomMain FarseerFan_RandomMain
        {
            get
            {
                if(compPropRandomMain == null)
                {
                    compPropRandomMain = StorytellerDefOf.FarseerFan_Yaoma.comps.FirstOrDefault(x => x.GetType() == typeof(StorytellerCompProperties_RandomMain)) as StorytellerCompProperties_RandomMain;
                }
                return compPropRandomMain;
            }
        }

        private static List<TabRecord> tabsList = new List<TabRecord>();

        private Tab tab;

        private StorytellerCompProperties_RandomMain compPropRandomMain = null;

        private enum Tab
        {
            FarseerFan,
            KaiyiKarmic,
            DeathlessDaji,
            JianghuJin
        }

        private bool initalizedDaji = false;

        
    }
}
