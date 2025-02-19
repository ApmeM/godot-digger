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

    protected Dictionary<int, InventorySlot.InventorySlotConfig> Resources = new Dictionary<int, InventorySlot.InventorySlotConfig>();

    public Header Stamina => this.header;

    public Random random = new Random();

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
        this.bagInventory.Size = 3; // ToDo: 

        this.CharacteristicsChanged();

        this.AddToGroup(Groups.LevelScene);
        ReFogMap();
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
        var moveDone = false;
        foreach (var definition in BlocksDefinition.KnownBlocks)
        {
            if (definition.Value.MoveDelay <= 0)
            {
                continue;
            }

            var usedells = blocks.GetUsedCellsById(definition.Key.Item1);
            foreach (Vector2 pos in usedells)
            {
                var metaName = $"Move_{pos}";

                if (!this.blocks.HasMeta(metaName))
                {
                    this.blocks.SetMeta(metaName, random.NextDouble() * definition.Value.MoveDelay);
                }

                if ((float)this.blocks.GetMeta(metaName) >= 0)
                {
                    this.blocks.SetMeta(metaName, (float)this.blocks.GetMeta(metaName) - delta);
                    continue;
                }

                this.blocks.SetMeta(metaName, definition.Value.MoveDelay);

                var possibleMoves = new Vector2[] { Vector2.Down, Vector2.Left, Vector2.Up, Vector2.Right }
                    .Select(dir => pos + dir)
                    .Where(cell => definition.Value.MoveFloor.Contains((this.floor.GetCell((int)cell.x, (int)cell.y), 0, 0)))
                    .Where(cell => this.blocks.GetCell((int)cell.x, (int)cell.y) == -1)
                    .Where(cell => this.loot.GetCell((int)cell.x, (int)cell.y) == -1)
                    .ToList();

                if (possibleMoves.Count <= 0)
                {
                    continue;
                }

                var move = possibleMoves[random.Next(possibleMoves.Count)];
                this.blocks.SetCellv(move, this.blocks.GetCellv(pos), autotileCoord: this.blocks.GetCellAutotileCoord((int)pos.x, (int)pos.y));
                this.loot.SetCellv(move, this.loot.GetCellv(pos), autotileCoord: this.loot.GetCellAutotileCoord((int)pos.x, (int)pos.y));
                this.blocks.SetCellv(pos, -1);
                this.loot.SetCellv(pos, -1);

                var cellString = pos.ToString();
                foreach (var meta in this.blocks.GetMetaList().Where(a => a.EndsWith(cellString)))
                {
                    var metaString = meta.Substring(0, meta.Length - cellString.Length);
                    this.blocks.SetMeta(metaString + move, this.blocks.GetMeta(meta));
                    this.blocks.SetMeta(meta, null);
                }

                moveDone = true;
            }
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

    private void CharacteristicsChanged()
    {
        var character = new Character();
        this.equipmentInventory.ApplyEquipment(character);
        this.header.ApplyBuffs(character);
        this.header.Character = character;
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
            TryLayer(this.fog, pos, FogDefinition.KnownFog, new HashSet<int> { 2, 1 }) &&
            TryLayer(this.blocks, pos, BlocksDefinition.KnownBlocks, new HashSet<int> { -1 }) &&
            TryLayer(this.loot, pos, LootDefinition.KnownLoot, new HashSet<int> { -1 }) &&
            TryLayer(this.constructions, pos, ConstructionsDefinition.KnownConstructions, new HashSet<int> { -1 }) &&
            TryLayer(this.floor, pos, FloorDefinition.KnownFloors, new HashSet<int> { -1 });

        if (result)
        {
            GD.Print($"Clicked outside of the map at {pos}.");
        }
    }

    private bool TryLayer<T>(TileMap map, Vector2 pos, Dictionary<ValueTuple<int, int, int>, T> knownActions, HashSet<int> ignorableCells) where T : IActionDefinition
    {
        var cell = map.GetCellv(pos);

        if (ignorableCells.Contains(cell))
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
                .Union(rewards.Select(a => (a.Item1.Item1, (int)a.Item2))));

        return success;
    }

    public virtual void CustomBlockClicked(Vector2 pos)
    {
        GD.Print($"Clicked on a block at {pos}, no custom action defined, dig it.");

        var blocksCell = this.blocks.GetCellv(pos);
        var blocksCellTile = this.blocks.GetCellAutotileCoord((int)pos.x, (int)pos.y);

        if (!header.Character.CanDig)
        {
            return;
        }

        if (this.header.CurrentStamina == 0)
        {
            return;
        }
        var definition = BlocksDefinition.KnownBlocks[(blocksCell, (int)blocksCellTile.x, (int)blocksCellTile.y)];

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

        var metaName = $"HP_{pos}";

        if (!this.blocks.HasMeta(metaName))
        {
            this.blocks.SetMeta(metaName, definition.HP);
        }

        var enemyAttack = definition.Attack;
        if (enemyAttack > 0)
        {
            if (enemyAttack > this.header.CurrentHp)
            {
                floatingTextManager.ShowValueDelayed(currentFloatingsDelay, (-this.header.CurrentHp).ToString(), this.blocks.MapToWorld(pos) + this.blocks.CellSize / 2, new Color(1, 0, 0));
                currentFloatingsDelay += floatingDelay;
                this.header.CurrentHp = 0;
                var buff = this.header.AddBuff(Buff.Dead);
                floatingTextManager.ShowValueDelayed(currentFloatingsDelay, (Control)buff.Duplicate(), this.blocks.MapToWorld(pos) + this.blocks.CellSize / 2);
                currentFloatingsDelay += floatingDelay;
            }
            else
            {
                this.header.CurrentHp -= (uint)enemyAttack;
                floatingTextManager.ShowValueDelayed(currentFloatingsDelay, (-enemyAttack).ToString(), this.blocks.MapToWorld(pos) + this.blocks.CellSize / 2, new Color(1, 0, 0));
                currentFloatingsDelay += floatingDelay;
            }
        }

        var spawnBlock = definition.SpawnBlock;
        if (spawnBlock.Item1 >= 0)
        {
            var possibleMoves = new Vector2[] { Vector2.Down, Vector2.Left, Vector2.Up, Vector2.Right }
                .Select(dir => pos + dir)
                .Where(cell => this.blocks.GetCell((int)cell.x, (int)cell.y) == -1)
                .Where(cell => this.loot.GetCell((int)cell.x, (int)cell.y) == -1)
                .ToList();

            if (possibleMoves.Count > 0)
            {
                var move = possibleMoves[random.Next(possibleMoves.Count)];
                this.blocks.SetCellv(move, spawnBlock.Item1, autotileCoord: new Vector2(spawnBlock.Item2, spawnBlock.Item3));
            }
        }

        var currentHp = (int)this.blocks.GetMeta(metaName);

        var digPower = this.header.Character.DigPower;
        if (currentHp > digPower)
        {
            floatingTextManager.ShowValueDelayed(currentFloatingsDelay, (-digPower).ToString(), this.blocks.MapToWorld(pos) + this.blocks.CellSize / 2, new Color(1, 1, 0));
            currentFloatingsDelay += floatingDelay;
            this.blocks.SetMeta(metaName, currentHp - digPower);
            return;
        }

        floatingTextManager.ShowValueDelayed(currentFloatingsDelay, (-currentHp).ToString(), this.blocks.MapToWorld(pos) + this.blocks.CellSize / 2, new Color(1, 1, 0));
        currentFloatingsDelay += floatingDelay;
        foreach (var meta in this.blocks.GetMetaList().Where(a => a.EndsWith(pos.ToString())))
        {
            this.blocks.SetMeta(meta, null);
            GD.PrintErr($"Remove meta from {meta}");
        }
        this.blocks.SetMeta(metaName, null);
        this.blocks.SetCellv(pos, -1);

        UnFogCell(pos + Vector2.Down);
        UnFogCell(pos + Vector2.Left);
        UnFogCell(pos + Vector2.Right);
        UnFogCell(pos + Vector2.Up);
    }

    protected void ReFogMap()
    {
        foreach (Vector2 cell in this.fog.GetUsedCellsById(Fog.NoFog.Item1))
        {
            this.fog.SetCellv(cell, Fog.Basic.Item1);
        }

        foreach (Vector2 pos in this.fog.GetUsedCellsById(Fog.UnfogStart.Item1))
        {
            UnFogCell(pos + Vector2.Down);
            UnFogCell(pos + Vector2.Left);
            UnFogCell(pos + Vector2.Right);
            UnFogCell(pos + Vector2.Up);
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

            if (this.blocks.GetCellv(cell) != -1 && BlocksDefinition.KnownBlocks[(this.blocks.GetCellv(cell), 0, 0)].FogBlocker)  // Blocks are not removed from the cell
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

        if (this.bagInventory.TryChangeCount(lootId, 1) == 0)
        {
            this.loot.SetCellv(pos, -1);
        }
    }

    public void AddBuff(Buff staminaRegen)
    {
        this.header.AddBuff(staminaRegen);
    }
}
