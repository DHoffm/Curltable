using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace Curl
{
    /**
    * Class Button is used to represent Buttons
    * @author David Hoffmann
    */
    public class Button
    {
        /**
        * stores Position, Dimension and Textures beloging to this Button
        */
        private struct Elem
        {
            private Vector2 pos;
            
            public Vector2 Pos 
            { 
                get 
                { 
                    return this.pos; 
                }
 
                set 
                { 
                    this.pos = value; 
                } 
            }

            private Vector2 dim;
            
            public Vector2 Dim 
            { 
                get 
                { 
                    return this.dim; 
                }
 
                set 
                { 
                    this.dim = value; 
                } 
            }   

            private Texture2D tex;
            
            public Texture2D Tex 
            { 
                get 
                { 
                    return this.tex; 
                } 

                set 
                { 
                   this.tex = value; 
                } 
            }

            private Texture2D normalTex;
            
            public Texture2D NormalTex 
            { 
                get 
                { 
                    return this.normalTex; 
                } 

                set 
                { 
                    this.normalTex = value; 
                } 
            }

            private Texture2D hoverTex;
            
            public Texture2D HoverTex 
            { 
                get 
                { 
                    return this.hoverTex; 
                } 

                set 
                {
                    this.hoverTex = value; 
                } 
            }

            private Texture2D normalTex2;
            
            public Texture2D NormalTex2 
            { 
                get 
                {
                    return this.normalTex2; 
                } 

                set 
                {
                    this.normalTex2 = value; 
                } 
            }

            private Texture2D hoverTex2;
            
            public Texture2D HoverTex2 
            { 
                get 
                {
                    return this.hoverTex2;
                }

                set 
                { 
                    this.hoverTex2 = value; 
                }
            }
        }

        private Elem element;

        private GraphicsDevice device;
        
        private bool alternative;

        /**
        * EventHandler assigned to this Button
        */
        public event EventHandler Action;

        /**
        * Constructor for normal Button without EventHandler
        * @param Game game - which Game to use this Button for
        * @param String normalTex - Texture File used for this Button
        * @param String hoverTex - Texture File used for the hover State of this Button
        * @param Vector2 pos - position of the Button
        */
        public Button(Game game, string normalTex, string hoverTex, Vector2 pos)
        {
            this.device = game.GraphicsDevice;
            this.alternative = false;
            this.element.Tex = game.Content.Load<Texture2D>(normalTex);
            this.element.NormalTex = game.Content.Load<Texture2D>(normalTex);
            this.element.HoverTex = game.Content.Load<Texture2D>(hoverTex);
            this.element.Pos = pos;
            this.element.Dim = new Vector2(this.element.Tex.Width, this.element.Tex.Height);
        }

        /**
        * Constructor for a Button with two different functions without EventHandler
        * @param Game game - which Game to use this Button for
        * @param String normalTex - Texture File used for this Button
        * @param String hoverTex - Texture File used for the hover State of this Button
        * @param String normalTex2 - alternative Texture File used for this Button
        * @param String hoverTex2 - alternative Texture File used for the hover State of this Button
        * @param Vector2 pos - position of the Button
        */
        public Button(Game game, string normalTex, string hoverTex, string normalTex2, string hoverTex2, Vector2 pos) : this(game, normalTex, hoverTex, pos)
        {
            this.element.NormalTex2 = game.Content.Load<Texture2D>(normalTex2);
            this.element.HoverTex2 = game.Content.Load<Texture2D>(hoverTex2);
        }

        /**
        * Constructor for normal Button with EventHandler
        * @param Game game - which Game to use this Button for
        * @param String normalTex - Texture File used for this Button
        * @param String hoverTex - Texture File used for the hover State of this Button
        * @param Vector2 pos - position of the Button
        * @param EventHandler collisionHandler - Event for handling a Collision with this Button
        */
        public Button(Game game, string normalTex, string hoverTex, Vector2 pos, EventHandler collisionHandler) 
            : this(game, normalTex, hoverTex, pos)
        {
            this.Action = collisionHandler;
        }

        /**
        * Constructor for a Button with two different functions with EventHandler
        * @param Game game - which Game to use this Button for
        * @param String normalTex - Texture File used for this Button
        * @param String hoverTex - Texture File used for the hover State of this Button
        * @param String normalTex2 - alternative Texture File used for this Button
        * @param String hoverTex2 - alternative Texture File used for the hover State of this Button
        * @param Vector2 pos - position of the Button
        * @param EventHandler collisionHandler - Event for handling a Collision with this Button
        */
        public Button(Game game, string normalTex, string hoverTex, string normalTex2, string hoverTex2, Vector2 pos, EventHandler collisionHandler)
            : this(game, normalTex, hoverTex, normalTex2, hoverTex2, pos)
        {
            this.Action = collisionHandler;
        }

        /**
        * draws the Button
        * @param SpriteBatch batch - the SpriteBatch to draw the Button on
        * @param Color color - the Color the Button is drawn with
        * @param bool toggle - defines if the Button is stil clickable when an overlaying Container is visible
        */
        public virtual void Draw(SpriteBatch batch, Color color, bool toggle)
        {
            ////first check if mouse is over and set the right texture
            if (!toggle)
            {
                this.RollOver();
            }
            ////then draw it
            batch.Draw(this.element.Tex, new Rectangle((int)this.element.Pos.X, (int)this.element.Pos.Y, (int)this.element.Dim.X, (int)this.element.Dim.Y), color);
        }

        private void RollOver()
        {
            ////when called with the first constructor alternative is always false
            ////that keeps preventing from unwanted stuff
            if (!this.alternative)
            {
                if (this.Collision())
                {
                    this.element.Tex = this.element.HoverTex;
                }
                else
                {
                    this.element.Tex = this.element.NormalTex;
                }
            }
            else 
            {
                if (this.Collision())
                {
                    this.element.Tex = this.element.HoverTex2;
                }
                else
                {
                    this.element.Tex = this.element.NormalTex2;
                }
            }
        }

        /**
        * manually alter the images
        * @param bool state - defines the image(hover or normal)
        */
        public void ChangeTex(bool state)
        {
            ////when called with the first constructor alternative is always false
            ////that keeps preventing from unwanted stuff
            if (!this.alternative)
            {
                if (!state)
                {
                    this.element.Tex = this.element.NormalTex;
                }
                else
                {
                    this.element.Tex = this.element.HoverTex;
                }
            }
            else
            {
                if (!state)
                {
                    this.element.Tex = this.element.NormalTex2;
                }
                else
                {
                    this.element.Tex = this.element.HoverTex2;
                }
            }
        }

        private bool IsMouseOver()
        {
            MouseState mouse = Mouse.GetState();
            if (mouse.X < this.device.Viewport.Width && mouse.Y < this.device.Viewport.Height)
            {
                if ((mouse.X >= this.element.Pos.X) && mouse.X < (this.element.Pos.X + this.element.Dim.X) && mouse.Y >= this.element.Pos.Y && mouse.Y < (this.element.Pos.Y + this.element.Dim.Y) && mouse.LeftButton == ButtonState.Pressed)
                {
                    return true;
                }
            }

            return false;
        }

        /**
        * if the Button is constructed without an EventHandler this method should be used to identify a Collision
        */
        public bool Collision()
        {
            if (this.IsMouseOver())
            {
                MouseState mouse = Mouse.GetState();
                Vector2 mousePos;
                mousePos.X = mouse.X;
                mousePos.Y = mouse.Y;

                Vector2 pixelPosition = Vector2.Zero;
                pixelPosition = mousePos - this.element.Pos;

                //// delare an Array of 1 just to store data for one pixel
                uint[] pixelData = new uint[1];
                //// get the texture data within the rectangle coords, in this case a 1 x 1 rectangle
                //// store the data in pixelData Array
                this.element.Tex.GetData<uint>(0, new Rectangle((int)pixelPosition.X, (int)pixelPosition.Y, 1, 1), pixelData, 0, 1);

                //// Check if pixel in Array is non Alpha
                //// we check if the pixel is 255 because we have edge glowing apllied to 
                //// the textures so they will picked to if we choose a lower value
                return (((pixelData[0] & 0xFF000000) >> 24) > 254) ? true : false;
            }
            else
            {
                return false;
            }
        }

        /**
        * if the Button is constructed with an EventHandler this method should be used to identify a Collision
        */
        public void CheckForCollision()
        {
            if (this.Collision())
            {
                this.Action(this, new EventArgs());
            }
        }

        /**
        * seperate Method to return the Position of the Button
        */
        public Vector2 GetPosition() 
        { 
            return this.element.Pos; 
        }

        /**
        * add Coordinates to the current Button Position
        * @param int x - offset Position in x direction
        * @param int Y - offset Position in y direction
        */
        public void SlideToPosition(int x, int y) 
        { 
            this.element.Pos += new Vector2((float)x, (float)y); 
        }

        /**
        * set a new Button Position
        * @param int x - new x Position
        * @param int Y - new y Position
        */
        public void SetPosition(int x, int y) 
        { 
            this.element.Pos = new Vector2((float)x, (float)y); 
        }

        /**
        * set a new Button Position and Dimension
        * @param int x - new x Position
        * @param int Y - new y Position
        * @param int width - new width
        * @param int height - new height
        */
        public void SetPosition(int x, int y, int width, int height)
        {
            this.element.Pos = new Vector2((float)x, (float)y);  
            this.element.Dim = new Vector2((float)width, (float)height);
        }

        /**
        * toggle the Button
        * Help: is only neccesary when the Button is constructed with 4 Textures
        * @param bool alternative - switch between the two functions 
        */
        public void Toggle(bool alternative)
        {
            ////toogle the button texture if an alternative is given 
            if (this.element.HoverTex2 != null && this.element.HoverTex2 != null)
            {
                this.alternative = alternative;
                if (!this.alternative)
                {
                    this.element.Tex = this.element.NormalTex;
                }
                else
                {
                    this.element.Tex = this.element.NormalTex2;
                }
            }
        }

        /**
        * return the Height of the Button
        */
        public float GetHeight() 
        { 
            return this.element.Dim.Y; 
        }

        /**
        * return the Width of the Button
        */
        public float GetWidth() 
        { 
            return this.element.Dim.X; 
        }

        /**
        * return which Button Function is active
        */
        public bool GetAlternative() 
        { 
            return this.alternative; 
        }

        /**
        * return PixelColor of Position inside the Button Texture
        * @param Vector2 pos - Position to get Color from
        */
        public Color GetColor(Vector2 pos)
        {
            Color[] colorData = new Color[1];
            this.element.Tex.GetData<Color>(0, new Microsoft.Xna.Framework.Rectangle((int)pos.X, (int)pos.Y, 1, 1), colorData, 0, 1);
            return colorData[0];
        }

        /**
        * return the current Texture of this Button
        */
        public Texture2D GetTexture() 
        { 
            return this.element.Tex; 
        }
    }
}
