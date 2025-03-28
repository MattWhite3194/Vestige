using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheGreen.Game.Input;
using TheGreen.Game.WorldGeneration;

namespace TheGreen.Game.Items
{
    public class TileItem : Item
    {
        private ushort _tileID;
        public TileItem(int id, string name, string description, Texture2D Image, ushort tileID) 
        { 
            this.ID = id;
            this.Name = name;
            this.Description = description;
            this.Image = Image;
            this.Stackable = true;
            this._tileID = tileID;
            this.UseSpeed = 0.15;
            this.AutoUse = true;
        }
        public override bool UseItem()
        {
            Point mouseTilePosition = InputManager.GetMouseWorldPosition() / new Point(Globals.TILESIZE, Globals.TILESIZE);
            if (WorldGen.World.GetTileID(mouseTilePosition.X, mouseTilePosition.Y) == 0)
            {
                if (WorldGen.World.SetTile(mouseTilePosition.X, mouseTilePosition.Y, _tileID))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
