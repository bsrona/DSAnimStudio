﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public static class XnaExtensions
    {
        //public static Quaternion QuatFromDirectionVector(Vector3 direction)
        //{
        //    if (direction == Vector3.Up)
        //}

        public static Quaternion QuatLookRotation(Vector3 forward, Vector3 up)
        {
            forward = Vector3.Normalize(forward);
            Vector3 right = Vector3.Normalize(Vector3.Cross(up, forward));
            up = Vector3.Cross(forward, right);
            var m00 = right.X;
            var m01 = right.Y;
            var m02 = right.Z;
            var m10 = up.X;
            var m11 = up.Y;
            var m12 = up.Z;
            var m20 = forward.X;
            var m21 = forward.Y;
            var m22 = forward.Z;


            float num8 = (m00 + m11) + m22;
            var quaternion = new Quaternion();
            if (num8 > 0f)
            {
                var num = (float)System.Math.Sqrt(num8 + 1f);
                quaternion.W = num * 0.5f;
                num = 0.5f / num;
                quaternion.X = (m12 - m21) * num;
                quaternion.Y = (m20 - m02) * num;
                quaternion.Z = (m01 - m10) * num;
                return quaternion;
            }
            if ((m00 >= m11) && (m00 >= m22))
            {
                var num7 = (float)System.Math.Sqrt(((1f + m00) - m11) - m22);
                var num4 = 0.5f / num7;
                quaternion.X = 0.5f * num7;
                quaternion.Y = (m01 + m10) * num4;
                quaternion.Z = (m02 + m20) * num4;
                quaternion.W = (m12 - m21) * num4;
                return quaternion;
            }
            if (m11 > m22)
            {
                var num6 = (float)System.Math.Sqrt(((1f + m11) - m00) - m22);
                var num3 = 0.5f / num6;
                quaternion.X = (m10 + m01) * num3;
                quaternion.Y = 0.5f * num6;
                quaternion.Z = (m21 + m12) * num3;
                quaternion.W = (m20 - m02) * num3;
                return quaternion;
            }
            var num5 = (float)System.Math.Sqrt(((1f + m22) - m00) - m11);
            var num2 = 0.5f / num5;
            quaternion.X = (m20 + m02) * num2;
            quaternion.Y = (m21 + m12) * num2;
            quaternion.Z = 0.5f * num5;
            quaternion.W = (m01 - m10) * num2;
            return quaternion;
        }

        public static Quaternion QuatFromToRotation(Vector3 fromDirection, Vector3 toDirection)
        {
            var fromQuat = QuatLookRotation(fromDirection, Vector3.Up);
            var toQuat = QuatLookRotation(toDirection, Vector3.Up);
            return toQuat * Quaternion.Inverse(fromQuat);
        }

        
        public static Vector3 GetLerpValARequiredForResult(Vector3 b, Vector3 r, float s)
        {
            return new Vector3(GetLerpValARequiredForResult(b.X, r.X, s),
                GetLerpValARequiredForResult(b.Y, r.Y, s),
                GetLerpValARequiredForResult(b.Z, r.Z, s));
        }

        // r = Lerp(a, b, s)
        // This returns 'a' given r, b, and s
        public static float GetLerpValARequiredForResult(float b, float r, float s)
        {
            return (r - (b * s)) / (1 - s);
        }

        public static Quaternion GetQuatOfBladePosDelta(NewDummyPolyManager.DummyPolyBladePos fromPos, NewDummyPolyManager.DummyPolyBladePos toPos)
        {
            return QuatFromToRotation(fromPos.Start - fromPos.End, toPos.Start - toPos.End);
        }

        public static Microsoft.Xna.Framework.Matrix ToXna(this System.Numerics.Matrix4x4 matrix)
        {
            return new Microsoft.Xna.Framework.Matrix(
                matrix.M11, matrix.M12, matrix.M13, matrix.M14,
                matrix.M21, matrix.M22, matrix.M23, matrix.M24,
                matrix.M31, matrix.M32, matrix.M33, matrix.M34,
                matrix.M41, matrix.M42, matrix.M43, matrix.M44
            );
        }

        public static Microsoft.Xna.Framework.Vector3 ToXna(this System.Numerics.Vector3 vector)
        {
            return new Microsoft.Xna.Framework.Vector3(vector.X, vector.Y, vector.Z);
        }

        public static Microsoft.Xna.Framework.Vector4 ToXna(this System.Numerics.Vector4 vector)
        {
            return new Microsoft.Xna.Framework.Vector4(vector.X, vector.Y, vector.Z, vector.W);
        }

        public static Microsoft.Xna.Framework.Quaternion ToXna(this System.Numerics.Quaternion quaternion)
        {
            return new Microsoft.Xna.Framework.Quaternion(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);
        }

        public static System.Numerics.Vector3 ToCS(this Microsoft.Xna.Framework.Vector3 vector)
        {
            return new System.Numerics.Vector3(vector.X, vector.Y, vector.Z);
        }

        public static System.Numerics.Vector4 ToCS(this Microsoft.Xna.Framework.Vector4 vector)
        {
            return new System.Numerics.Vector4(vector.X, vector.Y, vector.Z, vector.W);
        }

        public static System.Numerics.Quaternion ToCS(this Microsoft.Xna.Framework.Quaternion quaternion)
        {
            return new System.Numerics.Quaternion(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);
        }

        public static System.Numerics.Vector3 ToVector3(this System.Numerics.Vector4 vector)
        {
            return new System.Numerics.Vector3(vector.X, vector.Y, vector.Z);
        }

        public static System.Numerics.Quaternion ToQuat(this System.Numerics.Vector4 vector)
        {
            return new System.Numerics.Quaternion(vector.X, vector.Y, vector.Z, vector.W);
        }
    }
}