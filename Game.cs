using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Kinect;          // access to kinect

namespace KinectWithGameState
{
    
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        KinectSensor kinect;          // actual sensor
        Skeleton skeleton;
        Skeleton[] skeletonData;
        Joint leftHand, rightHand;
        Vector2 positionRight, positionLeft;

        enum GameStatus { beforeMain,mainScreen,instructLoading, instructions, loading, playing,nexLevel,credits };
        GameStatus status = GameStatus.mainScreen;

        Screen main, nextLevelScreen,creditScreen;
        LoadingScreen loadingScreen;
        Random rand;
        Texture2D handRight,handLeft,mainMenu,nextLv;
        

        int levelStatus,beforeTime,randomNum;

        Level level;       // actual gameplay
        int loadingTimer,creditTimer;
        InstructionSlides instructions;  // wahey, a slideshow :)

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 720; 
        }

        
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            kinect = KinectSensor.KinectSensors[0];   // first sensor of tose that may be connected

            if (kinect != null)              // make sure we have a sensor
            {
                
                kinect.SkeletonStream.Enable();   // enable skelton stream to track the players body
            }
            kinect.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(kinect_AllFramesReady);
            kinect.Start();
            kinect.ElevationAngle =10;          // normally on a low table, need to see above head as well


            status = GameStatus.beforeMain;            // we start with a titlescreen
            main = new Screen();

            loadingTimer = 0;                      // timers for when we look at the instructions and credits
            creditTimer = 0;

            rand = new Random();              // random generator
            beforeTime = 0;
            nextLevelScreen = new Screen();
            creditScreen = new Screen();
            randomNum = rand.Next(50);
            
            base.Initialize();
        }



        void kinect_AllFramesReady(object sender, AllFramesReadyEventArgs imageFrame)
        {

            using (SkeletonFrame skeletonFrame = imageFrame.OpenSkeletonFrame())
            {                    // get the sensor to start looking at skeleton data
                if (skeletonFrame != null)
                {

                    if ((skeletonData == null) || (this.skeletonData.Length != skeletonFrame.SkeletonArrayLength))
                    {
                        this.skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    }

                    skeletonFrame.CopySkeletonDataTo(this.skeletonData);     // copy into our skeleton array

                }

                if (skeletonData != null)
                {
                    foreach (Skeleton skel in skeletonData)
                    {
                        if (skel.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            skeleton = skel;       // our single skeleton is the one that is currently being tracked
                        }
                    }

                }
                
            }
        }
        
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
           
            main.image = Content.Load<Texture2D>("Images\\MenuScreen");
            
            nextLevelScreen.image = Content.Load<Texture2D>("Images\\EndLevelScreen");
            creditScreen.image = Content.Load<Texture2D>("Images\\CreditsPage");

            handRight = Content.Load<Texture2D>("Images\\handRight");
            handLeft = Content.Load<Texture2D>("Images\\handLeft");
            mainMenu = Content.Load<Texture2D>("Images\\MainMenu");
            nextLv = Content.Load<Texture2D>("Images\\NextLevel");
            loadingScreen = new LoadingScreen(Content);      // loads itself

            if (skeleton != null)
            {
                if (skeleton.Joints != null)
                {

                    leftHand = skeleton.Joints[JointType.HandLeft];     // going to constantly tracking these,
                    rightHand = skeleton.Joints[JointType.HandRight];  // need to draw them for the user
                }
            }
            // TODO: use this.Content to load your game content here
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();


            if (beforeTime > 200)      // how long we look at the title screen
            {
                status = GameStatus.mainScreen;
                beforeTime = 0;
            }

            if (status == GameStatus.mainScreen)
            {
                if (skeleton != null)
                {

                    if (skeleton.Joints != null)
                    {

                        leftHand = skeleton.Joints[JointType.HandLeft];
                        rightHand = skeleton.Joints[JointType.HandRight];


                        // constantly have to update the position of our hands, otherwise user will think input is wrong, as these are used for drawing as well
                        positionLeft = new Vector2((((0.5f * leftHand.Position.X) + 0.5f) * graphics.PreferredBackBufferWidth), (((-0.5f * leftHand.Position.Y) + 0.5f) * graphics.PreferredBackBufferHeight));
                        positionRight = new Vector2((((0.5f * rightHand.Position.X) + 0.5f) * graphics.PreferredBackBufferWidth), (((-0.5f * rightHand.Position.Y) + 0.5f) * graphics.PreferredBackBufferHeight));
                        if ( (Keyboard.GetState().IsKeyDown(Keys.P)) || ( (positionLeft.X>240) && (positionLeft.X<350) && (positionLeft.Y>100) && (positionLeft.Y<210) && (positionRight.X<790) && (positionRight.X>680) && (positionRight.Y<210) && (positionRight.Y>100)  )     )
                        {
                            // checking to see if the hands are in the top two dots, then start loading 
                            status = GameStatus.loading;
                            // hands can't actually be in the opposite side dot
                        }

                        else if ((((positionLeft.X > 240) && (positionLeft.X < 350) && (positionLeft.Y > 240) && (positionLeft.Y < 350) && (positionRight.X < 790) && (positionRight.X > 680) && (positionRight.Y < 350) && (positionRight.Y > 240))))
                        {
                            // check in middle two dots
                            status = GameStatus.instructLoading;

                        }

                        else if ((((positionLeft.X > 240) && (positionLeft.X < 350) && (positionLeft.Y > 380) && (positionLeft.Y < 490) && (positionRight.X < 790) && (positionRight.X > 680) && (positionRight.Y < 490) && (positionRight.Y > 380))))
                        {
                            // check bottom two
                            status = GameStatus.credits;
                            creditTimer = 0;

                        }
                    }

                }


            }

            if (status == GameStatus.loading)
            {
                
                loadingTimer++;

                // loading screen draws

                if (loadingTimer == 50)  // initialize the level in the background
                {
                    level = new Level(graphics);
                    level.Initializes(kinect,skeleton,skeletonData);
                    level.LoadContents(spriteBatch, this.Content,2.3f,0.0f);
                }

                /*
                if (loadingTimer == 200)
                {
                    level.LoadContents(spriteBatch);
                }
                */
                if (loadingTimer > 100)   // then go to the actual level
                {
                    loadingTimer = 0;
                    level.startCrowd();
                    status = GameStatus.playing;
                    
                }
            }


            if (status == GameStatus.instructLoading)
            {
                instructions = new InstructionSlides(Content);   // a lot less stuff to load in the instructions, so go straight to it.
                status = GameStatus.instructions;
            }

            if (status == GameStatus.instructions)
            {
               int ready= instructions.update();
               if (ready == 1)  // slideshow is finished , OH NOOO!!  http://www.youtube.com/watch?v=zzhVgWwS_gM
               {
                   status = GameStatus.mainScreen;
                   
               }
            }


            // TODO: Add your update logic here

            if (status == GameStatus.playing)
            {
               levelStatus = level.Update(gameTime);  // checking the current level, if finished
            }

            if (levelStatus == 1)
            {
                status = GameStatus.nexLevel; // go to next level screen
                levelStatus = 0;
                randomNum = rand.Next(50);
            }


            if (status == GameStatus.nexLevel)
            {

                if (skeleton != null)
                {

                    if (skeleton.Joints != null)
                    {

                       leftHand = skeleton.Joints[JointType.HandLeft];
                        rightHand = skeleton.Joints[JointType.HandRight];

                        positionLeft = new Vector2((((0.5f * leftHand.Position.X) + 0.5f) * graphics.PreferredBackBufferWidth), (((-0.5f * leftHand.Position.Y) + 0.5f) * graphics.PreferredBackBufferHeight));
                        positionRight = new Vector2((((0.5f * rightHand.Position.X) + 0.5f) * graphics.PreferredBackBufferWidth), (((-0.5f * rightHand.Position.Y) + 0.5f) * graphics.PreferredBackBufferHeight));
                        

                        if (positionRight.X > 800)
                        {   /// make a new level
                            
                            status = GameStatus.loading;
                        }
                        
                        if (positionLeft.X < 200)
                        {
                            level.Dispose();
                            status = GameStatus.mainScreen;  // back to main menu
                        }
                    }
                }


                if (status == GameStatus.credits)
                {
                    creditTimer=100;
                    if (creditTimer > 50)
                    {     // length look at credits
                        status = GameStatus.mainScreen;
                    }
                }


                
            }



            base.Update(gameTime);
        }

        
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            spriteBatch.Begin();

            if (status == GameStatus.beforeMain)
            {
                loadingScreen.DrawStart(spriteBatch);  // draw the title screen
                beforeTime++;
            }

            if (status == GameStatus.instructions)
            {
                instructions.draw(spriteBatch);       // let the slideshoe begin!!!!
            }

            if (status == GameStatus.mainScreen)
            {
                spriteBatch.Draw(main.image, main.position, Color.White);    // just the main menu
                drawSkeleton(spriteBatch);
            }

            if (status == GameStatus.loading)
            {

                loadingScreen.DrawLoad(spriteBatch,randomNum);  // loading screen before each level
                

            }

            if (status == GameStatus.playing)
            {
                level.Draw(spriteBatch);        // let the level do its own work
            }

            if (status == GameStatus.nexLevel)
            {
                spriteBatch.Draw(nextLevelScreen.image, nextLevelScreen.position, Color.White);      // originally one picture 
                spriteBatch.Draw(mainMenu, new Vector2(170,300), Color.White);     // but then david made cool text
                spriteBatch.Draw(nextLv, new Vector2(725, 300), Color.White);
                drawSkeleton(spriteBatch);         // draw the hands
            }


            if (status == GameStatus.credits)
            {
                creditTimer++;
                spriteBatch.Draw(creditScreen.image, creditScreen.position, Color.White);    // quick look at credits
            }


            spriteBatch.End();

            base.Draw(gameTime);
        }

        void drawSkeleton(SpriteBatch spriteBatch)
        {
            
                
            // draw the two hands, nothing complicated
                 
                spriteBatch.Draw(handLeft, new Rectangle(Convert.ToInt32(positionLeft.X), Convert.ToInt32(positionLeft.Y), 50, 50), Color.White);

                
                
                spriteBatch.Draw(handRight, new Rectangle(Convert.ToInt32(positionRight.X), Convert.ToInt32(positionRight.Y), 50, 50), Color.White);

            
        }
    }
}
