using FixedMath;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Fenrir.ECS.Tests.Integration
{
    [TestClass]
    public unsafe class PerformanceTest
    {
        [TestMethod]
        public void TestPerformance()
        {
            int numEntities = 10000;
            int numFrames = 10;

            // ---------------------------------------
            // Test 1
            // ---------------------------------------
            // Data
            PositionComponent[] positionComponents = new PositionComponent[numEntities];
            VelocityComponent[] velocityComponents = new VelocityComponent[numEntities];
            RotationComponent[] rotationComponents = new RotationComponent[numEntities];
            SpinComponent[] spinComponents = new SpinComponent[numEntities];

            Random rnd = new Random();
            for (int i = 0; i < numEntities; i++)
            {
                velocityComponents[i].X = (Fixed)rnd.Next(-5, 5);
                velocityComponents[i].Y = (Fixed)rnd.Next(-5, 5);
                velocityComponents[i].Z = (Fixed)rnd.Next(-5, 5);

                spinComponents[i].X = (Fixed)rnd.Next(-5, 5);
                spinComponents[i].Y = (Fixed)rnd.Next(-5, 5);
                spinComponents[i].Z = (Fixed)rnd.Next(-5, 5);
            }

            // Run
            Stopwatch sw = new Stopwatch();
            sw.Start();

            for (int frame = 0; frame < numFrames; frame++)
            {
                for (int numEntity = 0; numEntity < numEntities; numEntity++)
                {
                    positionComponents[numEntity].X += velocityComponents[numEntity].X;
                    positionComponents[numEntity].Y += velocityComponents[numEntity].Y;
                    positionComponents[numEntity].Z += velocityComponents[numEntity].Z;
                }

                for (int numEntity = 0; numEntity < numEntities; numEntity++)
                {
                    rotationComponents[numEntity].X += spinComponents[numEntity].X;
                    rotationComponents[numEntity].Y += spinComponents[numEntity].Y;
                    rotationComponents[numEntity].Z += spinComponents[numEntity].Z;
                }
            }

            sw.Stop();
            Console.WriteLine("Naive loop took: " + sw.ElapsedMilliseconds);

            // ---------------------------------------
            // Test 2
            // ---------------------------------------
            var positionComponentSize = Marshal.SizeOf(typeof(PositionComponent));
            var velocityComponentSize = Marshal.SizeOf(typeof(VelocityComponent));
            var rotationComponentSize = Marshal.SizeOf(typeof(RotationComponent));
            var spinComponentSize = Marshal.SizeOf(typeof(SpinComponent));

            // data

            int blockSize = positionComponentSize + velocityComponentSize + rotationComponentSize + spinComponentSize;

            if(blockSize * numEntities > int.MaxValue)
            {
                throw new InvalidOperationException("Too many entities");
            }

            byte[] data = new byte[blockSize * numEntities];

            fixed (byte* dataBuffer = data)
            {
                for (int numEntity = 0; numEntity < numEntities; numEntity++)
                {
                    byte* blockStartPtr = dataBuffer + (blockSize * numEntity);

                    PositionComponent* positionComponentPtr = (PositionComponent*)(blockStartPtr);
                    VelocityComponent* velocityComponentPtr = (VelocityComponent*)(blockStartPtr + positionComponentSize);
                    RotationComponent* rotationComponentPtr = (RotationComponent*)(blockStartPtr + positionComponentSize + velocityComponentSize);
                    SpinComponent* spinComponentPtr = (SpinComponent*)(blockStartPtr + positionComponentSize + velocityComponentSize + rotationComponentSize);

                    velocityComponentPtr->X = (Fixed)rnd.Next(-5, 5);
                    velocityComponentPtr->Y = (Fixed)rnd.Next(-5, 5);
                    velocityComponentPtr->Z = (Fixed)rnd.Next(-5, 5);

                    spinComponentPtr->X = (Fixed)rnd.Next(5, 5);
                    spinComponentPtr->Y = (Fixed)rnd.Next(5, 5);
                    spinComponentPtr->Z = (Fixed)rnd.Next(5, 5);
                }
            }

            // Tightly packed array
            sw = new Stopwatch();
            sw.Start();

            fixed (byte* dataBuffer = data)
            {
                for (int frame = 0; frame < numFrames; frame++)
                {
                    for (int numEntity = 0; numEntity < numEntities; numEntity++)
                    {
                        byte* blockStartPtr = dataBuffer + (blockSize * numEntity);

                        PositionComponent* positionComponentPtr = (PositionComponent*)(blockStartPtr);
                        VelocityComponent* velocityComponentPtr = (VelocityComponent*)(blockStartPtr + positionComponentSize);
                        RotationComponent* rotationComponentPtr = (RotationComponent*)(blockStartPtr + positionComponentSize + velocityComponentSize);
                        SpinComponent* spinComponentPtr = (SpinComponent*)(blockStartPtr + positionComponentSize + velocityComponentSize + rotationComponentSize);

                        positionComponentPtr->X += velocityComponentPtr->X;
                        positionComponentPtr->Y += velocityComponentPtr->Y;
                        positionComponentPtr->Z += velocityComponentPtr->Z;

                        rotationComponentPtr->X += spinComponentPtr->X;
                        rotationComponentPtr->Y += spinComponentPtr->Y;
                        rotationComponentPtr->Z += spinComponentPtr->Z;
                    }
                }
            }

            sw.Stop();
            Console.WriteLine("Tightly packed array took: " + sw.ElapsedMilliseconds);

            // Check logic?

            fixed (byte* dataBuffer = data)
            {
                int numEntity = numEntities / 2; // half way through?
                byte* blockStartPtr = dataBuffer + (blockSize * numEntity);

                PositionComponent positionComponent = *(PositionComponent*)(blockStartPtr);
                VelocityComponent velocityComponent = *(VelocityComponent*)(blockStartPtr + positionComponentSize);
                RotationComponent rotationComponent = *(RotationComponent*)(blockStartPtr + positionComponentSize + velocityComponentSize);
                SpinComponent spinComponent = *(SpinComponent*)(blockStartPtr + positionComponentSize + velocityComponentSize + rotationComponentSize);

                Assert.AreEqual(velocityComponent.X * (Fixed)numFrames, positionComponent.X);
            }


            // ---------------------------------------
            // Test 3
            // ---------------------------------------
            // Data
            PositionComponentCls[] positionComponentArray = new PositionComponentCls[numEntities];
            VelocityComponentCls[] velocityComponentArray = new VelocityComponentCls[numEntities];
            RotationComponentCls[] rotationComponentArray = new RotationComponentCls[numEntities];
            SpinComponentCls[] spinComponentArray = new SpinComponentCls[numEntities];

            for (int i = 0; i < numEntities; i++)
            {
                positionComponentArray[i] = new PositionComponentCls();
                velocityComponentArray[i] = new VelocityComponentCls();
                rotationComponentArray[i] = new RotationComponentCls();
                spinComponentArray[i] = new SpinComponentCls();

                velocityComponentArray[i].X = (Fixed)rnd.Next(-5, 5);
                velocityComponentArray[i].Y = (Fixed)rnd.Next(-5, 5);
                velocityComponentArray[i].Z = (Fixed)rnd.Next(-5, 5);

                spinComponentArray[i].X = (Fixed)rnd.Next(-5, 5);
                spinComponentArray[i].Y = (Fixed)rnd.Next(-5, 5);
                spinComponentArray[i].Z = (Fixed)rnd.Next(-5, 5);
            }

            // Run
            sw = new Stopwatch();
            sw.Start();

            for (int frame = 0; frame < numFrames; frame++)
            {
                for (int numEntity = 0; numEntity < numEntities; numEntity++)
                {
                    positionComponentArray[numEntity].X += velocityComponentArray[numEntity].X;
                    positionComponentArray[numEntity].Y += velocityComponentArray[numEntity].Y;
                    positionComponentArray[numEntity].Z += velocityComponentArray[numEntity].Z;
                }

                for (int numEntity = 0; numEntity < numEntities; numEntity++)
                {
                    rotationComponentArray[numEntity].X += spinComponentArray[numEntity].X;
                    rotationComponentArray[numEntity].Y += spinComponentArray[numEntity].Y;
                    rotationComponentArray[numEntity].Z += spinComponentArray[numEntity].Z;
                }
            }

            sw.Stop();
            Console.WriteLine("Poor approach with classes took: " + sw.ElapsedMilliseconds);

        }
    }
}
