using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("BaseLevel.tscn")]
public partial class BaseLevel
{
    [Signal]
    public delegate void ChangeLevel(string nextLevel);

    protected Inventory.InventoryConfig Resources = new Inventory.InventoryConfig();

    [Export]
    public uint DigPower = 1;
    private long money;

    public long Money
    {
        get => money;
        set
        {
            money = value;
            if (this.IsInsideTree())
            {
                this.bagMoney.Text = $"{money} coins";
            }
        }
    }

    public Stamina Stamina => this.stamina;

    public virtual void InitMap(uint maxNumberOfTurns, uint inventorySlots, uint digPower)
    {
        foreach (int id in this.loot.TileSet.GetTilesIds())
        {
            var tex = this.loot.TileSet.TileGetTexture(id);
            this.Resources.SlotConfigs.Add(id, new Inventory.InventorySlotConfig { Texture = tex });
            GD.Print($"Add loot {id} to inventory.");
        }

        this.bagInventory.Config = Resources;
        this.bagInventory.Size = inventorySlots;

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
        this.bagInventory.Connect(nameof(Inventory.UseItem), this, nameof(InventoryUseItem));

        this.AddToGroup(Groups.LevelScene);

        this.Money = this.money;
    }

    protected void InventoryUseItem(InventorySlot slot)
    {
        var tileId = slot.ItemId;
        if (LootDefinition.KnownLoot[(tileId, 0, 0)].UseAction != null)
        {
            LootDefinition.KnownLoot[(tileId, 0, 0)].UseAction(this);
            slot.TryAddItem(slot.ItemId, -1);
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
            TryLayer(this.fog, pos, FogDefinition.KnownFog) &&
            TryLayer(this.blocks, pos, BlocksDefinition.KnownBlocks) &&
            TryLayer(this.loot, pos, LootDefinition.KnownLoot) &&
            TryLayer(this.constructions, pos, ConstructionsDefinition.KnownConstructions) &&
            TryLayer(this.floor, pos, FloorDefinition.KnownFloors);

        if (result)
        {
            GD.Print($"Clicked outside of the map at {pos}.");
        }
    }

    private bool TryLayer<T>(TileMap map, Vector2 pos, Dictionary<ValueTuple<int, int, int>, T> knownActions) where T : IActionDefinition
    {
        var cell = map.GetCellv(pos);

        if (cell == -1)
        {
            return true;
        }

        var cellTile = map.GetCellAutotileCoord((int)pos.x, (int)pos.y);
        var key = (cell, (int)cellTile.x, (int)cellTile.y);

        if (!knownActions.ContainsKey(key))
        {
            GD.PrintErr($"Unknown key {key} in knownActions.");
            return false;
        }
        knownActions[key].ClickAction.Invoke(this, pos);
        return false;
    }

    private void ShowInventoryPopup()
    {
        this.bagInventoryPopup.Show();
    }

    public async Task<bool> ShowQuestPopup(string description, ValueTuple<ValueTuple<int, int, int>, uint>[] requirements, ValueTuple<ValueTuple<int, int, int>, uint>[] rewards)
    {
        var inventory = this.bagInventory;

        this.questRequirements.Content = description;
        this.requirementsList.ClearChildren();

        var isEnough = true;
        foreach (var req in requirements)
        {
            var lootId = req.Item1.Item1;
            this.requirementsList.AddChild(new TextureRect
            {
                Texture = inventory.Config.SlotConfigs[lootId].Texture
            });

            var existing = inventory.GetItemCount(lootId);

            this.requirementsList.AddChild(new Label
            {
                Text = $"x {existing} / {req.Item2}"
            });

            isEnough = isEnough && existing >= req.Item2;
        }

        this.questRequirements.AllowYes = isEnough;
        this.questRequirements.Show();

        if (!isEnough)
        {
            return false;
        }

        this.questRequirements.Show();
        var decision = (bool)(await ToSignal(this.questRequirements, nameof(CustomConfirmPopup.ChoiceMade))).GetValue(0);
        if (!decision)
        {
            return false;
        }

        var items = inventory.GetItems().ToList();
        var success = true;
        foreach (var req in requirements)
        {
            var lootId = req.Item1.Item1;

            var result = inventory.TryRemoveItems(lootId, req.Item2);
            if (result != 0)
            {
                success = false;
            }
        }

        foreach (var reward in rewards)
        {
            var lootId = reward.Item1.Item1;

            var result = inventory.TryAddItem(lootId, reward.Item2);
            if (result != 0)
            {
                success = false;
            }
        }

        if (!success)
        {
            inventory.ClearItems();
            var result = inventory.TryAddItems(items);
            if (result.Any())
            {
                GD.PrintErr($"Cant restore inventory status!!!");
            }
        }

        return success;
    }

    public virtual void CustomConstructionClickedAsync(Vector2 pos)
    {
        GD.PrintErr($"Clicked on a custom construction with no action set at {pos} for {this.Name}");
    }

    public virtual void CustomBlockClicked(Vector2 pos)
    {
        GD.Print($"Clicked on a block at {pos}, no custom action defined, dig it.");

        var blocksCell = this.blocks.GetCellv(pos);
        var blocksCellTile = this.blocks.GetCellAutotileCoord((int)pos.x, (int)pos.y);

        if (this.stamina.CurrentNumberOfTurns == 0)
        {
            return;
        }

        this.stamina.CurrentNumberOfTurns--;

        var metaName = $"HP_{pos}";

        if (!this.blocks.HasMeta(metaName))
        {
            this.blocks.SetMeta(metaName, BlocksDefinition.KnownBlocks[(blocksCell, (int)blocksCellTile.x, (int)blocksCellTile.y)].HP);
        }

        var currentHp = (int)this.blocks.GetMeta(metaName);

        if (currentHp == 0)
        {
            return;
        }

        if (currentHp > this.DigPower)
        {
            this.blocks.SetMeta(metaName, currentHp - this.DigPower);
            return;
        }

        this.blocks.SetMeta(metaName, null);
        this.blocks.SetCellv(pos, -1);
        this.UnFogCell(pos);
    }

    protected void UnFogCell(Vector2 cell)
    {
        if (
            this.fog.GetCellv(cell) != -1 ||  // Unfog should be started from already unfoged cell
            this.blocks.GetCellv(cell) != -1)  // Can start unfog if the block is not yet removed
        {
            return;
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

            if (this.blocks.GetCellv(cell) != -1)  // Blocks are not removed from the cell
            {
                continue;
            }

            queue.Enqueue(cell + Vector2.Down);
            queue.Enqueue(cell + Vector2.Left);
            queue.Enqueue(cell + Vector2.Up);
            queue.Enqueue(cell + Vector2.Right);
        }
    }

    public virtual void CustomLootClickedAsync(Vector2 pos)
    {
        GD.Print($"Clicked on a loot at {pos}, no custom action defined, put to inventory.");

        var lootId = this.loot.GetCellv(pos);

        if (this.bagInventory.TryAddItem(lootId, 1) == 0)
        {
            this.loot.SetCellv(pos, -1);
        }
    }
}
