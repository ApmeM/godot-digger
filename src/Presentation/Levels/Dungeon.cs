using System;
using GodotDigger.LevelSelector;

namespace GodotDigger.Levels
{
    public class Dungeon : ILevelToSelect
    {
        private readonly Random r = new Random();
        public string Name => "Dungeon";

        public void Init(Game game)
        {
            game.InitMap();
        }
    }
}
