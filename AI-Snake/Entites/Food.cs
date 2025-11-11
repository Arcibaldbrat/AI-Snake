using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace AISnake.Entities
{
    public class Food : IEntity
    {
        public Vector2 Position;
        public int Size;
        private Texture2D tex;
        private GraphicsDevice graphics;
        private Color color;

        public Food(GraphicsDevice graphics, int size, Color color)
        {
            this.graphics = graphics;
            Size = size;
            this.color = color;
            tex = CreateRect(graphics, Size, Size, color);
            Position = Vector2.Zero;
        }

        public void SpawnRandom(int width, int height, Random rng, int margin = 0)
        {
            float x = rng.Next(margin, width - margin - Size);
            float y = rng.Next(margin, height - margin - Size);
            Position = new Vector2(x, y);
        }

        public void Update(GameTime gameTime) { }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(tex, Position, Color.White);
        }

        private Texture2D CreateRect(GraphicsDevice g, int w, int h, Color c)
        {
            Texture2D t = new Texture2D(g, w, h);
            Color[] data = new Color[w * h];
            for (int i = 0; i < data.Length; i++) data[i] = c;
            t.SetData(data);
            return t;
        }
    }
}
