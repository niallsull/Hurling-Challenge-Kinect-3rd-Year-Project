using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Kinect;

namespace KinectWithGameState
{
    class Screen
    {
        public Texture2D image;
        public Vector2 position;


        public Screen()
        {
            position = new Vector2(0, 0);

        }

        public Screen(Vector2 pos)
        {

            position = pos;
        }


        public void Draw(SpriteBatch spritebatch)
        {
            spritebatch.Draw(image, position, Color.White);
        }


    }
}
