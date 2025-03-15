using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrainAI.Pathfinding;
using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("BaseLevel.tscn")]
public partial class BaseLevel : IUnweightedGraph<Vector2>
{
    [Signal]
    public delegate void ChangeLevel(string nextLevel);

    protected Dictionary<int, InventorySlot.InventorySlotConfig> Resources = new Dictionary<int, InventorySlot.InventorySlotConfig>();

    public readonly Vector2[] cardinalDirections = new Vector2[] { Vector2.Down, Vector2.Left, Vector2.Up, Vector2.Right };

    public TileMap FloorMap => this.floor;
    public TileMap BlocksMap => this.blocks;
    public TileMap LootMap => this.loot;
    public Header HeaderControl => this.header;
    public FloatingTextManager FloatingTextManagerControl => this.floatingTextManager;

    public Dictionary<Vector2, BlocksDefinition> Meta = new Dictionary<Vector2, BlocksDefinition>();

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.draggableCamera.LimitLeft = (int)Math.Min(0, this.floor.GetUsedCells().Cast<Vector2>().Min(a => a.x) * this.floor.CellSize.x * this.floor.Scale.x);
        this.draggableCamera.LimitRight = (int)Math.Max(this.GetViewport().Size.x, this.floor.GetUsedCells().Cast<Vector2>().Max(a => a.x + 1) * this.floor.CellSize.x * this.floor.Scale.x);
        this.draggableCamera.LimitTop = (int)Math.Min(0, this.floor.GetUsedCells().Cast<Vector2>().Min(a => a.y) * this.floor.CellSize.y * this.floor.Scale.x);
        this.draggableCamera.LimitBottom = (int)Math.Max(this.GetViewport().Size.y, this.floor.GetUsedCells().Cast<Vector2>().Max(a => a.y + 1) * this.floor.CellSize.y * this.floor.Scale.x);

        // this.achievementNotifications.UnlockAchievement("MyFirstAchievement");

        this.header.Connect(nameof(Header.InventoryButtonClicked), this, nameof(ShowInventoryPopup));
        this.bagInventory.Connect(nameof(Inventory.UseItem), this, nameof(InventoryUseItem));
        this.bagInventory.Connect(nameof(Inventory.DragOnAnotherItemType), this, nameof(InventoryTryMergeItems));
        this.bagSlot.Connect(nameof(InventorySlot.ItemCountChanged), this, nameof(BagChanged));
        this.equipmentInventory.Connect(nameof(EquipmentInventory.ItemCountChanged), this, nameof(EquipmentChanged));
        this.header.Connect(nameof(Header.BuffsChanged), this, nameof(BuffsChanged));

        foreach (int id in this.loot.TileSet.GetTilesIds())
        {
            var tex = this.loot.TileSet.TileGetTexture(id);
            var definition = LootDefinition.KnownLoot[(id, 0, 0)];
            this.Resources.Add(id, new InventorySlot.InventorySlotConfig
            {
                Texture = tex,
                MaxCount = definition.MaxCount,
                ItemType = (int)definition.ItemType
            });
        }
        this.equipmentInventory.Config = Resources;

        this.bagInventory.Config = Resources;

        this.bagSlot.AcceptedTypes.Add((int)ItemType.Bag);
        this.bagSlot.Config = Resources;

        this.CharacteristicsChanged();

        foreach (Vector2 cell in this.blocks.GetUsedCells())
        {
            var tile = this.blocks.GetCellv(cell);
            var definition = BlocksDefinition.KnownBlocks[(tile, 0, 0)].Clone();
            if (this.loot.GetCellv(cell) != -1)
            {
                definition.Loot.Add((this.loot.GetCellv(cell), 0, 0));
                this.loot.SetCellv(cell, -1);
            }

            definition.Group = this.groups.GetCellv(cell);
            this.groups.SetCellv(cell, -1);

            this.Meta[cell] = definition;
        }

        foreach (Vector2 cell in this.floor.GetUsedCells())
        {
            if (this.fog.GetCellv(cell) == -1)
            {
                this.fog.SetCellv(cell, Fog.Basic.Item1, autotileCoord: new Vector2(Fog.Basic.Item2, Fog.Basic.Item3));
            }
        }

        ReFogMap();

        this.AddToGroup(Groups.LevelScene);
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
        var moveDone = false;

        foreach (Vector2 cell in this.blocks.GetUsedCells())
        {
            var definition = this.Meta[cell];
            moveDone |= definition.OnTickMove(this, cell, delta);
        }

        foreach (var def in this.Meta.Where(a => a.Value.IsDead).ToList())
        {
            DoDead(def.Value, def.Key);
        }

        if (moveDone)
        {
            ReFogMap();
        }
    }

    private void BuffsChanged()
    {
        this.CharacteristicsChanged();
    }

    private void EquipmentChanged(InventorySlot slot, int itemId, int from, int to)
    {
        this.CharacteristicsChanged();
    }

    private async void BagChanged(int itemId, int from, int to)
    {
        await this.ToSignal(GetTree().CreateTimer(0.01f), CommonSignals.Timeout); // Hack to update bag size after all signals handled.
        this.CharacteristicsChanged();
    }

    private void CharacteristicsChanged()
    {
        var character = new Character();
        this.equipmentInventory.ApplyEquipment(character);
        if (this.bagSlot.ItemId >= 0)
        {
            var definition = LootDefinition.KnownLoot[(this.bagSlot.ItemId, 0, 0)];
            character.DigPower += (uint)definition.DigPower;
            character.MaxStamina += (uint)definition.NumberOfTurns;
            character.BagSlots += (uint)definition.AdditionalSlots;
        }
        this.header.ApplyBuffs(character);
        this.header.Character = character;

        this.bagInventory.Size = character.BagSlots;
    }

    protected void InventoryUseItem(InventorySlot slot)
    {
        var tileId = slot.ItemId;
        if (LootDefinition.KnownLoot[(tileId, 0, 0)].UseAction != null)
        {
            LootDefinition.KnownLoot[(tileId, 0, 0)].UseAction(this);
            slot.TryChangeCount(slot.ItemId, -1);
        }
    }
    protected void InventoryTryMergeItems(InventorySlot fromSlot, InventorySlot toSlot)
    {
        if (!LootDefinition.KnownLoot[(toSlot.ItemId, 0, 0)].MergeActions.ContainsKey((fromSlot.ItemId, 0, 0)))
        {
            return;
        }

        var mergeResult = LootDefinition.KnownLoot[(toSlot.ItemId, 0, 0)].MergeActions[(fromSlot.ItemId, 0, 0)];
        fromSlot.TryChangeCount(fromSlot.ItemId, -1);
        toSlot.TryChangeCount(toSlot.ItemId, -1);

        toSlot.TryChangeCount(mergeResult.Item1, 1);
    }

    private static Action<BaseLevel, Vector2> DoNothing = (level, pos) => { };
    private static Action<BaseLevel, Vector2> LootClicked = (level, pos) => { level.CustomLootClickedAsync(pos); };
    private static Action<BaseLevel, Vector2> BlockClicked = (level, pos) => { level.CustomBlockClicked(pos); };

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
            TryLayer(this.fog, pos, DoNothing, new HashSet<int> { 2, 1 }) &&
            TryLayer(this.blocks, pos, BlockClicked, new HashSet<int> { -1 }) &&
            TryLayer(this.loot, pos, LootClicked, new HashSet<int> { -1 }) &&
            TryLayer(this.constructions, pos, DoNothing, new HashSet<int> { -1 }) &&
            TryLayer(this.floor, pos, DoNothing, new HashSet<int> { -1 });

        if (result)
        {
            GD.Print($"Clicked outside of the map at {pos}.");
        }
    }

    private bool TryLayer(TileMap map, Vector2 pos, Action<BaseLevel, Vector2> clickAction, HashSet<int> ignorableCells)
    {
        var cell = map.GetCellv(pos);

        if (ignorableCells.Contains(cell))
        {
            return true;
        }

        clickAction(this, pos);
        return false;
    }

    private void ShowInventoryPopup()
    {
        this.bagInventoryPopup.Show();
    }

    public async void ShowPopup(string text)
    {
        this.signLabel.Text = text;
        signPopup.Show();
        await ToSignal(this.questRequirements, nameof(CustomPopup.PopupClosed));
    }

    public async Task<bool> ShowQuestPopup(string description, ValueTuple<ValueTuple<int, int, int>, uint>[] requirements, ValueTuple<ValueTuple<int, int, int>, uint>[] rewards)
    {
        var inventory = this.bagInventory;

        this.questRequirements.Content = description;
        this.requirementsList.RemoveChildren();

        var isEnough = true;
        foreach (var req in requirements)
        {
            var lootId = req.Item1.Item1;
            this.requirementsList.AddChild(new TextureRect
            {
                Texture = inventory.Config[lootId].Texture
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
        var decision = (bool)(await ToSignal(this.questRequirements, nameof(CustomConfirmPopup.ChoiceMade))).GetValue(0);

        if (!isEnough)
        {
            return false;
        }

        if (!decision)
        {
            return false;
        }

        var success = inventory.TryChangeCountsOrCancel(
            requirements
                .Select(a => (a.Item1.Item1, -(int)a.Item2))
                .Concat(rewards.Select(a => (a.Item1.Item1, (int)a.Item2))));

        return success;
    }

    public virtual void CustomBlockClicked(Vector2 pos)
    {
        GD.Print($"Clicked on a block at {pos}, no custom action defined, dig it.");

        if (!header.Character.CanDig)
        {
            return;
        }

        if (this.header.CurrentStamina == 0)
        {
            floatingTextManager.ShowValue("Too tired", this.blocks.MapToWorld(pos) + this.blocks.CellSize / 2, new Color(0.60f, 0.85f, 0.91f));
            return;
        }

        var definition = Meta[pos];

        if (definition.HP == 0)
        {
            // If block has no HP - it is a custom building / ally, no need to do anything here.
            return;
        }

        const float floatingDelay = 0.3f;
        var currentFloatingsDelay = 0f;

        floatingTextManager.ShowValueDelayed(currentFloatingsDelay, (-1).ToString(), this.blocks.MapToWorld(pos) + this.blocks.CellSize / 2, new Color(0.60f, 0.85f, 0.91f));
        currentFloatingsDelay += floatingDelay;

        this.header.CurrentStamina--;

        definition.OnClickMove(this, pos);
        if (definition.IsDead)
        {
            DoDead(definition, pos);
        }
    }

    private void DoDead(BlocksDefinition definition, Vector2 pos)
    {
        var loots = definition.Loot;

        // Clear block and its meta 
        this.blocks.SetCellv(pos, -1);
        this.Meta.Remove(pos);
        foreach (var meta in this.blocks.GetMetaList().Where(a => a.EndsWith(pos.ToString())))
        {
            this.blocks.SetMeta(meta, null);
        }

        // Drop loot
        foreach (var loot in loots)
        {
            if (this.loot.GetCellv(pos) == -1 && this.blocks.GetCellv(pos) == -1)
            {
                this.loot.SetCellv(pos, loot.Item1, autotileCoord: new Vector2(loot.Item2, loot.Item3));
                continue;
            }

            foreach (var dir in cardinalDirections)
            {
                if (this.loot.GetCellv(pos + dir) == -1 && this.blocks.GetCellv(pos + dir) == -1)
                {
                    this.loot.SetCellv(pos + dir, loot.Item1, autotileCoord: new Vector2(loot.Item2, loot.Item3));
                    break;
                }
            }
        }

        // Unfog nearby cells
        foreach (var dir in cardinalDirections)
        {
            UnFogCell(pos + dir);
        }
    }

    protected void ReFogMap()
    {
        foreach (Vector2 cell in this.fog.GetUsedCellsById(Fog.NoFog.Item1))
        {
            this.fog.SetCellv(cell, Fog.Basic.Item1, autotileCoord: new Vector2(Fog.Basic.Item2, Fog.Basic.Item3));
        }

        foreach (Vector2 pos in this.fog.GetUsedCellsById(Fog.UnfogStart.Item1))
        {
            foreach (var dir in cardinalDirections)
            {
                UnFogCell(pos + dir);
            }
        }
    }

    protected void UnFogCell(Vector2 cell)
    {
        var queue = new Queue<Vector2>();
        queue.Enqueue(cell);

        while (queue.Any())
        {
            cell = queue.Dequeue();

            var fog = this.fog.GetCellv(cell);

            if (fog == -1 || fog == Fog.NoFog.Item1 || fog == Fog.UnfogStart.Item1)
            {
                continue;
            }

            this.fog.SetCellv(cell, Fog.NoFog.Item1);

            if (this.blocks.GetCellv(cell) != -1 && !this.Meta[cell].NoFogBlocker)  // Blocks are not removed from the cell
            {
                continue;
            }

            foreach (var dir in cardinalDirections)
            {
                queue.Enqueue(cell + dir);
            }
        }
    }

    public virtual void CustomLootClickedAsync(Vector2 pos)
    {
        GD.Print($"Clicked on a loot at {pos}, no custom action defined, put to inventory.");

        var lootId = this.loot.GetCellv(pos);

        if (this.bagInventory.TryChangeCount(lootId, 1) == 0)
        {
            this.loot.SetCellv(pos, -1);
        }
    }

    public void GetNeighbors(Vector2 node, ICollection<Vector2> result)
    {
        if (this.blocks.GetCellv(node - Vector2.Down) == -1 && this.floor.GetCellv(node - Vector2.Down) != -1) result.Add(node - Vector2.Down);
        if (this.blocks.GetCellv(node - Vector2.Left) == -1 && this.floor.GetCellv(node - Vector2.Left) != -1) result.Add(node - Vector2.Left);
        if (this.blocks.GetCellv(node - Vector2.Right) == -1 && this.floor.GetCellv(node - Vector2.Right) != -1) result.Add(node - Vector2.Right);
        if (this.blocks.GetCellv(node - Vector2.Up) == -1 && this.floor.GetCellv(node - Vector2.Up) != -1) result.Add(node - Vector2.Up);
    }

    public virtual LevelDump GetLevelDump()
    {
        return new LevelDump
        {
            Floor = this.floor.GetUsedCells().Cast<Vector2>().Select(a => (a, this.floor.GetCellv(a))).ToList(),
            Constructions = this.constructions.GetUsedCells().Cast<Vector2>().Select(a => (a, this.constructions.GetCellv(a))).ToList(),
            Loot = this.loot.GetUsedCells().Cast<Vector2>().Select(a => (a, this.loot.GetCellv(a))).ToList(),
            Blocks = this.blocks.GetUsedCells().Cast<Vector2>().Select(a => (a, this.blocks.GetCellv(a))).ToList(),
            Fog = this.fog.GetUsedCells().Cast<Vector2>().Select(a => (a, this.fog.GetCellv(a))).ToList(),
            Meta = this.Meta.ToList(),
        };
    }

    public InventoryDump GetInventoryDump()
    {
        return new InventoryDump
        {
            Bag = this.bagSlot.GetItem(),
            Equipment = this.equipmentInventory.GetItems(),
            Inventory = this.bagInventory.GetItems(),
        };
    }

    public virtual void LoadLevelDump(LevelDump levelDump)
    {
        if (levelDump == null)
        {
            return;
        }

        this.floor.Clear();
        this.constructions.Clear();
        this.loot.Clear();
        this.blocks.Clear();
        this.fog.Clear();
        this.groups.Clear();

        levelDump.Floor.ForEach(a => this.floor.SetCellv(a.Item1, a.Item2));
        levelDump.Constructions.ForEach(a => this.constructions.SetCellv(a.Item1, a.Item2));
        levelDump.Loot.ForEach(a => this.loot.SetCellv(a.Item1, a.Item2));
        levelDump.Blocks.ForEach(a => this.blocks.SetCellv(a.Item1, a.Item2));
        levelDump.Fog.ForEach(a => this.fog.SetCellv(a.Item1, a.Item2));
        this.Meta = levelDump.Meta.ToDictionary(a => a.Key, a => a.Value);
    }
    public void LoadInventoryDump(InventoryDump inventoryDump)
    {
        this.bagSlot.ClearItem();
        this.equipmentInventory.ClearItems();
        this.bagInventory.ClearItems();

        if (inventoryDump == null)
        {
            return;
        }

        if (inventoryDump.Bag != default)
        {
            this.bagSlot.ForceSetCount(inventoryDump.Bag.Item1, inventoryDump.Bag.Item2);
        }

        if (inventoryDump.Equipment != null)
        {
            this.equipmentInventory.ForceSetItems(inventoryDump.Equipment);
        }

        if (inventoryDump.Inventory != null)
        {
            this.bagInventory.SetItems(inventoryDump.Inventory);
        }
    }
}
