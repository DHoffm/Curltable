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
    * Class CheckBox is used to represent Checkboxes
    * @author David Hoffmann
    * @see Button
    */
    public class CheckBox : Button
    {
        private SpriteFont sF;
        private string label;
        private Color labelColor = new Color(93, 92, 90);
        /**
        * Constructor for Checkbox with EventHandler
        * @param Game game - which Game to use this Checkbox for
        * @param String normalTex - Texture File used for this Checkbox
        * @param String hoverTex - Texture File used for the hover State of this Checkbox
        * @param String normalTex2 - alternative Texture File used for this Checkbox
        * @param String hoverTex2 - alternative Texture File used for the hover State of this Checkbox
        * @param Vector2 pos - position of the Checkbox
        * @param EventHandler collisionHandler - Event for handling a Collision with this Checkbox
        * @param String label - Label beside the Checkbox
        */
        public CheckBox(Game game, string normalTex, string hoverTex, string normalTex2, string hoverTex2, Vector2 pos, EventHandler collisionHandler, string label)
            : base(game, normalTex, hoverTex, normalTex2, hoverTex2, pos, collisionHandler)
        {
            this.sF = game.Content.Load<SpriteFont>("Arial");
            this.label = label;
        }

        /**
        * Constructor for Checkbox without EventHandler
        * @param Game game - which Game to use this Checkbox for
        * @param String normalTex - Texture File used for this Checkbox
        * @param String hoverTex - Texture File used for the hover State of this Checkbox
        * @param String normalTex2 - alternative Texture File used for this Checkbox
        * @param String hoverTex2 - alternative Texture File used for the hover State of this Checkbox
        * @param Vector2 pos - position of the Checkbox
        * @param String label - Label beside the Checkbox
        */
        public CheckBox(Game game, string normalTex, string hoverTex, string normalTex2, string hoverTex2, Vector2 pos, string label)
            : base(game, normalTex, hoverTex, normalTex2, hoverTex2, pos)
        {
            this.sF = game.Content.Load<SpriteFont>("Arial");
            this.label = label;
        }

        /**
        * draws the Checkbox
        * @param SpriteBatch batch - the SpriteBatch to draw the Checkbox on
        * @param Color color - the Color the Checkbox is drawn with
        * @param bool toggle - defines if the Checkbox is stil clickable when an overlaying Container is visible
        */
        public override void Draw(SpriteBatch batch, Color color, bool toggle)
        {
            base.Draw(batch, color, toggle);
            batch.DrawString(this.sF, this.label, new Vector2(this.GetPosition().X + 50, this.GetPosition().Y + 7), this.labelColor);
        }
    }
}
