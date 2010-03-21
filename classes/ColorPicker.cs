using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using General;

namespace Curl
{
    /**
    * Class ColorPicker is used to select a Color and Hue
    * @author David Hoffmann
    * @see SetupComponent, Settings
    */
    public class ColorPicker
    {
        private SpriteBatch sB;
        private Texture2D colorTex, hueTex, colorPickerBg, colorPickerCurrent, huePickerCurrent;
        private Vector2 colorTexPos = new Vector2(370, -700);
        private Vector2 hueTexPos = new Vector2(370, -700);
        private Vector2 colorPickerCurrentPos;
        private Vector2 huePickerCurrentPos;
        private Color[] colorArr;
        private Color[] hueArr;
        private Color[] gradient = new Color[384];

        private Color color;
        private Color hue;
        private Vector2 huePosition;
        private GraphicsDevice device;
        /**
        * Constructor displays the Colorpicker
        * @param Game game - which Game to use this Colorpicker for
        */
        public ColorPicker(Game game)
        {
            this.colorPickerCurrentPos = Settings.Get<Vector2>("colorPickerCurrentPos");
            this.huePickerCurrentPos = Settings.Get<Vector2>("huePickerCurrentPos");
            this.huePosition = Settings.Get<Vector2>("huePosition");
            this.hue = Settings.Get<Color>("hue");
            this.color = Settings.Get<Color>("color");
            this.device = game.GraphicsDevice;
            this.colorTex = new Texture2D(this.device, 384, 40, 0, TextureUsage.None, SurfaceFormat.Color);
            this.hueTex = new Texture2D(this.device, 384, 40, 0, TextureUsage.None, SurfaceFormat.Color);

            this.colorPickerBg = game.Content.Load<Texture2D>("icons/colorPickerBg");
            this.huePickerCurrent = game.Content.Load<Texture2D>("icons/colorPickerCurrent");
            this.colorPickerCurrent = game.Content.Load<Texture2D>("icons/colorPickerCurrent");
            this.hueArr = new Color[this.hueTex.Width * this.hueTex.Height];
            this.colorArr = new Color[this.colorTex.Width * this.colorTex.Height];
            this.sB = new SpriteBatch(this.device);
            float startR = 255.0f, startG = 0.0f, startB = 0.0f;
            string[] acArr = { "bUp", "rDown", "gUp", "bDown", "rUp", "gDown" };
            for (int k = 0; k < this.colorTex.Height; k++)
            {
                for (int i = 0; i < acArr.Length; i++)
                {
                    for (int j = 0; j < 64; j++)
                    {
                        if (acArr[i] == "rUp") 
                        { 
                            startR += 4; 
                        }

                        if (acArr[i] == "rDown") 
                        { 
                            startR -= 4; 
                        }

                        if (acArr[i] == "gUp") 
                        { 
                            startG += 4; 
                        }

                        if (acArr[i] == "gDown") 
                        { 
                            startG -= 4; 
                        }

                        if (acArr[i] == "bUp") 
                        { 
                            startB += 4; 
                        }

                        if (acArr[i] == "bDown") 
                        { 
                            startB -= 4; 
                        }

                        this.colorArr[(j + (i * 64)) + (k * 384)] = new Color(startR / 255, startG / 255, startB / 255);
                    }
                }
            }

            this.colorTex.SetData<Color>(this.colorArr);
            this.SetHue();
        }

        /**
        * draws the Colorpicker
        */
        public void Draw()
        {
            this.sB.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
            this.sB.Draw(this.colorTex, new Rectangle((int)this.colorTexPos.X, (int)this.colorTexPos.Y, this.colorTex.Width, this.colorTex.Height), Color.White);
            this.sB.Draw(this.hueTex, new Rectangle((int)this.hueTexPos.X, (int)this.hueTexPos.Y, this.hueTex.Width, this.hueTex.Height), Color.White);
            this.sB.Draw(this.colorPickerBg, new Rectangle((int)this.hueTexPos.X - 1, (int)this.hueTexPos.Y - 1, this.colorPickerBg.Width, this.colorPickerBg.Height), Color.White);
            this.sB.Draw(this.colorPickerBg, new Rectangle((int)this.colorTexPos.X - 1, (int)this.colorTexPos.Y - 1, this.colorPickerBg.Width, this.colorPickerBg.Height), Color.White);
            this.sB.Draw(this.colorPickerCurrent, new Rectangle((int)this.colorPickerCurrentPos.X, (int)this.colorTexPos.Y - 11, this.colorPickerCurrent.Width, this.colorPickerCurrent.Height), Color.White);
            this.sB.Draw(this.huePickerCurrent, new Rectangle((int)this.huePickerCurrentPos.X, (int)this.hueTexPos.Y - 11, this.huePickerCurrent.Width, this.huePickerCurrent.Height), Color.White);
            this.sB.End();
        }

        /**
        * checks for a Collision with the Colorpicker
        */
        public bool Collision()
        {
            if (this.CheckForCollision(this.colorTex, this.colorTexPos))
            {
                Vector2 pixelPosition = new Vector2(Mouse.GetState().X, Mouse.GetState().Y) - new Vector2(this.colorTexPos.X, this.colorTexPos.Y);
                this.colorPickerCurrentPos.X = Mouse.GetState().X - (this.colorPickerCurrent.Width / 2);
                this.color = this.GetColor(this.colorTex, pixelPosition);
                this.SetHue();
                this.hue = this.GetColor(this.hueTex, this.huePosition);
                Settings.Set("hue", this.hue);
                Settings.Set("color", this.color);
                Settings.Set("colorPickerCurrentPos", this.colorPickerCurrentPos);
                Settings.Set("huePickerCurrentPos", this.huePickerCurrentPos);
                Settings.Set("huePosition", this.huePosition);
                return true;
            }

            if (this.CheckForCollision(this.hueTex, this.hueTexPos))
            {
                MouseState mouse = Mouse.GetState();
                this.huePosition = new Vector2(Mouse.GetState().X, Mouse.GetState().Y) - new Vector2(this.hueTexPos.X, this.hueTexPos.Y);
                this.huePickerCurrentPos.X = Mouse.GetState().X - (this.huePickerCurrent.Width / 2);
                this.hue = this.GetColor(this.hueTex, this.huePosition);
                Settings.Set("hue", this.hue);
                Settings.Set("color", this.color);
                Settings.Set("colorPickerCurrentPos", this.colorPickerCurrentPos);
                Settings.Set("huePickerCurrentPos", this.huePickerCurrentPos);
                Settings.Set("huePosition", this.huePosition);
                return true;
            }

            return false;
        }

        private bool CheckForCollision(Texture2D tex, Vector2 texPos)
        {
            MouseState mouse = Mouse.GetState();
            if (mouse.X < this.device.Viewport.Width && mouse.Y < this.device.Viewport.Height)
            {
                if ((mouse.X >= texPos.X) && mouse.X < (tex.Width + texPos.X) && mouse.Y >= texPos.Y && mouse.Y < (texPos.Y + tex.Height) && mouse.LeftButton == ButtonState.Pressed)
                {
                    return true;
                }
            }

            return false;
        }
        
        private void SetHue()
        {
            for (int i = 0; i < this.gradient.Length; i++)
            {
                this.gradient[i].R = Convert.ToByte(MathHelper.Lerp(255.0f, this.color.R, (1.0f / this.gradient.Length) * i));
                this.gradient[i].G = Convert.ToByte(MathHelper.Lerp(255.0f, this.color.G, (1.0f / this.gradient.Length) * i));
                this.gradient[i].B = Convert.ToByte(MathHelper.Lerp(255.0f, this.color.B, (1.0f / this.gradient.Length) * i));
            }

            int j = 0;
            for (int i = 0; i < this.hueArr.Length; i++)
            {
                this.hueArr[i] = new Color(this.gradient[j].R, this.gradient[j].G, this.gradient[j].B);
                if (j < 383)
                {
                    j++;
                }
                else
                {
                    j = 0;
                }
            }

            this.hueTex.SetData(this.hueArr);
        }

        private Color GetColor(Texture2D tex, Vector2 pixelPos)
        {
            Color[] colorData = new Color[1];
            tex.GetData<Color>(0, new Rectangle((int)pixelPos.X, (int)pixelPos.Y, 1, 1), colorData, 0, 1);
            return colorData[0];
        }

        /**
        * return the Color selected by the Colorpicker
        */
        public Color GetColor() 
        { 
            return this.hue; 
        }
        
        /**
        * set the Position of the Colorpicker
        */
        public void SetPosition(int x, int y) 
        { 
            this.colorTexPos = new Vector2((float)x, (float)y);
            this.hueTexPos = new Vector2((float)x, (float)y + 70); 
        }
        
        /**
        * get the Position of the Colorpicker
        */
        public Vector2 GetPosition() 
        { 
            return this.colorTexPos; 
        }
    }
}
