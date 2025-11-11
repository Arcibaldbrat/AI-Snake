
using AISnake.AI;
using AISnake.Entities;
using AISnake.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AISnake
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private GameWorld world;
        private SpriteFont font;
        private KeyboardState previousState;
        private Texture2D backgroundTex;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.AllowUserResizing = true;
            _graphics.PreferredBackBufferWidth = 900;
            _graphics.PreferredBackBufferHeight = 600;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Načtení fontu (pokud máš ve složce Content)
            try
            {
                font = Content.Load<SpriteFont>("DefaultFont");
            }
            catch
            {
                font = null; // fallback pokud font chybí
            }

            int w = GraphicsDevice.PresentationParameters.BackBufferWidth;
            int h = GraphicsDevice.PresentationParameters.BackBufferHeight;
            world = new GameWorld(GraphicsDevice, w, h);

            backgroundTex = CreateRect(GraphicsDevice, w, h, new Color(12, 18, 25));
        }

        protected override void Update(GameTime gameTime)
        {
            var ks = Keyboard.GetState();

            // Exit
            if (ks.IsKeyDown(Keys.Escape)) Exit();

            // Toggle AI / manual
            if (WasPressed(ks, Keys.Space))
            {
                world.UseAI = !world.UseAI;
            }

            // Toggle training
            if (WasPressed(ks, Keys.T))
            {
                world.Training = !world.Training;
            }

            // Save / Load Q-table
            if (WasPressed(ks, Keys.S)) world.Agent.Save("qtable.json");
            if (WasPressed(ks, Keys.L)) world.Agent.Load("qtable.json");

            // Manual control pokud AI off
            if (!world.UseAI)
            {
                if (ks.IsKeyDown(Keys.Up) || ks.IsKeyDown(Keys.W)) world.Snake.SetHeading(Direction.Up);
                else if (ks.IsKeyDown(Keys.Down) || ks.IsKeyDown(Keys.S)) world.Snake.SetHeading(Direction.Down);
                else if (ks.IsKeyDown(Keys.Left) || ks.IsKeyDown(Keys.A)) world.Snake.SetHeading(Direction.Left);
                else if (ks.IsKeyDown(Keys.Right) || ks.IsKeyDown(Keys.D)) world.Snake.SetHeading(Direction.Right);
            }

            world.Update(gameTime);

            previousState = ks;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);

            // Pozadí
            _spriteBatch.Draw(backgroundTex, new Rectangle(0, 0, GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight), Color.White);

            // Herní svět
            world.Draw(_spriteBatch);

            // UI overlay
            string mode = world.UseAI ? (world.Training ? "AI (training)" : "AI (running)") : "Manual";
            string text = $"Mode: {mode}   Score: {world.Score}   Segments: {world.Snake.Segments.Count}\n" +
                          "Space: toggle AI  T: toggle train  S: save Q  L: load Q";
            if (font != null)
            {
                _spriteBatch.DrawString(font, text, new Vector2(8, 8), Color.White);
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private bool WasPressed(KeyboardState ks, Keys key)
        {
            return ks.IsKeyDown(key) && !previousState.IsKeyDown(key);
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
