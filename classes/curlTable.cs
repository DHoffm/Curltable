using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
////bass also has a color value
using Color = Microsoft.Xna.Framework.Graphics.Color;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using Un4seen.Bass;
using Un4seen.Bass.Misc;
using Un4seen.Bass.AddOn.Wma;
using Un4seen.Bass.AddOn.Aac;
using Un4seen.Bass.AddOn.Flac;
using Un4seen.Bass.AddOn.Fx;
using Un4seen.Bass.AddOn.Vis;
using System.Windows.Forms;
using System.Collections;
using System.Threading;

using General;

namespace Curl
{
    /**
    * Class CurlTable - is the main Game   
    * @author David Hoffmann
    */
    public class CurlTable : Game
    {
        #region declaration

        #region general declaration

        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private SpriteFont spriteFont;
        private SpriteFont spriteFontSmall;
        private Vector2 newMousePosition = Vector2.Zero;
        private Vector2 mousePosition = Vector2.Zero;

        #endregion

        #region audio declaration
        
        private FileInfo trackinfo;
        ////the current track
        private string track = String.Empty;
        ////defines a set of boolean vars for the state of a track
        private struct StreamState
        {
            public bool Paused;
            public bool Play;
            public bool Scratch;
            public bool Reverse;
        }
        ////the object for the streamState struct
        private StreamState state;
        ////the stream in which we load the track
        private int stream = 0;
        ////the stream which handles the direction
        private int streamDirection = 0;
        ////the streamwhich handles the tempo
        private int streamTempo = 0;
        ////the waveform/visualization of the track
        private WaveForm wf = null;
        ////those vars are used to transform a bitmap with its frames
        ////into a texture2d
        private Bitmap wavepoints;
        private BitmapData data;
        ////this is the resultset of the temporarly created bitmaps of a waveform
        private Texture2D[] waveformDisplayer;
        ////a timer for the scratching
        private int timer = 50;
        ////if the user decided to click on the waveform then we need to save that new position
        private int selectedFrame;
        ////normal playback frequency
        private float currentFreq = 44100;
        ////contains iformation about the channel of the stream, we use it for the samplerate
        private Un4seen.Bass.BASS_CHANNELINFO info;
        private int loopOutFrame;
        private long loopOut = -1;
        private int loopInFrame;
        private long loopIn = -1;
        private bool looping = false;
        ////we need to transform the current bytes of a position 
        ////into a frame number
        private long currentFrameBytes;
        private double currentFrameSecond;
        private int currentFrame;
        ////unfortunally bass isn't able to handle waveform's that are displayed
        ////vertically and horizontally at the same time, so we need to save a straight horizontal
        ////position for the jumping to a specific set of frames
        //// this is actually the same as the total frames of a track
        private int[] realCurrentX;
        ////this represents the upper bitmap count, it gets set in the callback method of
        ////the waveform
        ////while the waveform is drawn in another thread upper means the last highest frame count
        private int framePos = 0;
        ////the coordintes in the spiral of every texture2d we've created
        private Vector2[] waveformCoords;
        private float[] waveformAngle;
        ////the angle in the spiral
        private float angle;
        ////the radius, increased by the angle to get a archimedical spiral
        private float radius;
        ////the total length of the waveform in frames
        private int len;
        ////this is the maximum nr of textures, most tracks don't reach it because the total length of frames
        //// of a track gets devided by this nr and the resulting frames per picture value devided the total length
        ////of the frames again to get a more accurate value for the total textures,..this is belonging to float
        ////and int values for positioning of a sprite
        private int totalTextures = 3000;
        ////this is the real amount of textures
        private int newTotalTextures;
        ////the tempo frequency in percent
        private float tempo = 0.0f;
        private bool rewind = false;
        private float scratchAngle = 0;
        private float plateAngleHelper;
        ////the angle of the plate
        private float plateAngle = 0;
        ////the previous angle of the plate
        private float oldPlateAngle = 0;
        ////the distance in x direction of the mouse x coordinate and the plate center x
        private float dx = 0;
        ////the distance in y direction of the mouse y coordinate and the plate center y
        private float dy = 0;

        #endregion

        #region gui declaration

        ////all textures
        private Texture2D currentPosIcon, loopOutPosIcon, loopInPosIcon, background, tempoBg, buttonBg, additionalBg, tag;
        ////the position of the background of the close button
        private Vector2 additionalBgPos = new Vector2(0, -492);
        private Vector2 loopOutCoord = new Vector2(-100, 0);
        private Vector2 loopInCoord = new Vector2(-100, 0);
        ////the position of the black icon
        private Vector2 currentPos;
        ////a bool var which prevents clicking a button to often, the user needs to release the mouse first before
        ////hitting the button again
        private bool isAllreadyClicked;
        private Button playAndStop, mode, backward, forward, playCue, file, tempoSlider, close, setup, jmpToZero, Plate, setLoopIn, setLoopOut, setTempoOrPitch;
        private ArrayList guiButtons = new ArrayList();
        ////slides
        private Slide additionalBgSlider;
       
        ////checks if the sliding window for the additional buttons is up or down
        private bool slided;
        ////when the shutdown window is up we need to stop the user from clicking something else
        private bool blocked;
        ////the keyboard for inserting text
        ////private VirtualKeyboard vKeyboard;
        ////position of a pixel to check for alpha
        private Vector2 pixelPosition = Vector2.Zero;
        ////Array of 1 pixel
        private uint[] pixelData = new uint[1];
        private Color tempoColor = new Color(116, 116, 116);
        private Vector2 scratchCenter;
        ////mainly used for the temposlider, because it jumps on the first contact
        private bool firstContact = true;
        private bool showTempo = false;
        private SetupComponent setupComp;
        private ShutdownComponent shutdownComp;
        private List<Container> componentList = new List<Container>();
        
        private Slide jmpToZeroSlider;
        ////true is tempo, false = pitch
        private bool isTempoOrPitch = true;
        
        #endregion

        #endregion

        #region constructor
        /**
        * Constructor
        *
        */
        public CurlTable()
        {
            Settings.Read();

            ////we need to init bass with our desired frequency
            Bass.BASS_Init(-1, (int)this.currentFreq, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
            ////this needs to be explicit called to render the waveform for wma,aac and flac
            Bass.BASS_PluginLoad("basswma.dll");
            Bass.BASS_PluginLoad("bass_aac.dll");
            Bass.BASS_PluginLoad("bassflac.dll");
            ////it's not possible to run a windows form when in fullscreen
            ////to do that we need to use the tooglefullscreen help of xna
            ////and this 2 lines which hide the border when in window mode
            ////so the gui stays at the same position
            Form form = (Form)Form.FromHandle(this.Window.Handle);
            form.FormBorderStyle = FormBorderStyle.None;
            this.graphics = new GraphicsDeviceManager(this);
            this.graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            this.graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            
            this.graphics.PreferMultiSampling = true;
            this.graphics.SynchronizeWithVerticalRetrace = false;
            this.graphics.IsFullScreen = true;
            this.IsMouseVisible = true; //// Show the mouse cursor ( default is false )
            this.Content.RootDirectory = "Content";
            this.IsFixedTimeStep = true;
            this.componentList.Add(this.setupComp = new SetupComponent(this));
            this.componentList.Add(this.shutdownComp = new ShutdownComponent(this));
        }

        #endregion   

        #region initialize

        protected override void Initialize()
        {
            ////create an array for our waveform textures
            this.waveformDisplayer = new Texture2D[this.totalTextures];
            ////and for our coords of the waveform textures too         
            this.waveformCoords = new Vector2[this.totalTextures];
            ////and also one for the straight horizontal position of the waveform textures
            this.realCurrentX = new int[this.totalTextures];
            ////we need this so seperatly position the cue and the current pos icon
            this.waveformAngle = new float[this.totalTextures];
           
            base.Initialize();
        }

        #endregion

        #region load content

        protected override void LoadContent()
        {
            this.spriteBatch = new SpriteBatch(GraphicsDevice);
            this.spriteFont = Content.Load<SpriteFont>("Arial");
            this.spriteFontSmall = Content.Load<SpriteFont>("ArialSmall");
            ////assign all assets to our textures
            ////the icon for the current position
            this.currentPosIcon = Content.Load<Texture2D>("icons/current_pos");
            ////the scratched background
            this.background = Content.Load<Texture2D>("icons/bg");
            ////background for the tempo sloder
            this.tempoBg = Content.Load<Texture2D>("icons/tempo_bg");
            ////the background for all buttons
            this.buttonBg = Content.Load<Texture2D>("icons/buttonBg");
            ////background of the sliding menu
            this.additionalBg = Content.Load<Texture2D>("icons/additionalBg");
            ////background of the setup window
            ////this.setupBg = Content.Load<Texture2D>("icons/setupBg");

            this.loopOutPosIcon = Content.Load<Texture2D>("icons/loopOutPos");
            this.loopInPosIcon = Content.Load<Texture2D>("icons/loopInPos");

            ////icon for the tag positions
            this.tag = Content.Load<Texture2D>("icons/tag");
            ////play and stop button
            this.guiButtons.Add(this.playAndStop = new Button(this, "icons/playbutton", "icons/playbutton_RollOver", "icons/stopbutton", "icons/stopbutton_RollOver", new Vector2(30, 850), new EventHandler(this.PlayAndStopAction)));
            ////mode button(select/scratch)
            this.guiButtons.Add(this.mode = new Button(this, "icons/changeButton", "icons/changeButtonRollOver", "icons/scratchButton", "icons/scratchButtonRollOver", new Vector2(430, 430), new EventHandler(this.ModeAction)));
            ////play backward button
            this.guiButtons.Add(this.backward = new Button(this, "icons/backward", "icons/backward_RollOver", new Vector2(900, 850), new EventHandler(this.BackwardAction)));
            ////play foward button
            this.guiButtons.Add(this.forward = new Button(this, "icons/forward", "icons/forward_RollOver", new Vector2(1100, 850), new EventHandler(this.ForwardAction)));
            ////play cue point button
            this.guiButtons.Add(this.playCue = new Button(this, "icons/cueplaybutton", "icons/cueplaybutton_RollOver", new Vector2(1100, 50), new EventHandler(this.CuePlayAction)));
            ////load a file button
            this.guiButtons.Add(this.file = new Button(this, "icons/filebutton", "icons/filebutton_RollOver", new Vector2(0, -300), new EventHandler(this.FileAction)));
            ////tempo slider button
            this.guiButtons.Add(this.tempoSlider = new Button(this, "icons/tempo", "icons/tempo_RollOver", new Vector2(1105, 450), new EventHandler(this.TempoAction)));
            ////close button
            this.guiButtons.Add(this.close = new Button(this, "icons/closeButton", "icons/closeButtonRollOver", new Vector2(0, -300), new EventHandler(this.CloseAction)));
            ////setup button(to open the setup window)
            this.guiButtons.Add(this.setup = new Button(this, "icons/setupButton", "icons/setupButtonRollOver", new Vector2(0, -300), new EventHandler(this.SetupAction)));
            ////button to set the temposlider back to zero
            this.guiButtons.Add(this.jmpToZero = new Button(this, "icons/lock-on", "icons/lock-off", new Vector2(1052, 503), new EventHandler(this.JmpToZeroAction)));
            this.guiButtons.Add(this.Plate = new Button(this, "icons/plate", "icons/plate", new Vector2(20, 20), new EventHandler(this.PlateAction)));
            this.guiButtons.Add(this.setLoopIn = new Button(this, "icons/loopIn", "icons/loopInRollOver", new Vector2(900, 50), new EventHandler(this.SetLoopInAction)));
            this.guiButtons.Add(this.setLoopOut = new Button(this, "icons/loopOut", "icons/loopOutRollOver", new Vector2(900, 144), new EventHandler(this.SetLoopOutAction)));
            this.guiButtons.Add(this.setTempoOrPitch = new Button(this, "icons/tempoAdjust", "icons/tempoAdjustRollOver", "icons/pitchAdjust", "icons/pitchAdjustRollOver", new Vector2(1065, 750), new EventHandler(this.SetTempoOrPitchAction)));

            this.scratchCenter.X = this.Plate.GetWidth() / 2;
            this.scratchCenter.Y = this.Plate.GetHeight() / 2;
        }

        #endregion

        #region unload content

        protected override void UnloadContent()
        {
            Settings.Write();
            Bass.BASS_Stop();
            Bass.BASS_StreamFree(this.streamTempo);
            Bass.BASS_StreamFree(this.streamDirection);
            Bass.BASS_StreamFree(this.stream);
            Bass.BASS_Free();
        }

        #endregion

        #region update

        protected override void Update(GameTime gameTime)
        {
            //// update the mouse position
            this.mousePosition = this.UpdateMouse();

            this.blocked = this.CheckBlocking();
           
            ////slide down the button on the top left
            if (this.mousePosition.X <= 247 && this.mousePosition.Y <= 50 && this.CheckMouseClick() && !this.blocked)
            {
                this.additionalBgSlider = new Slide(this.additionalBgPos.Y, 0, 0.5f);
                this.slided = true;
            }
            ////slide them up if the mouse is no longer over them
            if (this.slided == true && (this.mousePosition.X > 201 || this.mousePosition.Y > 540))
            {
                this.additionalBgSlider = new Slide(this.additionalBgPos.Y, -492, 0.5f);
                this.slided = false;
            }
            ////slide the menu
            if (this.additionalBgSlider != null)
            {
                this.additionalBgPos.Y = this.additionalBgSlider.Position;
                this.close.SetPosition((int)this.close.GetPosition().X, (int)this.additionalBgSlider.Position - 20);
                this.file.SetPosition((int)this.file.GetPosition().X, (int)this.additionalBgSlider.Position + 150);
                this.setup.SetPosition((int)this.file.GetPosition().X, (int)this.additionalBgSlider.Position + 320);
                this.additionalBgSlider.Update(gameTime);
            }

            if (this.jmpToZeroSlider != null)
            {
                this.tempoSlider.SetPosition((int)this.tempoSlider.GetPosition().X, (int)this.jmpToZeroSlider.Position);
                this.jmpToZeroSlider.Update(gameTime);
                if (this.jmpToZeroSlider.Position == 450)
                {
                    this.jmpToZeroSlider = null;
                }
            }

            ////check if mouse was clicked
            if (this.CheckMouseClick())
            {
                if (!this.blocked)
                {
                    ////checks all available buttons
                    foreach (Button button in this.guiButtons) 
                    { 
                        button.CheckForCollision(); 
                    }

                    //// Check if mouse is inside texture area
                    if (this.OnMouseOver() && !this.state.Scratch && !this.slided)
                    {
                        ////pause the stream
                        this.state.Paused = true;
                        ////get and set the new position
                        long pos = this.wf.GetBytePositionFromX(this.realCurrentX[this.currentFrame], this.framePos, -1, -1);
                        Bass.BASS_ChannelSetPosition(this.streamTempo, pos);
                        if (this.currentFrame > this.loopOutFrame || this.currentFrame < this.loopInFrame && this.loopOut != -1)
                        {
                            this.ResetLoopOut();
                        }
                    }
                    else if (this.FrameOnMouseOver() && !this.state.Scratch && !this.slided)
                    {
                        ////normally I would also use a collision detection for alpha values but sometimes a waveform 
                        ////is too small specially when there is no sound at a certain point
                        ////so we skip that alpha check
                        this.state.Paused = true;

                        ////set the new frame
                        this.currentFrame = this.selectedFrame;
                        long pos = this.wf.GetBytePositionFromX(this.realCurrentX[this.selectedFrame], this.framePos, -1, -1);
                        Bass.BASS_ChannelSetPosition(this.streamTempo, pos);
                        if (this.currentFrame > this.loopOutFrame || this.currentFrame < this.loopInFrame && this.loopOut != -1)
                        {
                            this.ResetLoopOut();
                        } 
                    }
                }
            }
            else
            {
                ////set back the frequency
                if (this.info != null)
                {
                    this.currentFreq = this.info.freq;
                }

                this.state.Paused = false;
                ////set back the button is clicked var
                this.isAllreadyClicked = false;
                this.plateAngleHelper = 0;
                this.firstContact = true;
                this.showTempo = false;
            }

            if (this.currentFrame >= this.loopOutFrame && this.loopOutFrame > this.loopInFrame && this.loopOut != -1 && !this.isAllreadyClicked)
            {
                Bass.BASS_ChannelSetPosition(this.streamTempo, Bass.BASS_ChannelBytes2Seconds(this.streamTempo, this.loopIn));
            }

            if (this.state.Scratch && this.plateAngleHelper == 0 && this.state.Play)
            {
                if (this.state.Reverse == false)
                {
                    this.scratchAngle += 0.01f + (this.tempo / 10000);
                }
                else
                {
                    this.scratchAngle -= 0.01f + (this.tempo / 10000);
                }
            }

           
            this.newMousePosition = this.UpdateMouse();
            base.Update(gameTime);
        }

        #endregion

        #region Draw

        protected override void Draw(GameTime gameTime)
        {
            Color buttonColor = this.setupComp.ButColor;
            Color complementColor = this.setupComp.CompColor;

            this.GraphicsDevice.Clear(Color.Black);
            this.spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
            this.spriteBatch.Draw(this.background, new Microsoft.Xna.Framework.Rectangle(0, 0, this.background.Width, this.background.Height), Color.White);
            this.spriteBatch.Draw(this.buttonBg, new Microsoft.Xna.Framework.Rectangle(900, 50, this.buttonBg.Width, this.buttonBg.Height), Color.White);
            this.spriteBatch.Draw(this.buttonBg, new Microsoft.Xna.Framework.Rectangle(30, 850, this.buttonBg.Width, this.buttonBg.Height), Color.White);
            this.spriteBatch.Draw(this.buttonBg, new Microsoft.Xna.Framework.Rectangle(900, 850, this.buttonBg.Width, this.buttonBg.Height), Color.White);
            this.spriteBatch.Draw(this.buttonBg, new Microsoft.Xna.Framework.Rectangle(1100, 50, this.buttonBg.Width, this.buttonBg.Height), Color.White);
            this.spriteBatch.Draw(this.buttonBg, new Microsoft.Xna.Framework.Rectangle(1100, 850, this.buttonBg.Width, this.buttonBg.Height), Color.White);
            this.spriteBatch.Draw(this.Plate.GetTexture(), new Microsoft.Xna.Framework.Rectangle((int)( this.Plate.GetWidth() / 2 ) + (int)this.Plate.GetPosition().X, (int) ( this.Plate.GetHeight() / 2 ) + (int)this.Plate.GetPosition().Y, (int)this.Plate.GetWidth(), (int)this.Plate.GetHeight()), null, buttonColor, this.plateAngle, new Vector2(this.Plate.GetWidth() / 2, this.Plate.GetHeight() / 2), SpriteEffects.None, 0);

            if (this.state.Play && !this.state.Paused)
            {
                if (!this.state.Reverse)
                {
                    if (this.currentFreq < -100)
                    {
                        if (this.rewind == false)
                        {
                            this.rewind = true;
                            Bass.BASS_ChannelStop(this.streamTempo);
                            Bass.BASS_ChannelSetPosition(this.streamTempo, Bass.BASS_ChannelGetPosition(this.streamTempo, BASSMode.BASS_POS_BYTES), BASSMode.BASS_POS_BYTES);
                            Bass.BASS_ChannelSetAttribute(this.streamDirection, BASSAttribute.BASS_ATTRIB_REVERSE_DIR, (float)BASSFXReverse.BASS_FX_RVS_REVERSE);
                            Bass.BASS_ChannelPlay(this.streamTempo, false);
                        }

                        Bass.BASS_ChannelSlideAttribute(this.streamTempo, BASSAttribute.BASS_ATTRIB_FREQ, -this.currentFreq, this.timer);
                    }
                    else if (this.currentFreq > 100)
                    {
                        if (this.rewind == true)
                        {
                            this.rewind = false;
                            Bass.BASS_ChannelStop(this.streamTempo);
                            Bass.BASS_ChannelSetPosition(this.streamTempo, Bass.BASS_ChannelGetPosition(this.streamTempo, BASSMode.BASS_POS_BYTES), BASSMode.BASS_POS_BYTES);
                            Bass.BASS_ChannelSetAttribute(this.streamDirection, BASSAttribute.BASS_ATTRIB_REVERSE_DIR, (float)BASSFXReverse.BASS_FX_RVS_FORWARD);
                            Bass.BASS_ChannelPlay(this.streamTempo, false);
                        }

                        Bass.BASS_ChannelSlideAttribute(this.streamTempo, BASSAttribute.BASS_ATTRIB_FREQ, this.currentFreq, this.timer);
                    }
                }

                if (this.isTempoOrPitch)
                {
                    Bass.BASS_ChannelSetAttribute(this.streamTempo, BASSAttribute.BASS_ATTRIB_TEMPO, this.tempo);
                    Bass.BASS_ChannelSetAttribute(this.streamTempo, BASSAttribute.BASS_ATTRIB_TEMPO_PITCH, 0);
                }
                else
                {
                    Bass.BASS_ChannelSetAttribute(this.streamTempo, BASSAttribute.BASS_ATTRIB_TEMPO, 0);
                    Bass.BASS_ChannelSetAttribute(this.streamTempo, BASSAttribute.BASS_ATTRIB_TEMPO_PITCH, this.tempo);
                }

                ////thats a bass problem it seems that there is no zero for the frequency
                ////the range is from 100-100000
                ////to avoid some bad sounds while holding the plate we need to stop the stream
                if (this.currentFreq < -100 || this.currentFreq > 100)
                {
                    Bass.BASS_ChannelPlay(this.streamTempo, false);
                }
                else
                {
                    Bass.BASS_ChannelStop(this.streamTempo);
                }

                this.playAndStop.Toggle(true);
            }
            else
            {
                Bass.BASS_ChannelStop(this.streamTempo);
                this.playAndStop.Toggle(false);
            }

            this.playAndStop.Draw(this.spriteBatch, buttonColor, this.blocked);

            if (this.state.Scratch == true)
            {
                this.mode.Toggle(true);
            }
            else
            {
                this.mode.Toggle(false);
            }

            this.mode.Draw(this.spriteBatch, buttonColor, this.blocked);

            this.spriteBatch.Draw(this.tempoBg, new Microsoft.Xna.Framework.Rectangle(975, 212, this.tempoBg.Width, this.tempoBg.Height), Color.White);

            if (this.isTempoOrPitch == true)
            {
                this.setTempoOrPitch.Toggle(false);
            }
            else
            {
                this.setTempoOrPitch.Toggle(true);
            }

            this.setTempoOrPitch.Draw(this.spriteBatch, buttonColor, this.blocked);
            this.jmpToZero.Draw(this.spriteBatch, buttonColor, this.blocked);
            this.tempoSlider.Draw(this.spriteBatch, buttonColor, this.blocked);
            if (this.showTempo)
            {
                this.spriteBatch.DrawString(this.spriteFontSmall, string.Format("{0:0.#}", ((this.tempo < 0.0f) ? -this.tempo : this.tempo)), new Vector2(this.tempoSlider.GetPosition().X - 11, this.tempoSlider.GetPosition().Y + (this.tempoSlider.GetHeight() / 2) - 11), this.tempoColor);
            }

            this.backward.Draw(this.spriteBatch, buttonColor, this.blocked);
            this.forward.Draw(this.spriteBatch, buttonColor, this.blocked);
            this.setLoopIn.Draw(this.spriteBatch, buttonColor, this.blocked);
            this.setLoopOut.Draw(this.spriteBatch, buttonColor, this.blocked);
            this.playCue.Draw(this.spriteBatch, buttonColor, this.blocked);
            if (this.stream != 0)
            {
                this.angle = (float)Math.PI / 2 * 4;
                if (this.wf != null)
                {
                    for (int i = this.framePos - 1; i >= 0; i--)
                    {
                        Vector2 origin;
                        ////the center of a waveform part

                        origin.X = this.waveformDisplayer[i].Width / 2;
                        origin.Y = this.waveformDisplayer[i].Height / 2;

                        ////the radius for an archimedical spiral
                        this.radius = (12.0f * this.angle) + 1;

                        ////calculate the next position
                        this.currentPos.X = 480 + (this.radius * (float)Math.Cos(this.scratchAngle + this.angle));
                        this.currentPos.Y = 480 + (this.radius * (float)Math.Sin(this.scratchAngle + this.angle));

                        ////the coord in the spiral
                        this.waveformCoords[i].X = (int)this.currentPos.X;
                        this.waveformCoords[i].Y = (int)this.currentPos.Y;

                        this.waveformAngle[i] = this.angle + this.scratchAngle - MathHelper.ToRadians(-90);

                        if (this.loopIn != -1)
                        {
                            this.loopInCoord.X = this.waveformCoords[this.loopInFrame].X - (this.loopInPosIcon.Width / 2);
                            this.loopInCoord.Y = this.waveformCoords[this.loopInFrame].Y - (this.loopInPosIcon.Height / 2);
                        }

                        if (this.loopOut != -1)
                        {
                            this.loopOutCoord.X = this.waveformCoords[this.loopOutFrame].X - (this.loopOutPosIcon.Width / 2);
                            this.loopOutCoord.Y = this.waveformCoords[this.loopOutFrame].Y - (this.loopOutPosIcon.Height / 2);
                        }
                        ////
                        ////the coords in direction x, this one is neccessary because the lib is only
                        //// able to handle position changing on a waveform in x direction, pretty shitty

                        this.realCurrentX[i] = i;
                        ////draw a waveform part
                        if (i < this.currentFrame)
                        {
                            this.spriteBatch.Draw(this.waveformDisplayer[i], new Microsoft.Xna.Framework.Rectangle((int)this.currentPos.X, (int)this.currentPos.Y, 2, 50), null, Color.DimGray, this.angle + this.scratchAngle - MathHelper.ToRadians(-90), origin, SpriteEffects.None, 0);
                        }
                        else
                        {
                            this.spriteBatch.Draw(this.waveformDisplayer[i], new Microsoft.Xna.Framework.Rectangle((int)this.currentPos.X, (int)this.currentPos.Y, 2, 50), null, Color.White, this.angle + this.scratchAngle - MathHelper.ToRadians(-90), origin, SpriteEffects.None, 0);
                        }

                        this.angle += 0.01f;
                    }

                    ////this calculates the current position which is used for the icon following the waveform
                    if ((!this.state.Paused) && this.len != 0 && this.framePos != 0)
                    {
                        this.currentFrameBytes = Bass.BASS_ChannelGetPosition(this.streamTempo);
                        this.currentFrameSecond = Bass.BASS_ChannelBytes2Seconds(this.streamTempo, this.currentFrameBytes);
                        this.currentFrame = this.wf.Position2Frames(this.currentFrameSecond);
                        this.currentFrame = this.currentFrame / (this.len / this.framePos);

                        if (this.currentFrame >= this.framePos || this.currentFrame < 0)
                        {
                            this.currentFrame = 0;
                        }
                    }

                    this.spriteBatch.Draw(this.currentPosIcon, new Microsoft.Xna.Framework.Rectangle((int)this.waveformCoords[this.currentFrame].X, (int)this.waveformCoords[this.currentFrame].Y, this.currentPosIcon.Width, this.currentPosIcon.Height), null, Color.White, this.waveformAngle[this.currentFrame], new Vector2(1, 25), SpriteEffects.None, 0);
                    this.spriteBatch.Draw(this.loopInPosIcon, new Microsoft.Xna.Framework.Rectangle((int)this.loopInCoord.X, (int)this.loopInCoord.Y, this.loopInPosIcon.Width, this.loopInPosIcon.Height), complementColor);
                    this.spriteBatch.Draw(this.loopOutPosIcon, new Microsoft.Xna.Framework.Rectangle((int)this.loopOutCoord.X, (int)this.loopOutCoord.Y, this.loopOutPosIcon.Width, this.loopOutPosIcon.Height), complementColor);  
                }
            }

            ////menu
            this.spriteBatch.Draw(this.additionalBg, new Microsoft.Xna.Framework.Rectangle((int)this.additionalBgPos.X, (int)this.additionalBgPos.Y, this.additionalBg.Width, this.additionalBg.Height), Color.White);

            this.close.Draw(this.spriteBatch, buttonColor, this.blocked);
            this.file.Draw(this.spriteBatch, buttonColor, this.blocked);
            this.setup.Draw(this.spriteBatch, buttonColor, this.blocked);

            this.spriteBatch.End();
            ////f.Draw();
            base.Draw(gameTime);
        }

        #endregion

        #region update mouse

        private Vector2 UpdateMouse()
        {
            MouseState mouseState = Mouse.GetState();
            //// The mouse X and Y positions are returned relative to the top-left of the game window.
            return new Vector2(mouseState.X, mouseState.Y);
        }

        #endregion

        #region onmouseover

        private bool OnMouseOver()
        {
            if (this.waveformCoords != null)
            {
                if ((this.mousePosition.X >= this.waveformCoords[this.currentFrame].X - (this.currentPosIcon.Width / 2)) && this.mousePosition.X < (this.waveformCoords[this.currentFrame].X - (this.currentPosIcon.Width / 2) + this.currentPosIcon.Width) &&
                    this.mousePosition.Y >= this.waveformCoords[this.currentFrame].Y - (this.currentPosIcon.Height / 2) && this.mousePosition.Y < (this.waveformCoords[this.currentFrame].Y - (this.currentPosIcon.Height / 2) + this.currentPosIcon.Height))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

        #endregion

        #region frameonmouseover

        private bool FrameOnMouseOver()
        {
            if (this.waveformCoords != null && this.waveformCoords[0].X != 0)
            {
                for (int i = 0; i < this.framePos; i++)
                {
                    if ((this.mousePosition.X >= this.waveformCoords[i].X - (this.waveformDisplayer[i].Width / 2)) && this.mousePosition.X < (this.waveformCoords[i].X - (this.waveformDisplayer[i].Width / 2) + this.waveformDisplayer[i].Width) &&
                            this.mousePosition.Y >= this.waveformCoords[i].Y - (this.waveformDisplayer[i].Height / 2) && this.mousePosition.Y < (this.waveformCoords[i].Y - (this.waveformDisplayer[i].Height / 2) + this.waveformDisplayer[i].Height))
                    {
                        this.selectedFrame = i;
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion

        #region checkmouseclick

        private bool CheckMouseClick()
        {
            MouseState mouseState = Mouse.GetState();
            if (mouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region getwave

        private void GetWave()
        {
            this.wf = new Un4seen.Bass.Misc.WaveForm(this.track, new WAVEFORMPROC(this.WaveFormCallback), null);
            this.wf.FrameResolution = 0.01f; //// neccesarry to fit the angle increase
            this.wf.CallbackFrequency = 250;
            this.wf.ColorBackground = System.Drawing.Color.FromArgb(0, System.Drawing.Color.White); //// transparent

            ////wf.ColorBackground = System.Drawing.Color.Black;
            this.wf.ColorLeft = System.Drawing.Color.White;

            this.wf.ColorLeftEnvelope = System.Drawing.Color.White;
            this.wf.ColorRight = System.Drawing.Color.White;
            this.wf.ColorRightEnvelope = System.Drawing.Color.White;

            this.wf.DrawWaveForm = WaveForm.WAVEFORMDRAWTYPE.Stereo;
            this.wf.RenderStart(true, BASSFlag.BASS_DEFAULT);
        }

        #endregion

        #region wave callback

        private void WaveFormCallback(int framesDone, int framesTotal, TimeSpan elapsedTime, bool finished)
        {
            if (finished)
            {
                //// if neccesary we can do something after the track is loaded fully
            }

            this.len = this.wf.Wave.data.Length;

            int frames_per_picture = (int)Math.Ceiling((double)this.len / (double)this.totalTextures);

            this.newTotalTextures = this.len / frames_per_picture;

            for (int i = this.framePos; i < (int)Math.Ceiling((double)framesDone / (double)frames_per_picture); i++)
            {
                ////that's kind of a special case: when a track is too short
                if (frames_per_picture == 1)
                {
                    this.wavepoints = this.wf.CreateBitmap(2, 50, (i * frames_per_picture), (i * frames_per_picture) + frames_per_picture, true);
                }
                else
                {
                    this.wavepoints = this.wf.CreateBitmap(2, 50, (i * frames_per_picture), (i * frames_per_picture) + frames_per_picture - 1, true);
                }

                this.waveformDisplayer[i] = new Texture2D(this.GraphicsDevice, 2, 50, 0, TextureUsage.None, SurfaceFormat.Color);

                Color[] dots = new Color[this.wavepoints.Width * this.wavepoints.Height];

                this.waveformDisplayer[i].SetData(dots);

                this.data = this.wavepoints.LockBits(new System.Drawing.Rectangle(0, 0, this.wavepoints.Width, this.wavepoints.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, this.wavepoints.PixelFormat);
                int bufferSize = this.data.Height * this.data.Stride; //// stride already incorporates 4 bytes per pixel

                //// create buffer
                byte[] bytes = new byte[bufferSize];
                //// copy bitmap data into buffer
                Marshal.Copy(this.data.Scan0, bytes, 0, bytes.Length);

                //// copy our buffer to the texture
                this.waveformDisplayer[i].SetData(bytes);

                //// unlock the bitmap data
                this.wavepoints.UnlockBits(this.data);
            }
            ////set the current position this will get rendered parallel in the draw method
            this.framePos = (int)Math.Floor((double)framesDone / (double)frames_per_picture);
        }

        #endregion

        #region EventHandling

        ////check if the user clicked the start/stop button
        private void PlayAndStopAction(object sender, EventArgs e)
        {   
            ////alternate the button image
            if (this.playAndStop.GetAlternative() && !this.isAllreadyClicked)
            {
                this.state.Play = false;
                this.currentFreq = 0;
                ////depends on the user if he wants to restart at the beginning or only start and stop 
                ////at the current position
                Bass.BASS_ChannelSetPosition(this.streamTempo, 0);
                this.isAllreadyClicked = true; 
            }
            else if (!this.playAndStop.GetAlternative() && !this.isAllreadyClicked)
            {
                this.state.Play = true;
                if (this.info != null)
                {
                    this.currentFreq = this.info.freq;
                }

                this.isAllreadyClicked = true;
            }
        }

        ////check if the user clicked the mode button
        private void ModeAction(object sender, EventArgs e)
        {
            ////alternate the button image
            if (this.mode.GetAlternative() && !this.isAllreadyClicked)
            {
                this.state.Scratch = false;
                this.isAllreadyClicked = true;
            }
            else if (!this.mode.GetAlternative() && !this.isAllreadyClicked)
            {
                this.state.Scratch = true;
                this.isAllreadyClicked = true;
            }
        }
        ////check if the user clicked the tempo slider
        private void TempoAction(object sender, EventArgs e)
        {
            if (this.firstContact == true)
            {
                this.newMousePosition.Y = this.mousePosition.Y;
            }

            this.firstContact = false;
            ////compare old and new mouse positions and set the slider
            if (this.tempoSlider.GetPosition().Y >= 250 && this.tempoSlider.GetPosition().Y <= 650)
            {
                if ((this.mousePosition.Y > (this.tempoSlider.GetPosition().Y + this.tempoSlider.GetHeight())) || (this.mousePosition.Y < this.tempoSlider.GetPosition().Y))
                {
                    this.mousePosition.Y = this.newMousePosition.Y;
                }

                if ((this.newMousePosition.Y > (this.tempoSlider.GetPosition().Y + this.tempoSlider.GetHeight())) || (this.newMousePosition.Y < this.tempoSlider.GetPosition().Y))
                {
                    this.newMousePosition.Y = this.mousePosition.Y;
                }

                ////if:down, else:up
                if (this.mousePosition.Y > this.newMousePosition.Y)
                {
                    float diff = this.mousePosition.Y - this.newMousePosition.Y;
                    this.tempoSlider.SlideToPosition(0, (int)diff);
                    this.tempo = -((this.tempoSlider.GetPosition().Y - 450) / 20);
                }
                else if (this.mousePosition.Y < this.newMousePosition.Y) 
                {
                    float diff = this.newMousePosition.Y - this.mousePosition.Y;
                    this.tempoSlider.SlideToPosition(0, -(int)diff);
                    this.tempo = -(this.tempoSlider.GetPosition().Y - 450) / 20;
                }
            }
            else if (this.tempoSlider.GetPosition().Y > 650)
            {
                this.tempoSlider.SetPosition((int)this.tempoSlider.GetPosition().X, 650);
                this.tempo = -10;
            }
            else
            {
                this.tempoSlider.SetPosition((int)this.tempoSlider.GetPosition().X, 250);
                this.tempo = 10;
            }

            if (this.tempoSlider.GetPosition().Y == 450)
            {
                this.jmpToZero.ChangeTex(false);
            }
            else
            {
                this.jmpToZero.ChangeTex(true);
            }

            this.showTempo = true;
        }

        private void BackwardAction(object sender, EventArgs e)
        {
            if (!this.isAllreadyClicked && !this.state.Reverse)
            {
                Bass.BASS_ChannelStop(this.streamTempo);
                Bass.BASS_ChannelSetPosition(this.streamTempo, Bass.BASS_ChannelGetPosition(this.streamTempo, BASSMode.BASS_POS_BYTES), BASSMode.BASS_POS_BYTES);
                Bass.BASS_ChannelSetAttribute(this.streamDirection, BASSAttribute.BASS_ATTRIB_REVERSE_DIR, (float)BASSFXReverse.BASS_FX_RVS_REVERSE);
                Bass.BASS_ChannelPlay(this.streamTempo, false);
                this.state.Reverse = true;
                this.isAllreadyClicked = true;
            }
        }

        private void ForwardAction(object sender, EventArgs e)
        {
            if (!this.isAllreadyClicked && this.state.Reverse)
            {
                Bass.BASS_ChannelStop(this.streamTempo);
                Bass.BASS_ChannelSetPosition(this.streamTempo, Bass.BASS_ChannelGetPosition(this.streamTempo, BASSMode.BASS_POS_BYTES), BASSMode.BASS_POS_BYTES);
                Bass.BASS_ChannelSetAttribute(this.streamDirection, BASSAttribute.BASS_ATTRIB_REVERSE_DIR, (float)BASSFXReverse.BASS_FX_RVS_FORWARD);
                Bass.BASS_ChannelPlay(this.streamTempo, false);
                this.state.Reverse = false;
                this.isAllreadyClicked = true;
            }
        }

        private void CuePlayAction(object sender, EventArgs e)
        {
            if (!this.isAllreadyClicked)
            {
                this.isAllreadyClicked = true;
                if (this.loopIn != -1)
                {
                    Bass.BASS_ChannelSetPosition(this.streamTempo, Bass.BASS_ChannelBytes2Seconds(this.streamTempo, this.loopIn));
                }
            }
        }

        private void FileAction(object sender, EventArgs e)
        {
            if (!this.isAllreadyClicked)
            {
                this.graphics.ToggleFullScreen();
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "All Audio Files |*.mp3;*.wma;*.aiff;*.ogg;*.wav;*.mp4;*.aac;*.flac|MPEG-1 Audio Layer 3 (*.mp3)|*.mp3|Windows Media Audio (.wma)|*.wma|Audio Interchange File Format (*.aiff)|*.aiff|OGG Vorbis (*.ogg)|*.ogg|Audio for Windows (*.wav)|*.wav|AAC/MP4 (*.aac,*.mp4)|*.aac;*.mp4|Free Lossless Audio Codec (*.flac) |*.flac";
                openFileDialog.FileName = this.track;
                if (DialogResult.OK == openFileDialog.ShowDialog())
                {
                    if (File.Exists(openFileDialog.FileName))
                    {
                        this.track = openFileDialog.FileName;
                        ////load the desired track to the stream
                        if (this.track != String.Empty)
                        {
                            Bass.BASS_ChannelStop(this.streamTempo);
                            Bass.BASS_StreamFree(this.streamTempo);
                            Bass.BASS_StreamFree(this.streamDirection);
                            Bass.BASS_StreamFree(this.stream);
                            this.framePos = 0;
                            this.ResetLoopOut();
                            this.ResetLoopIn();
                            this.scratchAngle = 0;
                            this.trackinfo = new FileInfo(this.track);
                            if (this.trackinfo.Extension.ToLower() == ".wma")
                            {
                                this.stream = BassWma.BASS_WMA_StreamCreateFile(this.track, 0, 0, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_STREAM_PRESCAN);
                            }
                            else if (this.trackinfo.Extension.ToLower() == ".mp4")
                            {
                                this.stream = BassAac.BASS_MP4_StreamCreateFile(this.track, 0, 0, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_STREAM_PRESCAN);
                            }
                            else if (this.trackinfo.Extension.ToLower() == ".aac")
                            {
                                this.stream = BassAac.BASS_AAC_StreamCreateFile(this.track, 0, 0, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_STREAM_PRESCAN);
                            }
                            else if (this.trackinfo.Extension.ToLower() == ".flac")
                            {
                                this.stream = BassFlac.BASS_FLAC_StreamCreateFile(this.track, 0, 0, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_STREAM_PRESCAN);
                            }
                            else
                            {
                                this.stream = Bass.BASS_StreamCreateFile(this.track, 0, 0, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_STREAM_PRESCAN);
                            }

                            this.streamDirection = BassFx.BASS_FX_ReverseCreate(this.stream, 1, BASSFlag.BASS_STREAM_DECODE);
                            this.streamTempo = BassFx.BASS_FX_TempoCreate(this.streamDirection, BASSFlag.BASS_FX_FREESOURCE | BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_SAMPLE_LOOP);

                            Bass.BASS_ChannelSetAttribute(this.streamDirection, BASSAttribute.BASS_ATTRIB_REVERSE_DIR, (float)BASSFXReverse.BASS_FX_RVS_FORWARD);
                            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_BUFFER, 40);
                            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_UPDATEPERIOD, 5);

                            ////we need to retrieve the samplerate of the track
                            ////because we change it during scratching and afterwards we set it back
                            ////not every track has a samplerate of 44100
                            this.info = Bass.BASS_ChannelGetInfo(this.streamTempo);

                            this.currentFreq = this.info.freq;
                            ////get the waveform
                            this.GetWave();

                            this.len = this.wf.Wave.data.Length;
                            int frames_per_picture = (int)Math.Ceiling((double)this.len / (double)this.totalTextures);
                            double duration = Bass.BASS_ChannelBytes2Seconds(this.streamTempo, Bass.BASS_ChannelGetLength(this.streamTempo)) * 1000;

                            this.state.Play = true;
                        }
                        else
                        {
                            this.track = String.Empty;
                        }
                    }
                }

                this.graphics.ToggleFullScreen();
            }
        }

        private void CloseAction(object sender, EventArgs e)
        {
            this.shutdownComp.Show();
        }

        private void SetupAction(object sender, EventArgs e)
        {
            this.setupComp.Show();
        }

        private void SetTempoOrPitchAction(object sender, EventArgs e)
        {
            ////alternate the button image
            if (this.setTempoOrPitch.GetAlternative() && !this.isAllreadyClicked)
            {
                this.isTempoOrPitch = true;
                this.isAllreadyClicked = true;
            }
            else if (!this.setTempoOrPitch.GetAlternative() && !this.isAllreadyClicked)
            {
                this.isTempoOrPitch = false;
                this.isAllreadyClicked = true;
            }
        }

        private void JmpToZeroAction(object sender, EventArgs e)
        {
            if (this.tempoSlider.GetPosition().Y != 450)
            {
                ////tempoSlider.SetPosition((int)this.tempoSlider.GetPosition().X, 450);
                this.jmpToZeroSlider = new Slide(this.tempoSlider.GetPosition().Y, 450, 0.5f);
                this.tempo = 0;
                this.jmpToZero.ChangeTex(false);
            }
        }

        private void PlateAction(object sender, EventArgs e)
        {
            if (this.state.Scratch && !this.state.Reverse && !this.slided)
            {
                this.oldPlateAngle = this.plateAngleHelper;

                this.dx = this.mousePosition.X - this.scratchCenter.X;
                this.dy = this.mousePosition.Y - this.scratchCenter.Y;
                this.plateAngle = 180 - MathHelper.ToDegrees((float)Math.Atan2((double)this.dx, (double)this.dy));

                ////oldPlateAngle is 0 when the User clicks the first time
                ////the plate jumps 
                if ((this.plateAngle - this.oldPlateAngle <= -180) || (this.plateAngle - this.oldPlateAngle >= 180))
                {
                    if (this.oldPlateAngle != 0)
                    {
                        this.scratchAngle += MathHelper.ToRadians(360 + this.plateAngle - this.oldPlateAngle);
                        if (this.info != null)
                        {
                            this.currentFreq = (int)((this.info.freq / 4) * (360 - this.plateAngle - this.oldPlateAngle));
                        }
                    }
                }
                else
                {
                    if (this.oldPlateAngle != 0)
                    {
                        this.scratchAngle += MathHelper.ToRadians(this.plateAngle - this.oldPlateAngle);
                        if (this.info != null)
                        {
                            this.currentFreq = (int)((this.info.freq / 4) * (this.plateAngle - this.oldPlateAngle));
                        }
                    }
                }

                if (this.info != null)
                {
                    if (Math.Abs(this.currentFreq) > 1.1337f * this.info.freq)
                    {
                        if (this.currentFreq > 100)
                        {
                            this.currentFreq = 1.1337f * this.info.freq;
                        }
                        else if (this.currentFreq < -100)
                        {
                            this.currentFreq = 1.1337f * -this.info.freq;
                        }
                    }
                }

                if (this.currentFrame >= this.loopOutFrame) 
                {
                    this.ResetLoopOut(); 
                }

                this.plateAngleHelper = this.plateAngle;   
            }
        }

        private void SetLoopInAction(object sender, EventArgs e)
        {
            this.loopInFrame = this.currentFrame;
            this.loopInCoord.X = this.waveformCoords[this.currentFrame].X - (this.loopInPosIcon.Width / 2);
            this.loopInCoord.Y = this.waveformCoords[this.currentFrame].Y - (this.loopInPosIcon.Height / 2);
            this.loopIn = Bass.BASS_ChannelGetPosition(this.streamTempo);
            if (this.loopInFrame > this.loopOutFrame && this.loopOut != -1)
            {
                this.ResetLoopOut();
            }
        }

        private void SetLoopOutAction(object sender, EventArgs e)
        {
            if (this.currentFrame > this.loopInFrame)
            {
                if (!this.isAllreadyClicked)
                {
                    this.looping = (!this.looping) ? true : false;
                }

                if (this.looping)
                {
                    this.loopOutFrame = this.currentFrame;
                    this.loopOutCoord.X = this.waveformCoords[this.currentFrame].X - (this.loopOutPosIcon.Width / 2);
                    this.loopOutCoord.Y = this.waveformCoords[this.currentFrame].Y - (this.loopOutPosIcon.Height / 2);
                    this.loopOut = Bass.BASS_ChannelGetPosition(this.streamTempo);
                }
                else 
                { 
                    this.ResetLoopOut(); 
                }
            }

            this.isAllreadyClicked = true;
        }

        #endregion

        ////check for an overlayed component that is blocking the main window
        private bool CheckBlocking()
        {
            foreach (Container c in this.componentList)
            {
                if (c.IsBlocking())
                {
                    return true;
                }
            }

            return false;
        }

        private void ResetLoopOut()
        {
            this.loopOut = -1;
            this.loopOutCoord.X = -100;
            this.loopOutCoord.Y = 0;
            this.looping = false;
        }

        private void ResetLoopIn()
        {
            this.loopIn = 0;
            this.loopInCoord.X = this.waveformCoords[0].X;
            this.loopInCoord.Y = this.waveformCoords[0].Y;
            this.looping = false;
        }

        #region start

        /**
        * starts the CurlTable
        */
        public static void Start()
        {
            using (CurlTable table = new CurlTable())
            { 
                table.Run(); 
            }
        }

        #endregion
    }
}
