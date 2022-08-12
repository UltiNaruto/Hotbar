using ModAPI;
using ModAPI.Attributes;
using TheForest.Items;
using TheForest.UI;
using TheForest.Utils;
using UnityEngine;
using static TheForest.UI.InputMappingIcons;

namespace Hotbar
{
    class Hotbar : MonoBehaviour
    {
        enum InputDeviceType
        {
            KeyboardAndMouse,
            PS,
            Xbox
        }

        bool isInGame;
        InputDeviceType inputDevice = InputDeviceType.KeyboardAndMouse;
        string[] Hotbar_Items = new string[4];
        bool[] Equippable_Hotbar_Items = new bool[4];
        protected GUIStyle labelStyle = null;
        float[] hotbarRectSize = new float[2] { 160.0f, 120.0f };

        [ExecuteOnGameStart]
        private static void AddMeToScene()
        {
            new GameObject("__Hotbar__").AddComponent<Hotbar>();
        }

        private void InitOSD()
        {
            GUI.skin = Interface.Skin;
            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(GUI.skin.label);
                labelStyle.fontSize = 16;
                labelStyle.alignment = TextAnchor.UpperCenter;
                labelStyle.normal.textColor = Color.white;
            }
        }

        private void DrawLTrigger(Rect rect)
        {
            GUI.DrawTexture(rect, InputMappingIcons.GetTextureFor(Actions.AltFire));
        }

        private void DrawItemSlotKey(Rect rect, int i)
        {
            Actions[] actions = new Actions[4] { Actions.ItemSlot1, Actions.ItemSlot2, Actions.ItemSlot3, Actions.ItemSlot4 };
            GUI.DrawTexture(rect, InputMappingIcons.GetTextureFor(actions[i]));
        }

        private void OnGUI()
        {
            InitOSD();

            int i;
            float x = Screen.width, y = Screen.height;
            string item = string.Empty;

            if (isInGame)
            {
                GUI.BeginGroup(new Rect((x / 2.0f) - 2f * hotbarRectSize[0], y - 80f - 0.5f * hotbarRectSize[1], hotbarRectSize[0] * 4f, hotbarRectSize[1]));
                GUI.Box(
                    new Rect(0f, 0f, hotbarRectSize[0] * 4f, hotbarRectSize[1]),
                    "",
                    GUI.skin.window
                );
                for (i = 0; i < 4; i++)
                {
                    item = Equippable_Hotbar_Items[i] ? Hotbar_Items[i] : string.Empty;
                    if (inputDevice > InputDeviceType.KeyboardAndMouse)
                    {
                        DrawLTrigger(
                            new Rect(i * hotbarRectSize[0] + hotbarRectSize[0] * 0.15f, hotbarRectSize[1] * 0.05f, hotbarRectSize[0] * 0.2f, hotbarRectSize[1] * 0.2f)
                        );
                        labelStyle.fontSize = 18;
                        GUI.Label(
                            new Rect(i * hotbarRectSize[0], hotbarRectSize[1] * 0.05f, hotbarRectSize[0] * 0.9f, hotbarRectSize[1] * 0.9f),
                            "+",
                            labelStyle
                        );
                        labelStyle.fontSize = 16;
                        DrawItemSlotKey(
                            new Rect(i * hotbarRectSize[0] + hotbarRectSize[0] * 0.55f, hotbarRectSize[1] * 0.05f, hotbarRectSize[0] * 0.2f, hotbarRectSize[1] * 0.2f),
                            i
                        );
                    } else {
                        DrawItemSlotKey(
                            new Rect(i * hotbarRectSize[0] + hotbarRectSize[0] * 0.3f, 0f, hotbarRectSize[0] * 0.3f, hotbarRectSize[1] * 0.3f),
                            i
                        );
                        labelStyle.normal.textColor = Color.black;
                        labelStyle.fontSize = 18;
                        GUI.Label(
                            new Rect(i * hotbarRectSize[0], 0f, hotbarRectSize[0] * 0.9f, hotbarRectSize[1] * 0.9f),
                            $"{i+1}",
                            labelStyle
                        );
                        labelStyle.fontSize = 16;
                        labelStyle.normal.textColor = Color.white;
                    }
                    GUI.Label(
                        new Rect(i * hotbarRectSize[0], hotbarRectSize[1] * 0.4f, hotbarRectSize[0] * 0.8f, hotbarRectSize[1] * 0.8f),
                        $"{item}", 
                        labelStyle
                    );
                }
                GUI.EndGroup();
            }
        }

        private void Update()
        {
            int i, amount;
            Item item = null;
            isInGame = !LocalPlayer.IsInPauseMenu &&
                       (LocalPlayer.IsInWorld || LocalPlayer.IsInCaves || LocalPlayer.IsInClosedArea) &&
                       !LocalPlayer.IsInEndgame &&
                       !LocalPlayer.IsInInventory;

            inputDevice = TheForest.Utils.Input.IsGamePad && TheForest.Utils.Input.UsingDualshock ? InputDeviceType.PS : (!TheForest.Utils.Input.IsGamePad ? InputDeviceType.KeyboardAndMouse : InputDeviceType.Xbox);

            // Original 4 item slots in backpack
            for (i = 0; i < 4; i++)
            {
                item = ItemDatabase.ItemById(LocalPlayer.Inventory.QuickSelectItemIds[i]);
                if (item != null)
                {
                    Hotbar_Items[i] = string.Empty;

                    amount = LocalPlayer.Inventory.HasOwned(LocalPlayer.Inventory.QuickSelectItemIds[i]) ? LocalPlayer.Inventory.AmountOf(LocalPlayer.Inventory.QuickSelectItemIds[i]) : 0;
                    if (amount >= 1)
                    {
                        if (amount > 1) Hotbar_Items[i] = $"{amount} ";
                        Hotbar_Items[i] += Scene.HudGui.GetItemName(LocalPlayer.Inventory.QuickSelectItemIds[i], amount > 1, false).ToUpperFirstLowerLast();
                        Equippable_Hotbar_Items[i] = LocalPlayer.Inventory.HasOwned(LocalPlayer.Inventory.QuickSelectItemIds[i]);
                    }
                }
                else
                {
                    Hotbar_Items[i] = string.Empty;
                    Equippable_Hotbar_Items[i] = false;
                }
            }
        }
    }
}
