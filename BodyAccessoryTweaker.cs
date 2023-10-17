using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;
using static RimWorld.MechClusterSketch;

namespace BodyAccessoryTweaker
{
    public class BATweakerMod : Mod
    {
        public static int AccessoryStartIndex = 0;
        internal static string search = "";
        public static BATweakerSetting setting;
        public static Color border = new ColorInt(97, 108, 122).ToColor;
        public static GUIStyle GUIStyle
        {
            get
            {
                return new GUIStyle(Text.CurFontStyle)
                {
                    alignment = TextAnchor.MiddleCenter
                };
            }
        }
        public static GUIStyle GUIStyle_1
        {
            get
            {
                return new GUIStyle(GUIStyle)
                {
                    alignment = TextAnchor.MiddleLeft
                };
            }
        }
        public BATweakerMod(ModContentPack content) : base(content)
        {
            setting = GetSettings<BATweakerSetting>();
            Harmony harmony = new Harmony(this.Content.Name);
            harmony.Patch(AccessTools.Method(typeof(PawnRenderer), "DrawBodyApparel"), transpiler: new HarmonyMethod(AccessTools.Method(typeof(HarmonyPatchA6), nameof(HarmonyPatchA6.TranDrawBodyApparel_1))));
        }


        public override void DoSettingsWindowContents(Rect inRect)
        {
            Rect r1 = inRect.TopPart(0.05f).LeftPart(0.32f);
            search = Widgets.TextArea(r1, search);
            r1.x += r1.width + 5f;
            if (Mouse.IsOver(r1))
            {
                Widgets.DrawHighlight(r1);
                TooltipHandler.TipRegion(r1, "Only_HideOnColoist_Tooltip".Translate());
            }
            Widgets.CheckboxLabeled(r1, "Only_HideOnColoist".Translate(), ref BATweakerSetting.OnlyHideOnColoist);
            r1.x += r1.width + 5f;
            if (Mouse.IsOver(r1))
            {
                Widgets.DrawHighlight(r1);
                TooltipHandler.TipRegion(r1, "Only_ChangeOnColoist_Tooltip".Translate());
            }
            Widgets.CheckboxLabeled(r1, "Only_ChangeOnColoist".Translate(), ref BATweakerSetting.OnlyChangeOnColoist);
            int ShowCount = 5;
            List<string> list = BATweakerCache.AllAccessory.Where(delegate (string defName)
            {
                ThingDef thingDef = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
                return thingDef != null && thingDef.label.IndexOf(search) != -1;
            }).ToList();
            if (Widgets.ButtonText(inRect.BottomPart(0.95f).TopPart(0.052f), "↑"))
            {
                if (AccessoryStartIndex - ShowCount >= 0)
                {
                    AccessoryStartIndex -= ShowCount;
                }
            }

            if (Widgets.ButtonText(inRect.BottomPart(0.95f).BottomPart(0.052f), "↓"))
            {
                if (list != null && AccessoryStartIndex + ShowCount < list.Count)
                {
                    AccessoryStartIndex += ShowCount;
                }
            }

            Rect r2 = new Rect(inRect.x, (inRect.y + inRect.height * 0.05f) + inRect.height * 0.95f * 0.052f, inRect.width, inRect.height * 0.95f * 0.896f);
            Widgets.DrawWindowBackground(r2);
            if (!list.NullOrEmpty())
            {
                if (AccessoryStartIndex >= list.Count)
                {
                    AccessoryStartIndex = 0;
                }
                Rect rt3 = new Rect(r2.x, r2.y, r2.width, r2.height / 5);
                for (int i = AccessoryStartIndex; i < AccessoryStartIndex + 5 && i < list.Count; i++)
                {
                    string defName = list[i];
                    ThingDef thingDef = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
                    if (thingDef != null)
                    {
                        GUI.color = border;
                        GUI.DrawTexture(new Rect(rt3.x, rt3.y + rt3.height, rt3.width, 2f), BaseContent.WhiteTex);
                        Widgets.DrawLineHorizontal(rt3.x + 0.85f * rt3.height, rt3.y + rt3.height / 2, rt3.width - 0.85f * rt3.height);
                        Widgets.DrawLineVertical(rt3.x + 0.85f * rt3.height, rt3.y, rt3.height);
                        Widgets.DrawLineVertical(rt3.x + 0.85f * rt3.height + (rt3.width - 0.85f * rt3.height) / 3, rt3.y, rt3.height);
                        Widgets.DrawLineVertical(rt3.x + 0.85f * rt3.height + 2 * (rt3.width - 0.85f * rt3.height) / 3, rt3.y, rt3.height);
                        GUI.color = Color.white;
                        Rect rect = new Rect(rt3.x + 0.1f * rt3.height, rt3.y + 0.1f * rt3.height, rt3.height * 0.6f, rt3.height * 0.6f);
                        Widgets.DrawBox(rect);
                        GUI.DrawTexture(rect, thingDef.uiIcon);
                        Rect rect1 = new Rect(rt3.x + 0.05f * rt3.height, rt3.y + rt3.height * 0.65f, rt3.height * 0.75f, rt3.height * 0.35f);
                        GUI.Label(rect1, thingDef.label, GUIStyle);

                        if (!BATweakerSetting.AccessoryData.ContainsKey(defName) || BATweakerSetting.AccessoryData[defName] == null)
                        {
                            BATweakerSetting.InitialSingleSetting(defName);
                        }
                        BATweakerSetting.BATSettingData data = BATweakerSetting.AccessoryData[defName];
                        Rect rect2 = new Rect(rt3.x + rt3.height, rt3.y, (rt3.width - 0.85f * rt3.height) / 3 - 0.3f * rt3.height, rt3.height);
                        Widgets.CheckboxLabeled(rect2.TopHalf(), "Hide_Or_Not".Translate(), ref data.HideOrNot);
                        GUI.Label(rect2.BottomHalf().TopPart(0.7f), "Size".Translate() + ":" + data.size.ToString("F2"), GUIStyle_1);
                        data.size.x = Widgets.HorizontalSlider_NewTemp(rect2.BottomHalf().BottomPart(0.4f).LeftHalf(), data.size.x, 0.5f, 2.0f);
                        data.size.y = Widgets.HorizontalSlider_NewTemp(rect2.BottomHalf().BottomPart(0.4f).RightHalf(), data.size.y, 0.5f, 2.0f);
                        if (Widgets.ButtonText(rect2.BottomHalf().RightPart(0.3f).TopHalf().BottomPart(0.85f), "Reset".Translate()))
                        {
                            data.size = Vector2.one;
                        }
                        rect2.x += (rt3.width - 0.85f * rt3.height) / 3;
                        GUI.Label(rect2.TopHalf().TopPart(0.7f), "East".Translate() + ":" + data.EastOffset.ToString("F2"), GUIStyle_1);
                        data.EastOffset.x = Widgets.HorizontalSlider_NewTemp(rect2.TopHalf().BottomPart(0.4f).LeftHalf(), data.EastOffset.x, -2.0f, 2.0f);
                        data.EastOffset.y = Widgets.HorizontalSlider_NewTemp(rect2.TopHalf().BottomPart(0.4f).RightHalf(), data.EastOffset.y, -2.0f, 2.0f);
                        if (Widgets.ButtonText(rect2.TopHalf().RightPart(0.3f).TopHalf().BottomPart(0.85f), "Reset".Translate()))
                        {
                            data.EastOffset = Vector2.zero;
                        }
                        GUI.Label(rect2.BottomHalf().TopPart(0.7f), "North".Translate() + ":" + data.NorthOffset.ToString("F2"), GUIStyle_1);
                        data.NorthOffset.x = Widgets.HorizontalSlider_NewTemp(rect2.BottomHalf().BottomPart(0.4f).LeftHalf(), data.NorthOffset.x, -2.0f, 2.0f);
                        data.NorthOffset.y = Widgets.HorizontalSlider_NewTemp(rect2.BottomHalf().BottomPart(0.4f).RightHalf(), data.NorthOffset.y, -2.0f, 2.0f);
                        if (Widgets.ButtonText(rect2.BottomHalf().RightPart(0.3f).TopHalf().BottomPart(0.85f), "Reset".Translate()))
                        {
                            data.NorthOffset = Vector2.zero;
                        }
                        rect2.x += (rt3.width - 0.85f * rt3.height) / 3;
                        GUI.Label(rect2.TopHalf().TopPart(0.7f), "South".Translate() + ":" + data.SouthOffset.ToString("F2"), GUIStyle_1);
                        data.SouthOffset.x = Widgets.HorizontalSlider_NewTemp(rect2.TopHalf().BottomPart(0.4f).LeftHalf(), data.SouthOffset.x, -2.0f, 2.0f);
                        data.SouthOffset.y = Widgets.HorizontalSlider_NewTemp(rect2.TopHalf().BottomPart(0.4f).RightHalf(), data.SouthOffset.y, -2.0f, 2.0f);
                        if (Widgets.ButtonText(rect2.TopHalf().RightPart(0.3f).TopHalf().BottomPart(0.85f), "Reset".Translate()))
                        {
                            data.SouthOffset = Vector2.zero;
                        }
                        GUI.Label(rect2.BottomHalf().TopPart(0.7f), "Rotate".Translate() + ":" + "East".Translate() + "[" + data.EastRotate.ToString("F0") + "] | " + "South".Translate() + "[" + data.SouthRotate.ToString("F0") + "]", GUIStyle_1);
                        data.EastRotate = Widgets.HorizontalSlider_NewTemp(rect2.BottomHalf().BottomPart(0.4f).LeftHalf(), data.EastRotate, -180f, 180f);
                        data.SouthRotate = Widgets.HorizontalSlider_NewTemp(rect2.BottomHalf().BottomPart(0.4f).RightHalf(), data.SouthRotate, -180f, 180f);
                        if (Widgets.ButtonText(rect2.BottomHalf().RightPart(0.3f).TopHalf().BottomPart(0.85f), "Reset".Translate()))
                        {
                            data.EastRotate = 0;
                            data.SouthRotate = 0;
                        }
                        rt3.y += rt3.height;
                    }
                }
            }
        }


        public override string SettingsCategory()
        {
            return this.Content.Name;
        }

        public override void WriteSettings()
        {
            ResolveAllApparelGraphics();
            base.WriteSettings();
        }
        public static void ResolveAllApparelGraphics()
        {
            if (Current.Game == null || Current.Game.CurrentMap == null)
            {
                return;
            }
            Map map = Current.Game.CurrentMap;
            if (map.mapPawns != null && !map.mapPawns.AllPawns.NullOrEmpty())
            {
                foreach (Pawn pawn in map.mapPawns.AllPawns)
                {
                    if (pawn.apparel != null && pawn.apparel.AnyApparel)
                    {
                        pawn.apparel.Notify_ApparelChanged();
                    }
                }
            }
        }
    }


    public class BATweakerSetting : ModSettings
    {
        public static bool OnlyHideOnColoist = true;
        public static bool OnlyChangeOnColoist = true;
        public static Dictionary<string, BATSettingData> AccessoryData = new Dictionary<string, BATSettingData>();

        public override void ExposeData()
        {
            Scribe_Values.Look(ref OnlyChangeOnColoist, "OnlyChangeOnColoist", true);
            Scribe_Values.Look(ref OnlyHideOnColoist, "OnlyHideOnColoist", true);
            Scribe_Collections.Look(ref AccessoryData, "AccessoryData", LookMode.Value, LookMode.Deep);
        }

        public class BATSettingData : IExposable
        {
            public bool HideOrNot = false;
            public Vector2 EastOffset = Vector2.zero;
            public Vector2 SouthOffset = Vector2.zero;
            public Vector2 NorthOffset = Vector2.zero;
            public Vector2 size = Vector2.one;
            public float EastRotate = 0f;
            public float SouthRotate = 0f;

            public void ExposeData()
            {
                Scribe_Values.Look(ref HideOrNot, "HideOrNot", true);
                Scribe_Values.Look(ref EastOffset, "EastOffset", Vector2.zero);
                Scribe_Values.Look(ref SouthOffset, "SouthOffset", Vector2.zero);
                Scribe_Values.Look(ref NorthOffset, "NorthOffset", Vector2.zero);
                Scribe_Values.Look(ref size, "size", Vector2.one);
                Scribe_Values.Look(ref EastRotate, "EastRotate", 0f);
                Scribe_Values.Look(ref SouthRotate, "SouthRotate", 0f);


            }
            public Vector2 GetOffset(Rot4 bodyFacing)
            {
                if (bodyFacing == Rot4.East)
                {
                    return EastOffset;
                }
                else if (bodyFacing == Rot4.South)
                {
                    return SouthOffset;
                }
                else if (bodyFacing == Rot4.North)
                {
                    return NorthOffset;
                }
                else
                {
                    return new Vector2(-EastOffset.x, EastOffset.y);
                }
            }
            public float GetRotate(Rot4 bodyFacing)
            {
                if (bodyFacing == Rot4.East)
                {
                    return EastRotate;
                }
                else if (bodyFacing == Rot4.South)
                {
                    return SouthRotate;
                }
                else if (bodyFacing == Rot4.North)
                {
                    return -SouthRotate;
                }
                else
                {
                    return -EastRotate;
                }
            }


        }
        public static void InitialSetting()
        {
            List<string> list = BATweakerCache.AllAccessory;
            for (int i = 0; i < list.Count; i++)
            {
                string defName = list[i];
                InitialSingleSetting(defName);
            }
        }

        internal static void InitialSingleSetting(string defName)
        {
            if (AccessoryData == null)
            {
                AccessoryData = new Dictionary<string, BATSettingData>();
            }
            if (AccessoryData.ContainsKey(defName))
            {
                if (AccessoryData[defName] == null)
                {
                    AccessoryData[defName] = new BATSettingData();
                    return;
                }
                else
                {
                    return;
                }
            }
            else
            {
                AccessoryData.Add(defName, new BATSettingData());
            }
        }
    }
    [StaticConstructorOnStartup]
    public static class BATweakerCache
    {
        public static List<string> AllAccessory = new List<string>();
        static BATweakerCache()
        {
            AllAccessory = DefDatabase<ThingDef>.AllDefs.
                Where(x => x.IsApparel && x.apparel.LastLayer.IsUtilityLayer && (x.apparel.wornGraphicData == null || x.apparel.wornGraphicData.renderUtilityAsPack)&&!x.apparel.wornGraphicPath.NullOrEmpty()).
                Select(x => x.defName).ToList();
            BATweakerSetting.InitialSetting();
        }
    }

    public static class HarmonyPatchA6
    {
        public static IEnumerable<CodeInstruction> TranDrawBodyApparel_1(IEnumerable<CodeInstruction> codes)
        {
            List<CodeInstruction> list = codes.ToList();
            int a = 0;
            int c = 0;
            FieldInfo field = AccessTools.Field(typeof(PawnRenderer), "pawn");
            MethodInfo method = AccessTools.Method(typeof(PawnRenderer), nameof(PawnRenderer.RenderAsPack));
            for (int i = 0; i < list.Count; i++)
            {
                CodeInstruction code = list[i];
                string operand = code.operand.ToStringSafe();
                if (code.opcode == OpCodes.Stloc_S && operand == "UnityEngine.Vector2 (8)")
                {
                    a = i;
                }
                if (a != 0 && i >= a)
                {
                    if (code.opcode == OpCodes.Ldarg_2)
                    {
                        yield return code;
                        yield return new CodeInstruction(OpCodes.Ldarg_S, 5);
                        yield return new CodeInstruction(OpCodes.Ldloc_3);
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldfld, field);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatchA6), nameof(SetAccessoryLoc)));
                        c++;
                    }
                    else
                    if (code.opcode == OpCodes.Ldloc_1)
                    {
                        yield return code;
                        yield return new CodeInstruction(OpCodes.Ldarg_S, 5);
                        yield return new CodeInstruction(OpCodes.Ldloc_3);
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldfld, field);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatchA6), nameof(SetAccessoryRotate)));
                        c++;
                    }
                    else
                    if (code.opcode == OpCodes.Ldarg_3)
                    {
                        yield return code;
                        yield return new CodeInstruction(OpCodes.Ldloc_3);
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldfld, field);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatchA6), nameof(SetAccessoryMesh)));
                        c++;
                    }
                    else
                    if (code.opcode == OpCodes.Ldarg_1)
                    {
                        yield return code;
                        yield return new CodeInstruction(OpCodes.Ldarg_S, 5);
                        yield return new CodeInstruction(OpCodes.Ldloc_3);
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldfld, field);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatchA6), nameof(SetAccessoryLoc)));
                        c++;
                    }
                    else
                    {
                        yield return code;
                    }
                }
                else
                if (i > 3 && code.opcode == OpCodes.Brfalse && CodeInstructionExtensions.Is(list[i - 1], OpCodes.Call, method))
                {
                    yield return code;
                    yield return new CodeInstruction(OpCodes.Ldloc_3);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, field);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatchA6), nameof(DrawOrNot)));
                    yield return code;
                    c++;
                }
                else
                {
                    yield return code;
                }
            }
            if (c != 7)
            {
                Log.Warning("BATweakerPatch-Fail-At HarmonyPatch:'TranDrawBodyApparel_1'");
            }
        }

        public static bool DrawOrNot(ApparelGraphicRecord apparelGraphic, Pawn pawn)
        {
            if (pawn == null || BATweakerSetting.OnlyHideOnColoist && !pawn.IsColonist)
            {
                return true;
            }
            else
            {
                string defNmae = apparelGraphic.sourceApparel.def.defName;
                if (BATweakerSetting.AccessoryData.ContainsKey(defNmae))
                {
                    BATweakerSetting.BATSettingData data = BATweakerSetting.AccessoryData[defNmae];
                    return !data.HideOrNot;
                }
                else
                {
                    return true;
                }
            }
        }

        public static Mesh SetAccessoryMesh(Mesh origin, ApparelGraphicRecord apparelGraphic, Pawn pawn)
        {
            if (pawn == null || BATweakerSetting.OnlyChangeOnColoist && !pawn.IsColonist)
            {
                return origin;
            }
            else
            {
                string defNmae = apparelGraphic.sourceApparel.def.defName;
                if (BATweakerSetting.AccessoryData.ContainsKey(defNmae))
                {
                    BATweakerSetting.BATSettingData data = BATweakerSetting.AccessoryData[defNmae];
                    Mesh a = new Mesh()
                    {
                        name = origin.name,
                        vertices = origin.vertices,
                        triangles = origin.triangles,
                        normals = origin.normals,
                        uv = origin.uv,
                        bounds = origin.bounds
                    };
                    Vector2 size = data.size;
                    Vector3[] ve = origin.vertices;
                    for (int k = 0; k < ve.Length; k++)
                    {
                        ve[k].x *= size.x;
                        ve[k].z *= size.y;
                    }
                    a.vertices = ve;
                    return a;
                }
                else
                {
                    return origin;
                }
            }
        }

        public static Quaternion SetAccessoryRotate(Quaternion origin, Rot4 bodyFacing, ApparelGraphicRecord apparelGraphic, Pawn pawn)
        {
            if (pawn == null || BATweakerSetting.OnlyChangeOnColoist && !pawn.IsColonist)
            {
                return origin;
            }
            else
            {
                string defNmae = apparelGraphic.sourceApparel.def.defName;
                if (BATweakerSetting.AccessoryData.ContainsKey(defNmae))
                {
                    BATweakerSetting.BATSettingData data = BATweakerSetting.AccessoryData[defNmae];
                    float x = data.GetRotate(bodyFacing);
                    Quaternion a = new Quaternion()
                    {
                        eulerAngles = origin.eulerAngles,
                        x = origin.x,
                        y = origin.y,
                        z = origin.z,
                        w = origin.w
                    };
                    Vector3 b = new Vector3(origin.eulerAngles.x, origin.eulerAngles.y + x, origin.eulerAngles.z);
                    a.eulerAngles = b;
                    return a;
                }
                else
                {
                    return origin;
                }
            }
        }

        public static Vector3 SetAccessoryLoc(Vector3 origin, Rot4 bodyFacing, ApparelGraphicRecord apparelGraphic, Pawn pawn)
        {
            if (pawn == null || BATweakerSetting.OnlyChangeOnColoist && !pawn.IsColonist)
            {
                return origin;
            }
            else
            {
                string defNmae = apparelGraphic.sourceApparel.def.defName;
                if (BATweakerSetting.AccessoryData.ContainsKey(defNmae))
                {
                    BATweakerSetting.BATSettingData data = BATweakerSetting.AccessoryData[defNmae];
                    Vector2 b = data.GetOffset(bodyFacing);
                    Vector3 a = new Vector3(origin.x + b.x, origin.y, origin.z + b.y);
                    return a;
                }
                else
                {
                    return origin;
                }
            }
        }
    }
}
