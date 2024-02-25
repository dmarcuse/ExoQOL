using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using HarmonyLib;

namespace ExoQOL;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    private void Awake()
    {
        // Plugin startup logic
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        Harmony.CreateAndPatchAll(typeof(Plugin), PluginInfo.PLUGIN_NAME);
    }

    /// <summary>
    /// Always show a checkmark for choices that have been seen, even when the
    /// "skip seen text" setting is disabled
    /// </summary>
    [HarmonyPatch(typeof(NWButtonResults), "SetRequirements")]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> AlwaysShowSeenIcon(IEnumerable<CodeInstruction> instructions)
    {
        FieldInfo skipSeenTextField = typeof(Settings)
            .GetField(nameof(Settings.skipSeenText), BindingFlags.Instance);

        return new CodeMatcher(instructions)
            .MatchForward(useEnd: true,
                new CodeMatch(OpCodes.Ldfld, skipSeenTextField),
                new CodeMatch(OpCodes.Brfalse)
            )
            .Repeat(matcher => matcher.SetAndAdvance(OpCodes.Pop, null))
            .InstructionEnumeration();
    }
}

