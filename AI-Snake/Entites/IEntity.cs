using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AISnake.Entities
{
    public interface IEntity
    {
        void Update(GameTime gameTime);
        void Draw(SpriteBatch spriteBatch);
    }
}
