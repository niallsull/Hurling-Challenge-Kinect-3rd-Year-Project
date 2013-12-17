using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BEPUphysics.Entities.Prefabs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KinectWithGameState
{
    class BallObject
    {          // same as boxObject, except for sphere
        public Sphere sphere;
        public Vector3 position;
        Model model;

        public BallObject(Vector3 p, Model m, float r)
        {
            position = p;
            model = m;
            sphere = new Sphere(p, r, 0.4f);

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
                        1.0f, 10000.0f);

                }

                // Draw the mesh, using the effects set above.
                mesh.Draw();
            }

        }

        public void Update()
        {
            position = sphere.Position; // we need to draw the ball in the same spot that the physical ball is
        }

    }
}
