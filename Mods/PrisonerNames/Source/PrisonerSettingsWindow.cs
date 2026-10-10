using System;
using IrisMenus;
using UnityEngine;
using Verse;

namespace Odf.PrisonerNames
{
    static class PrisonerSettingsWindow
    {
        static NamePolicy draft;
        static Game bound;
        static NamePolicy appliedPolicy;
        static string scoreBuffer = "0";
        static string resistanceBuffer = "0";
        static string custodyBuffer = "0";
        static string breakBuffer = "0";
        static float threshold;
        static string thresholdBuffer = "0";
        static string gradeBuffer = string.Empty;
        static string raceBuffer = string.Empty;
        static string prefixBuffer = string.Empty;
        static string preview = string.Empty;
        static string previewError = string.Empty;
        static int removeRace = -1;
        static int removeGrade = -1;

        internal static void Register(Mod owner)
        {
            MenuRegistry.RegisterListing(owner, "names", () => "OPN_PolicyPage".Translate(), Draw);
        }

        static void Draw(Listing_Standard listing)
        {
            GameComponent_PrisonerNames runtime = GameComponent_PrisonerNames.Current;
            listing.Label("OPN_PolicyIntro".Translate());
            if (runtime == null)
            {
                draft = null;
                listing.Label("OPN_NoGame".Translate());
                return;
            }
            EnsureDraft(runtime);
            listing.Label("OPN_SourceMode".Translate() + ": " + (runtime.CustomMode ? "OPN_CustomSource".Translate() : "OPN_ConfigSource".Translate()));
            if (!runtime.CustomMode && listing.ButtonText("OPN_UseCustom".Translate())) runtime.UseCustom(runtime.ActiveSource);
            if (runtime.CustomMode && listing.ButtonText("OPN_UsePattern".Translate())) Save(runtime, true);
            if (listing.ButtonText("OPN_NameStyle".Translate() + ": " + ("OPN_Style_" + NamePolicy.StyleName(draft.Style)).Translate()))
            {
                draft.Style = (NameStyle)(((int)draft.Style + 1) % 5);
                RefreshPreview();
            }
            float neutral = (float)draft.NeutralScore;
            MenuControls.Number(listing, "OPN_NeutralScore".Translate(), ref neutral, ref scoreBuffer, -100000f, 100000f);
            if (draft.NeutralScore != neutral)
            {
                draft.NeutralScore = neutral;
                RefreshPreview();
            }
            float resistance = (float)draft.ResistancePerPoint;
            MenuControls.Number(listing, "OPN_ResistanceScore".Translate(), ref resistance, ref resistanceBuffer, -100000f, 100000f);
            draft.ResistancePerPoint = resistance;
            float custody = (float)draft.CustodyDayScore;
            MenuControls.Number(listing, "OPN_CustodyScore".Translate(), ref custody, ref custodyBuffer, -100000f, 100000f);
            draft.CustodyDayScore = custody;
            float prisonBreak = (float)draft.PrisonBreakScore;
            MenuControls.Number(listing, "OPN_PrisonBreakScore".Translate(), ref prisonBreak, ref breakBuffer, -100000f, 100000f);
            draft.PrisonBreakScore = prisonBreak;
            listing.Label("OPN_Placeholders".Translate());
            DrawText(listing, "OPN_Pattern".Translate(), NamingLanguage.Chinese ? draft.ChinesePattern : draft.Pattern, value =>
            {
                if (NamingLanguage.Chinese) draft.ChinesePattern = value;
                else draft.Pattern = value;
            });
            DrawRaces(listing);
            DrawGrades(listing);
            if (listing.ButtonText("OPN_SavePolicy".Translate())) Save(runtime);
            for (int i = 0; i < NamePresets.All.Count; i++)
            {
                NamePresets.Preset preset = NamePresets.All[i];
                if (!listing.ButtonText(("OPN_Preset_" + preset.Id).Translate())) continue;
                draft.Pattern = preset.Pattern;
                draft.ChinesePattern = preset.ChinesePattern;
                draft.Style = preset.Style;
                Save(runtime, true);
            }
            if (!string.IsNullOrEmpty(previewError)) listing.Label("OPN_PolicyPreviewFailed".Translate() + ": " + previewError);
            else listing.Label("OPN_PolicyPreview".Translate() + ": " + preview);
        }

        static void EnsureDraft(GameComponent_PrisonerNames runtime)
        {
            if (draft != null && ReferenceEquals(bound, Current.Game) && ReferenceEquals(appliedPolicy, runtime.Policy)) return;
            draft = runtime.Policy.Clone();
            bound = Current.Game;
            appliedPolicy = runtime.Policy;
            scoreBuffer = NamePolicy.FormatNumber(draft.NeutralScore);
            resistanceBuffer = NamePolicy.FormatNumber(draft.ResistancePerPoint);
            custodyBuffer = NamePolicy.FormatNumber(draft.CustodyDayScore);
            breakBuffer = NamePolicy.FormatNumber(draft.PrisonBreakScore);
            threshold = 0f;
            thresholdBuffer = "0";
            RefreshPreview();
        }

        static void Save(GameComponent_PrisonerNames runtime, bool replaceNamingRule = false)
        {
            if (replaceNamingRule && runtime.CustomMode)
            {
                NamePolicy candidate = draft.Clone();
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("OPN_UsePatternConfirm".Translate(),
                    () => SavePolicy(runtime, candidate, true)));
                return;
            }
            SavePolicy(runtime, draft, replaceNamingRule);
        }

        static void SavePolicy(GameComponent_PrisonerNames runtime, NamePolicy candidate, bool replaceNamingRule)
        {
            string error;
            if (!runtime.TrySetPolicy(candidate, out error, replaceNamingRule))
            {
                previewError = error ?? "OPN_PolicyFailed".Translate();
                return;
            }
            appliedPolicy = runtime.Policy;
            draft = runtime.Policy.Clone();
            RefreshPreview();
        }

        static void DrawText(Listing_Standard listing, string label, string value, Action<string> assign)
        {
            listing.Label(label);
            string edited = Widgets.TextField(listing.GetRect(28f), value ?? string.Empty);
            if (!string.Equals(edited, value ?? string.Empty, StringComparison.Ordinal))
            {
                assign(edited);
                RefreshPreview();
            }
        }

        static void DrawRaces(Listing_Standard listing)
        {
            listing.Label("OPN_RacePrefixes".Translate());
            for (int i = 0; i < draft.Races.Count; i++)
            {
                listing.Label(draft.Races[i].Race + " -> " + draft.Races[i].Prefix);
                if (listing.ButtonText("OPN_Remove".Translate())) removeRace = i;
            }
            if (removeRace >= 0 && removeRace < draft.Races.Count)
            {
                draft.Races.RemoveAt(removeRace);
                RefreshPreview();
            }
            removeRace = -1;
            listing.Label("OPN_Race".Translate() + " (defName)");
            raceBuffer = Widgets.TextField(listing.GetRect(28f), raceBuffer ?? string.Empty);
            listing.Label("OPN_Prefix".Translate());
            prefixBuffer = Widgets.TextField(listing.GetRect(28f), prefixBuffer ?? string.Empty);
            if (listing.ButtonText("OPN_AddRace".Translate()) && !string.IsNullOrWhiteSpace(raceBuffer))
            {
                draft.Races.Add(new RacePrefix { Race = raceBuffer.Trim(), Prefix = prefixBuffer ?? string.Empty });
                raceBuffer = string.Empty;
                prefixBuffer = string.Empty;
                RefreshPreview();
            }
        }

        static void DrawGrades(Listing_Standard listing)
        {
            listing.Label("OPN_Grades".Translate());
            for (int i = 0; i < draft.Grades.Count; i++)
            {
                string minimum = double.IsNegativeInfinity(draft.Grades[i].Minimum) ? "−∞" : NamePolicy.FormatNumber(draft.Grades[i].Minimum);
                listing.Label(minimum + "  " + draft.Grades[i].Name);
                if (listing.ButtonText("OPN_Remove".Translate())) removeGrade = i;
            }
            if (removeGrade >= 0 && removeGrade < draft.Grades.Count)
            {
                draft.Grades.RemoveAt(removeGrade);
                RefreshPreview();
            }
            removeGrade = -1;
            MenuControls.Number(listing, "OPN_Threshold".Translate(), ref threshold, ref thresholdBuffer, -100000f, 100000f);
            listing.Label("OPN_GradeName".Translate());
            gradeBuffer = Widgets.TextField(listing.GetRect(28f), gradeBuffer ?? string.Empty);
            if (listing.ButtonText("OPN_AddGrade".Translate()) && !string.IsNullOrWhiteSpace(gradeBuffer))
            {
                draft.Grades.Add(new GradeBand { Minimum = threshold, Name = gradeBuffer.Trim() });
                gradeBuffer = string.Empty;
                RefreshPreview();
            }
        }

        static void RefreshPreview()
        {
            preview = string.Empty;
            previewError = string.Empty;
            try
            {
                NameTokens sample = Sample(draft);
                preview = NameFormatter.ApplyStyle(draft.Style,
                    NameFormatter.Format(NamingLanguage.Chinese ? draft.ChinesePattern : draft.Pattern, sample), sample);
            }
            catch (Exception exception)
            {
                previewError = exception.Message;
            }
        }


        static NameTokens Sample(NamePolicy policy)
        {
            return new NameTokens
            {
                Number = 1,
                NumberText = "001",
                Score = policy.NeutralScore,
                Grade = policy.GradeFor(policy.NeutralScore),
                Race = "Human",
                RaceLabel = "human",
                RacePrefix = policy.PrefixFor("Human", NamingLanguage.Chinese),
                OriginalName = "Ada",
                OriginalShort = "Ada",
                OriginalFullName = "Ada Sample",
                FirstName = "Ada",
                LastName = "Sample",
                InteractionLabel = "recruit"
            };
        }
    }
}
