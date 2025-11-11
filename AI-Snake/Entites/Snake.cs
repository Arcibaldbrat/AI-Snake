using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace AISnake.Entities
{
    public enum Direction { Up, Right, Down, Left }

    public static class Utils
    {
        public static Direction Rotate(Direction dir, int delta)
        {
            int v = (int)dir;
            v = (v + delta) % 4;
            if (v < 0) v += 4;
            return (Direction)v;
        }

        public static Vector2 DirectionToVector(Direction d)
        {
            return d switch
            {
                Direction.Up => new Vector2(0, -1),
                Direction.Down => new Vector2(0, 1),
                Direction.Left => new Vector2(-1, 0),
                Direction.Right => new Vector2(1, 0),
                _ => Vector2.Zero
            };
        }
    }

    public class Snake : IEntity
    {
        public List<Vector2> Segments = new List<Vector2>();
        public float SegmentSize;
        public Direction Heading;
        public float Speed;
        public bool Alive = true;
        public Texture2D SegmentTex;
        private GraphicsDevice graphics;
        private Color baseColor;
        public List<Vector2> positionHistory = new List<Vector2>();

        private float followSpacing = 8f;

        public Snake(GraphicsDevice graphics, Vector2 startPos, int initialSegments = 5, float segmentSize = 12f, float speed = 120f, Color color = default)
        {
            this.graphics = graphics;
            SegmentSize = segmentSize;
            Speed = speed;
            baseColor = color == default ? Color.LimeGreen : color;
            SegmentTex = CreateRect(graphics, (int)SegmentSize, (int)SegmentSize, baseColor);

            Segments.Clear();
            for (int i = 0; i < initialSegments; i++)
            {
                Segments.Add(startPos + new Vector2(-i * followSpacing, 0));
            }

            Heading = Direction.Right;

            positionHistory.Clear();
            for (int i = 0; i < 500; i++) positionHistory.Add(startPos);
        }

        public void Grow()
        {
            Vector2 tail = Segments[Segments.Count - 1];
            Segments.Add(tail);
        }

        public void SetHeading(Direction d)
        {
            if ((int)d == ((int)Heading + 2) % 4) return;
            Heading = d;
        }

        public void Move(float deltaSeconds)
        {
            if (!Alive) return;

            Vector2 dir = Utils.DirectionToVector(Heading);
            Vector2 head = Segments[0];
            Vector2 newHead = head + dir * (Speed * deltaSeconds);

            positionHistory.Insert(0, newHead);
            while (positionHistory.Count > (int)(Segments.Count * (followSpacing / Math.Max(1, SegmentSize / 4)) + 10))
                positionHistory.RemoveAt(positionHistory.Count - 1);

            Segments[0] = newHead;
            for (int i = 1; i < Segments.Count; i++)
            {
                int idx = (int)(i * (followSpacing / (SegmentSize)));
                if (idx < positionHistory.Count)
                    Segments[i] = positionHistory[Math.Min(idx, positionHistory.Count - 1)];
            }
        }

        public bool CheckSelfCollision()
        {
            Vector2 head = Segments[0];
            for (int i = 1; i < Segments.Count; i++)
            {
                if (Vector2.Distance(head, Segments[i]) < SegmentSize * 0.6f) return true;
            }
            return false;
        }

        public bool CheckOutOfBounds(int worldWidth, int worldHeight)
        {
            Vector2 head = Segments[0];
            if (head.X < 0 || head.Y < 0 || head.X > worldWidth || head.Y > worldHeight) return true;
            return false;
        }

        public void Kill() { Alive = false; }

        public void Update(GameTime gameTime) { }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < Segments.Count; i++)
            {
                var pos = Segments[i];
                Rectangle dst = new Rectangle((int)pos.X, (int)pos.Y, (int)SegmentSize, (int)SegmentSize);
                spriteBatch.Draw(SegmentTex, dst, Color.White);
            }
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
