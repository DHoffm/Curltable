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
    * Class ShutdownComponent is used to display a shutdown window
    * @author David Hoffmann
    * @see Container
    */
    sealed public class ShutdownComponent : Container
    {
        private Texture2D darken;
        private Button closeOk, closeNo;
        private Slide shutdownBgSlider, darkenAlpha;
        private ArrayList shutdownControlButtons = new ArrayList();
        
        /**
        * Constructor
        * @param Game game - which Game to use this Keyboard Component for
        */
        public ShutdownComponent(Game game) : base(game)
        {
        }

        public override void Initialize()
        { 
            base.Initialize(); 
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            this.position = new Vector2(426, -400);
            
            ////background of the shutdown window
            this.bg = Game.Content.Load<Texture2D>("icons/shutdownBg");
            ////fader to darken the screen when the shutdown window is active
            this.darken = Game.Content.Load<Texture2D>("icons/darken");
            ////close ok button(used in the shutdown window)
            this.shutdownControlButtons.Add(this.closeOk = new Button(Game, "icons/closeOkButton", "icons/closeOkButtonRollOver", new Vector2(450, -200), new EventHandler(this.CloseOkAction)));
            ////close no button(used in the shutdown window)
            this.shutdownControlButtons.Add(this.closeNo = new Button(Game, "icons/closeNoButton", "icons/closeNoButtonRollOver", new Vector2(600, -200), new EventHandler(this.CloseNoAction)));
        }

        public override void Update(GameTime gameTime)
        {
            if (this.shutdownBgSlider != null)
            {
                this.position.Y = this.shutdownBgSlider.Position;
                this.closeOk.SetPosition((int)this.closeOk.GetPosition().X, (int)this.shutdownBgSlider.Position + 45);
                this.closeNo.SetPosition((int)this.closeNo.GetPosition().X, (int)this.shutdownBgSlider.Position + 45);
                this.shutdownBgSlider.Update(gameTime);
                this.darkenAlpha.Update(gameTime);
            }

            ////slides the shutdown window in if the user hits escape
            if (Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape) && !this.IsBlocking())
            {
                this.shutdownBgSlider = new Slide(this.position.Y, 455, 0.5f);
                this.darkenAlpha = new Slide(0, 255, 0.5f);
                this.SetBlocking(true);
            }

            if (this.CheckMouseClick())
            {
                ////checks all available buttons
                foreach (Button button in this.shutdownControlButtons) 
                { 
                    button.CheckForCollision(); 
                }
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            this.spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
            ////shutdown 
            Color helper = Color.White;
            if (this.darkenAlpha != null)
            {
                helper.A = (byte)this.darkenAlpha.Position;
            }
            else
            {
                helper.A = 0;
            }

            this.spriteBatch.Draw(this.darken, new Microsoft.Xna.Framework.Rectangle(0, 0, this.darken.Width, this.darken.Height), helper);
            this.spriteBatch.Draw(this.bg, new Microsoft.Xna.Framework.Rectangle((int)this.position.X, (int)this.position.Y, this.bg.Width, this.bg.Height), Color.White);
            this.closeNo.Draw(this.spriteBatch, Color.White, false);
            this.closeOk.Draw(this.spriteBatch, Color.White, false);
            this.spriteBatch.End();
            base.Draw(gameTime);
        }

        private void CloseOkAction(object sender, EventArgs e)
        {
            Game.Exit();
        }

        private void CloseNoAction(object sender, EventArgs e)
        {
            this.Hide();
        }

        public override void Show()
        {
            this.shutdownBgSlider = new Slide(this.position.Y, 455, 0.5f);
            this.darkenAlpha = new Slide(0, 255, 0.5f);
            this.SetBlocking(true);
        }

        public override void Hide()
        {
            this.shutdownBgSlider = new Slide(this.position.Y, -200, 0.5f);
            this.darkenAlpha = new Slide(255, 0, 0.5f);
            this.SetBlocking(false);
        }
    }
}