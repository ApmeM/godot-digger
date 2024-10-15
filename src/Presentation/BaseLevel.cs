using Godot;

[SceneReference("BaseLevel.tscn")]
public partial class BaseLevel
{
    private CellDefinition[,] CurrentMap = new CellDefinition[10, 16];

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
        this.FillCurrentMap();
    }

    public void FillCurrentMap()
    {
        for (var x = 0; x < 9; x++)
        {
            for (var y = 0; y < 15; y++)
            {
                var cell = new Vector2(x, y);
                var set = this.map.GetCellv(cell);
                if (set >= 0)
                {
                    var tile = this.map.GetCellAutotileCoord(x, y);
                    if (CellDefinition.KnownCells.ContainsKey(tile))
                    {
                        this.CurrentMap[x, y] = CellDefinition.KnownCells[tile].Clone();
                    }
                    else
                    {
                        GD.PrintErr($"Unkonwn cell: {tile}");
                    }
                }
            }
        }
    }
}
