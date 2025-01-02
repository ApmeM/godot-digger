using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("BaseLevel.tscn")]
public partial class BaseLevel
{
    [Signal]
    public delegate void ExitDungeon(List<Loot> resources);

    [Signal]
    public delegate void ChangeLevel(string nextLevel, List<Loot> resources);

    [Export]
    public List<Texture> Resources = new List<Texture>();

    [Export]
    public uint DigPower = 1;

    public Stamina Stamina => this.stamina;

    public void InitMap(uint maxNumberOfTurns, uint inventorySlots, uint digPower)
    {
        this.inventory.Resources = Resources;
        this.inventory.Size = inventorySlots;

        this.stamina.MaxNumberOfTurns = maxNumberOfTurns;
        this.stamina.CurrentNumberOfTurns = this.stamina.MaxNumberOfTurns;

        this.DigPower = digPower;
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.draggableCamera.LimitLeft = (int)Math.Min(0, this.floor.GetUsedCells().Cast<Vector2>().Min(a => a.x) * this.floor.CellSize.x * this.floor.Scale.x);
        this.draggableCamera.LimitRight = (int)Math.Max(this.GetViewport().Size.x, this.floor.GetUsedCells().Cast<Vector2>().Max(a => a.x + 1) * this.floor.CellSize.x * this.floor.Scale.x);
        this.draggableCamera.LimitTop = (int)Math.Min(0, this.floor.GetUsedCells().Cast<Vector2>().Min(a => a.y) * this.floor.CellSize.y * this.floor.Scale.x);
        this.draggableCamera.LimitBottom = (int)Math.Max(this.GetViewport().Size.y, this.floor.GetUsedCells().Cast<Vector2>().Max(a => a.y + 1) * this.floor.CellSize.y * this.floor.Scale.x);

        // this.achievementNotifications.UnlockAchievement("MyFirstAchievement");

        this.inventoryButton.Connect(CommonSignals.Pressed, this, nameof(ShowInventoryPopup));
        this.inventory.Connect(nameof(Inventory.UseItem), this, nameof(InventoryUseItem));

        this.AddToGroup(Groups.LevelScene);
    }

    private void InventoryUseItem(InventorySlot slot)
    {
        if (LootDefinition.KnownLoot[(Loot)slot.ItemIndex].UseAction != null)
        {
            LootDefinition.KnownLoot[(Loot)slot.ItemIndex].UseAction(this);
            slot.TryAddItem(slot.ItemIndex, -1);
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
        if (!(@event is InputEventScreenTouch eventMouseButton) || eventMouseButton.Pressed)
        {
            return;
        }

        var newPos = this.GetViewport().CanvasTransform.AffineInverse() * eventMouseButton.Position;
        var pos = this.floor.WorldToMap(this.floor.ToLocal(newPos));

        var result =
            TryPassFog(pos) &&
            TryDigBlock(pos) &&
            UnFogCell(pos) &&
            TryGrabLoot(pos) &&
            TryChangeFloor(pos);
    }

    private bool TryChangeFloor(Vector2 pos)
    {
        var floorsCell = this.floor.GetCellv(pos);
        var floorsCellTile = this.floor.GetCellAutotileCoord((int)pos.x, (int)pos.y);

        if (floorsCell == -1)
        {
            return true;
        }

        FloorDefinition.KnownFloors[(Floor)floorsCellTile.x].ClickAction?.Invoke(this, pos);
        return true;
    }

    public void ExitDungeonClicked()
    {
        var resources = this.inventory.GetItems().Select(a => (Loot)a.Item1).ToList();
        this.inventory.ClearItems();
        this.EmitSignal(nameof(ExitDungeon), resources);
    }

    private bool TryGrabLoot(Vector2 pos)
    {
        var lootCell = this.loot.GetCellv(pos);
        var lootCellTile = this.loot.GetCellAutotileCoord((int)pos.x, (int)pos.y);

        if (lootCell == -1)
        {
            return true;
        }

        if (this.inventory.TryAddItem((int)lootCellTile.x, 1) != 0)
        {
            return false;
        }

        this.loot.SetCellv(pos, -1);
        return true;
    }

    private bool TryDigBlock(Vector2 pos)
    {
        var blocksCell = this.blocks.GetCellv(pos);
        var blocksCellTile = this.blocks.GetCellAutotileCoord((int)pos.x, (int)pos.y);

        if (blocksCell == -1)
        {
            return true;
        }

        if (this.stamina.CurrentNumberOfTurns == 0)
        {
            return false;
        }

        this.stamina.CurrentNumberOfTurns--;

        var metaName = $"HP_{pos}";

        if (!this.blocks.HasMeta(metaName))
        {
            this.blocks.SetMeta(metaName, BlocksDefinition.KnownBlocks[(Blocks)blocksCellTile.x].HP);
        }

        var currentHp = (int)this.blocks.GetMeta(metaName);

        if (currentHp > this.DigPower)
        {
            this.blocks.SetMeta(metaName, currentHp - this.DigPower);
            return false;
        }

        this.blocks.SetMeta(metaName, null);
        this.blocks.SetCellv(pos, -1);
        this.UnFogCell(pos);

        return false;
    }

    private bool TryPassFog(Vector2 pos)
    {
        var fogCell = this.fog.GetCellv(pos);
        var fogCellTile = this.fog.GetCellAutotileCoord((int)pos.x, (int)pos.y);
        return fogCell == -1;
    }

    private void ShowInventoryPopup()
    {
        this.customPopupInventory.Show();
    }

    private bool UnFogCell(Vector2 cell)
    {
        if (
            this.fog.GetCellv(cell) != -1 ||  // Unfog should be started from already unfoged cell
            this.blocks.GetCellv(cell) != -1 ||  // Can start unfog if the block is not yet removed
            this.floor.GetCellAutotileCoord((int)cell.x, (int)cell.y).x == 1) // Cant start unfog from the wall
        {
            return true;
        }

        this.fog.SetCellv(cell, -1);

        var queue = new Queue<Vector2>();
        queue.Enqueue(cell + Vector2.Down);
        queue.Enqueue(cell + Vector2.Left);
        queue.Enqueue(cell + Vector2.Up);
        queue.Enqueue(cell + Vector2.Right);

        while (queue.Any())
        {
            cell = queue.Dequeue();

            if (this.fog.GetCellv(cell) == -1)
            {
                continue;
            }

            this.fog.SetCellv(cell, -1);

            if (
                this.blocks.GetCellv(cell) != -1 ||  // Blocks are not removed from the cell
                this.floor.GetCellAutotileCoord((int)cell.x, (int)cell.y).x == 1) // The floor is the wall
            {
                continue;
            }

            queue.Enqueue(cell + Vector2.Down);
            queue.Enqueue(cell + Vector2.Left);
            queue.Enqueue(cell + Vector2.Up);
            queue.Enqueue(cell + Vector2.Right);
        }

        return true;
    }

    public virtual void ShowPopup(Vector2 pos)
    {
        GD.PrintErr($"Clicked on a sign with no text at {pos} for {this.Name}");
    }

    public virtual void ChangeLevelClicked(Vector2 pos)
    {
        GD.PrintErr($"Clicked on a change level with no level set at {pos} for {this.Name}");
    }
}
