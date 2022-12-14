using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;

namespace YaomaStorytellers
{
    public class YaomaStorytellerSettings : ModSettings
    {
        public bool DajiRessurectMechsDisable = false;

        public bool DajiRetrieveWeaponsDisable = false;

        public float DajiCrimsonSeverityGain = 0.3f;

        public float FarseerFanGracePeriodFactor = 1f;

        public float FarseerFanMaxPeriodFactor = 5f;

        public bool FarseerFanPredictionDetail = false;

        public float KaiyiKarmicKarma = 0f;

        public float KaiyiKarmicBasePriceFactor = 1f;

        public float KaiyiKarmicScalingPositive = 0.25f;

        public float KaiyiKarmicScalingNegative = 1f;

        public float KaiyiKarmicThresholdEndgamePositive = 500f;

        public float KaiyiKarmicThresholdEndgameNegative = -500f;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref DajiRessurectMechsDisable, "DajiRessurectMechsDisable", false);
            Scribe_Values.Look(ref DajiRetrieveWeaponsDisable, "DajiRetrieveWeaponsDisable", false);
            Scribe_Values.Look(ref DajiCrimsonSeverityGain, "DajiCrimsonSeverityGain", 0.3f);
            Scribe_Values.Look(ref FarseerFanGracePeriodFactor, "FarseerFanGracePeriodFactor", 1f);
            Scribe_Values.Look(ref FarseerFanMaxPeriodFactor, "FarseerFanMaxPeriodFactor", 4f);
            Scribe_Values.Look(ref FarseerFanPredictionDetail, "FarseerFanPredictionDetail", false);
            Scribe_Values.Look(ref KaiyiKarmicKarma, "KaiyiKarmicKarma", 0f);
            Scribe_Values.Look(ref KaiyiKarmicBasePriceFactor, "KaiyiKarmicBasePriceFactor", 1f);
            Scribe_Values.Look(ref KaiyiKarmicScalingPositive, "KaiyiKarmicPointScalingPositive", 0.25f);
            Scribe_Values.Look(ref KaiyiKarmicScalingNegative, "KaiyiKarmicPointScalingNegative", 1f);
            Scribe_Values.Look(ref KaiyiKarmicThresholdEndgamePositive, "KaiyiKarmicThresholdEndgamePositive", 500f);
            Scribe_Values.Look(ref KaiyiKarmicThresholdEndgameNegative, "KaiyiKarmicThresholdEndgameNegative", -500f);
            base.ExposeData();
        }
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
                    Widgets.DrawTextureFitted(otherTwoThird, ContentFinder<Texture2D>.Get("UI/Storytellers/FarseerFan", true), 0.9f);
                    break;

                case Tab.KaiyiKarmic:
                    Widgets.DrawTextureFitted(otherTwoThird, ContentFinder<Texture2D>.Get("UI/Storytellers/KaiyiKarmic", true), 0.9f);
                    //KaiyiKarmicSettings(ref listing);
                    break;

                case Tab.DeathlessDaji:
                    //DeathlessDajiSettings(ref listing);
                    break;

                default: break;
            }

            var listing = new Listing_Standard();
            listing.Begin(leftThird);
            listing.Gap(16f);

            switch (this.tab)
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

                default: break;
            }

            // Reset to default
            listing.Gap(16f);
            if(listing.ButtonText("Reset to global default"))
            {
                settings.DajiRessurectMechsDisable = false;
                settings.DajiRetrieveWeaponsDisable = false;
                settings.DajiCrimsonSeverityGain = 0.3f;
                settings.FarseerFanGracePeriodFactor = 1f;
                settings.FarseerFanMaxPeriodFactor = 4f;
                settings.FarseerFanPredictionDetail = false;
                settings.KaiyiKarmicKarma = 0f;
                settings.KaiyiKarmicBasePriceFactor = 1f;
                settings.KaiyiKarmicScalingPositive = 0.25f;
                settings.KaiyiKarmicScalingNegative = 1f;
                settings.KaiyiKarmicThresholdEndgamePositive = 500f;
                settings.KaiyiKarmicThresholdEndgameNegative = -500f;
            }
            listing.End();
            
            base.DoSettingsWindowContents(inRect);
        }

        public void FarseerFanSettings(ref Listing_Standard listing)
        {
            // Farseer Fan
            Text.Font = GameFont.Medium;
            listing.Label("Farseer Fan");
            Text.Font = GameFont.Small;
            listing.GapLine();
            string days = GenDate.ToStringTicksToDays((int)(0.95f * 60000 * settings.FarseerFanGracePeriodFactor), "F2");
            listing.Label("YS_SettingsFanGrace".Translate(days), -1,
                "YS_SettingsFanGraceTooltip".Translate());
            settings.FarseerFanGracePeriodFactor = listing.Slider((float)settings.FarseerFanGracePeriodFactor, 0f, 5f);

            string days2 = GenDate.ToStringTicksToDays((int)(1.15f * 60000 * settings.FarseerFanMaxPeriodFactor), "F2");
            listing.Label("YS_SettingsFanPredict".Translate(days2), -1,
                "YS_SettingsFanPredictTooltip".Translate());
            settings.FarseerFanMaxPeriodFactor = listing.Slider((float)settings.FarseerFanMaxPeriodFactor, 2f, 10f);
            listing.CheckboxLabeled("YS_SettingsFanPredictDetail".Translate(settings.FarseerFanPredictionDetail.ToString()),
                ref settings.FarseerFanPredictionDetail, "YS_SettingsFanPredictDetailTooltip".Translate());

            GenDate.ToStringTicksToDays(10);

            listing.Gap(16f);
            if (listing.ButtonText("Reset to default"))
            {
                settings.FarseerFanGracePeriodFactor = 1f;
                settings.FarseerFanMaxPeriodFactor = 4f;
                settings.FarseerFanPredictionDetail = false;
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
            settings.KaiyiKarmicKarma = listing.Slider((float)settings.KaiyiKarmicKarma, -200f, 200f);
            listing.Label("YS_SettingsKaiyiCostFactor".Translate(settings.KaiyiKarmicBasePriceFactor.ToString("F2")), -1,
                "YS_SettingsKaiyiCostFactorTooltip".Translate());
            settings.KaiyiKarmicBasePriceFactor = listing.Slider((float)settings.KaiyiKarmicBasePriceFactor, 0f, 5f);
            listing.Label("YS_SettingsKaiyiPointFactorPos".Translate(settings.KaiyiKarmicScalingPositive.ToString("F2")), -1,
                "YS_SettingsKaiyiPointFactorPosTooltip".Translate());
            settings.KaiyiKarmicScalingPositive = listing.Slider((float)settings.KaiyiKarmicScalingPositive, 0f, 2f);
            listing.Label("YS_SettingsKaiyiPointFactorNeg".Translate(settings.KaiyiKarmicScalingNegative.ToString("F2")), -1,
                "YS_SettingsKaiyiPointFactorNegTooltip".Translate());
            settings.KaiyiKarmicScalingNegative = listing.Slider((float)settings.KaiyiKarmicScalingNegative, 0f, 2f);

            listing.Gap(16f);
            if (listing.ButtonText("Reset to default"))
            {
                settings.KaiyiKarmicKarma = 0f;
                settings.KaiyiKarmicBasePriceFactor = 1f;
                settings.KaiyiKarmicScalingPositive = 0.25f;
                settings.KaiyiKarmicScalingNegative = 1f;
                settings.KaiyiKarmicThresholdEndgamePositive = 500f;
                settings.KaiyiKarmicThresholdEndgameNegative = -500f;
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

            listing.Gap(16f);
            if (listing.ButtonText("Reset to default"))
            {
                settings.DajiRessurectMechsDisable = false;
                settings.DajiRetrieveWeaponsDisable = false;
                settings.DajiCrimsonSeverityGain = 0.3f;
            }
        }

        public override string SettingsCategory()
        {
            return "YaomaStorytellersSettings".Translate();
        }

        private static List<TabRecord> tabsList = new List<TabRecord>();

        private Tab tab;

        private enum Tab
        {
            FarseerFan,
            KaiyiKarmic,
            DeathlessDaji
        }
    }
}
