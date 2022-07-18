using ModAPI;
using ModAPI.Attributes;
using TheForest.Items;
using TheForest.Utils;
using UnityEngine;

namespace Hotbar
{
    class Hotbar : MonoBehaviour
    {
        bool isInGame = true;
        string[] Hotbar_Items = new string[4];
        bool[] Equippable_Hotbar_Items = new bool[4];
        protected GUIStyle labelStyle = null;
        float[] hotbarRectSize = new float[2] { 120.0f, 80.0f };

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
            }
        }

        private void OnGUI()
        {
            InitOSD();

            int i;
            float x = Screen.width, y = Screen.height;
            string item = string.Empty;

            if (isInGame)
            {
                GUI.BeginGroup(new Rect((x / 2.0f) - 2f * hotbarRectSize[0], 40f - 0.5f * hotbarRectSize[1], hotbarRectSize[0] * 4f, hotbarRectSize[1]));
                GUI.Box(
                    new Rect(0f, 0f, hotbarRectSize[0] * 4f, hotbarRectSize[1]),
                    "",
                    GUI.skin.window
                );
                for (i = 0; i < 4; i++)
                {
                    item = Equippable_Hotbar_Items[i] ? Hotbar_Items[i] : string.Empty;
                    GUI.Label(
                        new Rect(i * hotbarRectSize[0], 0f, hotbarRectSize[0], hotbarRectSize[1]),
                        $"{i + 1}",
                        labelStyle
                    );
                    GUI.Label(
                        new Rect(i * hotbarRectSize[0], 30f, hotbarRectSize[0], hotbarRectSize[1]),
                        $"{item}", 
                        labelStyle
                    );
                }
                GUI.EndGroup();
            }
        }

        private void Update()
        {
            int i;
            Item item = null;
            isInGame = !LocalPlayer.IsInPauseMenu &&
                       (LocalPlayer.IsInWorld || LocalPlayer.IsInCaves || LocalPlayer.IsInClosedArea) &&
                       !LocalPlayer.IsInEndgame &&
                       !LocalPlayer.IsInInventory;

            // Original 4 item slots in backpack
            for (i = 0; i < 4; i++)
            {
                item = ItemDatabase.ItemById(LocalPlayer.Inventory.QuickSelectItemIds[i]);
                if (item != null)
                {
                    int amount = LocalPlayer.Inventory.AmountOf(LocalPlayer.Inventory.QuickSelectItemIds[i]);
                    if (amount > 1)
                        Hotbar_Items[i] = $"{amount} ";
                    else
                        Hotbar_Items[i] = string.Empty;
                    Hotbar_Items[i] += Scene.HudGui.GetItemName(LocalPlayer.Inventory.QuickSelectItemIds[i], amount > 1, false).ToUpperFirstLowerLast();
                    Equippable_Hotbar_Items[i] = LocalPlayer.Inventory.HasOwned(LocalPlayer.Inventory.QuickSelectItemIds[i]);
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
