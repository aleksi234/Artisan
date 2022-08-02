﻿using Artisan.RawInformation;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Artisan.CraftingLogic.CurrentCraft;

namespace Artisan
{
    internal unsafe class ActionWatching
    {
        public delegate byte UseActionDelegate(ActionManager* actionManager, uint actionType, uint actionID, long targetObjectID, uint param, uint useType, int pvp, bool* isGroundTarget);
        public static Hook<UseActionDelegate> UseActionHook;
        private static byte UseActionDetour(ActionManager* actionManager, uint actionType, uint actionID, long targetObjectID, uint param, uint useType, int pvp, bool* isGroundTarget)
        {
            try
            {
                if (LuminaSheets.ActionSheet.TryGetValue(actionID, out var act1))
                {
                    string skillName = act1.Name;
                    var allOfSameName = LuminaSheets.ActionSheet.Where(x => x.Value.Name == skillName).Select(x => x.Key);

                    if (allOfSameName.Any(x => x == Skills.Manipulation))
                        ManipulationUsed = true;

                    if (allOfSameName.Any(x => x == Skills.WasteNot || x == Skills.WasteNot2))
                        WasteNotUsed = true;

                    if (allOfSameName.Any(x => x == Skills.FinalAppraisal))
                    {
                        CurrentRecommendation = 0;
                        JustUsedFinalAppraisal = true;
                    }
                    else
                        JustUsedFinalAppraisal = false;

                    if (allOfSameName.Any(x => x == Skills.Innovation))
                        InnovationUsed = true;

                    if (allOfSameName.Any(x => x == Skills.Veneration))
                        VenerationUsed = true;

                }
                if (LuminaSheets.CraftActions.TryGetValue(actionID, out var act2))
                {
                    string skillName = act2.Name;
                    var allOfSameName = LuminaSheets.CraftActions.Where(x => x.Value.Name == skillName).Select(x => x.Key);

                    if (allOfSameName.Any(x => x == Skills.Observe))
                        JustUsedObserve = true;
                    else
                        JustUsedObserve = false;
                }
                return UseActionHook!.Original(actionManager, actionType, actionID, targetObjectID, param, useType, pvp, isGroundTarget);
            }
            catch (Exception ex)
            {
                Dalamud.Logging.PluginLog.Error(ex, "UseActionDetour");
                return UseActionHook!.Original(actionManager, actionType, actionID, targetObjectID, param, useType, pvp, isGroundTarget);
            }
        }

        static ActionWatching()
        {
            UseActionHook ??= new Hook<UseActionDelegate>((IntPtr)ActionManager.fpUseAction, UseActionDetour);
        }

        public static void Enable()
        {
            UseActionHook?.Enable();
        }

        public static void Disable()
        {
            UseActionHook?.Disable();
        }

        public static void Dispose()
        {
            UseActionHook?.Dispose();
        }
    }
}
