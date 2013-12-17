using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BEPUphysics.Entities.Prefabs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KinectWithGameState
{                              // same as boObject but for the goals,  would be a bit more useful if we were using two goals
                            // would validate using a goal class, that would probably be used by a stadium class or pitch class
                            // essentially is a goal class
                            
    class GoalPostObject
    {
        public Cylinder post1;
        public Cylinder post2;
        Vector3 position;
        Model model;

        public GoalPostObject(Vector3 p, Model m, int h, float r)
        {
            position = p;
            model = m;
            post1 = new Cylinder(new Vector3(p.X - 6.5f, h / 2, p.Z), h, r);
            post2 = new Cylinder(new Vector3(p.X + 6.5f, h / 2, p.Z), h, r);



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
    }
}
