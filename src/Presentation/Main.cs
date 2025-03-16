using System;
using System.Collections.Generic;
using Godot;
using GodotDigger.Presentation.Utils;
using Newtonsoft.Json;

[SceneReference("Main.tscn")]
public partial class Main
{
    private LevelData CurrentSave = new LevelData();

    [Export]
    public TileSet LootTileSet;

    public Header HeaderControl => this.header;

    protected Dictionary<int, InventorySlot.InventorySlotConfig> Resources = new Dictionary<int, InventorySlot.InventorySlotConfig>();

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        // For debug purposes all achievements can be reset
        // this.di.localAchievementRepository.ResetAchievements();

        ChangeLevel("Level1");

        this.header.Connect(nameof(Header.InventoryButtonClicked), this, nameof(ShowInventoryPopup));
        this.bagInventory.Connect(nameof(Inventory.UseItem), this, nameof(InventoryUseItem));
        this.bagInventory.Connect(nameof(Inventory.DragOnAnotherItemType), this, nameof(InventoryTryMergeItems));
        this.bagSlot.Connect(nameof(InventorySlot.ItemCountChanged), this, nameof(BagChanged));
        this.equipmentInventory.Connect(nameof(EquipmentInventory.ItemCountChanged), this, nameof(EquipmentChanged));
        this.header.Connect(nameof(Header.BuffsChanged), this, nameof(BuffsChanged));

        foreach (int id in this.LootTileSet.GetTilesIds())
        {
            var tex = this.LootTileSet.TileGetTexture(id);
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

    private void ShowInventoryPopup()
    {
        this.bagInventoryPopup.Show();
    }

    public void ChangeLevel(string nextLevel)
    {
        if (this.gamePosition.GetChildCount() > 0)
        {
            UpdateCurrentDump();
        }
        this.gamePosition.RemoveChildren();
        var levelScene = ResourceLoader.Load<PackedScene>($"res://Presentation/levels/{nextLevel}.tscn");
        var game = levelScene.Instance<BaseLevel>();
        game.HeaderControl = this.header;
        game.BagInventory = this.bagInventory;
        game.BagInventoryPopup = this.bagInventoryPopup;
        game.Resources = this.Resources;
        game.Connect(nameof(BaseLevel.ChangeLevel), this, nameof(ChangeLevel));
        this.gamePosition.AddChild(game);
        CurrentSave.CurrentLevel = game.Name;
        LoadCurrentDump();

        CharacteristicsChanged();
    }

    private void LoadCurrentDump()
    {
        BaseLevel game = (BaseLevel)this.gamePosition.GetChild(0);
        game.LoadLevelDump(CurrentSave.Levels.ContainsKey(game.Name) ? CurrentSave.Levels[game.Name] : null);
        game.HeaderControl.LoadHeaderDump(CurrentSave.Header);
        this.LoadInventoryDump(CurrentSave.Inventory);
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

    private void UpdateCurrentDump()
    {
        BaseLevel prevLevel = (BaseLevel)this.gamePosition.GetChild(0);
        CurrentSave.Levels[prevLevel.Name] = prevLevel.GetLevelDump();
        CurrentSave.Header = prevLevel.HeaderControl.GetHeaderDump();
        CurrentSave.Inventory = this.GetInventoryDump();
    }

    private InventoryDump GetInventoryDump()
    {
        return new InventoryDump
        {
            Bag = this.bagSlot.GetItem(),
            Equipment = this.equipmentInventory.GetItems(),
            Inventory = this.bagInventory.GetItems(),
        };
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if ((KeyList)keyEvent.Scancode == KeyList.F2)
            {
                Save("quicksave");
            }

            if ((KeyList)keyEvent.Scancode == KeyList.F3)
            {
                Load("quicksave");
            }
        }
    }

    public void Save(string name)
    {
        UpdateCurrentDump();

        var f = new File();
        f.Open($"user://SaveLevel_{name}.dat", File.ModeFlags.Write);
        f.StorePascalString(JsonConvert.SerializeObject(this.CurrentSave, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        }));
        f.Close();

        ((BaseLevel)this.gamePosition.GetChild(0)).ShowPopup("Saved.");
    }

    public void Load(string name)
    {
        this.gamePosition.RemoveChildren();
        var f = new File();
        if (!f.FileExists($"user://SaveLevel_{name}.dat"))
        {
            this.CurrentSave = new LevelData();
            ChangeLevel("Level1");
            return;
        }

        f.Open($"user://SaveLevel_{name}.dat", File.ModeFlags.Read);
        this.CurrentSave = JsonConvert.DeserializeObject<LevelData>(f.GetPascalString(), new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });
        f.Close();

        ChangeLevel(this.CurrentSave.CurrentLevel);

        ((BaseLevel)this.gamePosition.GetChild(0)).ShowPopup("Loaded.");
    }

    public void ConfigureInventory(Inventory newInventory)
    {
        newInventory.GetParent().RemoveChild(newInventory);
        this.differentInventoriesContainer.AddChild(newInventory);
    }
}
