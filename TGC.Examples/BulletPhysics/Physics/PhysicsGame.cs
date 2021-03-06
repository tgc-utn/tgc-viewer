﻿using BulletSharp;
using TGC.Core.Mathematica;

namespace TGC.Examples.BulletPhysics.Physics
{
    public abstract class PhysicsGame
    {
        protected BulletExampleWall Ctx;
        protected BulletExampleBasic Ctx2;
        protected DiscreteDynamicsWorld dynamicsWorld;
        protected CollisionDispatcher dispatcher;
        protected DefaultCollisionConfiguration collisionConfiguration;
        protected SequentialImpulseConstraintSolver constraintSolver;
        protected BroadphaseInterface overlappingPairCache;

        public virtual void Init(BulletExampleWall ctx)
        {
            Ctx = ctx;
            //Creamos el mundo fisico por defecto.
            collisionConfiguration = new DefaultCollisionConfiguration();
            dispatcher = new CollisionDispatcher(collisionConfiguration);
            GImpactCollisionAlgorithm.RegisterAlgorithm(dispatcher);
            constraintSolver = new SequentialImpulseConstraintSolver();
            overlappingPairCache = new DbvtBroadphase(); //AxisSweep3(new BsVector3(-5000f, -5000f, -5000f), new BsVector3(5000f, 5000f, 5000f), 8192);
            dynamicsWorld = new DiscreteDynamicsWorld(dispatcher, overlappingPairCache, constraintSolver, collisionConfiguration);
            dynamicsWorld.Gravity = new TGCVector3(0, -20f, 0).ToBulletVector3();
        }

        public virtual void Init(BulletExampleBasic ctx)
        {
            Ctx2 = ctx;
            //Creamos el mundo fisico por defecto.
            collisionConfiguration = new DefaultCollisionConfiguration();
            dispatcher = new CollisionDispatcher(collisionConfiguration);
            GImpactCollisionAlgorithm.RegisterAlgorithm(dispatcher);
            constraintSolver = new SequentialImpulseConstraintSolver();
            overlappingPairCache = new DbvtBroadphase();
            //AxisSweep3(new BsVector3(-5000f, -5000f, -5000f), new BsVector3(5000f, 5000f, 5000f), 8192);
            dynamicsWorld = new DiscreteDynamicsWorld(dispatcher, overlappingPairCache, constraintSolver, collisionConfiguration);
            dynamicsWorld.Gravity = new TGCVector3(0, -20f, 0).ToBulletVector3();
        }

        public abstract void Update(float lastFrameTime, float timeBetweenFrames);

        public abstract void Render();

        public abstract void Dispose();

        public abstract int GetElements();
    }
}