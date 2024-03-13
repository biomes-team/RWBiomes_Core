using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BiomesCore.Reflections;
using HarmonyLib;
using Verse;

namespace BiomesCore.Patches.Caverns
{
	/// <summary>
	/// Helpers to implement common transpiler operations for Caverns.
	/// </summary>
	public static class Transpilers
	{
		/// <summary>
		/// Transpiles all instances of Room.PsychologicallyOutdoors to be compatible with Caverns.
		/// </summary>
		/// <param name="instructions">Original instructions to be patched.</param>
		/// <param name="cellCode">OpCode to use for loading the current cell.</param>
		/// <returns>Patched instructions.</returns>
		public static List<CodeInstruction> CavernsAwarePsychologicallyOutdoors(List<CodeInstruction> instructions, OpCode cellCode)
		{
			return TranspilerHelper.ReplaceCall(instructions, Methods.PsychologicallyOutdoorsMethod,
				Methods.PsychologicallyOutdoorsOrCavernMethod,
				new List<CodeInstruction> {new CodeInstruction(cellCode)});
		}

		/// <summary>
		/// Replaces calls to GridsUtility.Roofed with IntVec3Extensions.UnbreachableRoofed.
		/// </summary>
		/// <param name="instructions"></param>
		/// <returns></returns>
		public static List<CodeInstruction> CellUnbreachableRoofed(List<CodeInstruction> instructions)
		{
			return TranspilerHelper.ReplaceCall(instructions, Methods.CellRoofedMethod, Methods.CellUnbreachableRoofedMethod);
		}

		public static List<CodeInstruction> RoofGridUnbreachableRoofed(List<CodeInstruction> instructions)
		{
			return TranspilerHelper.ReplaceCall(instructions, Methods.RoofGridRoofedOriginal, Methods.RoofGridRoofedPatched);
		}

		public static List<CodeInstruction> GetNonCavernRoof(List<CodeInstruction> instructions)
		{
			return TranspilerHelper.ReplaceCall(instructions, Methods.GetRoofMethod, Methods.GetNonCavernRoofMethod);
		}
	}

	[StaticConstructorOnStartup]
	public static class Methods
	{
		public static readonly MethodInfo PsychologicallyOutdoorsMethod =
			AccessTools.PropertyGetter(typeof(Room), nameof(Room.PsychologicallyOutdoors));

		public static readonly MethodInfo CellRoofedMethod =
			AccessTools.Method(typeof(GridsUtility), nameof(GridsUtility.Roofed));

		public static readonly MethodInfo RoofGridRoofedOriginal =
			AccessTools.Method(typeof(RoofGrid), nameof(RoofGrid.Roofed), new[] {typeof(IntVec3)});

		public static readonly MethodInfo PsychologicallyOutdoorsOrCavernMethod =
			AccessTools.Method(typeof(Utility), nameof(Utility.CavernAwarePsychologicallyOutdoors));

		public static readonly MethodInfo CellUnbreachableRoofedMethod =
			AccessTools.Method(typeof(IntVec3Extensions), nameof(IntVec3Extensions.UnbreachableRoofed));

		public static readonly MethodInfo HasNonCavernRoofMethod =
			AccessTools.Method(typeof(Utility), nameof(Utility.HasNonCavernRoof));

		public static readonly MethodInfo GetRoofThickIfCavernMethod =
			AccessTools.Method(typeof(Utility), nameof(Utility.GetRoofThickIfCavern));

		public static readonly MethodInfo RoofGridRoofedPatched =
			AccessTools.Method(typeof(RoofGridExtensions), nameof(RoofGridExtensions.UnbreachableRoofed),
				new[] {typeof(RoofGrid), typeof(IntVec3)});

		public static readonly MethodInfo UsesOutdoorTemperatureMethod =
			AccessTools.Method(typeof(GridsUtility), nameof(GridsUtility.UsesOutdoorTemperature));

		public static readonly MethodInfo NotCavernAndUsesOutdoorTemperatureMethod =
			AccessTools.Method(typeof(Utility), nameof(Utility.NotCavernAndUsesOutdoorTemperature));

		public static readonly MethodInfo GetRoofMethod =
			AccessTools.Method(typeof(GridsUtility), nameof(GridsUtility.GetRoof));

		public static readonly MethodInfo GetNonCavernRoofMethod =
			AccessTools.Method(typeof(Utility), nameof(Utility.GetNonCavernRoof));

		static Methods()
		{
			foreach (FieldInfo field in typeof(Methods).GetFields(BindingFlags.Public | BindingFlags.Static))
			{
				System.Type fieldType = field.FieldType;
				if (fieldType != typeof(MethodInfo))
				{
					BiomesCore.Error($"Transpiler.Methods: Field {field.Name} must be a MethodInfo instance.");
					continue;
				}

				if (field.GetValue(null) == null)
				{
					BiomesCore.Error($"Transpiler.Methods: Field {field.Name} must have a value.");
				}
			}
		}
	}
}