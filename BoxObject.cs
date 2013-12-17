using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BEPUphysics.Entities.Prefabs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KinectWithGameState
{
    class BoxObject    // for models that need a box around them for physics
    {                   // the stadium pitch

        public Box box;
        Model model;
        Vector3 position;

        public BoxObject(Vector3 p, Model m, float w, float h, float l)
        {

            //straightforward stuff
            box = new Box(position, w, h, l);
            position = p;
            model = m;

        }

        public void updatePosition()
        {

        }

        public void Draw(Vector3 camPosition, Vector3 camSpot, float aspectRatio)
        {
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in model.Meshes)
            {
                // This is where the mesh orientation is set, as well 
                // as our camera and projection.
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.World = transforms[mesh.ParentBone.Index]
                        * Matrix.CreateTranslation(position);


                    effect.View = Matrix.CreateLookAt(camPosition,
                        camSpot, Vector3.Up);


                    effect.Projection = Matrix.CreatePerspectiveFieldOfView(
                        MathHelper.ToRadians(45.0f), aspectRatio,
                        1.0f, 10000.0f);     // projection , near plane, far plane

                }

                // Draw the mesh, using the effects set above.
                mesh.Draw();
            }

        }
    }
}
