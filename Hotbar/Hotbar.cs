using Hotbar.Utils;
using ModAPI;
using ModAPI.Attributes;
using System;
using System.IO;
using System.Linq;
using TheForest.Items;
using TheForest.UI;
using TheForest.Utils;
using UnityEngine;

using static TheForest.UI.InputMappingIcons;

namespace Hotbar
{
    class Hotbar : MonoBehaviour
    {
        internal static string modsFolder => Path.GetFullPath(Application.dataPath + "/../Mods/Hotbar");
        int[] ITEMS_WITH_AMMO = new int[] { 44, 79, 230, 279, 283, 287, 289, 306 };
        int[] ITEMS_WITH_DURATION = new int[] { 51, 75, 79, 230, 261, 279, 291 };
        Actions[] QuickSelectActions = new Actions[4] { Actions.ItemSlot1, Actions.ItemSlot2, Actions.ItemSlot3, Actions.ItemSlot4 };

        Config config = null;
        DateTime lastConfigWriteTime = DateTime.MinValue;
        bool isController = false;
        int[] Hotbar_Items_Ammos = new int[4] { -1, -1, -1, -1 };
        float[] Hotbar_Items_Durations = new float[4] { float.NaN, float.NaN, float.NaN, float.NaN };
        string[] Hotbar_Items_TXT = new string[4] { string.Empty, string.Empty, string.Empty, string.Empty };
        Texture[] Hotbar_Items_TEX = new Texture[4] { null, null, null, null };
        bool[] Equippable_Hotbar_Items = new bool[4] { false, false, false, false };
        protected GUIStyle labelStyle = null;

        [ExecuteOnGameStart]
        private static void AddMeToScene()
        {
            new GameObject("__Hotbar__").AddComponent<Hotbar>();
        }

        private void InitOSD()
        {
            var configFile = Path.Combine(modsFolder, "config.json");
            GUI.skin = Interface.Skin;
            if (lastConfigWriteTime != File.GetLastWriteTime(configFile))
            {
                if(lastConfigWriteTime != DateTime.MinValue)
                    ModAPI.Console.Write("[Hotbar] Modification detected! Reloading config...");
                config = null;
            }

            if (config == null)
            {
                if (!Directory.Exists(modsFolder))
                    Directory.CreateDirectory(modsFolder);
                if (!File.Exists(configFile))
                    File.WriteAllBytes(configFile, Properties.Resources.default_config);

                lastConfigWriteTime = File.GetLastWriteTime(configFile);

                try {
                    config = new Config(File.ReadAllText(configFile));
                } catch (Exception ex) {
                    ModAPI.Console.Write(ex.Message);
                    ModAPI.Console.Write(ex.StackTrace.ToString());
                    if (File.Exists(configFile))
                        File.Delete(configFile);
                    return;
                }
            }

            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(GUI.skin.label);
                labelStyle.fontSize = 16;
                labelStyle.normal.textColor = Color.white;
            }
        }

        private void DrawLTrigger(Rect rect)
        {
            GUI.DrawTexture(rect, InputMappingIcons.GetTextureFor(Actions.AltFire));
        }

        private void DrawItemSlotKey(Rect rect, int i)
        {
            GUI.DrawTexture(rect, InputMappingIcons.GetTextureFor(QuickSelectActions[i]));
        }

        static readonly Texture2D backgroundTexture = new Texture2D(1, 1);

        public void GUIDrawRect(Rect position, Color color)
        {
            if (backgroundTexture.GetPixel(0, 0) != color)
            {
                backgroundTexture.SetPixel(0, 0, color);
                backgroundTexture.Apply();
            }

            GUI.DrawTexture(position, backgroundTexture, ScaleMode.StretchToFill, false);
        }

        private void OnGUI()
        {
            InitOSD();

            int i;
            float x = Screen.width, y = Screen.height, constW, constH;
            string text = string.Empty, ammoText = String.Empty;

            if (config == null)
                return;

            if (Scene.HudGui != null && !(LocalPlayer.IsInInventory || LocalPlayer.IsInPauseMenu))
            {
                GUI.BeginGroup(config.HotbarOrientation == Orientation.Horizontal ? config.Hotbar.Pad(-3f) : config.Hotbar.Pad(-3f - (isController ? 40f : 0f), -3f, -3 - (isController ? 72f : 32f), -3f));
                for (i = 0; i < 4; i++)
                {
                    text = Equippable_Hotbar_Items[i] ? Hotbar_Items_TXT[i] : string.Empty;
                    ammoText = (Equippable_Hotbar_Items[i] && Hotbar_Items_Ammos[i] != -1) ? $"{Hotbar_Items_Ammos[i]}" : string.Empty;
                    if (config.HotbarOrientation == Orientation.Horizontal)
                    {
                        if (ammoText != string.Empty)
                        {
                            GUI.Box(
                                new Rect((float)i * (config.Hotbar.width / 4f), config.Hotbar.height - 20f, config.Hotbar.width / 4f, 20f),
                                "",
                                GUI.skin.window
                            );
                        }
                        GUI.Box(
                            new Rect((float)i * (config.Hotbar.width / 4f), 0f, config.Hotbar.width / 4f, config.Hotbar.height - 25f),
                            "",
                            GUI.skin.window
                        );
                        GUI.BeginGroup(new Rect(3f + (float)i * (config.Hotbar.width / 4f), 3f, (config.Hotbar.width / 4f) - 3f, config.Hotbar.height - 3f));
                        labelStyle.alignment = TextAnchor.MiddleCenter;
                        GUI.Label(
                            new Rect(0f, config.Hotbar.height - 20f, config.Hotbar.width / 4f, 20f),
                            ammoText,
                            labelStyle
                        );
                        if (config.ShowItemAs == ShowToggle.Text)
                        {
                            labelStyle.alignment = TextAnchor.UpperCenter;
                            GUI.Label(
                                new Rect(0f, 33f, config.Hotbar.width / 4f, config.Hotbar.height - 33f),
                                text,
                                labelStyle
                            );
                        }

                        if (isController)
                        {
                            DrawLTrigger(
                                new Rect((config.Hotbar.width / 8f) - 36f, 0f, 24f, 24f)
                            );
                            
                            labelStyle.fontSize = 18;
                            GUI.Label(
                                new Rect((config.Hotbar.width / 8f) - 12f, 0f, 24f, 24f),
                                "+",
                                labelStyle
                            );
                            labelStyle.fontSize = 16;
                            DrawItemSlotKey(
                                new Rect((config.Hotbar.width / 8f) + 12f, 0f, 24f, 24f),
                                i
                            );
                        }
                        else
                        {
                            DrawItemSlotKey(
                                new Rect((config.Hotbar.width / 8f) - 16f, 0f, 32f, 32f),
                                i
                            );
                            labelStyle.normal.textColor = Color.black;
                            labelStyle.fontSize = 18;
                            GUI.Label(
                                new Rect((config.Hotbar.width / 8f) - 12f, 4f, 24f, 24f),
                                $"{i + 1}",
                                labelStyle
                            );
                            labelStyle.fontSize = 16;
                            labelStyle.normal.textColor = Color.white;
                        }
                        GUI.EndGroup();
                    }
                    else
                    {
                        if (ammoText != string.Empty)
                        {
                            GUI.Box(
                                new Rect(0f, (float)i * (config.Hotbar.height / 4f), 65f, config.Hotbar.height / 4f),
                                "",
                                GUI.skin.window
                            );
                        }
                        GUI.Box(
                            new Rect(70f, (float)i * (config.Hotbar.height / 4f), config.Hotbar.width - 70f, config.Hotbar.height / 4f),
                            "",
                            GUI.skin.window
                        );
                        GUI.BeginGroup(new Rect(3f, 3f + (float)i * (config.Hotbar.height / 4f), config.Hotbar.width + 3 + (isController ? 77f : 32f), (config.Hotbar.height / 4f) - 3f));
                        labelStyle.alignment = TextAnchor.MiddleRight;
                        GUI.Label(
                            new Rect(0f, 0f, 55f, config.Hotbar.height / 4f),
                            ammoText,
                            labelStyle
                        );
                        labelStyle.alignment = TextAnchor.MiddleCenter;
                        if (config.ShowItemAs == ShowToggle.Text)
                        {
                            GUI.Label(
                                new Rect(73f, 0f, config.Hotbar.width - 73f, config.Hotbar.height / 4f),
                                text,
                                labelStyle
                            );
                        }

                        if (isController)
                        {
                            DrawLTrigger(
                                new Rect(config.Hotbar.width + 3f, (((config.Hotbar.height / 4f) - 3f) / 2f) - 12f, 24f, 24f)
                            );

                            labelStyle.fontSize = 18;
                            GUI.Label(
                                new Rect(config.Hotbar.width + 27f, (((config.Hotbar.height / 4f) - 3f) / 2f) - 12f, 24f, 24f),
                                "+",
                                labelStyle
                            );
                            labelStyle.fontSize = 16;
                            DrawItemSlotKey(
                                new Rect(config.Hotbar.width + 51f, (((config.Hotbar.height / 4f) - 3f) / 2f) - 12f, 24f, 24f),
                                i
                            );
                        }
                        else
                        {
                            DrawItemSlotKey(
                                new Rect(config.Hotbar.width + 3f, (((config.Hotbar.height / 4f) - 3f) / 2f) - 16f, 32f, 32f),
                                i
                            );
                            labelStyle.normal.textColor = Color.black;
                            labelStyle.fontSize = 18;
                            GUI.Label(
                                new Rect(config.Hotbar.width + 7f, (((config.Hotbar.height / 4f) - 3f) / 2f) - 12f, 24f, 24f),
                                $"{i + 1}",
                                labelStyle
                            );
                            labelStyle.fontSize = 16;
                            labelStyle.normal.textColor = Color.white;
                        }
                        GUI.EndGroup();
                    }

                    if (!float.IsNaN(Hotbar_Items_Durations[i]))
                    {
                        constW = 3f;
                        if (config.HotbarOrientation == Orientation.Horizontal)
                        {
                            constH = config.Hotbar.height;
                            GUI.BeginGroup(new Rect((float)i * (config.Hotbar.width / 4f), 0f, constW, constH - 28f));
                        }
                        else
                        {
                            constH = (config.Hotbar.height / 4f) - 5f;
                            GUI.BeginGroup(new Rect(config.Hotbar.width - constW, (float)i * (config.Hotbar.height / 4f), constW, constH));
                        }
                        GUIDrawRect(new Rect(0f, (1f - Hotbar_Items_Durations[i]) * constH, constW, Hotbar_Items_Durations[i] * constH), Color.green);
                        GUIDrawRect(new Rect(0f, 0f, constW, (1f - Hotbar_Items_Durations[i]) * constH), Color.red);
                        GUI.EndGroup();
                    }
                }
                GUI.EndGroup();
            }
        }

        private void Update()
        {
            int i, amount;
            float duration = float.NaN;
            Item item = null, ammo = null;

            if (config == null)
                return;

            isController = TheForest.Utils.Input.IsGamePad;

            // Original 4 item slots in backpack
            for (i = 0; i < 4; i++)
            {
                item = ItemDatabase.ItemById(LocalPlayer.Inventory.QuickSelectItemIds[i]);
                if (item != null)
                {
                    Hotbar_Items_TXT[i] = string.Empty;
                    Hotbar_Items_TEX[i] = null;

                    if (item.Owns())
                    {
                        amount = item.GetAmount();
                        ammo = item.GetAmmo();
                        duration = item.GetDuration();
                        Hotbar_Items_TXT[i] += Scene.HudGui.GetItemName(item._id, amount > 1, false).ToUpperFirstLowerLast();
                        if (ITEMS_WITH_AMMO.Any(_id => _id == item._id))
                            Hotbar_Items_Ammos[i] = (config.ShowAmmo && ammo != null && ammo.GetMaxAmount() > 1) ? ammo.GetAmount() : -1;
                        else
                            Hotbar_Items_Ammos[i] = (config.ShowItemAmount && item.GetMaxAmount() > 1) ? amount : -1;
                        ModAPI.Console.Write($"{Hotbar_Items_TXT[i]} ({item._id}) => {duration}");
                        Hotbar_Items_Durations[i] = (config.ShowItemDuration && ITEMS_WITH_DURATION.Any(id => id == item._id)) ? duration : float.NaN;
                        Hotbar_Items_TEX[i] = item._pickupMaterial.mainTexture;
                        Equippable_Hotbar_Items[i] = item.Owns() && amount > 0;
                    }
                }
                else
                {
                    Hotbar_Items_TXT[i] = string.Empty;
                    Hotbar_Items_TEX[i] = null;
                    Equippable_Hotbar_Items[i] = false;
                }
            }
        }
    }
}
