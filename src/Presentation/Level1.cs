using System;
using GodotDigger.LevelSelector;

namespace GodotDigger.Levels
{
    public class Level1 : ILevelToSelect
    {
        private readonly Random r = new Random();
        public string Name => "Level 1";

        public void Init(Game game)
        {
            game.InitMap(game.Level1);
        }
    }
}
