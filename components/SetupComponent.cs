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
using General;

namespace Curl
{
    /**
    * Class SetupComponent is used to display a setup window to control the color and the cursor
    * @author David Hoffmann
    * @see Container, ColorPicker, Cursor
    */
    sealed public class SetupComponent : Container
    {
        private Button setupClose;
        private CheckBox cursorActiveChange;
        private Cursor cursor;
        private ColorPicker picker;

        private ArrayList setupControlButtons = new ArrayList();
        private Slide setupBgSlider;

        ////the overlay color of all gui Elements
        private Color buttonColor;

        /**
        * sets or gets the button color
        */
        public Color ButColor 
        { 
            get 
            { 
                return this.buttonColor; 
            } 
            
            set 
            { 
                this.buttonColor = value; 
            } 
        }

        ////the complement to button color
        private Color complementColor = new Color(0, 0, 0);

        /**
        * sets or gets the complementary color
        */
        public Color CompColor 
        { 
            get 
            { 
                return this.complementColor; 
            } 
            
            set 
            { 
                this.complementColor = value; 
            } 
        }

        /**
        * Constructor
        * @param Game game - which Game to use this Keyboard Component for
        */
        public SetupComponent(Game game) : base(game)
        {  
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        protected override void UnloadContent()
        {
            ////settings.Write();
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            this.position = new Vector2(316, -700);
            
            this.cursor = new Cursor(Game);
            this.cursor.Active = Settings.Get<bool>("cursor.Active");
            
            this.picker = new ColorPicker(Game); 
            this.buttonColor = this.picker.GetColor();
            this.complementColor.R = Convert.ToByte(Color.White.R - this.buttonColor.R);
            this.complementColor.G = Convert.ToByte(Color.White.G - this.buttonColor.G);
            this.complementColor.B = Convert.ToByte(Color.White.B - this.buttonColor.B);
            ////background of the setup window
            this.bg = Game.Content.Load<Texture2D>("icons/setupBg");
            ////setup close button(to close the setup window)
            this.setupControlButtons.Add(this.setupClose = new Button(Game, "icons/setupCloseButton", "icons/setupCloseButtonRollOver", new Vector2(820, -700), new EventHandler(this.SetupCloseAction)));
            this.setupControlButtons.Add(this.cursorActiveChange = new CheckBox(Game, "icons/cursorActive", "icons/cursorActiveRollOver", "icons/cursorInactive",  "icons/cursorInactiveRollOver", new Vector2(370, -700), new EventHandler(this.CursorActiveAction), "enable / disable"));

            this.cursorActiveChange.Toggle(this.cursor.Active ? false : true);
        }

        public override void Update(GameTime gameTime)
        {
            if (this.setupBgSlider != null)
            {
                this.position.Y = this.setupBgSlider.Position;
                this.picker.SetPosition((int)this.picker.GetPosition().X, (int)this.setupBgSlider.Position + 135);
                this.cursorActiveChange.SetPosition((int)this.cursorActiveChange.GetPosition().X, (int)this.setupBgSlider.Position + 320);
                this.setupClose.SetPosition((int)this.setupClose.GetPosition().X, (int)this.setupBgSlider.Position + 420);
                this.setupBgSlider.Update(gameTime);
            }

            if (this.picker.Collision())
            {
                this.buttonColor = this.picker.GetColor();
                this.complementColor.R = Convert.ToByte(Color.White.R - this.buttonColor.R);
                this.complementColor.G = Convert.ToByte(Color.White.G - this.buttonColor.G);
                this.complementColor.B = Convert.ToByte(Color.White.B - this.buttonColor.B);
            }
            else if (this.CheckMouseClick())
            {
                ////checks all available buttons
                foreach (Button button in this.setupControlButtons) 
                { 
                    button.CheckForCollision(); 
                }
            }
            else
            {
                this.isAllreadyClicked = false;
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            this.spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
            ////color Picker
            this.spriteBatch.Draw(this.bg, new Microsoft.Xna.Framework.Rectangle((int)this.position.X, (int)this.position.Y, this.bg.Width, this.bg.Height), Color.White);
            this.setupClose.Draw(this.spriteBatch, Color.White, false);
            this.cursorActiveChange.Draw(this.spriteBatch, Color.White, false);
            this.spriteBatch.End();
            this.picker.Draw();
            base.Draw(gameTime);
        }

        private void SetupCloseAction(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void CursorActiveAction(object sender, EventArgs e)
        {
            if (!this.isAllreadyClicked)
            {
                if (this.cursor.Active)
                {
                    this.cursor.Active = false;
                    this.cursorActiveChange.Toggle(true);
                }
                else
                {
                    this.cursor.Active = true;
                    this.cursorActiveChange.Toggle(false);
                }

                Settings.Set("cursor.Active", this.cursor.Active);
                this.isAllreadyClicked = true;
            }  
        }
      
        public override void Show()
        {
            this.setupBgSlider = new Slide(this.position.Y, 258, 0.5f);
            this.SetBlocking(true);
        }

        public override void Hide()
        {
            this.setupBgSlider = new Slide(this.position.Y, -700, 0.5f);
            this.SetBlocking(false);
            Settings.Write();
        } 
    }
}