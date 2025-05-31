using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Vestige.Game
{
    static class ContentLoader
    {
        public static Texture2D PlayerHead;
        public static Texture2D PlayerTorso;
        public static Texture2D PlayerArm;
        public static Texture2D PlayerLegs;
        public static Texture2D[] TileTextures;
        public static Texture2D[] WallTextures;
        public static Texture2D[] ItemTextures;
        public static Texture2D[] EnemyTextures;
        public static Texture2D ItemSlotTexture;
        public static SpriteFont GameFont;
        public static Texture2D Cracks;
        public static Texture2D TreesBackground;
        public static Texture2D TreesFartherBackground;
        public static Texture2D TreesFarthestBackground;
        public static Texture2D MountainsBackground;
        public static Texture2D LiquidTexture;

        //Shaders
        public static Effect WaterShader;

        public static void Load(ContentManager content)
        {
            //TODO: probably change this to the exact amount when the game is finished
            string fullContentDirectory = GetFullContentPath(content);
            TileTextures = new Texture2D[200];
            WallTextures = new Texture2D[200];
            ItemTextures = new Texture2D[200];
            EnemyTextures = new Texture2D[200];

            PlayerHead = content.Load<Texture2D>("Assets/Textures/Player/PlayerHead");
            PlayerTorso = content.Load<Texture2D>("Assets/Textures/Player/PlayerTorso");
            PlayerArm = content.Load<Texture2D>("Assets/Textures/Player/PlayerArm");
            PlayerLegs = content.Load<Texture2D>("Assets/Textures/Player/PlayerLegs");
            ItemSlotTexture = content.Load<Texture2D>("Assets/Textures/UIComponents/ItemSlot");
            GameFont = content.Load<SpriteFont>("Assets/Fonts/RetroGaming");
            Cracks = content.Load<Texture2D>("Assets/Textures/Tiles/Extras/Cracks");
            TreesBackground = content.Load<Texture2D>("Assets/Textures/Backgrounds/Normal/Trees");
            TreesFartherBackground = content.Load<Texture2D>("Assets/Textures/Backgrounds/Normal/TreesFarther");
            TreesFarthestBackground = content.Load<Texture2D>("Assets/Textures/Backgrounds/Normal/TreesFarthest");
            MountainsBackground = content.Load<Texture2D>("Assets/Textures/Backgrounds/Normal/Mountains");
            LiquidTexture = content.Load<Texture2D>("Assets/Textures/Tiles/Liquids/Liquid0");

            //load tile textures into an array
            int numTiles = Directory.GetFiles(Path.Combine(fullContentDirectory, "Assets/Textures/Tiles")).Length;
            for (int i = 1; i <= numTiles; i++)
            {
                TileTextures[i] = content.Load<Texture2D>("Assets/Textures/Tiles/Tile" + i);
            }

            int numWalls = Directory.GetFiles(Path.Combine(fullContentDirectory, "Assets/Textures/Walls")).Length;
            for (int i = 1; i <= numWalls; i++)
            {
                WallTextures[i] = content.Load<Texture2D>("Assets/Textures/Walls/Wall" + i);
            }

            int numItems = Directory.GetFiles(Path.Combine(fullContentDirectory, "Assets/Textures/Items")).Length;
            for (int i = 0; i < numItems; i++)
            {
                ItemTextures[i] = content.Load<Texture2D>("Assets/Textures/Items/Item" + i);
            }

            int numEnemies = Directory.GetFiles(Path.Combine(fullContentDirectory, "Assets/Textures/Enemies")).Length;
            for (int i = 0; i < numEnemies; i++)
            {
                EnemyTextures[i] = content.Load<Texture2D>("Assets/Textures/Enemies/Enemy" + i);
            }

            //load shaders
            WaterShader = content.Load<Effect>("Assets/Shaders/WaterShader");
        }
        private static string GetFullContentPath(ContentManager content)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return Path.Combine(AppContext.BaseDirectory, "..", "Resources", content.RootDirectory);
            }
            return Path.Combine(AppContext.BaseDirectory, content.RootDirectory);
        }
    }
}
