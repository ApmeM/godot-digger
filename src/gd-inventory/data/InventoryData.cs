
using System;
using System.Collections.Generic;
using System.Linq;

public class InventoryData
{
    public event Action SlotsCountChanged;

    private InventorySlotData[] slots = new[]
    {
        new InventorySlotData(),
        new InventorySlotData(),
        new InventorySlotData(),
        new InventorySlotData()
    };
    public InventorySlotData[] Slots => this.slots;

    public uint SlotsCount
    {
        get => (uint)this.slots.Length;
        set
        {
            if (value == this.slots.Length)
            {
                return;
            }

            if (value < slots.Length)
            {
                // WARNING: order of slots will be changed.
                this.slots = this.slots.Where(a => a.HasItem()).ToArray();
            }

            // WARNING: may loose items. Should be handled outside
            Array.Resize(ref this.slots, (int)value);
            for (var i = 0; i < value; i++)
            {
                if (this.slots[i] == null)
                {
                    this.slots[i] = new InventorySlotData();
                }
            }
            SlotsCountChanged?.Invoke();
        }
    }

    public int TryChangeCount(string lootName, int countDiff)
    {
        if (countDiff == 0)
        {
            return 0;
        }

        foreach (var slot in Slots)
        {
            countDiff = slot.TryChangeCount(lootName, countDiff);
            if (countDiff == 0)
            {
                return 0;
            }
        }

        return countDiff;
    }

    public int GetItemCount(string lootName)
    {
        return Slots
            .Where(a => a.LootName == lootName)
            .Sum(a => a.ItemsCount);
    }

    public void ClearItems()
    {
        foreach (var cell in Slots)
        {
            cell.ClearItem();
        }
    }

    public bool TryChangeCountsOrCancel(IEnumerable<(string, int)> items)
    {
        var before = Slots.Select(a => new InventorySlotData(a)).ToArray();

        foreach (var item in items)
        {
            var result = this.TryChangeCount(item.Item1, item.Item2);
            if (result != 0)
            {
                this.slots = before;
                return false;
            }
        }

        return true;
    }

    public void SetItems(IEnumerable<(string, int)> items)
    {
        var i = 0;
        foreach (var item in items)
        {
            Slots[i].ForceSetCount(item.Item1, item.Item2);
            i++;
        }
    }
}
