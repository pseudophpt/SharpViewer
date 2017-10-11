using System;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SharpViewer
{
    /// <summary>
    /// Description of Core.
    /// </summary>
    /// 
    public class Gfx3Instance
    {
        public Graphics Gfx;
        public Pen Pen;

        public int Height;
        public int Width;

        public float NearClippingDistance;

        public Gfx3Instance(Graphics Gfx, int Width, int Height)
        {
            this.Gfx = Gfx;

            this.Width = Width;
            this.Height = Height;

            this.NearClippingDistance = 0.01F;
        }

        public Gfx3Instance(Graphics Gfx, float NearClippingDistance, int Width, int Height)
        {
            this.Gfx = Gfx;

            this.Width = Width;
            this.Height = Height;

            this.NearClippingDistance = NearClippingDistance;
        }

        public Gfx3Instance(Graphics Gfx, Pen Pen, int Width, int Height)
        {
            this.Gfx = Gfx;
            this.Pen = Pen;

            this.Width = Width;
            this.Height = Height;

            this.NearClippingDistance = 0.01F;
        }

        public Gfx3Instance(Graphics Gfx, float NearClippingDistance, Pen Pen, int Width, int Height)
        {
            this.Gfx = Gfx;
            this.Pen = Pen;

            this.Width = Width;
            this.Height = Height;

            this.NearClippingDistance = NearClippingDistance;
        }

        public void SetPen(Pen Pen)
        {
            this.Pen = Pen;
        }

        public void DrawTriPersp(AffineVector3[] Points)
        {
            AffineVector3[] Clipped = Utils.ClipTri(Points, NearClippingDistance);
            if (Clipped == null)
                return;
            Vector2[] ClippedAndProjected = new Vector2[Clipped.Length];

            for (int i = 0; i < Clipped.Length; i++)
            {
                ClippedAndProjected[i] = Clipped[i]
                    .ProjectPersp()
                    .Scale(this.Width / 2, this.Width / 2)
                    .Translate(this.Width / 2, this.Height / 2);
            }

            Draw2DTri(ClippedAndProjected);
        }

        public void DrawTriOrtho(AffineVector3[] Points)
        {
            AffineVector3[] Clipped = Utils.ClipTri(Points, NearClippingDistance);
            Vector2[] ClippedAndProjected = new Vector2[Clipped.Length];

            for (int i = 0; i < Clipped.Length; i++)
            {
                ClippedAndProjected[i] = Clipped[i]
                    .ProjectOrtho()
                    .Scale(this.Width / 2, this.Width / 2)
                    .Translate(this.Width / 2, this.Height / 2);
            }

            Draw2DTri(ClippedAndProjected);
        }

        public void Draw2DTri(Vector2[] Points)
        {
            for (int i = 0; i < Points.Length; i++)
            {
                Gfx.DrawLine(this.Pen,
                             Points[i].x, Points[i].y,
                             Points[(i + 1) % Points.Length].x, Points[(i + 1) % Points.Length].y);
            }
        }
    }

    public class AffineMatrix3
    {
        public float[,] values = new float[4, 4];

        public AffineMatrix3()
        {
            this.values = new float[4, 4];
        }

        public AffineMatrix3(float[,] values)
        {
            this.values = values;
        }

        public void ApplyMap(AffineMatrix3 Map)
        {
            float[,] values = new float[4, 4];

            for (int row = 0; row < 4; row++)
            {
                AffineVector3 RowVector = new AffineVector3(
                    Map.values[row, 0],
                    Map.values[row, 1],
                    Map.values[row, 2],
                    Map.values[row, 3]);
                for (int col = 0; col < 4; col++)
                {
                    AffineVector3 ColVector = new AffineVector3(
                        this.values[0, col],
                        this.values[1, col],
                        this.values[2, col],
                        this.values[3, col]);
                    values[row, col] = AffineVector3.Dot(RowVector, ColVector);
                }
            }

            this.values = values;
        }

        public static AffineMatrix3 Translation(float x, float y, float z)
        {
            float[,] Result = new float[4, 4];

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (i == j)
                        Result[i, j] = 1;
                    else
                        Result[i, j] = 0;
                }
            }

            Result[0, 3] = x;
            Result[1, 3] = y;
            Result[2, 3] = z;

            return new AffineMatrix3(Result);
        }

        public static AffineMatrix3 Rotation(Quaternion AxisAndAngle)
        {
            float[,] Result = new float[4, 4];

            float x = AxisAndAngle.x;
            float y = AxisAndAngle.y;
            float z = AxisAndAngle.z;
            float w = AxisAndAngle.w;

            Result[0, 0] = 1 - 2 * y * y - 2 * z * z;
            Result[0, 1] = 2 * x * y + 2 * w * z;
            Result[0, 2] = 2 * x * z - 2 * w * y;
            Result[0, 3] = 0;

            Result[1, 0] = 2 * x * y - 2 * w * z;
            Result[1, 1] = 1 - 2 * x * x - 2 * z * z;
            Result[1, 2] = 2 * y * z + 2 * w * x;
            Result[1, 3] = 0;

            Result[2, 0] = 2 * x * z + 2 * w * y;
            Result[2, 1] = 2 * y * z - 2 * w * x;
            Result[2, 2] = 1 - 2 * x * x - 2 * y * y;
            Result[2, 3] = 0;

            Result[3, 0] = 0;
            Result[3, 1] = 0;
            Result[3, 2] = 0;
            Result[3, 3] = 1;

            return new AffineMatrix3(Result);
        }
    }

    public class AffineVector3
    {
        public float x, y, z, w;

        public AffineVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = 1;
        }

        public AffineVector3(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public static float Dot(AffineVector3 Vec1, AffineVector3 Vec2)
        {
            return
                (Vec1.x * Vec2.x) +
                (Vec1.y * Vec2.y) +
                (Vec1.z * Vec2.z) +
                (Vec1.w * Vec2.w);
        }

        public static AffineVector3 ApplyMap(AffineVector3 Vec1, AffineMatrix3 Matrix)
        {
            float[] Return = new float[4];
            for (int row = 0; row < 4; row++)
            {
                AffineVector3 RowVector = new AffineVector3(
                    Matrix.values[row, 0],
                    Matrix.values[row, 1],
                    Matrix.values[row, 2],
                    Matrix.values[row, 3]);
                Return[row] = AffineVector3.Dot(RowVector, Vec1); ;
            }

            return new AffineVector3(Return[0], Return[1], Return[2], Return[3]);
        }

        public Vector2 ProjectOrtho()
        {
            return new Vector2(x / w, y / w);
        }

        public Vector2 ProjectPersp()
        {
            return new Vector2(x / (w * z), y / (w * z));
        }
    }

    public class Vector2
    {
        public float x, y;

        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public Vector2 Translate(float x, float y)
        {
            return new Vector2(this.x + x, this.y + y);
        }

        public Vector2 Scale(float sx, float sy)
        {
            return new Vector2(this.x * sx, this.y * sy);
        }
    }

    public class Quaternion
    {
        public float x, y, z, w;

        public Quaternion(float x, float y, float z, float theta)
        {
            float Norm = (float)Math.Sqrt((x * x) + (y * y) + (z * z));
            if (Norm != 0)
            {
                x /= Norm;
                y /= Norm;
                z /= Norm;
            }

            float Sine = (float)Math.Sin(theta / 2);
            float Cosine = (float)Math.Cos(theta / 2);

            this.x = x * Sine;
            this.y = y * Sine;
            this.z = z * Sine;
            this.w = Cosine;
        }
    }

    public static class Utils
    {
        public static float Lerp(float Start, float End, float Interp)
        {
            return Start + ((Start - End) * Interp);
        }

        public static AffineVector3 IntersectClippingPlane(AffineVector3 Vec1, AffineVector3 Vec2, float NearClippingDistance)
        {
            float InterpolationFactor = (Vec1.z - NearClippingDistance) / (Vec1.z - Vec2.z);

            return new AffineVector3(Utils.Lerp(Vec1.x, Vec2.x, InterpolationFactor), Utils.Lerp(Vec1.y, Vec2.y, InterpolationFactor), NearClippingDistance);
        }

        public static AffineVector3[] ClipTri(AffineVector3[] Points, float NearClippingDistance)
        {
            AffineVector3 Vec1 = Points[0];
            AffineVector3 Vec2 = Points[1];
            AffineVector3 Vec3 = Points[2];

            bool Vec1Clip = (Vec1.z <= NearClippingDistance);
            bool Vec2Clip = (Vec2.z <= NearClippingDistance);
            bool Vec3Clip = (Vec3.z <= NearClippingDistance);

            if (!Vec1Clip & !Vec2Clip & !Vec3Clip)
            {
                /* All 3 are in front of clipping plane */
                return Points;
            }
            if (Vec1Clip & Vec2Clip & Vec3Clip)
            {
                /* All 3 are behind */
                return null;
            }
            /* All but 1 are behind */
            if (!Vec1Clip & Vec2Clip & Vec3Clip)
            {
                AffineVector3[] Return = new AffineVector3[3];

                Return[0] = Vec1;
                Return[1] = IntersectClippingPlane(Vec1, Vec2, NearClippingDistance);
                Return[2] = IntersectClippingPlane(Vec1, Vec3, NearClippingDistance);

                return Return;
            }
            if (Vec1Clip & !Vec2Clip & Vec3Clip)
            {
                AffineVector3[] Return = new AffineVector3[3];

                Return[0] = Vec2;
                Return[1] = IntersectClippingPlane(Vec2, Vec1, NearClippingDistance);
                Return[2] = IntersectClippingPlane(Vec2, Vec3, NearClippingDistance);

                return Return;
            }
            if (!Vec1Clip & Vec2Clip & Vec3Clip)
            {
                AffineVector3[] Return = new AffineVector3[3];

                Return[0] = Vec3;
                Return[1] = IntersectClippingPlane(Vec3, Vec2, NearClippingDistance);
                Return[2] = IntersectClippingPlane(Vec3, Vec1, NearClippingDistance);

                return Return;
            }
            /* All but 1 are in front */
            if (Vec1Clip & !Vec2Clip & !Vec3Clip)
            {
                AffineVector3[] Return = new AffineVector3[4];

                Return[0] = Vec2;
                Return[1] = IntersectClippingPlane(Vec2, Vec1, NearClippingDistance);
                Return[2] = IntersectClippingPlane(Vec3, Vec1, NearClippingDistance);
                Return[3] = Vec3;

                return Return;
            }
            if (!Vec1Clip & Vec2Clip & !Vec3Clip)
            {
                AffineVector3[] Return = new AffineVector3[4];

                Return[0] = Vec3;
                Return[1] = IntersectClippingPlane(Vec3, Vec2, NearClippingDistance);
                Return[2] = IntersectClippingPlane(Vec1, Vec2, NearClippingDistance);
                Return[3] = Vec1;

                return Return;
            }
            if (!Vec1Clip & !Vec2Clip & Vec3Clip)
            {
                AffineVector3[] Return = new AffineVector3[4];

                Return[0] = Vec1;
                Return[1] = IntersectClippingPlane(Vec1, Vec3, NearClippingDistance);
                Return[2] = IntersectClippingPlane(Vec2, Vec3, NearClippingDistance);
                Return[3] = Vec2;

                return Return;
            }

            return null;
        }
    }
}
