using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Curl
{
    /**
    * Class Cursor is used to display a mouse trail
    * @author David Hoffmann
    */
    public class Cursor : DrawableGameComponent
    {
        private class CursorPoint
        {
            public Vector2 P;
            public Vector2 V;
            public float Scale;
        }

        private Texture2D colorTex;
        private Vector2 texCenter;
        private SpriteBatch sB;
        private List<CursorPoint> points;
        private float elasticity = 0.2f;
        private float friction = 0.3f;
        private bool active;
        /**
        * Constructor
        * @param Game game - which Game to use this Cursor for
        */
        public Cursor(Game game) : base(game)
        {
            game.Components.Add(this);
            this.active = true;
            this.points = new List<CursorPoint>();
            Vector2 mouse = this.UpdateMouse();
            for (int i = 0; i < 20; i++)
            {
                this.points.Add(new CursorPoint());
                this.points[i].Scale = MathHelper.Lerp(1.0f, 0.1f, (1.0f / 20) * i);
                this.points[i].P = mouse;
            }  
        }

        protected override void LoadContent()
        {
            this.colorTex = Game.Content.Load<Texture2D>("icons/cursor");
            this.texCenter = new Vector2(this.colorTex.Width / 2, this.colorTex.Height / 2);
            this.sB = new SpriteBatch(GraphicsDevice);
            base.LoadContent();
        }

        public override void Initialize() 
        { 
            base.Initialize(); 
        }

        public override void Draw(GameTime gameTime)
        {
            if (this.active)
            {
                this.sB.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
                foreach (CursorPoint point in this.points)
                {
                    this.sB.Draw(this.colorTex, point.P, null, Color.White, 0.0f, this.texCenter, point.Scale, SpriteEffects.None, 0.0f);
                }

                this.sB.End();
            }
        }

        public override void Update(GameTime gameTime)
        {
            this.points[0].P = this.UpdateMouse();
            foreach (CursorPoint point in this.points)
            {
                if (this.points.IndexOf(point) > 0)
                {
                    point.V = ((point.V + (this.points[this.points.IndexOf(point) - 1].P - point.P) / this.elasticity) / this.friction) * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    point.P += point.V;
                }
            }
        }

        private Vector2 UpdateMouse()
        {
            return new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
        }

        /**
        * sets or gets the state of the cursor
        */
        public bool Active
        { 
            get 
            { 
                return this.active; 
            } 

            set 
            { 
                this.active = value; 
            } 
        }
    }
}
