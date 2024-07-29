using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.DebugPrimitives
{
    public class DbgPrimWireFrame : DbgPrimWire
    {
        public static readonly Color[] AxisColor = new Color[] { Color.Red, Color.Green, Color.Blue };

        private static DbgPrimGeometryData GeometryData = null;

        public DbgPrimWireFrame(Transform location)
        {
            
            KeepBuffersAlive = true;

            Category = DbgPrimCategory.AlwaysDraw;

            Transform = location;

            if (GeometryData != null)
            {
                SetBuffers(GeometryData.VertBuffer, GeometryData.IndexBuffer);
            }
            else
            {
                AddLine(Vector3.Zero, Vector3.UnitX, AxisColor[0]);
				AddLine(Vector3.Zero, Vector3.UnitY, AxisColor[1]);
				AddLine(Vector3.Zero, Vector3.UnitZ, AxisColor[2]);

                FinalizeBuffers(true);

                GeometryData = new DbgPrimGeometryData()
                {
                    VertBuffer = VertBuffer,
                    IndexBuffer = IndexBuffer,
                };
            }
        }
    }
}
