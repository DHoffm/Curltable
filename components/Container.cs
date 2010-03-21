using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace Curl
{
    /**
    * Class Container is used for Componentsthat overlay the main application
    * @author David Hoffmann
    * @see SetupComponent, KeyboardComponent, ShutdownComponent
    */
    public abstract class Container : Microsoft.Xna.Framework.DrawableGameComponent
    {
        private bool blocked = false;
        protected SpriteFont spriteFont;
        protected SpriteBatch spriteBatch;
        ////background position
        protected Vector2 position;
        ////background of the component
        protected Texture2D bg;
        ////used to prevent multiple clicks
        protected bool isAllreadyClicked = false;
        /**
        * Constructor
        * @param Game game - which Game to use this Container for
        */
        public Container(Game game) : base(game)
        {
            game.Components.Add(this);
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            this.spriteBatch = new SpriteBatch(GraphicsDevice);
            this.spriteFont = Game.Content.Load<SpriteFont>("Arial");
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }

        /**
        * return if the Container is blocking the main window
        */
        public bool IsBlocking() 
        { 
            return this.blocked; 
        }
       
        /**
        * set the blocking of the Container
        * @param bool value - true is blocking, false is not blocking
        */
        public void SetBlocking(bool value) 
        { 
            this.blocked = value; 
        }

        protected bool CheckMouseClick()
        {
            if (Mouse.GetState().LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /**
        * show the Container
        */
        public abstract void Show();

        /**
        * hide the Container
        */
        public abstract void Hide();
    }
}