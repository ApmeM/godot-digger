using System.Linq;
using Godot;

[SceneReference("BuffContainer.tscn")]
public partial class BuffContainer
{
    private BuffsListData slotData = new BuffsListData();
    public BuffsListData SlotData
    {
        get => slotData;
        set
        {
            if (slotData == value)
            {
                return;
            }
            this.slotData.BuffsChanged -= this.RefreshFromDump;
            slotData = value;
            this.slotData.BuffsChanged += this.RefreshFromDump;
            this.RefreshFromDump();
        }
    }

    public override void _Ready()
    {
        base._Ready();

        this.slotData.BuffsChanged += this.RefreshFromDump;
        RefreshFromDump();
    }

    private void RefreshFromDump()
    {
        this.RemoveChildren();
        foreach (var buff in SlotData.Buffs)
        {
            var buffInstance = Instantiator.CreateBuff(buff.Name);
            buffInstance.Connect(CommonSignals.Pressed, this, nameof(BuffClicked), new Godot.Collections.Array { buffInstance });
            buffInstance.buffData = buff;
            this.AddChild(buffInstance);
        }
    }

    private void BuffClicked(BaseBuff buff)
    {
        this.buffDescriptionLabel.Text = buff.Description;
        this.buffPopup.Show();
    }
}
