using Assimp;
using Assimp.Unmanaged;
using SharpDX.Direct2D1.Effects;
using SoulsAssetPipeline.Animation;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml;

namespace DSAnimStudio
{
    public class ToolExportUnrealEngine
    {
        public enum ExportFileType
        {
            SkeletalMesh_Fbx,
            AnimationSequence_Fbx,
        }

        HavokSplineFixer splineFixer = null;
        public void InitForAnimContainer(NewAnimationContainer animContainer)
        {
            if (splineFixer == null)
                splineFixer = new HavokSplineFixer(animContainer.Skeleton);

            
        }

		public static readonly Vector3D UnitScale = new Vector3D(100, 100, 100); // Unit from FromSoftware Meter to UnrealEngine Centimeter and mirror
		public static readonly Vector3D Mirror = new Vector3D(1, 1, -1);


		public bool Export(ExportFileType fileType, string path, string filename, out bool userRequestCancel)
        {
			bool requestCancel = false;
            bool result = false;
            try
            {
                if (fileType == ExportFileType.SkeletalMesh_Fbx)
                {
                    result = ExportSkeletalMesh(Scene.MainModel.flver, path);
                }
                else if (fileType == ExportFileType.AnimationSequence_Fbx)
                {
                }
            }
            catch (Exception ex)
            {
                var dlgRes = System.Windows.Forms.MessageBox.Show($"Failed to export file '{path}'.\nWould you like to continue anyways?\n\n\nError shown below:\n\n{ex}",
                    "Continue With Errors?", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Warning);
                requestCancel = (dlgRes == System.Windows.Forms.DialogResult.No);
            }
            userRequestCancel = requestCancel;
            return result;
        }

        public bool ExportSkeletalMesh(FLVER2 flver, string path)
        {
			ExportFormatDescription[] exportFormats = AssimpLibrary.Instance.GetExportFormatDescriptions();
            Console.Write(exportFormats);
            using(var context = new AssimpContext())
            {
				Assimp.Scene scene = CreateScene(flver);
				//Assimp.Scene scene = CreateTestScene();
                return context.ExportFile(scene, path, "fbxa");
                //var fbx = context.ExportFile(fbxPath, PostProcessSteps.CalculateTangentSpace | PostProcessSteps.GlobalScale | PostProcessSteps.OptimizeGraph);
                //return ImportFromAssimpScene(fbx, settings);
            }
        }

		public Assimp.Scene CreateScene(FLVER2 flver)
		{
			Node root = new Node("RootNode");

			List<Node> boneNodes = CreateBoneNodes(flver);
			List<Material> materials = CreateMaterials(flver);
			List<Mesh> meshes = CreateMeshes(flver, boneNodes);

			Assimp.Scene scene = new Assimp.Scene();

			for(int i = 0; i < boneNodes.Count; ++i)
			{
				Node boneNode = boneNodes[i];
				if (boneNode.Parent != null)
					continue;

				boneNode.Parent = root;

				root.Children.Add(boneNode);
			}

			if (meshes.Count > 0)
			{
				Node meshNode = new Node("Mesh");

				meshNode.Parent = root;
				for (int i = 0; i < meshes.Count; ++i)
					meshNode.MeshIndices.Add(i);

				meshNode.Transform = meshNode.Transform;

				root.Children.Add(meshNode);
			}

			scene.RootNode = root;
			scene.Materials = materials;
			scene.Meshes = meshes;

			return scene;
		}

		List<Node> CreateBoneNodes(FLVER2 flver)
		{
			List<FLVER.Bone> bones = flver.Bones;

			List<Node> nodes = new List<Node>(bones.Count);
			for (int i = 0; i < bones.Count; ++i)
			{
				FLVER.Bone bone = bones[i];
				Node node = CreateBoneNode(bone);
				nodes.Add(node);
			}

			for (int i = 0; i < bones.Count; ++i)
			{
				FLVER.Bone bone = bones[i];
				int parentIndex = bone.ParentIndex;
				if (parentIndex < 0)
					continue;

				Node node = nodes[i];
				Node parent = nodes[parentIndex];
				node.Parent = parent;
				parent.Children.Add(node);
			}

			return nodes;
		}

		Node CreateBoneNode(FLVER.Bone bone)
		{
			System.Numerics.Vector3 scale = bone.Scale;
			System.Numerics.Vector3 rotation = bone.Rotation;
			System.Numerics.Vector3 translation = bone.Translation;

			rotation = System.Numerics.Vector3.Multiply(rotation, To(-Mirror));
			translation = System.Numerics.Vector3.Multiply(translation, To(UnitScale * Mirror));

			System.Numerics.Matrix4x4 t = System.Numerics.Matrix4x4.CreateScale(scale)
					* System.Numerics.Matrix4x4.CreateRotationX(rotation.X)
					* System.Numerics.Matrix4x4.CreateRotationZ(rotation.Z)
					* System.Numerics.Matrix4x4.CreateRotationY(rotation.Y)
					* System.Numerics.Matrix4x4.CreateTranslation(translation);

			Node node = new Node();

			node.Name = bone.Name;
			node.Transform = From(t);

			return node;
		}

		List<Material> CreateMaterials(FLVER2 flver)
		{
			List<FLVER2.Material> flverMaterials = flver.Materials;
			int count = flverMaterials.Count;

			List<Material> materials = new List<Material>(count);

			for (int i = 0; i < count; ++i)
			{
				FLVER2.Material flverMaterial = flverMaterials[i];

				Material material = new Material();

				material.Name = flverMaterial.Name;

				materials.Add(material);
			}

			return materials;
		}

		List<Bone> CreateBones(List<FLVER.Bone> flverBones, List<Node> nodes)
		{
			List<Bone> bones = new List<Bone>(flverBones.Count);

			for(int i = 0; i < flverBones.Count; ++i)
			{
				FLVER.Bone flverBone = flverBones[i];
				Node node = nodes[i];

				Bone bone = new Bone();
				bone.Name = flverBone.Name;
				bone.Node = node;

				bones.Add(bone);
			}

			return bones;
		}


		List<Mesh> CreateMeshes(FLVER2 flver, List<Node> boneNodes)
		{
			List<FLVER2.Mesh> flverMeshes = flver.Meshes;
			List<FLVER.Bone> flverBones = flver.Bones;
			List<Mesh> meshes = new List<Mesh>(flverMeshes.Count);

			for(int i = 0; i < flverMeshes.Count; ++i)
			{
				List<Bone> bones = CreateBones(flverBones, boneNodes);

				FLVER2.Mesh m = flverMeshes[i];
				Mesh mesh = CreateMesh(m, $"Section{i}", bones);
				meshes.Add(mesh);
			}

			return meshes;
		}

		Mesh CreateMesh(FLVER2.Mesh m, String name, List<Bone> bones)
		{
			List<FLVER.Vertex> vertices = m.Vertices;
			int vertexCount = vertices.Count;

			List<Vector3D> positions = new List<Vector3D>(vertexCount);
			List<Vector3D> normals = new List<Vector3D>(vertexCount);
			List<Vector3D> tangents = new List<Vector3D>(vertexCount);
			List<Vector3D> bitangents = new List<Vector3D>(vertexCount);

			int colorCount = vertices.Count > 0 ? vertices[0].Colors.Count : 0;
			int uvCount = vertices.Count > 0 ? vertices[0].UVs.Count : 0;

			List<Color4D>[] colors = new List<Color4D>[colorCount].Select(e => new List<Color4D>(vertexCount)).ToArray();
			List<Vector3D>[] uvs = new List<Vector3D>[uvCount].Select(e => new List<Vector3D>(vertexCount)).ToArray();

			for (int i = 0; i < vertices.Count; ++i)
			{
				FLVER.Vertex vertex = vertices[i];

				Vector3D position = From(vertex.Position);
				Vector3D normal = From(vertex.Normal);
				Vector3D tangent = From(vertex.Tangents[0]);
				Vector3D bitangent = From(vertex.Bitangent);

				positions.Add(position * Mirror * UnitScale);
				normals.Add(normal * Mirror);
				tangents.Add(tangent * Mirror);
				bitangents.Add(bitangent * Mirror);

				for (int j = 0; j < colorCount; ++j)
				{
					Color4D color = From(vertex.Colors[j]);
					colors[j].Add(color);
				}

				for(int j = 0; j < uvCount; ++j)
				{
					Vector3D uv = From(vertex.UVs[j]);
					uvs[j].Add(uv);
				}
			}

			List<Face> faces = new List<Face>();

			FLVER2.FaceSet faceSet = m.FaceSets?.Find(e => e.Flags == FLVER2.FaceSet.FSFlags.None);
			if (faceSet != null)
			{
				List<int> indices = faceSet.Indices;
				for (int i = 0; i < indices.Count; i += 3)
				{
					int index0 = indices[i];
					int index1 = indices[i + 1];
					int index2 = indices[i + 2];

					List<int> index = new List<int> { index0, index1, index2 };

					Face face = new Face();
					face.Indices = index;

					faces.Add(face);
				}
			}

			for (int i = 0; i < vertices.Count; ++i)
			{
				FLVER.Vertex vertex = vertices[i];

				FLVER.VertexBoneIndices boneIndices = vertex.BoneIndices;
				FLVER.VertexBoneWeights boneWeights = vertex.BoneWeights;

				for (int j = 0; j < 4; ++j)
				{
					float boneWeight = boneWeights[j];
					if (boneWeight <= 0)
						continue;

					int boneIndex = boneIndices[j];
					VertexWeight vertexWeight = new VertexWeight(i, boneWeight);
					bones[boneIndex].VertexWeights.Add(vertexWeight);
				}
			}
			bones.RemoveAll(e => e.VertexWeights.Count == 0);

			Mesh mesh = new Mesh(name);

			mesh.Vertices = positions;
			mesh.Normals = normals;
			mesh.Tangents = tangents;
			mesh.BiTangents = bitangents;
			mesh.VertexColorChannels = colors;
			mesh.TextureCoordinateChannels = uvs;

			mesh.Faces = faces;

			mesh.Bones = bones;

			mesh.MaterialIndex = m.MaterialIndex;

			return mesh;
		}

		public Assimp.Scene CreateTestScene()
        {
            using(var context = new AssimpContext())
            {
				Assimp.Scene testScene = context.ImportFile("D:\\games\\Sekiro Shadows Die Twice\\chr\\c7100-chrbnd-dcx\\chr\\c7100\\c7100_2.FBX");
				List<Mesh> testMeshes = testScene.Meshes;
				Node rootNode = testScene.RootNode;
				return testScene;
			}
			

			List<Vector3D> vertices = new List<Vector3D>()
			{
				new Vector3D(-1.0f/2.0f,1.0f/2.0f,1.0f/2.0f),
				new Vector3D(1.0f/2.0f,1.0f/2.0f,1.0f/2.0f),
				new Vector3D(-1.0f/2.0f,-1.0f/2.0f,1.0f/2.0f),
				new Vector3D(1.0f/2.0f,-1.0f/2.0f,1.0f/2.0f),
				new Vector3D(-1.0f/2.0f,1.0f/2.0f,-1.0f/2.0f),
				new Vector3D(1.0f/2.0f,1.0f/2.0f,-1.0f/2.0f),
				new Vector3D(-1.0f/2.0f,-1.0f/2.0f,-1.0f/2.0f),
				new Vector3D(1.0f/2.0f,-1.0f/2.0f,-1.0f/2.0f)
			};

			//Default Fill Location Vector                                   
			List<int> indices = new List<int>()
			{
				0,2,1,      2,3,1,
				1,3,5,      3,7,5,
				5,7,4,      7,6,4,
				4,6,0,      6,2,0,
				4,0,5,      0,1,5,
				2,6,3,      6,7,3
			};

			List<Face> faces = new List<Face>();
			for(int i = 0; i < (indices.Count / 3); i++)
			{
				Face face = new Face();
				face.Indices.Add(indices[i * 3]);
				face.Indices.Add(indices[i * 3 + 1]);
				face.Indices.Add(indices[i * 3 + 2]);
				faces.Add(face);
			}

			List<Vector3D> uvs = new List<Vector3D>()
			{
				new Vector3D(0, 1, 0),
				new Vector3D(0, 0, 0),
				new Vector3D(1, 1, 0),
				new Vector3D(1, 0, 0),
				new Vector3D(0, 0, 1),
				new Vector3D(1, 0, 1),
				new Vector3D(1, 1, 1),
				new Vector3D(1, 0, 1),
			};

			Mesh mesh = new Mesh();
			mesh.Name = "Mesh";
			mesh.Vertices = vertices;
			mesh.TextureCoordinateChannels[0] = uvs;
			mesh.Faces = faces;
			mesh.MaterialIndex = 0;

			Node node = new Node("Node");
			node.MeshIndices.Add(0);

			Assimp.Scene scene = new Assimp.Scene();
			scene.RootNode = node;
			scene.Meshes.Add(mesh);

			Material material = new Material();
			material.Name = "Material";
			scene.Materials.Add(material);

			return scene;
		}

		Vector3D From(System.Numerics.Vector3 v)
		{
			Vector3D result = new Vector3D(v.X, v.Y, v.Z);

			return result;
		}

		System.Numerics.Vector3 To(Vector3D v)
		{
			System.Numerics.Vector3 result = new System.Numerics.Vector3(v.X, v.Y, v.Z);

			return result;
		}

		Vector3D From(System.Numerics.Vector4 v)
		{
			Vector3D result = new Vector3D(v.X, v.Y, v.Z);

			return result;
		}

		Color4D From(SoulsFormats.FLVER.VertexColor c)
		{
			Color4D result = new Color4D(c.R, c.G, c.B, c.A);

			return result;
		}

		Matrix4x4 From(System.Numerics.Matrix4x4 m)
		{
			Matrix4x4 result = new Matrix4x4
			(
				m.M11, m.M21, m.M31, m.M41,
				m.M12, m.M22, m.M32, m.M42,
				m.M13, m.M23, m.M33, m.M43,
				m.M14, m.M24, m.M34, m.M44
			);

			return result;
		}
    }
}
