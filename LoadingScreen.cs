using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace KinectWithGameState
{
    class LoadingScreen
    {
        Texture2D[] Screens;

       //this originally had around 4 loading screens for the level, david did some art, and the originals didn't fit the new theme

        public LoadingScreen(ContentManager C)
        {

            Screens = new Texture2D[2];
           
            Screens[0]=C.Load<Texture2D>("Images\\TitleScreen");  // title screen
            Screens[1]=C.Load<Texture2D>("Images\\HurlingChallengeScreen");  // level load screen
            

        }

        //public void upDate()
        //{
        //    inc++;

        //    if (inc % 15 == 0)
        //        frameNum++;
                                               // for moving the loading dot
        //    if (frameNum == 3)
        //    {
        //        frameNum = 0;
        //        inc = 0;
        //    }
        //}

        public void DrawStart(SpriteBatch s)
        {
            s.Draw(Screens[0], new Vector2(0, 0), Color.White);

           // s.Draw(Screens[5], new Vector2(900+(33*frameNum), 600),new Rectangle(33*frameNum,0,33,40), Color.White);
            // don't draw the loading dot anymore
            //this.upDate();
        }


        public void DrawLoad(SpriteBatch s,int r)
        {
            s.Draw(Screens[r%2], new Vector2(0, 0), Color.White);

            //s.Draw(Screens[5], new Vector2(900+(33*frameNum), 600), new Rectangle(33 * frameNum, 0, 33, 40), Color.White);
           // this.upDate();
        }
    }
}
