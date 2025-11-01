using System.Collections.Generic;
using System.Linq;

public class EquipmentInventoryData
{
    public InventorySlotData Neck = new InventorySlotData(string.Empty, 0, new HashSet<ItemType> { ItemType.Neck });
    public InventorySlotData Helm = new InventorySlotData(string.Empty, 0, new HashSet<ItemType> { ItemType.Helm });
    public InventorySlotData Weapon = new InventorySlotData(string.Empty, 0, new HashSet<ItemType> { ItemType.Weapon });
    public InventorySlotData Chest = new InventorySlotData(string.Empty, 0, new HashSet<ItemType> { ItemType.Chest });
    public InventorySlotData Shield = new InventorySlotData(string.Empty, 0, new HashSet<ItemType> { ItemType.Shield });
    public InventorySlotData Ring1 = new InventorySlotData(string.Empty, 0, new HashSet<ItemType> { ItemType.Ring });
    public InventorySlotData Belt = new InventorySlotData(string.Empty, 0, new HashSet<ItemType> { ItemType.Belt });
    public InventorySlotData Ring2 = new InventorySlotData(string.Empty, 0, new HashSet<ItemType> { ItemType.Ring });
    public InventorySlotData Pants = new InventorySlotData(string.Empty, 0, new HashSet<ItemType> { ItemType.Pants });
    public InventorySlotData Boots = new InventorySlotData(string.Empty, 0, new HashSet<ItemType> { ItemType.Boots });

    public readonly InventorySlotData[] All;

    public EquipmentInventoryData()
    {
        this.All = new InventorySlotData[] { Neck, Helm, Weapon, Chest, Shield, Ring1, Belt, Ring2, Pants, Boots };
    }

    public List<InventorySlotData> GetItems()
    {
        return All.ToList();
    }

    public void ClearItems()
    {
        foreach (var slot in All)
        {
            if (!slot.HasItem())
            {
                continue;
            }

            slot.ClearItem();
        }
    }

    public void ForceSetItems(IEnumerable<(string, int)> items)
    {
        this.ClearItems();
        foreach (var item in items)
        {
            var loot = LootDefinition.LootByName[item.Item1];
            switch (loot.ItemType)
            {
                case ItemType.Neck: this.Neck.ForceSetCount(item.Item1, item.Item2); break;
                case ItemType.Helm: this.Helm.ForceSetCount(item.Item1, item.Item2); break;
                case ItemType.Weapon: this.Weapon.ForceSetCount(item.Item1, item.Item2); break;
                case ItemType.Chest: this.Chest.ForceSetCount(item.Item1, item.Item2); break;
                case ItemType.Shield: this.Shield.ForceSetCount(item.Item1, item.Item2); break;
                case ItemType.Ring:
                    {
                        if (!this.Ring1.HasItem())
                        {
                            this.Ring1.ForceSetCount(item.Item1, item.Item2);
                        }
                        else
                        {
                            this.Ring2.ForceSetCount(item.Item1, item.Item2);
                        }
                    }
                    break;
                case ItemType.Belt: this.Belt.ForceSetCount(item.Item1, item.Item2); break;
                case ItemType.Pants: this.Pants.ForceSetCount(item.Item1, item.Item2); break;
                case ItemType.Boots: this.Boots.ForceSetCount(item.Item1, item.Item2); break;
            }
        }
    }
}
