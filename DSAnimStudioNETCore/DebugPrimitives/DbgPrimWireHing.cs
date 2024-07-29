using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.DebugPrimitives
{
    public class DbgPrimWireHing : DbgPrimWire
    {
        public DbgPrimWireHing(Vector3 axis, Vector3 perp, float minAngle, float maxAngle, float radius = 0.1f)
        {
            Category = DbgPrimCategory.Ragdoll;

            var minAxis = Vector3.Transform(perp, Quaternion.CreateFromAxisAngle(axis, minAngle));
            var maxAxis = Vector3.Transform(perp, Quaternion.CreateFromAxisAngle(axis, maxAngle));

			AddLine(Vector3.Zero, axis * radius, Color.Green);
			AddLine(Vector3.Zero, perp * radius, Color.Red);
            AddLine(Vector3.Zero, Vector3.Cross(perp, axis) * radius, Color.Blue);
            AddLine(Vector3.Zero, minAxis * radius, Color.Yellow);
            AddLine(Vector3.Zero, maxAxis * radius, Color.Yellow);
            AddLine(minAxis * radius, maxAxis * radius, Color.Yellow);
		}
    }
}
