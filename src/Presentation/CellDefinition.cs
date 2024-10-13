using System.Collections.Generic;
using Godot;

public partial class Game
{
    public class CellDefinition
    {
        public static Vector2 Path = new Vector2(1, 0);
        public static Vector2 Wood = new Vector2(13, 0);
        public static Vector2 Wall = new Vector2(4, 0);
        public static Vector2 Stairs = new Vector2(7, 0);

        public static Dictionary<Vector2, CellDefinition> KnownCells = new Dictionary<Vector2, CellDefinition>{
            { Path, new CellDefinition{HP = 0, Clickable = false} },
            { Wood, new CellDefinition{HP = 1, Clickable = true} },
            { Wall, new CellDefinition{HP = 0, Clickable = false} },
            { Stairs, new CellDefinition{HP = 0, Clickable = false} },
        };

        public int HP;
        public bool Clickable;

        public CellDefinition Clone()
        {
            return new CellDefinition
            {
                HP = this.HP,
                Clickable = this.Clickable
            };
        }
    }
}
