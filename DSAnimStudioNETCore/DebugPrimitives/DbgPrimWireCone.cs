using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.DebugPrimitives
{
    public class DbgPrimWireCone : DbgPrimWire
    {
        public DbgPrimWireCone(Vector3 axis, float angle, Color color, float height = 0.1f, int numSegments = 20)
        {
            Vector3 perp = Vector3.Zero;
            if (Vector3.DistanceSquared(axis, Vector3.UnitX) > 0.000001f)
                perp = Vector3.Cross(axis, Vector3.UnitX);
            else
                perp = Vector3.Cross(axis, Vector3.UnitY);

            perp.Normalize();

			var start = Vector3.Transform(axis, Quaternion.CreateFromAxisAngle(perp, angle)) * height;
            var last = start;

            for (int i = 0; i <= numSegments; i++)
            {
                float twist = (1.0f * i / numSegments) * MathHelper.TwoPi;
				Quaternion delta = Quaternion.CreateFromAxisAngle(axis, twist);
                Vector3 current = Vector3.Transform(start, delta);
                AddLine(Vector3.Zero, current, color);
                AddLine(last, current, color);
                last = current;
            }
        }
    }
}
