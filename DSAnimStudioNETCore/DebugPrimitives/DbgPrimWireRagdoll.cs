using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.DebugPrimitives
{
    public class DbgPrimWireRagdoll : DbgPrimWire
    {
        public DbgPrimWireRagdoll(Vector3 twist, float twistMin, float twistMax, Vector3 normal, float radius = 0.1f)
        {
            Category = DbgPrimCategory.Ragdoll;

            var minTwistAxis = Vector3.Transform(normal, Quaternion.CreateFromAxisAngle(twist, twistMin));
            var maxTwistAxis = Vector3.Transform(normal, Quaternion.CreateFromAxisAngle(twist, twistMax));

			AddLine(Vector3.Zero, twist * radius, Color.Red);
            AddLine(Vector3.Zero, normal * radius, Color.Green);
            AddLine(Vector3.Zero, Vector3.Cross(twist, normal) * radius, Color.Blue);
			AddLine(Vector3.Zero, minTwistAxis * radius, Color.Green);
			AddLine(Vector3.Zero, maxTwistAxis * radius, Color.Green);
            AddLine(minTwistAxis * radius, maxTwistAxis * radius, Color.Green);
		}
	}
}
