using Assimp;
using Assimp.Unmanaged;
using HKX2;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using SharpDX.Direct2D1.Effects;
using SharpDX.MediaFoundation;
using SoulsAssetPipeline;
using SoulsAssetPipeline.Animation;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml;
using System.Runtime.InteropServices;
using Pfim;
using static DSAnimStudio.NewAnimSkeleton_HKX;
using static SoulsAssetPipeline.Animation.HKX;
using System.Drawing.Imaging;
using Org.BouncyCastle.Utilities.Encoders;
using DSAnimStudio.TaeEditor;
using System.Reflection;

namespace DSAnimStudio
{
    public class ToolExportUnrealEngine
    {
        public enum ExportFileType
        {
			All,
            SkeletalMesh_Fbx,
			AnimationSkeleton_Fbx,
            AnimationSequence_Fbx,
			AnimationSequences_Fbx,
			Textures,
			Materials_Json,
        }

        HavokSplineFixer splineFixer = null;
        public void InitForAnimContainer(NewAnimationContainer animContainer)
        {
            if (splineFixer == null)
                splineFixer = new HavokSplineFixer(animContainer.Skeleton);

            
        }

		public static readonly Vector3D UnitScale = new Vector3D(100, 100, 100); // Unit from FromSoftware Meter to UnrealEngine Centimeter and mirror
		public static readonly Vector3D Mirror = new Vector3D(1, 1, -1);


		public void Export(ExportFileType fileType, string path, string filename, out bool userRequestCancel)
        {
			bool requestCancel = false;
            try
            {
                if (fileType == ExportFileType.All)
                {
                    ExportAll(path);
                }
				else if(fileType == ExportFileType.SkeletalMesh_Fbx)
				{
					List<IBinder> binders = new List<IBinder>();
					List<FLVER2> flvers = GetAllFlvers(ref binders);

					ExportSkeletalMeshs(path, flvers, binders);
				}
				else if(fileType == ExportFileType.AnimationSkeleton_Fbx)
				{
					ExportSkeletons(path);
				}
				else if (fileType == ExportFileType.AnimationSequence_Fbx)
				{
					NewAnimationContainer animContainer = Scene.MainModel.AnimContainer;
					ExportAnimation(animContainer, animContainer.CurrentAnimationName, path);
				}
				else if(fileType == ExportFileType.AnimationSequences_Fbx)
				{
					ExportAnimations(path);
				}
				else if(fileType == ExportFileType.Textures)
				{
					List<IBinder> binders = null;
					List<FLVER2> flvers = GetAllFlvers(ref binders);

					ExportTextures(path, flvers);
				}
				else if(fileType == ExportFileType.Materials_Json)
				{
					List<IBinder> binders = new List<IBinder>();
					List<FLVER2> flvers = GetAllFlvers(ref binders);

					ExportMaterials(path, flvers, binders);
				}
			}
			catch (Exception ex)
            {
                var dlgRes = System.Windows.Forms.MessageBox.Show($"Failed to export file '{path}'.\nWould you like to continue anyways?\n\n\nError shown below:\n\n{ex}",
                    "Continue With Errors?", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Warning);
                requestCancel = (dlgRes == System.Windows.Forms.DialogResult.No);
            }
            userRequestCancel = requestCancel;
        }

		public void ExportAll(string path)
		{
			List<IBinder> binders = new List<IBinder>();
			List<FLVER2> flvers = GetAllFlvers(ref binders);

			ExportSkeletalMeshs(path, flvers, binders);

			ExportMaterials(path, flvers, binders);

			ExportTextures(path, flvers);

			ExportSkeletons(path);

			ExportAnimations(path);
		}

		public void ExportSkeletalMeshs(string path, List<FLVER2> flvers, List<IBinder> binders)
		{
			for (int i = 0; i < flvers.Count; ++i)
			{
				FLVER2 flver = flvers[i];
				IBinder binder = binders[i];

				string originPath = binder.Files.Find(e => e.Name.ToLower().Contains(".flver"))?.Name;
				string relativePath = ToRelativePath(originPath);
				relativePath = ReplacePaths(relativePath);

				string relativeToRoot = RelativeToRoot(relativePath);

				string skeletalMeshPath = Path.ChangeExtension(path + relativePath, "fbx");

				ExportSkeletalMesh(flver, skeletalMeshPath, relativeToRoot);
			}
		}

		public bool ExportSkeletalMesh(FLVER2 flver, string path, string relativeToRoot)
        {
			CreateDirectory(path);

            using(var context = new AssimpContext())
            {
				Assimp.Scene scene = CreateScene(flver, relativeToRoot);
				//Assimp.Scene scene = CreateTestScene();
				return context.ExportFile(scene, path, ExportFormatID);
                //var fbx = context.ExportFile(fbxPath, PostProcessSteps.CalculateTangentSpace | PostProcessSteps.GlobalScale | PostProcessSteps.OptimizeGraph);
                //return ImportFromAssimpScene(fbx, settings);
            }
        }

		public void ExportSkeletons(string path)
		{
			List<NewAnimationContainer> animationContainers = GetAllAnimationContainer();
			for (int i = 0; i < animationContainers.Count; ++i)
			{
				NewAnimationContainer animationContainer = animationContainers[i];
				if (animationContainer.baseANIBND == null)
					continue;

				ExportSkeleton(animationContainer, path);
			}
		}

		public bool ExportSkeleton(NewAnimationContainer animationContainer, string path)
		{
			IBinder anibnd = animationContainer.baseANIBND;
			string originPath = anibnd.Files.Find(e => e.Name.ToLower().Contains("skeleton"))?.Name;
			string relativePath = ToRelativePath(originPath);
			relativePath = ReplacePaths(relativePath);
			string fullpath = Path.ChangeExtension(path + relativePath, "fbx");
			CreateDirectory(fullpath);

			using(var context = new AssimpContext())
			{
				Assimp.Scene scene = CreateScene(animationContainer);
				return context.ExportFile(scene, fullpath, ExportFormatID);
			}
		}

		public void ExportAnimations(string path)
		{
			List<NewAnimationContainer> animationContainers = GetAllAnimationContainer();
			for (int i = 0; i < animationContainers.Count; ++i)
			{
				List<IBinder> binders = new List<IBinder>();

				NewAnimationContainer animationContainer = animationContainers[i];
				binders.Add(animationContainer.baseANIBND);
				binders.AddRange(animationContainer.additionalANIBNDs);

				for (int j = 0; j < binders.Count; ++j)
				{
					IBinder binder = binders[j];
					if (binder == null)
						continue;
	
					for (int k = 0; k < binder.Files.Count; ++k)
					{
						BinderFile file = binder.Files[k];

						string originalPath = file.Name;
						string filename = Path.GetFileName(originalPath).ToLower();
						if (filename[0] != 'a' || Path.GetExtension(filename) != ".hkx")
							continue;

						string relativePath = ToRelativePath(originalPath);
						relativePath = ReplacePaths(relativePath);
						string fullPath = Path.ChangeExtension(path + relativePath, "fbx");

						try
						{
							ExportAnimation(animationContainer, filename, fullPath);
						}
						catch(Exception ex) 
						{
							ErrorLog.LogWarning($"Unable to export {fullPath}");
						}
					}
				}
			}
		}

		public void ExportTAEs(string path)
		{
			TaeFileContainer container = Main.TAE_EDITOR.FileContainer;
			IBinder binder = container.containerANIBND;
			IReadOnlyDictionary<string, TAE> taes = container.AllTAEDict;

			NewAnimationContainer animationContainer = Scene.MainModel.AnimContainer;
			var mainChrSolver = new TaeAnimRefChainSolver(taes, animationContainer.Animations);

			foreach(var tae in taes.Values)
			{
				List<TAE.Animation> animations = tae.Animations;
				for(int i = 0; i < animations.Count; ++i)
				{
					TAE.Animation animation = animations[i];
					string animationName = mainChrSolver.GetHKXName(tae, animation);
					if(string.IsNullOrEmpty(animationName))
						continue;

					string originalPath = binder.Files.Find(e => e.Name.Contains(animationName))?.Name;
					if(string.IsNullOrEmpty(originalPath))
						originalPath = animationContainer.GetAnimationPath(animationName);
					if(string.IsNullOrEmpty(originalPath))
					{
						ErrorLog.LogWarning($@"Unable to find the animation '{animationName}' to export");
						continue;
					}
					string relativePath = ToRelativePath(originalPath);
					relativePath = ReplacePaths(relativePath);
					string fullPath = Path.ChangeExtension(path + relativePath, "fbx");
					ExportAnimation(animationContainer, animationName, fullPath);
				}
			}
		}

		public bool ExportAnimation(NewAnimationContainer animContainer, string name, string path)
		{
			CreateDirectory(path);
			return ExportAnimationFBX(animContainer, name, path);
		}

		public void ExportMaterials(string path, List<FLVER2> flvers, List<IBinder> binders)
		{
			for (int i = 0; i < flvers.Count; ++i)
			{
				FLVER2 flver = flvers[i];
				IBinder binder = binders[i];
				ExportMaterials(path, flver, binder);
			}
		}

		public void ExportMaterials(string path, FLVER2 flver, IBinder binder)
		{
			string originPath = binder.Files.Find(e => e.Name.ToLower().Contains(".flver"))?.Name;
			string relativePath = ToRelativePath(originPath);
			relativePath = ReplacePaths(relativePath);
			string fullpath = Path.GetDirectoryName(path + relativePath);

			List<FLVER2.Material> flverMaterials = flver.Materials;

			for (int i = 0; i < flverMaterials.Count; ++i)
			{
				FLVER2.Material flverMaterial = flverMaterials[i];

				string name = GetIndexName(flverMaterial.Name, i);
				string jsonFlverMaterialPath = $"{fullpath}\\mat\\{name}.mat";

				ExportFlverMaterial(flverMaterial, name, jsonFlverMaterialPath);

				FlverMaterialDefInfo flverMaterialDefInfo = FlverMaterialDefInfo.Lookup(flverMaterial.MTD);
				if (flverMaterialDefInfo == null)
					continue;

				MTD mtd = flverMaterialDefInfo.mtd;
				if (mtd == null)
					continue;

				string mtdPath = ToRelativePath(flverMaterial.MTD);
				string jsonMtdPath = path + mtdPath;

				ExportMTD(mtd, jsonMtdPath);
			}
		}

		public void ExportFlverMaterial(FLVER2.Material material, string name, string path)
		{
			List<FLVER2.Texture> textures = new List<FLVER2.Texture>(material.Textures.Count);
			for(int i = 0; i < material.Textures.Count; ++i)
			{
				FLVER2.Texture texture = material.Textures[i];
				FLVER2.Texture jsonTexture = new FLVER2.Texture();

				jsonTexture.Type = texture.Type;
				jsonTexture.Path = ToRelativePath(ToTexturePath(texture.Path));
				jsonTexture.Scale = texture.Scale;
				jsonTexture.Unk10 = texture.Unk10;
				jsonTexture.Unk11 = texture.Unk11;
				jsonTexture.Unk14 = texture.Unk14;
				jsonTexture.Unk18 = texture.Unk18;
				jsonTexture.Unk1C = texture.Unk1C;

				textures.Add(jsonTexture);
			}

			FLVER2.Material jsonMaterial = new FLVER2.Material();

			jsonMaterial.Name = name;
			jsonMaterial.MTD = ToRelativePath(material.MTD);
			jsonMaterial.Flags = material.Flags;
			jsonMaterial.Textures = textures;
			jsonMaterial.GXIndex = material.GXIndex;

			var json = Newtonsoft.Json.JsonConvert.SerializeObject(jsonMaterial, Newtonsoft.Json.Formatting.Indented);

			WriteTextFile(json, path);
		}

		public void ExportMTD(MTD material, string path)
		{
			List<MTD.Texture> textures = new List<MTD.Texture>(material.Textures.Count);
			for (int i = 0; i < material.Textures.Count; ++i)
			{
				MTD.Texture texture = material.Textures[i];
				MTD.Texture jsonTexture = new MTD.Texture();

				jsonTexture.Type = texture.Type;
				jsonTexture.Extended = texture.Extended;
				jsonTexture.UVNumber = texture.UVNumber;
				jsonTexture.ShaderDataIndex = texture.ShaderDataIndex;
				jsonTexture.Path = ToRelativePath(ToTexturePath(texture.Path));
				jsonTexture.UnkFloats = texture.UnkFloats;

				textures.Add(jsonTexture);
			}

			MTD jsonMaterial = new MTD();

			jsonMaterial.ShaderPath = ToRelativePath(material.ShaderPath);
			jsonMaterial.Description = material.Description;
			jsonMaterial.Params = material.Params;
			jsonMaterial.Textures = textures;

			var json = Newtonsoft.Json.JsonConvert.SerializeObject(jsonMaterial, Newtonsoft.Json.Formatting.Indented);

			WriteTextFile(json, path);
		}

		public void ExportTextures(string path, List<FLVER2> flvers)
		{
			for (int i = 0; i < flvers.Count; ++i)
			{
				FLVER2 flver = flvers[i];
				ExportTextures(path, flver);
			}
		}

		public void ExportTextures(string path, FLVER2 flver)
		{
			List<string> filePaths = GetReferenceTexturePaths(flver);
			for (int i = 0; i < filePaths.Count; ++i)
			{
				string filePath = filePaths[i];
				string exportPath = path + ToRelativePath(filePath);
				ExportTexture(exportPath);
			}
		}

		public void ExportTexture(string path)
		{
			var shortName = Utils.GetShortIngameFileName(path).ToLower();
			if (!TexturePool.Fetches.ContainsKey(shortName))
				return;

			TextureFetchRequest request = TexturePool.Fetches[shortName];
			byte[] ddsBytes = request?.TexInfo?.DDSBytes;
			if (ddsBytes == null)
				return;

			ExportTexture(ddsBytes, path);
		}

		public bool ExportAnimationFBX(NewAnimationContainer animContainer, string name, string path)
		{
			using(var context = new AssimpContext())
			{
				Assimp.Scene scene = CreateScene(animContainer, name);
				//Assimp.Scene scene = CreateTestScene();
				return context.ExportFile(scene, path, ExportFormatID);
				//var fbx = context.ExportFile(fbxPath, PostProcessSteps.CalculateTangentSpace | PostProcessSteps.GlobalScale | PostProcessSteps.OptimizeGraph);
				//return ImportFromAssimpScene(fbx, settings);
			}
		}

		public bool ExportAnimationTable(NewAnimationContainer animContainer, string name, string path)
		{
			List<HkxBoneInfo> hkxBoneInfos = animContainer.Skeleton.HkxSkeleton;
			NewHavokAnimation newHavokAnimation = animContainer.FindAnimation(name);
			HavokAnimationData havokAnimationData = newHavokAnimation.data;

			using (StreamWriter writer = File.CreateText(path))
			{
				string columeHeader = $"{havokAnimationData.Duration}";
				for (int frameIndex = 0; frameIndex < havokAnimationData.FrameCount; ++frameIndex)
				{
					float elapse = frameIndex * havokAnimationData.FrameDuration;
					columeHeader += $"\t{frameIndex}({elapse})";
				}
				writer.WriteLine(columeHeader);

				RootMotionData rootMotionData = havokAnimationData.RootMotion;
				System.Numerics.Vector4[] rootMotionFrames = rootMotionData.Frames;

				string rootMotion = $"RootMotion({rootMotionData.Duration}{rootMotionData.Up.ToString()}{rootMotionData.Forward.ToString()})";
				for(int frameIndex = 0; frameIndex < rootMotionFrames.Length; ++frameIndex)
				{
					var frame = rootMotionFrames[frameIndex];

					rootMotion += "\t";
					rootMotion += frame.ToString();
				}

				writer.WriteLine(rootMotion);

				for(int hkxBoneIndex = 0; hkxBoneIndex < hkxBoneInfos.Count; ++hkxBoneIndex)
				{
					HkxBoneInfo hkxBoneInfo = hkxBoneInfos[hkxBoneIndex];
					string hkxBoneName = hkxBoneInfo.Name;

					string line = hkxBoneName;

					for (int frameIndex = 0; frameIndex < havokAnimationData.FrameCount; ++frameIndex)
					{
						float elapse = frameIndex * havokAnimationData.FrameDuration;
						var frame = havokAnimationData.GetTransformOnFrameByBone(hkxBoneIndex, elapse, false);

						line += "\t";
						line += frame.Translation.ToString() + frame.Rotation.ToString() + frame.Scale.ToString();
					}

					writer.WriteLine(line);
				}

				writer.Close();
			}

			return true;
		}

		static readonly Dictionary<string, System.Drawing.Imaging.ImageFormat> ExtensionFormats = new Dictionary<string, System.Drawing.Imaging.ImageFormat>()
		{
			{ ".tif", System.Drawing.Imaging.ImageFormat.Tiff },
			{ ".png", System.Drawing.Imaging.ImageFormat.Png },
			{ ".jpg", System.Drawing.Imaging.ImageFormat.Png },
			{ ".bmp", System.Drawing.Imaging.ImageFormat.Bmp },
		};

		byte[] GetFormatData(IImage image, string extension, out System.Drawing.Imaging.ImageFormat imagef, out PixelFormat pf, out int stride)
		{
			System.Drawing.Imaging.ImageFormat imageFormat = System.Drawing.Imaging.ImageFormat.Tiff;
			if (ExtensionFormats.TryGetValue(extension, out System.Drawing.Imaging.ImageFormat format))
			{
				imageFormat = format;
			}

			PixelFormat pixelFormat = PixelFormat.Format32bppArgb;

			stride = image.Stride;

			byte[] data = image.Data;
			if (image.BitsPerPixel == 8)
			{
				stride = stride * 4;
				int count = image.Width * image.Height;
				byte[] rgbData = new byte[count * 4];
				for (int i = 0; i < count; ++i)
				{
					byte gray = data[i];
					
					for (int j = 0; j < 3; ++j)
						rgbData[i * 4 + j] = gray;
					rgbData[i * 4 + 3] = 0xFF;
				}
				data = rgbData;
			}
		
			imagef = imageFormat;
			pf = pixelFormat;

			return data;
		}

		void ExportTexture(byte[] ddsBytes, string path)
		{
			CreateDirectory(path);

			using (MemoryStream stream = new MemoryStream(ddsBytes))
			{
				using (var image = Pfimage.FromStream(stream))
				{
					var handle = GCHandle.Alloc(image.Data, GCHandleType.Pinned);
					try
					{
						string extension = Path.GetExtension(path);
						if (string.IsNullOrEmpty(extension))
							extension = ".tif";

						var imageData = GetFormatData(image, extension, out System.Drawing.Imaging.ImageFormat imageFormat, out System.Drawing.Imaging.PixelFormat pixelFormat, out int stride);
						string exportPath = Path.ChangeExtension(path, extension);

						var data = Marshal.UnsafeAddrOfPinnedArrayElement(imageData, 0);

						var bitmap = new System.Drawing.Bitmap(image.Width, image.Height, stride, pixelFormat, data);
						bitmap.Save(exportPath, imageFormat);
					}
					finally
					{
						handle.Free();
					}
				}
			}
		}

		public Assimp.Scene CreateScene(FLVER2 flver, string relativeToRoot)
		{
			Node root = new Node("RootNode");

			List<Node> boneNodes = CreateBoneNodes(flver, root);
			List<Material> materials = CreateMaterials(flver, relativeToRoot);
			List<Mesh> meshes = CreateMeshes(flver, boneNodes);

			if (meshes.Count > 0)
			{
				for (int i = 0; i < meshes.Count; ++i)
				{
					Mesh mesh = meshes[i];
					FLVER2.Mesh flverMesh = flver.Meshes[i];

					Node parentNode = boneNodes[flverMesh.DefaultBoneIndex];
					Node meshNode = new Node(mesh.Name);

					meshNode.MeshIndices.Add(i);
					meshNode.Parent = parentNode;
					parentNode.Children.Add(meshNode);
				}
			}

			Assimp.Scene scene = new Assimp.Scene();

			scene.RootNode = root;
			scene.Materials = materials;
			scene.Meshes = meshes;

			if (meshes.Count <= 0)
				AddDummySkin(scene);

			return scene;
		}

		public Assimp.Scene CreateScene(NewAnimationContainer animContainer)
		{
			Node root = new Node("RootNode");

			HKX.HKASkeleton skeleton = animContainer.Skeleton.OriginalHavokSkeleton;
			List<Node> boneNodes = CreateBoneNodes(skeleton, root);

			Assimp.Scene scene = new Assimp.Scene();

			scene.RootNode = root;
			AddDummySkin(scene, boneNodes);

			return scene;
		}

		public Assimp.Scene CreateScene(NewAnimationContainer animContainer, string name)
		{
			Assimp.Scene scene = CreateScene(animContainer);

			Animation animation = CreateAnimation(animContainer, name);
			List<Animation> animations = new List<Animation>();
			animations.Add(animation);

			scene.Animations = animations;

			return scene;
		}

		List<Node> CreateBoneNodes(FLVER2 flver, Node root)
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
				Node parent = root;

				int parentIndex = bone.ParentIndex;
				if (parentIndex >= 0)
					parent = nodes[parentIndex];

				Node node = nodes[i];
				node.Parent = parent;
				if (parent != null)
					parent.Children.Add(node);
			}

			return nodes;
		}

		List<Node> CreateBoneNodes(HKASkeleton skeleton, Node root)
		{
			HKArray<HKX.Bone> bones = skeleton.Bones;
			HKArray<HKX.Transform> transforms = skeleton.Transforms;

			int count = (int)bones.Size;
			List<Node> nodes = new List<Node>(count);
			for(int i = 0; i < count; ++i)
			{
				HKX.Bone bone = bones[i];
				HKX.Transform transform = transforms[i];
				Node node = CreateBoneNode(bone, transform);
				nodes.Add(node);
			}

			HKArray<HKShort> parentIndices = skeleton.ParentIndices;
			for (int i = 0; i < count; ++i)
			{
				Node parent = root;

				int parentIndex = parentIndices[i].data;
				if (parentIndex >= 0)
					parent = nodes[parentIndex];

				Node node = nodes[i];
				node.Parent = parent;

				if (parent != null)
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

			System.Numerics.Matrix4x4 transform = System.Numerics.Matrix4x4.CreateScale(scale)
				* System.Numerics.Matrix4x4.CreateRotationX(rotation.X)
				* System.Numerics.Matrix4x4.CreateRotationZ(rotation.Z)
				* System.Numerics.Matrix4x4.CreateRotationY(rotation.Y)
				* System.Numerics.Matrix4x4.CreateTranslation(translation);


			Node node = CreateNode(bone.Name, transform);

			return node;
		}

		Node CreateBoneNode(HKX.Bone bone, HKX.Transform t)
		{
			System.Numerics.Vector3 scale = new System.Numerics.Vector3(t.Scale.Vector.X, t.Scale.Vector.Y, t.Scale.Vector.Z);
			System.Numerics.Quaternion rotation = new System.Numerics.Quaternion(t.Rotation.Vector.X, t.Rotation.Vector.Y, t.Rotation.Vector.Z, t.Rotation.Vector.W);
			System.Numerics.Vector3 translation = new System.Numerics.Vector3(t.Position.Vector.X, t.Position.Vector.Y, t.Position.Vector.Z);

			rotation = new System.Numerics.Quaternion(-Mirror.X * rotation.X, -Mirror.Y * rotation.Y, -Mirror.Z * rotation.Z, rotation.W);
			translation = System.Numerics.Vector3.Multiply(translation, To(UnitScale * Mirror));

			System.Numerics.Matrix4x4 transform = System.Numerics.Matrix4x4.CreateScale(scale)
				* System.Numerics.Matrix4x4.CreateFromQuaternion(rotation)
				* System.Numerics.Matrix4x4.CreateTranslation(translation);

			Node node = CreateNode(bone.Name.GetString(), transform);

			return node;
		}

		Node CreateNode(string name, System.Numerics.Matrix4x4 transform)
		{
			Node node = new Node();

			node.Name = name;
			node.Transform = From(transform);

			return node;
		}

		static readonly List<string> TransparentHints = new List<string>()
		{
			"_fur_",
			"_hair",
		};

		List<Material> CreateMaterials(FLVER2 flver, string relativeToRoot)
		{
			List<FLVER2.Material> flverMaterials = flver.Materials;
			int count = flverMaterials.Count;

			List<Material> materials = new List<Material>(count);

			for (int i = 0; i < count; ++i)
			{
				FLVER2.Material flverMaterial = flverMaterials[i];
				List<FLVER2.Texture> Textures = flverMaterial.Textures;

				List<TextureSlot> textureSlots = new List<TextureSlot>(Textures.Count);

				FlverMaterialDefInfo flverMaterialDefInfo = FlverMaterialDefInfo.Lookup(flverMaterial.MTD);
				var samplerConfigs = flverMaterialDefInfo.SamplerConfigs;

				bool isTransparentMaterial = TransparentHints.FindIndex(e => flverMaterial.Name.ToLower().Contains(e)) >= 0
					|| TransparentHints.FindIndex(e => flverMaterial.MTD.ToLower().Contains(e)) >= 0;

				List<int> typeCount = new List<int>();

				for (int j = 0; j < Textures.Count; ++j)
				{
					FLVER2.Texture texture = Textures[j];

					string paramName = texture.Type;
					string path = texture.Path;

					if (string.IsNullOrEmpty(path))
					{
						if (samplerConfigs.ContainsKey(paramName))
						{
							var samplerConfig = samplerConfigs[paramName];
							if (!string.IsNullOrEmpty(samplerConfig.TexPath))
								path = samplerConfig.TexPath;
							else
								path = samplerConfig.DefaultTexPath;
						}
					}

					if (string.IsNullOrEmpty(path))
						continue;

					bool isTransparent = false;
					Assimp.TextureType type = GetTextureType(paramName);

					if (isTransparentMaterial && type == Assimp.TextureType.Diffuse)
					{
						isTransparent = IsHasAlpha(path);						
						path = relativeToRoot + ToRelativePath(ToTexturePath(path, false, isTransparent));
					}
					else
					{
						path = relativeToRoot + ToRelativePath(ToTexturePath(path));
					}

					int uvIndex = GetUVIndex(type, typeCount);

					TextureSlot slot = new TextureSlot();

					slot.FilePath = path;
					slot.TextureType = type;
					slot.TextureIndex = typeCount[(int)type] - 1;
					slot.UVIndex = 0;

					textureSlots.Add(slot);

					if (isTransparent)
					{
						slot.TextureType = Assimp.TextureType.Opacity;
						textureSlots.Add(slot);
					}
				}

				SortTextureSlots(textureSlots);

				Material material = new Material();

				material.Name = GetIndexName(flverMaterial.Name, i);
				material.ShadingMode = ShadingMode.Phong;
				material.BlendMode = BlendMode.Default;
				material.Shininess = 0.01f;
				material.ShininessStrength = 0.01f;
				for (int j = 0; j < textureSlots.Count; ++j)
				{
					TextureSlot textureSlot = textureSlots[j];
					material.AddMaterialTexture(ref textureSlot);
				}

				materials.Add(material);
			}

			return materials;
		}

		List<Assimp.Bone> CreateBones(List<Node> nodes)
		{
			List<Assimp.Bone> bones = new List<Assimp.Bone>(nodes.Count);

			for (int i = 0; i < nodes.Count; ++i)
			{
				Node node = nodes[i];

				Assimp.Bone bone = new Assimp.Bone();
				bone.Name = node.Name;
				bone.Node = node;

				bones.Add(bone);
			}

			return bones;
		}


		List<Mesh> CreateMeshes(FLVER2 flver, List<Node> boneNodes)
		{
			List<FLVER2.Mesh> flverMeshes = flver.Meshes;
			List<FLVER2.Material> flverMaterials = flver.Materials;
			List<Mesh> meshes = new List<Mesh>(flverMeshes.Count);

			for (int i = 0; i < flverMeshes.Count; ++i)
			{
				List<Assimp.Bone> bones = CreateBones(boneNodes);

				FLVER2.Mesh m = flverMeshes[i];
				int index = m.MaterialIndex;
				string name = GetIndexName(flverMaterials[index].Name, index);
				Mesh mesh = CreateMesh(m, name, bones);
				meshes.Add(mesh);
			}

			return meshes;
		}

		Mesh CreateMesh(FLVER2.Mesh m, String name, List<Assimp.Bone> bones)
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

			for(int i = 0; i < vertices.Count; ++i)
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

				for (int j = 0; j < uvCount; ++j)
				{
					Vector3D uv = From(vertex.UVs[j]);
					// flip vertical
					uv.Y = -(uv.Y - 0.5f) + 0.5f;
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

			for (int i = 0; i < uvCount; ++i)
			{
				mesh.UVComponentCount[i] = 2;
			}

			return mesh;
		}

		public Animation CreateAnimation(NewAnimationContainer animContainer, string name)
		{
			List<HkxBoneInfo> hkxBoneInfos = animContainer.Skeleton.HkxSkeleton;
			NewHavokAnimation newHavokAnimation = animContainer.FindAnimation(name);
			HavokAnimationData havokAnimationData = newHavokAnimation.data;

			List<NodeAnimationChannel> tracks = new List<NodeAnimationChannel>(hkxBoneInfos.Count);

			for (int hkxBoneIndex = 0; hkxBoneIndex < hkxBoneInfos.Count; ++hkxBoneIndex)
			{
				NodeAnimationChannel track = CreateTrack(havokAnimationData, hkxBoneInfos, hkxBoneIndex);
				tracks.Add(track);
			}

			// root motion
			if (tracks.Count > 0 && havokAnimationData.RootMotion != null)
			{
				NodeAnimationChannel track = tracks[0];
				RootMotionData rootMotion = havokAnimationData.RootMotion;

				List<VectorKey> positionKeys = track.PositionKeys;
				List<QuaternionKey> rotationKeys = track.RotationKeys;
				List<VectorKey> scaleKeys = track.ScalingKeys;

				List<VectorKey> newPositionKeys = new List<VectorKey>(positionKeys.Count);
				List<QuaternionKey> newRotationKeys = new List<QuaternionKey>(rotationKeys.Count);
				List<VectorKey> newScaleKeys = new List<VectorKey>(scaleKeys.Count);

				if (positionKeys.Count == rootMotion.Frames.Length)
				{
					for (int i = 0; i < positionKeys.Count; ++i)
					{
						Vector4 frame = rootMotion.Frames[i];

						VectorKey positionKey = positionKeys[i];
						QuaternionKey rotationKey = rotationKeys[i];
						VectorKey scaleKey = scaleKeys[i];

						System.Numerics.Matrix4x4 transform = System.Numerics.Matrix4x4.CreateScale(To(scaleKey.Value))
							* System.Numerics.Matrix4x4.CreateFromQuaternion(To(rotationKey.Value))
							* System.Numerics.Matrix4x4.CreateTranslation(To(positionKey.Value))
							* System.Numerics.Matrix4x4.CreateRotationY(-Mirror.Y * frame.W)
							* System.Numerics.Matrix4x4.CreateTranslation(new System.Numerics.Vector3(frame.X, frame.Y, frame.Z) * (To(UnitScale) * To(Mirror)));

						System.Numerics.Matrix4x4.Decompose(transform, out System.Numerics.Vector3 scale, out System.Numerics.Quaternion rotation, out System.Numerics.Vector3 translate);

						VectorKey newPositionKey = new VectorKey(positionKey.Time, From(translate));
						QuaternionKey newRotationKey = new QuaternionKey(rotationKey.Time, From(rotation));
						VectorKey newScaleKey = new VectorKey(rotationKey.Time, From(scale));

						newPositionKeys.Add(newPositionKey);
						newRotationKeys.Add(newRotationKey);
						newScaleKeys.Add(newScaleKey);
					}

					track.PositionKeys = newPositionKeys;
					track.RotationKeys = newRotationKeys;
					track.ScalingKeys = newScaleKeys;
				}
			}

			Animation animation = new Animation();

			animation.Name = name;
			animation.DurationInTicks = havokAnimationData.FrameCount;
			animation.TicksPerSecond = 1 / (double)havokAnimationData.FrameDuration;
			animation.NodeAnimationChannels = tracks;

			return animation;
		}

		NodeAnimationChannel CreateTrack(HavokAnimationData havokAnimationData, List<HkxBoneInfo> hkxBoneInfos, int hkxBoneIndex)
		{
			int frameCount = havokAnimationData.FrameCount;

			List<VectorKey> positionKeys = new List<VectorKey>(frameCount);
			List<QuaternionKey> rotationKeys = new List<QuaternionKey>(frameCount);
			List<VectorKey> scaleKeys = new List<VectorKey>(frameCount);

			for (int frameIndex = 0; frameIndex < frameCount; ++frameIndex)
			{
				NewBlendableTransform frame = havokAnimationData.GetTransformOnFrameByBone(hkxBoneIndex, frameIndex, false);

				Vector3D translate = From(frame.Translation) * Mirror * UnitScale;
				Assimp.Quaternion rotation = From(frame.Rotation);
				rotation = new Assimp.Quaternion(rotation.W, -Mirror.X * rotation.X, -Mirror.Y * rotation.Y, -Mirror.Z * rotation.Z);
				Vector3D scale = From(frame.Scale);

				VectorKey positionKey = new VectorKey(frameIndex, translate);
				QuaternionKey rotationKey = new QuaternionKey(frameIndex, rotation);
				VectorKey scaleKey = new VectorKey(frameIndex, scale);

				positionKeys.Add(positionKey);
				rotationKeys.Add(rotationKey);
				scaleKeys.Add(scaleKey);
			}

			HkxBoneInfo hkxBoneInfo = hkxBoneInfos[hkxBoneIndex];
			string hkxBoneName = hkxBoneInfo.Name;

			NodeAnimationChannel track = new NodeAnimationChannel();

			track.NodeName = hkxBoneName;

			track.PositionKeys = positionKeys;
			track.RotationKeys = rotationKeys;
			track.ScalingKeys = scaleKeys;

			return track;
		}

		void AddDummySkin(Assimp.Scene scene)
		{
			Node master = scene.RootNode.FindNode("Master");
			if (master == null)
				return;

			List<Node> boneNodes = new List<Node>();
			Queue<Node> queue = new Queue<Node>();
			queue.Enqueue(master);
			while (queue.Count > 0)
			{
				Node node = queue.Dequeue();
				for (int i = 0; i < node.ChildCount; ++i)
				{
					Node child = node.Children[i];
					queue.Enqueue(child);
				}
				boneNodes.Add(node);
			}

			AddDummySkin(scene, boneNodes);
		}

		void AddDummySkin(Assimp.Scene scene, List<Node> boneNodes)
		{
			Node node = new Node("DummySkin");
			node.Parent = scene.RootNode;
			node.Parent.Children.Add(node);

			Mesh dummySkin = CreateDummySkin(boneNodes);
			node.MeshIndices.Add(scene.Meshes.Count);
			scene.Meshes.Add(dummySkin);

			if (scene.Materials.Count <= 0)
			{
				Material material = new Material();
				material.Name = "DummySkinMaterial";
				scene.Materials.Add(material);
			}
		}

		Mesh CreateDummySkin(List<Node> boneNodes)
		{
			BoundingBox aabb = CalculateAABB(boneNodes);

			Vector3D min = new Vector3D(aabb.Min.X, aabb.Min.Y, aabb.Min.Z);
			Vector3D max = new Vector3D(aabb.Max.X, aabb.Max.Y, aabb.Max.Z);

			List<Vector3D> vertices = new List<Vector3D>()
			{
				min,
				min,
				min,
				max,
				max,
				max
			};

			List<int> indices = new List<int>()
			{
				0,1,2,3,4,5
			};

			List<Face> faces = new List<Face>();
			for (int i = 0; i < (indices.Count / 3); i++)
			{
				Face face = new Face();
				face.Indices.Add(indices[i * 3]);
				face.Indices.Add(indices[i * 3 + 1]);
				face.Indices.Add(indices[i * 3 + 2]);
				faces.Add(face);
			}

			List<Assimp.Bone> bones = CreateBones(boneNodes);
			float weight = 1.0f / bones.Count;
			for (int i = 0; i < bones.Count; ++i)
			{
				Assimp.Bone bone = bones[i];
				for (int j = 0; j < vertices.Count; ++j)
				{
					bone.VertexWeights.Add(new VertexWeight(j, weight));
				}
			}

			Mesh mesh = new Mesh();

			mesh.Name = "DummySkin";
			mesh.Vertices = vertices;
			mesh.Faces = faces;
			mesh.MaterialIndex = 0;
			mesh.Bones = bones;

			return mesh;
		}

		public Assimp.Scene CreateTestScene()
        {
            using(var context = new AssimpContext())
            {
				Assimp.Scene testScene = context.ImportFile("C:\\Users\\chypy\\Documents\\3dsMax\\export\\test.FBX");
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

		List<FLVER2> GetAllFlvers(ref List<IBinder> binders)
		{
			List<FLVER2> flvers = new List<FLVER2>();

			Model main = Scene.MainModel;
			flvers.Add(main.flver);
			binders?.Add(main.binder);

			NewChrAsm charAsm = main.ChrAsm;
			if (charAsm == null)
				return flvers;

			List<NewMesh> meshes = new List<NewMesh>()
			{
				charAsm.HeadMesh,
				charAsm.BodyMesh,
				charAsm.ArmsMesh,
				charAsm.LegsMesh,
				charAsm.FaceMesh,
				charAsm.FacegenMesh,
				charAsm.HairMesh,
			};

			for (int i = 0; i < meshes.Count; ++i)
			{
				NewMesh mesh = meshes[i];
				if (mesh == null)
					continue;

				FLVER2 flver = mesh.flver;
				if (flver == null)
					continue;

				flvers.Add(flver);
				binders?.Add(mesh.binder);
			}

			List<Model> models = new List<Model>()
			{
				charAsm.LeftWeaponModel0,
				charAsm.LeftWeaponModel1,
				charAsm.LeftWeaponModel2,
				charAsm.LeftWeaponModel3,
				charAsm.RightWeaponModel0,
				charAsm.RightWeaponModel1,
				charAsm.RightWeaponModel2,
				charAsm.RightWeaponModel3,
			};

			for (int i = 0; i < models.Count; ++i)
			{
				Model model = models[i];
				if (model == null)
					continue;

				FLVER2 flver = model.flver;
				if(flver == null)
					continue;

				flvers.Add(flver);
				binders?.Add(model.binder);
			}

			return flvers;
		}

		List<NewAnimationContainer> GetAllAnimationContainer()
		{
			List<NewAnimationContainer> animationContainers = new List<NewAnimationContainer>();
			
			Model main = Scene.MainModel;
			animationContainers.Add(main.AnimContainer);

			NewChrAsm charAsm = main.ChrAsm;
			if (charAsm == null)
				return animationContainers;

			List<Model> models = new List<Model>()
			{
				charAsm.LeftWeaponModel0,
				charAsm.LeftWeaponModel1,
				charAsm.LeftWeaponModel2,
				charAsm.LeftWeaponModel3,
				charAsm.RightWeaponModel0,
				charAsm.RightWeaponModel1,
				charAsm.RightWeaponModel2,
				charAsm.RightWeaponModel3,
			};

			for (int i = 0; i < models.Count; ++i)
			{
				Model model = models[i];
				if (model == null)
					continue;

				NewAnimationContainer animationContainer = model.AnimContainer;
				if (animationContainer == null)
					continue;

				animationContainers.Add(animationContainer);
			}

			return animationContainers;
		}

		bool IsHasAlpha(string path)
		{
			var shortName = Utils.GetShortIngameFileName(path).ToLower();
			if (!TexturePool.Fetches.ContainsKey(shortName))
				return false;

			TextureFetchRequest request = TexturePool.Fetches[shortName];
			byte[] ddsBytes = request?.TexInfo?.DDSBytes;
			if (ddsBytes == null)
				return false;

			return IsHasAlpha(ddsBytes);
		}

		bool IsHasAlpha(byte[] ddsBytes)
		{
  			using (MemoryStream stream = new MemoryStream(ddsBytes))
			{
				using (var image = Pfimage.FromStream(stream))
				{
					return IsHasAlpha(image);
				}
			}
		}

		bool IsHasAlpha(IImage image)
		{
			byte[] data = image.Data;
			if (image.BitsPerPixel != 32)
				return false;

			int stride = image.Stride;

			for (int i = 0; i < image.Width; ++i)
			{
				for (int j = 0; j < image.Height; ++j)
				{
					byte alpha = data[j * stride + i * 4 + 3];
					if (alpha >= 254)
						continue;

					return true;
				}
			}

			return false;
		}


		List<string> GetReferenceTexturePaths(FLVER2 flver)
		{
			List<string> texturePaths = new List<string>();
			List<FLVER2.Material> flverMaterials = flver.Materials;

			for (int i = 0; i < flverMaterials.Count; ++i)
			{
				FLVER2.Material flverMaterial = flverMaterials[i];
				GetReferenceTexturePaths(flverMaterial, texturePaths);
			}

			return texturePaths;
		}

		void GetReferenceTexturePaths(FLVER2.Material flverMaterial, List<string> texturePaths)
		{
			List<FLVER2.Texture> flverTextures = flverMaterial.Textures;
			for (int j = 0; j < flverTextures.Count; ++j)
			{
				FLVER2.Texture flverTexture = flverTextures[j];
				string filePath = ToTexturePath(flverTexture.Path);
				if (string.IsNullOrEmpty(filePath) || texturePaths.Contains(filePath))
					continue;

				texturePaths.Add(filePath);
			}

			FlverMaterialDefInfo flverMaterialDefInfo = FlverMaterialDefInfo.Lookup(flverMaterial.MTD);
			if (flverMaterialDefInfo == null)
				return;

			List<FlverMaterialDefInfo.SamplerConfig> samplerConfigs = flverMaterialDefInfo.SamplerConfigs.Values.ToList();
			if (samplerConfigs == null)
				return;

			for (int j = 0; j < samplerConfigs.Count; ++j)
			{
				FlverMaterialDefInfo.SamplerConfig samplerConfig = samplerConfigs[j];
				string filePath = ToTexturePath(samplerConfig.TexPath);
				if (!string.IsNullOrEmpty(filePath) && !texturePaths.Contains(filePath))
					texturePaths.Add(filePath);

				string defaultPath = ToTexturePath(samplerConfig.DefaultTexPath);
				if(!string.IsNullOrEmpty(defaultPath) && !texturePaths.Contains(defaultPath))
					texturePaths.Add(defaultPath);
			}
		}

		static readonly Dictionary<string, Assimp.TextureType> paramTypeDic = new Dictionary<string, Assimp.TextureType>()
		{
			{ "DIFFUSE", Assimp.TextureType.Diffuse },
			{ "ALBEDO", Assimp.TextureType.Diffuse },
			{ "SPECULAR", Assimp.TextureType.Specular },
			{ "SHININESS", Assimp.TextureType.Specular },
			{ "REFLECTANCE", Assimp.TextureType.Specular },
			{ "METALLIC", Assimp.TextureType.Specular },
			{ "AMBIENT", Assimp.TextureType.Ambient },
			{ "EMISSIVE", Assimp.TextureType.Emissive },
			{ "NORMAL", Assimp.TextureType.Normals },
			{ "BUMP", Assimp.TextureType.Normals },
			{ "Displacement", Assimp.TextureType.Displacement },
			{ "MASK1", Assimp.TextureType.Reflection },
			{ "MASK3", Assimp.TextureType.Lightmap },
			{ "BLENDMASK", Assimp.TextureType.Lightmap },
		};

		Assimp.TextureType GetTextureType(string paramName)
		{
			Assimp.TextureType type = Assimp.TextureType.None;

			string formatName = paramName.ToUpper();
			List<string> keys = paramTypeDic.Keys.ToList();
			int index = keys.FindIndex( e => formatName.Contains(e));
			if (index >= 0)
			{
				string key = keys[index];
				type = paramTypeDic[key];
			}

			return type;
		}

		int GetUVIndex(Assimp.TextureType type, List<int> typeCount)
		{
			int uvIndex = 0;
			int typeIndex = (int)type;
			if (typeIndex >= typeCount.Count)
			{
				for (int i = typeCount.Count; i <= typeIndex; ++i)
					typeCount.Add(0);
			}

			uvIndex = typeCount[typeIndex]++;

			if (uvIndex > 0 && typeIndex >= (int)Assimp.TextureType.Opacity)
				uvIndex = 2;
			else if (type == Assimp.TextureType.Displacement)
				uvIndex = 1;
			else if (type == Assimp.TextureType.Lightmap)
				uvIndex = 0;

			return uvIndex;
		}

		static readonly List<string> SortHint = new List<string>()
		{
			"systex_",
			"_dummy_",
			"_expression_",
			"_skin_a_",
			"_skin_b_",
			"_skin_c_",
			"_skin_d_",
			"_damage_",
		};

		int FindSortIndex(string path)
		{
			for (int i = 0; i < SortHint.Count; ++i)
			{
				string hint = SortHint[i];
				if (path.Contains(hint))
					return i;
			}

			return -1;
		}

		void SortTextureSlots(List<TextureSlot> slots)
		{
			int count = (int)Assimp.TextureType.Unknown + 1;
			for (int i = 0; i < count; ++i)
			{
				Assimp.TextureType textureType = (Assimp.TextureType)i;

				List<int> typeSlots = new List<int>();
				int index = 0;
				while ((index = slots.FindIndex(index, e => e.TextureType == textureType)) != -1)
					typeSlots.Add(index++);

				typeSlots.Sort((x, y) =>
				{
					string xName = slots[x].FilePath.ToLower();
					string yName = slots[y].FilePath.ToLower();

					int xIndex = FindSortIndex(xName);
					int yIndex = FindSortIndex(yName);

					return xIndex - yIndex;
				});

				for (int j = 0; j < typeSlots.Count; ++j)
				{
					int slotIndex = typeSlots[j];
					TextureSlot slot = slots[slotIndex];
					slot.TextureIndex = j;
					slots[slotIndex] = slot;
				}
			}
		}

		string GetIndexName(string name, int index)
		{
			string result = name;
			if (!string.IsNullOrEmpty(name))
				result = $"{name}_{index}";

			return result;
		}

		BoundingBox CalculateAABB(List<Node> boneNodes)
		{
			List<Vector3> positions = new List<Vector3>(boneNodes.Count);
			
			for (int i = 0; i < boneNodes.Count; ++i)
			{
				Node node = boneNodes[i];
				Matrix4x4 transform = node.Transform;

				Node parent = node.Parent;
				while (parent != null && boneNodes.Contains(parent))
				{
					transform = transform * parent.Transform;
					parent = parent.Parent;
				}

				Vector3 position = new Vector3(transform.A4, transform.B4, transform.C4);
				positions.Add(position);
			}

			BoundingBox aabb = BoundingBox.CreateFromPoints(positions);
			return aabb;
		}

		Vector3D From(System.Numerics.Vector3 v)
		{
			Vector3D result = new Vector3D(v.X, v.Y, v.Z);

			return result;
		}

		Assimp.Quaternion From(System.Numerics.Quaternion q)
		{
			Assimp.Quaternion result = new Assimp.Quaternion(q.W, q.X, q.Y, q.Z);

			return result;
		}

		System.Numerics.Vector3 To(Vector3D v)
		{
			System.Numerics.Vector3 result = new System.Numerics.Vector3(v.X, v.Y, v.Z);

			return result;
		}

		System.Numerics.Quaternion To(Assimp.Quaternion q)
		{
			System.Numerics.Quaternion result = new System.Numerics.Quaternion(q.X, q.Y, q.Z, q.W);

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

		void WriteTextFile(string content, string path)
		{
			CreateDirectory(path);
			File.WriteAllText(path, content);
		}

		void CreateDirectory(string path)
		{
			var dir = Path.GetDirectoryName(path);
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);
		}

		static readonly string[] Directories = new string[]
		{
			"INTERROOT_win64",
			"Model",
			"Material",
			"data",
		};

		string ToRelativePath(string path)
		{
			string relativePath = path;

			for (int i = 0; i < Directories.Length; ++i)
			{
				string directory = Directories[i];
				int index = path.IndexOf(directory);
				if (index < 0)
					continue;

				relativePath = path.Substring(index + directory.Length);
				break;
			}

			return relativePath;
		}

		string ToTexturePath(string path, bool isCheckTransparent = true, bool isTransparent = false)
		{
			if(string.IsNullOrEmpty(path))
				return path;

			string extension = Path.GetExtension(path);
			if(extension.ToLower().Contains("tif"))
			{
				if((isCheckTransparent && IsHasAlpha(path)) || (!isCheckTransparent && isTransparent))
					path = Path.ChangeExtension(path, ".bmp");
			}

			return path;
		}

		static readonly Dictionary<string, string> ReplaceDictionary = new Dictionary<string, string>()
		{
			{"hkx", "ani"},
			{"hkx_compendium", "ani"},
		};

		string ReplacePaths(string path)
		{
			if (string.IsNullOrEmpty(path))
				return path;

			string result = path;
			foreach (KeyValuePair<string, string> entry in ReplaceDictionary)
				result = result.Replace($"\\{entry.Key}\\", $"\\{entry.Value}\\");
			return result;
		}

		string RelativeToRoot(string path)
		{
			string relativeToRoot = "";

			string directoryName = Path.GetDirectoryName(path);
			string[] directories = directoryName.Split("\\");

			for (int j = directories.Length - 1; j >= 0; --j)
			{
				string directory = directories[j];
				if (string.IsNullOrEmpty(directory))
					continue;

				relativeToRoot += "..\\";
			}

			int index = relativeToRoot.LastIndexOf("\\");
			if (index >= 0)
				relativeToRoot = relativeToRoot.Substring(0, index);

			return relativeToRoot;
		}

		const string ExportFormatID = "fbxa";
	}
}
