using TheForest.Items;
using TheForest.Utils;

namespace Hotbar.Utils
{
    internal static class ItemUtils
    {
        internal static int GetAmount(this Item item)
        {
            int amount = item.Owns() ? LocalPlayer.Inventory.AmountOf(item._id) : 0;
            if (item.IsEquippingItem())
                amount++;
            return amount;
        }

        internal static int GetMaxAmount(this Item item)
        {
            // flintlock pistol ammo (not sure why it doesn't say 100000 for max amount)
            // or flare gun ammo
            if (item._id == 231 || item._id == 107) return 100000;
            return item._maxAmount;
        }

        internal static Item GetAmmo(this Item item)
        {
            try
            {
                if (item._ammoItemId < 0)
                    return null;

                var ammo = ItemDatabase.ItemById(item._ammoItemId);

                if (ammo == null)
                    return null;

                return ammo;
            } catch { }

            return null;
        }

        internal static float GetDuration(this Item item)
        {
            // fire stick
            /*if (LocalPlayer.ActiveBurnableItem is BurnableItem)
            {
                return ((BurnableItem)LocalPlayer.ActiveBurnableItem).;
            }*/
            // flash light on weapons or itself
            if (item._id == 51  ||
                (item._id == 79 && LocalPlayer.Inventory.Owns(283)) ||
                (item._id == 279 && LocalPlayer.Inventory.Owns(287)) ||
                (item._id == 261 && LocalPlayer.Inventory.Owns(288)) ||
                (item._id == 230 && LocalPlayer.Inventory.Owns(289)))
            {
                return LocalPlayer.Stats.BatteryCharge / 100f;
            }
            // hairspray
            if (item._id == 291)
            {
                return LocalPlayer.Stats.hairSprayFuel.CurrentFuel / LocalPlayer.Stats.hairSprayFuel.MaxFuelCapacity;
            }
            return float.NaN;
        }

        internal static bool IsInHand(this Item item, bool rightElseLeft)
        {
            return LocalPlayer.Inventory.HasInSlot(rightElseLeft ? Item.EquipmentSlot.RightHand : Item.EquipmentSlot.LeftHand, item._id);
        }

        internal static bool IsEquippingItem(this Item item)
        {
            var equippingHand = new bool[] {
                LocalPlayer.Inventory.IsSlotLocked(Item.EquipmentSlot.LeftHand),
                LocalPlayer.Inventory.IsSlotLocked(Item.EquipmentSlot.RightHand)
            };

            if (!equippingHand[0] && !equippingHand[1])
                return false;

            var hand = default(Item.EquipmentSlot);
            try {
                hand = equippingHand[0] ? Item.EquipmentSlot.LeftHand : (equippingHand[1] ? Item.EquipmentSlot.RightHand : throw new System.Exception());
            } catch {
                return false;
            }

            return !LocalPlayer.Inventory.IsSlotNextEmpty(hand) && LocalPlayer.Inventory.HasInNextSlot(hand, item._id);
        }

        internal static bool Owns(this Item item)
        {
            return LocalPlayer.Inventory.Owns(item._id) ||
                   item.IsInHand(true) ||
                   item.IsInHand(false) ||
                   item.IsEquippingItem();
        }
    }
}
