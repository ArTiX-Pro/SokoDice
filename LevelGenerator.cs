using System.Collections.Generic;
using Com.IsartDigital.Sokoban.SpecialFeature;
using Godot;

namespace Com.IsartDigital.Sokoban
{
    public static class LevelGenerator
    {
        private static readonly Godot.Collections.Dictionary<char, PackedScene> sceneAccordingToChar = new Godot.Collections.Dictionary<char, PackedScene>
        {
            { '#',  ResourceLoader.Load<PackedScene>("res://Scenes/GameObjects/Wall.tscn") }, // Wall scene
            { '@',  ResourceLoader.Load<PackedScene>("res://Scenes/GameObjects/Moveable/Player.tscn")}, // Player scene
            { '$',  ResourceLoader.Load<PackedScene>("res://Scenes/SpecialFeature/CubeSF.tscn")}, // Dice scene
            { '&',  ResourceLoader.Load<PackedScene>("res://Scenes/SpecialFeature/CasinoBlock.tscn")}, // Casino Block scene
            { '.',  ResourceLoader.Load<PackedScene>("res://Scenes/GameObjects/Target.tscn")}, // Target scene
            { ' ',  null } // Empty emplacment
        };

        public static int NbLevel => levels.Count;

        private const string LEVELS_FILE_PATH = "res://SavedDatas/";
        private const string LEVELS_FILE_NAME = "Levels.json";

        private static List<Level> levels;

        static LevelGenerator()
        {
            levels = JsonReader.LoadData<List<Level>>(LEVELS_FILE_PATH + LEVELS_FILE_NAME);
        }

        public static void GenerateLevel(int pLevelIndex)
        {
            Level lLevel = levels[pLevelIndex];

            List<List<Node2D>> lTiles = new List<List<Node2D>>();

            string[] lMap = lLevel.Map;
            int lNbLine = lMap.Length;
            Node2D lTile;

            int lStartingFaceIndex = 0;
            int lX;
            int lLineLength;
            string lLine;
            for (int lY = 0; lY < lNbLine; lY++)
            {
                lTiles.Add(new List<Node2D>());
                lLine = lMap[lY];
                lLineLength = lLine.Length;
                for (lX = 0; lX < lLineLength; lX++)
                {
                    lTile = sceneAccordingToChar[lLine[lX]]?.Instantiate<Node2D>();
                    lTiles[lY].Add(lTile);

                    if (lTile is CubeSF)
                    {
                        (lTile as CubeSF).startingFace = lLevel.StartingFaces[lStartingFaceIndex];
                        lStartingFaceIndex++;
                    }
                    if (lTile is CasinoBlock lCasino) lCasino.Load(lLevel.CasinoBlockT);
                }
            }

            Main.GetInstance().CreateLevel(
                    pLevelIndex, lTiles, lLevel.Par, lLevel.CreatorName, lLevel.ZoomLevel);
        }

        public static void UpdateLevelPar(int pLevelIndex, int pStep)
        {
            levels[pLevelIndex].Par = pStep;
            JsonReader.WriteData(LEVELS_FILE_PATH, LEVELS_FILE_NAME, levels);
        }
    }
}
