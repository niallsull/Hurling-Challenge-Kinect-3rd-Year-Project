using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace KinectWithGameState
{
    class InstructionSlides
    {
        Texture2D[] slides;
        int timer;


        public InstructionSlides(ContentManager C)
        {
            slides = new Texture2D[5];
            slides[0] = C.Load<Texture2D>("Instructions\\Rotate");
            slides[1] = C.Load<Texture2D>("Instructions\\Trajectory");
            slides[2] = C.Load<Texture2D>("Instructions\\power");
            slides[3] = C.Load<Texture2D>("Instructions\\squat");
            slides[4] = C.Load<Texture2D>("Instructions\\swing");
            timer = 0;
        }

        public int update()
        {
            timer++;

            int ret = 0;

            if (timer / 200 > 4)
                ret = 1;

            return ret;
        }

        public void draw(SpriteBatch s)
        {
            
            s.Draw(slides[timer / 200], new Vector2(0, 0), Color.White);
        }


    }
}
