using System;
using Microsoft.Xna.Framework;

namespace Curl
{
    /**
    * Class Slide smoothly slides an Object from one Position to another
    * @author David Hoffmann
    */
    public class Slide
    {
        private float from;
        private float duration;
        private float change;
        private float elapsed;
        private float position;
        
        public float Position 
        { 
            get 
            { 
                return this.position; 
            }

            protected set 
            { 
                this.position = value; 
            } 
        }

        /**
        * Constructor initialize the sliding
        * @param float from - start Position
        * @param float to - end Position
        * @param float duration - time for the sliding
        */
        public Slide(float from, float to, float duration)
        {
            this.from = from;
            this.position = from;
            this.change = to - from;
            this.duration = duration;
        }

        /**
        * Update the sliding Position
        * @param GameTime gameTime - snapshot of the game timing
        */
        public void Update(GameTime gameTime)
        {
            if (this.elapsed == this.duration)
            {
                return;
            }

            this.Position = this.EaseOut(this.elapsed, this.from, this.change, this.duration);
            this.elapsed += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (this.elapsed >= this.duration)
            {
                this.elapsed = this.duration;
                this.Position = this.from + this.change;  
            }
        }

        /**
        * EaseOut Movement
        * based on the algorithm of Robert Penner
        * http:////www.robertpenner.com/easing/penner_chapter7_tweening.pdf
        * @param float t - time
        * @param float b - beginning position
        * @param float c - change
        * @param float d - duration
        */
        private float EaseOut(float t, float b, float c, float d)
        {
            return (t == d) ? b + c : c * (float)(-Math.Pow(2, -10 * t / d) + 1) + b;
        }
    }
}