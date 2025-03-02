using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrainAI.Pathfinding;
using Godot;
using GodotDigger.Presentation.Utils;
using Newtonsoft.Json;

[SceneReference("BaseLevel.tscn")]
public partial class BaseLevel : IUnweightedGraph<Vector2>
{
    [Signal]
    public delegate void ChangeLevel(string nextLevel);

    protected Dictionary<int, InventorySlot.InventorySlotConfig> Resources = new Dictionary<int, InventorySlot.InventorySlotConfig>();

    public Header Stamina => this.header;

    public Random random = new Random();

    private Vector2[] cardinalDirections = new Vector2[] { Vector2.Down, Vector2.Left, Vector2.Up, Vector2.Right };

    public TileMap FloorMap => this.floor;
    public TileMap BlocksMap => this.blocks;
    public TileMap LootMap => this.loot;
    public Header HeaderControl => this.header;
    public FloatingTextManager FloatingTextManagerControl => this.floatingTextManager;

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
        this.bagInventory.Size = 3;

        this.CharacteristicsChanged();

        foreach (Vector2 cell in this.blocks.GetUsedCells())
        {
            var tile = this.blocks.GetCellv(cell);
            var definition = BlocksDefinition.KnownBlocks[(tile, 0, 0)];
            this.blocks.SetMeta($"HP_{cell}", definition.HP);
            var loots = new List<int>();
            if (this.loot.GetCellv(cell) != -1)
            {
                loots.Add(this.loot.GetCellv(cell));
                this.loot.SetCellv(cell, -1);
            }
            this.blocks.SetMeta($"Loot_{cell}", loots);
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
        foreach (var definition in BlocksDefinition.KnownBlocks)
        {
            var usedells = blocks.GetUsedCellsById(definition.Key.Item1);
            foreach (Vector2 pos in usedells)
            {
                moveDone = definition.Value.OnTickAction?.Invoke(this, pos, delta) ?? false;
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
            floatingTextManager.ShowValue("Too tired", this.blocks.MapToWorld(pos) + this.blocks.CellSize / 2, new Color(0.60f, 0.85f, 0.91f));
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

        definition.OnClickAction?.Invoke(this, pos);

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

        var loots = (int[])this.blocks.GetMeta($"Loot_{pos}");

        foreach (var meta in this.blocks.GetMetaList().Where(a => a.EndsWith(pos.ToString())))
        {
            this.blocks.SetMeta(meta, null);
        }
        this.blocks.SetMeta(metaName, null);
        this.blocks.SetCellv(pos, -1);

        foreach (var loot in loots)
        {
            if (this.loot.GetCellv(pos) == -1 && this.blocks.GetCellv(pos) == -1)
            {
                this.loot.SetCellv(pos, loot);
                continue;
            }

            foreach (var dir in cardinalDirections)
            {
                if (this.loot.GetCellv(pos + dir) == -1 && this.blocks.GetCellv(pos + dir) == -1)
                {
                    this.loot.SetCellv(pos + dir, loot);
                    break;
                }
            }
        }

        foreach (var dir in cardinalDirections)
        {
            UnFogCell(pos + dir);
        }
    }

    protected void ReFogMap()
    {
        foreach (Vector2 cell in this.fog.GetUsedCellsById(Fog.NoFog.Item1))
        {
            this.fog.SetCellv(cell, Fog.Basic.Item1);
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

            if (this.blocks.GetCellv(cell) != -1 && BlocksDefinition.KnownBlocks[(this.blocks.GetCellv(cell), 0, 0)].FogBlocker)  // Blocks are not removed from the cell
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

    public void AddBuff(Buff staminaRegen)
    {
        this.header.AddBuff(staminaRegen);
    }

    public void GetNeighbors(Vector2 node, ICollection<Vector2> result)
    {
        if (this.blocks.GetCellv(node - Vector2.Down) == -1 && this.floor.GetCellv(node - Vector2.Down) != -1) result.Add(node - Vector2.Down);
        if (this.blocks.GetCellv(node - Vector2.Left) == -1 && this.floor.GetCellv(node - Vector2.Left) != -1) result.Add(node - Vector2.Left);
        if (this.blocks.GetCellv(node - Vector2.Right) == -1 && this.floor.GetCellv(node - Vector2.Right) != -1) result.Add(node - Vector2.Right);
        if (this.blocks.GetCellv(node - Vector2.Up) == -1 && this.floor.GetCellv(node - Vector2.Up) != -1) result.Add(node - Vector2.Up);
    }

    public virtual void Save()
    {
        var f = new File();
        f.Open($"user://SaveLevel_{this.Name}.dat", File.ModeFlags.Write);
        f.StorePascalString(JsonConvert.SerializeObject(this.floor.GetUsedCells().Cast<Vector2>().Select(a => (a, this.floor.GetCellv(a))).ToList()));
        f.StorePascalString(JsonConvert.SerializeObject(this.constructions.GetUsedCells().Cast<Vector2>().Select(a => (a, this.constructions.GetCellv(a))).ToList()));
        f.StorePascalString(JsonConvert.SerializeObject(this.loot.GetUsedCells().Cast<Vector2>().Select(a => (a, this.loot.GetCellv(a))).ToList()));
        f.StorePascalString(JsonConvert.SerializeObject(this.blocks.GetUsedCells().Cast<Vector2>().Select(a => (a, this.blocks.GetCellv(a))).ToList()));
        f.StorePascalString(JsonConvert.SerializeObject(this.fog.GetUsedCells().Cast<Vector2>().Select(a => (a, this.fog.GetCellv(a))).ToList()));
        f.StorePascalString(JsonConvert.SerializeObject(this.blocks.GetMetaList().Select(a => (a, JsonConvert.SerializeObject(this.blocks.GetMeta(a)))).ToList()));
        f.Close();

        f = new File();
        f.Open($"user://Invenory.dat", File.ModeFlags.Write);
        f.StorePascalString(JsonConvert.SerializeObject(this.equipmentInventory.GetItems()));
        f.StorePascalString(JsonConvert.SerializeObject(this.bagInventory.GetItems()));
        f.Close();

        this.header.Save();
    }

    public virtual void Load()
    {
        var f = new File();
        if (f.FileExists($"user://SaveLevel_{this.Name}.dat"))
        {
            this.floor.Clear();
            this.constructions.Clear();
            this.loot.Clear();
            this.blocks.Clear();
            this.fog.Clear();

            f.Open($"user://SaveLevel_{this.Name}.dat", File.ModeFlags.Read);
            JsonConvert.DeserializeObject<List<(Vector2, int)>>(f.GetPascalString()).ForEach(a => this.floor.SetCellv(a.Item1, a.Item2));
            JsonConvert.DeserializeObject<List<(Vector2, int)>>(f.GetPascalString()).ForEach(a => this.constructions.SetCellv(a.Item1, a.Item2));
            JsonConvert.DeserializeObject<List<(Vector2, int)>>(f.GetPascalString()).ForEach(a => this.loot.SetCellv(a.Item1, a.Item2));
            JsonConvert.DeserializeObject<List<(Vector2, int)>>(f.GetPascalString()).ForEach(a => this.blocks.SetCellv(a.Item1, a.Item2));
            JsonConvert.DeserializeObject<List<(Vector2, int)>>(f.GetPascalString()).ForEach(a => this.fog.SetCellv(a.Item1, a.Item2));
            JsonConvert.DeserializeObject<List<(string, string)>>(f.GetPascalString()).ForEach(a =>
                {
                    var name = a.Item1;
                    if (name.StartsWith("HP_"))
                    {
                        this.blocks.SetMeta(a.Item1, JsonConvert.DeserializeObject<uint>(a.Item2));
                    }
                    else
                    if (name.StartsWith("Loot_"))
                    {
                        this.blocks.SetMeta(a.Item1, JsonConvert.DeserializeObject<List<int>>(a.Item2));
                    }
                    else
                    if (name.StartsWith("Move_"))
                    {
                        this.blocks.SetMeta(a.Item1, JsonConvert.DeserializeObject<float>(a.Item2));
                    }
                });
            f.Close();
        }

        if (f.FileExists($"user://Invenory.dat"))
        {
            f.Open($"user://Invenory.dat", File.ModeFlags.Read);
            this.equipmentInventory.SetItems(JsonConvert.DeserializeObject<List<(int, int)>>(f.GetPascalString()));
            this.bagInventory.SetItems(JsonConvert.DeserializeObject<List<(int, int)>>(f.GetPascalString()));
            f.Close();
        }

        this.header.Load();
    }
}
