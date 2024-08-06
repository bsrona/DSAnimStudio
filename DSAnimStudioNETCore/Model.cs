using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using DSAnimStudio.GFXShaders;
using FMOD;
using SoulsAssetPipeline;
using DSAnimStudio.ImguiOSD;
using DSAnimStudio.DebugPrimitives;
using Havoc.Objects;
using Havoc.IO.Tagfile.Binary;
using SoulsAssetPipeline.Animation;
using HKX2;
using NAudio.Gui;
using Assimp;
using System.Security.Principal;

namespace DSAnimStudio
{
    public class Model : IDisposable
    {
        public static Model GetDummyModel()
        {
            var mdl = new Model()
            {
                AnimContainer = new NewAnimationContainer(),
                EnableSkinning = false,
                IsVisible = false,
                MainMesh = NewMesh.GetDummyMesh(),
                NpcParam = new ParamData.NpcParam(),
                
            };

            mdl.ChrAsm = new NewChrAsm(mdl);
            mdl.DbgPrimDrawer = new DBG.DbgPrimDrawer(mdl);
            mdl.DummyPolyMan = new NewDummyPolyManager(mdl);

            return mdl;
        }

        public string Name { get; set; } = "Model";

        //public FLVER2 FlverFileForDebug = null;

        public bool IsVisible { get; set; } = true;
        public void SetIsVisible(bool isVisible)
        {
            IsVisible = isVisible;
        }

        public float Opacity = 1;

        public Vector3? GetLockonPoint(bool isAbsoluteRootMotion = false)
        {
            if (IS_REMO_DUMMY || IS_REMO_NOTSKINNED)
                return CurrentTransformPosition;

            if (isAbsoluteRootMotion)
            {
                if (DummyPolyMan.DummyPolyByRefID.ContainsKey(220))
                {
                    var mat = DummyPolyMan.DummyPolyByRefID[220][0].CurrentMatrix;
                    return Vector3.Transform(Vector3.Zero, mat);
                }
                else
                {
                    return null;
                }
            }

            var possible = DummyPolyMan.GetDummyPosByID(220, getAbsoluteWorldPos: true);
            if (possible.Any())
                return possible.First();
            else
                return null;
        }

        public NewMesh MainMesh;

        public NewAnimSkeleton_FLVER SkeletonFlver;
        public NewAnimationContainer AnimContainer;
        public hkRootLevelContainer RagdollLevelContainer
        {
            get { return _ragdollLevelContainer; }
            set
            {
                if (_ragdollLevelContainer == value)
                    return;
                _ragdollLevelContainer = value;
                UpdateRagdollPose();
                CreateRagdollPrimitives();
            }
        }
        hkRootLevelContainer _ragdollLevelContainer;
        public Matrix[] RagdollPoseMatrices;

        public bool IsRemoModel = false;

        public NewDummyPolyManager DummyPolyMan;
        public DBG.DbgPrimDrawer DbgPrimDrawer;
        public List<IDbgPrim> RagdollBodies;
        public List<IDbgPrim> RagdollConstraints;
        public NewChrAsm ChrAsm = null;
        public ParamData.NpcParam NpcParam = null;

        public object _lock_NpcParams = new object();
        public List<ParamData.NpcParam> PossibleNpcParams = new List<ParamData.NpcParam>();
        public Dictionary<int, List<string>> NpcMaterialNamesPerMask = new Dictionary<int, List<string>>();
        public List<int> NpcMasksEnabledOnAllNpcParams = new List<int>();

        public IBinder binder;
        public FLVER2 flver;
        public string flverName;

        private int _selectedNpcParamIndex = -1;
        public int SelectedNpcParamIndex
        {
            get => _selectedNpcParamIndex;
            set
            {
                lock (_lock_NpcParams)
                {
                    NpcParam = (value >= 0 && value < PossibleNpcParams.Count)
                            ? PossibleNpcParams[value] : null;
                    _selectedNpcParamIndex = value;
                    if (NpcParam != null)
                    {
                        //CurrentModel.DummyPolyMan.RecreateAllHitboxPrimitives(CurrentModel.NpcParam);
                        NpcParam.ApplyToNpcModel(this);
                    }
                }
            }
        }

        public void RescanNpcParams()
        {
            lock (_lock_NpcParams)
            {
                PossibleNpcParams = ParamManager.FindNpcParams(Name);
                PossibleNpcParams = PossibleNpcParams.OrderBy(x => x.ID).ToList();
                var additional = ParamManager.FindNpcParams(Name, matchCXXX0: true);
                foreach (var n in additional)
                {
                    if (!PossibleNpcParams.Contains(n))
                        PossibleNpcParams.Add(n);
                }

                

                if (PossibleNpcParams.Count > 0)
                    SelectedNpcParamIndex = 0;
                else
                    SelectedNpcParamIndex = -1;
            }
        }

        public bool ApplyBindPose = false;

        public float BaseTrackingSpeed = 360;
        public float CurrentTrackingSpeed = 0;

        public float CharacterTrackingRotation = 0;

        public float TrackingTestInput = 0;

        public float DebugAnimWeight_Deprecated = 1;

        public bool EnableSkinning = true;

        public void UpdateTrackingTest(float elapsedTime)
        {
            try
            {
                TrackingTestInput = MathHelper.Clamp(TrackingTestInput, -1, 1);
                float delta = (MathHelper.ToRadians(CurrentTrackingSpeed)) * elapsedTime * TrackingTestInput;
                CharacterTrackingRotation += delta;

                if (float.IsNaN(CharacterTrackingRotation))
                    Console.WriteLine("Breakpoint hit");

                lock (Scene._lock_ModelLoad_Draw)
                {
                    AnimContainer?.AddRelativeRootMotionRotation(delta);
                }
            }
            catch
            {

            }
           
        }

        public static float GlobalTrackingInput = 0;

        public BoundingBox Bounds;

        public const int DRAW_MASK_LENGTH = 98;

        public bool[] DefaultDrawMask = new bool[DRAW_MASK_LENGTH];
        public bool[] DrawMask = new bool[DRAW_MASK_LENGTH];

        //public enum ModelType
        //{
        //    ModelTypeFlver,
        //    ModelTypeCollision,
        //};
        //ModelType Type;

        public Transform StartTransform = Transform.Default;

        

        public Transform CurrentTransform = Transform.Default;

        public Vector3 CurrentTransformPosition => Vector3.Transform(Vector3.Zero, CurrentTransform.WorldMatrix);

        /// <summary>
        /// This is needed to make weapon hitboxes work.
        /// </summary>
        public bool IS_PLAYER => Name == "c0000" || Name == "c0000_0000";

        public bool IS_REMO_DUMMY = false;
        public bool IS_REMO_NOTSKINNED = false;
        public DbgPrimWireArrow RemoDummyTransformPrim = null;
        public StatusPrinter RemoDummyTransformTextPrint = null;

        public bool IS_PLAYER_WEAPON = false;


        public Model()
        {
            AnimContainer = new NewAnimationContainer();

            DummyPolyMan = new NewDummyPolyManager(this);
            DbgPrimDrawer = new DBG.DbgPrimDrawer(this);

            for (int i = 0; i < DRAW_MASK_LENGTH; i++)
            {
                DefaultDrawMask[i] = DrawMask[i] = true;
            }
        }

        public Dictionary<int, List<string>> GetMaterialNamesPerMask()
        {
            Dictionary<int, List<FlverMaterial>> materialsByMask = 
                new Dictionary<int, List<FlverMaterial>>();

            foreach (var mat in MainMesh.Materials)
            {
                if (!materialsByMask.ContainsKey(mat.ModelMaskIndex))
                    materialsByMask.Add(mat.ModelMaskIndex, new List<FlverMaterial>());

                if (!materialsByMask[mat.ModelMaskIndex].Contains(mat))
                    materialsByMask[mat.ModelMaskIndex].Add(mat);
            }

            var result = new Dictionary<int, List<string>>();

            foreach (var kvp in materialsByMask)
            {
                result.Add(kvp.Key, kvp.Value.Select(sm => sm.Name).ToList());
            }

            return result;
        }

        public void ResetDrawMaskToAllVisible()
        {
            for (int i = 0; i < DRAW_MASK_LENGTH; i++)
            {
                DrawMask[i] = DefaultDrawMask[i] = true;
            }
        }

        public void ResetDrawMaskToDefault()
        {
            for (int i = 0; i < DRAW_MASK_LENGTH; i++)
            {
                DrawMask[i] = DefaultDrawMask[i];
            }
        }

        public void TryLoadGlobalShaderConfigs()
        {
            lock (Scene._lock_ModelLoad_Draw)
            {
                //FlverShaderConfig.ClearCache();
                MainMesh?.TryLoadGlobalShaderConfigs();
                if (ChrAsm != null)
                {
                    ChrAsm?.HeadMesh?.TryLoadGlobalShaderConfigs();
                    ChrAsm?.BodyMesh?.TryLoadGlobalShaderConfigs();
                    ChrAsm?.ArmsMesh?.TryLoadGlobalShaderConfigs();
                    ChrAsm?.LegsMesh?.TryLoadGlobalShaderConfigs();
                    ChrAsm?.RightWeaponModel0?.TryLoadGlobalShaderConfigs();
                    ChrAsm?.RightWeaponModel1?.TryLoadGlobalShaderConfigs();
                    ChrAsm?.RightWeaponModel2?.TryLoadGlobalShaderConfigs();
                    ChrAsm?.RightWeaponModel3?.TryLoadGlobalShaderConfigs();
                    ChrAsm?.LeftWeaponModel0?.TryLoadGlobalShaderConfigs();
                    ChrAsm?.LeftWeaponModel1?.TryLoadGlobalShaderConfigs();
                    ChrAsm?.LeftWeaponModel2?.TryLoadGlobalShaderConfigs();
                    ChrAsm?.LeftWeaponModel3?.TryLoadGlobalShaderConfigs();
                    ChrAsm?.FacegenMesh?.TryLoadGlobalShaderConfigs();
                    ChrAsm?.BodyMesh?.TryLoadGlobalShaderConfigs();
                    ChrAsm?.HairMesh?.TryLoadGlobalShaderConfigs();
                }
            }
        }

        public Model(IProgress<double> loadingProgress, string name, IBinder chrbnd, int modelIndex, 
            IBinder anibnd, IBinder texbnd = null, List<string> additionalTpfNames = null, 
            string possibleLooseTpfFolder = null, int baseDmyPolyID = 0, 
            bool ignoreStaticTransforms = false, IBinder additionalTexbnd = null,
            SoulsAssetPipeline.FLVERImporting.FLVER2Importer.ImportedFLVER2Model modelToImportDuringLoad = null,
            SapImportConfigs.ImportConfigFlver2 modelImportConfig = null, List<TPF> tpfsUsed = null)
            : this()
        {
            AnimContainer = new NewAnimationContainer();

            Name = name;
            binder = chrbnd; 
            List<BinderFile> flverFileEntries = new List<BinderFile>();

            if (tpfsUsed == null)
                tpfsUsed = new List<TPF>();

            if (additionalTpfNames != null)
            {
                foreach (var t in additionalTpfNames)
                {
                    if (File.Exists(t))
                        tpfsUsed.Add(TPF.Read(t));
                }
            }

            FLVER2 flver2 = null;
            FLVER0 flver0 = null;
            foreach (var f in chrbnd.Files)
            {
                if (TPF.Is(f.Bytes))
                {
                    var t = TPF.Read(f.Bytes);
                    if (modelToImportDuringLoad != null)
                    {
                        t.Textures.Clear();
                        foreach (var tx in modelToImportDuringLoad.Textures)
                        {
                            t.Textures.Add(tx);
                        }
                        f.Bytes = t.Write();
                    }
                    tpfsUsed.Add(t);
                }

                if ((f.ID % 10) != modelIndex && GameRoot.GameType != SoulsAssetPipeline.SoulsGames.DS2SOTFS)
                    continue;

                var nameCheck = f.Name.ToLower();
                if (GameRoot.GameType != SoulsGames.DES && flver2 == null && (nameCheck.EndsWith(".flver") || nameCheck.EndsWith(".flv") || FLVER2.Is(f.Bytes)))
                {
                    //if (nameCheck.EndsWith($"_{modelIndex}.flver") || modelIndex == 0)
                    flver2 = FLVER2.Read(f.Bytes);
                    flverName = f.Name;

                    if (modelToImportDuringLoad != null)
                    {
                        if (modelImportConfig.KeepOriginalDummyPoly)
                        {
                            modelToImportDuringLoad.Flver.Dummies.Clear();

                            List<string> existingDummyPolyParentBoneNames = new List<string>();
                            List<string> existingDummyPolyAttachBoneNames = new List<string>();
                            for (int i = 0; i < flver2.Dummies.Count; i++)
                            {
                                var dmy = flver2.Dummies[i];

                                // Clamp original value (presumably) how the game does.
                                if (dmy.ParentBoneIndex < 0 || dmy.ParentBoneIndex > flver2.Bones.Count)
                                    dmy.ParentBoneIndex = 0;

                                if (dmy.AttachBoneIndex < 0 || dmy.AttachBoneIndex > flver2.Bones.Count)
                                    dmy.AttachBoneIndex = 0;

                                // Remap bone indices.
                                dmy.ParentBoneIndex = (short)modelToImportDuringLoad.Flver.Bones.FindIndex(b => b.Name == flver2.Bones[dmy.ParentBoneIndex].Name);
                                dmy.AttachBoneIndex = (short)modelToImportDuringLoad.Flver.Bones.FindIndex(b => b.Name == flver2.Bones[dmy.AttachBoneIndex].Name);

                                // Hotfix
                                if (dmy.ParentBoneIndex == 1)
                                    dmy.ParentBoneIndex = 0;

                                modelToImportDuringLoad.Flver.Dummies.Add(dmy);
                            }

                            //var solarDmy = new FLVER.Dummy();

                            //var vanillaDmy = modelToImportDuringLoad.Flver.Dummies.FirstOrDefault(x => x.ReferenceID == 13);

                            //solarDmy.ReferenceID = 14;
                            //solarDmy.AttachBoneIndex = vanillaDmy.AttachBoneIndex;
                            //solarDmy.Forward = vanillaDmy.Forward;
                            //solarDmy.Upward = vanillaDmy.Upward;
                            //solarDmy.ParentBoneIndex = vanillaDmy.ParentBoneIndex;
                            //solarDmy.Unk30 = vanillaDmy.Unk30;
                            //solarDmy.Unk34 = vanillaDmy.Unk34;
                            //solarDmy.Position = vanillaDmy.Position;

                            //solarDmy.Position = new System.Numerics.Vector3(4.8f, solarDmy.Position.Y, -3f);

                            //solarDmy.UseUpwardVector = vanillaDmy.UseUpwardVector;
                            //solarDmy.Flag1 = vanillaDmy.Flag1;


                            //modelToImportDuringLoad.Flver.Dummies.RemoveAll(d => d.ReferenceID == 14);

                            // modelToImportDuringLoad.Flver.Dummies.Add(solarDmy);
                        }
                        flver2 = modelToImportDuringLoad.Flver;

                        f.Bytes = flver2.Write();
                    }
                }
                else if (GameRoot.GameType == SoulsGames.DES && flver0 == null && (nameCheck.EndsWith(".flver") || nameCheck.EndsWith(".flv") || FLVER0.Is(f.Bytes)))
                {
                    //if (nameCheck.EndsWith($"_{modelIndex}.flver") || modelIndex == 0)
                    flver0 = FLVER0.Read(f.Bytes);
                }

                else if (anibnd == null && nameCheck.EndsWith(".anibnd"))
                {
                    //if (nameCheck.EndsWith($"_{modelIndex}.anibnd") || modelIndex == 0)
                    if (BND3.Is(f.Bytes))
                    {
                        anibnd = BND3.Read(f.Bytes);
                    }
                    else
                    {
                        anibnd = BND4.Read(f.Bytes);
                    }
                }
                else if (nameCheck.EndsWith(".hkx") && !nameCheck.Contains("_c."))
                {
					IHavokObject container = HKX.Load(f.Bytes);
                    RagdollLevelContainer = container as hkRootLevelContainer;
                }
            }

            if (GameRoot.GameType == SoulsGames.DES)
            {
                if (flver0 == null)
                    return;

                LoadFLVER0(flver0, useSecondUV: false, baseDmyPolyID, ignoreStaticTransforms);
            }
            else
            {
                if (flver2 == null)
                    return;

                LoadFLVER2(flver2, name, useSecondUV: false, baseDmyPolyID, ignoreStaticTransforms);
                flver = flver2;
            }

           

            loadingProgress?.Report(1.0 / 4.0);

            if (anibnd != null)
            {
                
                    LoadingTaskMan.DoLoadingTaskSynchronous($"{Name}_ANIBND", $"Loading ANIBND for {Name}...", innerProg =>
                    {
                        try
                        {
                            AnimContainer.LoadBaseANIBND(anibnd, innerProg);
                            //SkeletonFlver.MapToSkeleton(AnimContainer.Skeleton, false);
                        }
                        catch (Exception ex)
                        {
                            //DialogManager.DialogOK(null, "Failed to load animations.");
                            ErrorLog.LogWarning($"Failed to load animations for model '{Name}'.");
                            //System.Windows.Forms.MessageBox.Show("Failed to load animations.", "Error",
                            //    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                        }
                    });
               
            }
            else
            {
                // This just messes up the model cuz they're already in 
                // reference pose, whoops
                //Skeleton.ApplyBakedFlverReferencePose();
            }

            loadingProgress?.Report(2.0 / 3.0);

            if (tpfsUsed.Count > 0)
            {
                LoadingTaskMan.DoLoadingTaskSynchronous($"{Name}_TPFs", $"Loading TPFs for {Name}...", innerProg =>
                {
                    for (int i = 0; i < tpfsUsed.Count; i++)
                    {
                        TexturePool.AddTpf(tpfsUsed[i]);
                        MainMesh.TextureReloadQueued = true;
                        innerProg.Report(1.0 * i / tpfsUsed.Count);
                    }
                    MainMesh.TextureReloadQueued = true;
                });

            }

            loadingProgress?.Report(3.0 / 4.0);

            if (texbnd != null)
            {
                LoadingTaskMan.DoLoadingTaskSynchronous($"{Name}_TEXBND", $"Loading TEXBND for {Name}...", innerProg =>
                {
                    TexturePool.AddTextureBnd(texbnd, innerProg);
                    MainMesh.TextureReloadQueued = true;
                });
            }

            loadingProgress?.Report(3.5 / 4.0);

            if (additionalTexbnd != null)
            {
                LoadingTaskMan.DoLoadingTaskSynchronous($"{Name}_AdditionalTEXBND", 
                    $"Loading extra TEXBND for {Name}...", innerProg =>
                {
                    TexturePool.AddTextureBnd(additionalTexbnd, innerProg);
                    MainMesh.TextureReloadQueued = true;
                });
            }

            loadingProgress?.Report(3.9 / 4.0);
            if (possibleLooseTpfFolder != null)
            {
                TexturePool.AddInterrootTPFFolder(possibleLooseTpfFolder);
                MainMesh.TextureReloadQueued = true;
            }

            MainMesh.TextureReloadQueued = true;

            AnimContainer.Update();
            UpdateSkeleton();

            if (this == Scene.MainModel)
                GFX.CurrentWorldView.RequestRecenter = true;

            Main.TAE_EDITOR.HardReset();
            
            loadingProgress?.Report(1.0);
        }

        public void CreateChrAsm()
        {
            ChrAsm = new NewChrAsm(this);
            ChrAsm.InitSkeleton(SkeletonFlver);

            MainMesh.Bounds = new BoundingBox(
                new Vector3(-0.5f, 0, -0.5f) * 1.75f, 
                new Vector3(0.5f, 1, 0.5f) * 1.75f);
        }

        private void LoadFLVER2(FLVER2 flver, string name, bool useSecondUV, int baseDmyPolyID = 0, bool ignoreStaticTransforms = false)
        {
            SkeletonFlver = new NewAnimSkeleton_FLVER(this, flver.Bones);
            MainMesh = new NewMesh(flver, name, useSecondUV, null, ignoreStaticTransforms);
            Bounds = MainMesh.Bounds;
            DummyPolyMan.AddAllDummiesFromFlver(flver);
        }

        private void LoadFLVER0(FLVER0 flver, bool useSecondUV, int baseDmyPolyID = 0, bool ignoreStaticTransforms = false)
        {
            SkeletonFlver = new NewAnimSkeleton_FLVER(this, flver.Bones);
            MainMesh = new NewMesh(flver, useSecondUV, null, ignoreStaticTransforms);
            Bounds = MainMesh.Bounds;
            DummyPolyMan.AddAllDummiesFromFlver(flver);
        }

        public Model(FLVER2 flver, string name, bool useSecondUV)
            : this()
        {
            AnimContainer = new NewAnimationContainer();
            LoadFLVER2(flver, name, useSecondUV);
        }

        public Model(FLVER0 flver, bool useSecondUV)
            : this()
        {
            AnimContainer = new NewAnimationContainer();
            LoadFLVER0(flver, useSecondUV);
        }


        public void AfterAnimUpdate(float timeDelta, bool ignorePosWrap = false)
        {
            if (IsRemoModel)
                CurrentTransform = new Transform(StartTransform.WorldMatrix * (AnimContainer?.Skeleton?.CurrentTransform.WorldMatrix ?? Matrix.Identity));

            ChrAsm?.Update(timeDelta);

            //UpdateSkeleton();

            DummyPolyMan.UpdateAllHitPrims();

            if (ChrAsm != null)
            {
                ChrAsm.RightWeaponModel0?.DummyPolyMan.UpdateAllHitPrims();
                ChrAsm.RightWeaponModel1?.DummyPolyMan.UpdateAllHitPrims();
                ChrAsm.RightWeaponModel2?.DummyPolyMan.UpdateAllHitPrims();
                ChrAsm.RightWeaponModel3?.DummyPolyMan.UpdateAllHitPrims();
                ChrAsm.LeftWeaponModel0?.DummyPolyMan.UpdateAllHitPrims();
                ChrAsm.LeftWeaponModel1?.DummyPolyMan.UpdateAllHitPrims();
                ChrAsm.LeftWeaponModel2?.DummyPolyMan.UpdateAllHitPrims();
                ChrAsm.LeftWeaponModel3?.DummyPolyMan.UpdateAllHitPrims();
            }

            if (Main.Config.CharacterTrackingTestIsIngameTime && timeDelta != 0)
            {
                UpdateTrackingTest(timeDelta);
            }
        }

        public void ScrubAnimRelative(float timeDelta, bool doNotScrubBackgroundLayers = false, bool updateSkeleton = true)
        {
            AnimContainer.ScrubRelative(timeDelta, doNotScrubBackgroundLayers);
            if (updateSkeleton)
            {
                if (SkeletonFlver != null)
                    UpdateSkeleton();
            }
        }

        public void UpdateAnimation()
        {
            if (!Main.Config.CharacterTrackingTestIsIngameTime)
            {
                UpdateTrackingTest(Main.DELTA_UPDATE);
            }

            if (AnimContainer.Skeleton.OriginalHavokSkeleton == null || AnimContainer == null)
                SkeletonFlver?.RevertToReferencePose();

            AnimContainer?.Update();

            if (AnimContainer?.ForcePlayAnim == true)
            {
                UpdateSkeleton();
            }

            
            //V2.0
            //if (AnimContainer.IsPlaying)
            //    AfterAnimUpdate();
        }

        public void TryToLoadTextures()
        {
            MainMesh?.TryToLoadTextures();
            ChrAsm?.TryToLoadTextures();
        }

        public void TryToLoadTexturesFromBinder(string path)
        {
            List<string> textures = MainMesh.GetAllTexNamesToLoad();
            TexturePool.AddSpecificTexturesFromBinder(path, textures);
        }

        public void DrawAllPrimitiveShapes()
        {
            DummyPolyMan.DrawAllHitPrims();

            DbgPrimDrawer.DrawPrimitives();

            if (ChrAsm != null)
            {
                ChrAsm.RightWeaponModel0?.DummyPolyMan.DrawAllHitPrims();
                ChrAsm.RightWeaponModel0?.DbgPrimDrawer.DrawPrimitives();
                ChrAsm.RightWeaponModel0?.SkeletonFlver?.DrawPrimitives();

                ChrAsm.RightWeaponModel1?.DummyPolyMan.DrawAllHitPrims();
                ChrAsm.RightWeaponModel1?.DbgPrimDrawer.DrawPrimitives();
                ChrAsm.RightWeaponModel1?.SkeletonFlver?.DrawPrimitives();

                ChrAsm.RightWeaponModel2?.DummyPolyMan.DrawAllHitPrims();
                ChrAsm.RightWeaponModel2?.DbgPrimDrawer.DrawPrimitives();
                ChrAsm.RightWeaponModel2?.SkeletonFlver?.DrawPrimitives();

                ChrAsm.RightWeaponModel3?.DummyPolyMan.DrawAllHitPrims();
                ChrAsm.RightWeaponModel3?.DbgPrimDrawer.DrawPrimitives();
                ChrAsm.RightWeaponModel3?.SkeletonFlver?.DrawPrimitives();

                ChrAsm.LeftWeaponModel0?.DummyPolyMan.DrawAllHitPrims();
                ChrAsm.LeftWeaponModel0?.DbgPrimDrawer.DrawPrimitives();
                ChrAsm.LeftWeaponModel0?.SkeletonFlver?.DrawPrimitives();

                ChrAsm.LeftWeaponModel1?.DummyPolyMan.DrawAllHitPrims();
                ChrAsm.LeftWeaponModel1?.DbgPrimDrawer.DrawPrimitives();
                ChrAsm.LeftWeaponModel1?.SkeletonFlver?.DrawPrimitives();

                ChrAsm.LeftWeaponModel2?.DummyPolyMan.DrawAllHitPrims();
                ChrAsm.LeftWeaponModel2?.DbgPrimDrawer.DrawPrimitives();
                ChrAsm.LeftWeaponModel2?.SkeletonFlver?.DrawPrimitives();

                ChrAsm.LeftWeaponModel3?.DummyPolyMan.DrawAllHitPrims();
                ChrAsm.LeftWeaponModel3?.DbgPrimDrawer.DrawPrimitives();
                ChrAsm.LeftWeaponModel3?.SkeletonFlver?.DrawPrimitives();
            }

            SkeletonFlver?.DrawPrimitives();

            if (DBG.GetCategoryEnableDraw(DebugPrimitives.DbgPrimCategory.HkxBone))
                AnimContainer?.Skeleton?.DrawPrimitives();

            DrawRagdoll(CurrentTransform.WorldMatrix);
        }

        public void DrawAllPrimitiveTexts()
        {
            DummyPolyMan.DrawAllHitPrimTexts();

            DbgPrimDrawer.DrawPrimitiveNames();

            if (ChrAsm != null)
            {
                ChrAsm.RightWeaponModel0?.DummyPolyMan.DrawAllHitPrimTexts();
                ChrAsm.RightWeaponModel1?.DummyPolyMan.DrawAllHitPrimTexts();
                ChrAsm.RightWeaponModel2?.DummyPolyMan.DrawAllHitPrimTexts();
                ChrAsm.RightWeaponModel3?.DummyPolyMan.DrawAllHitPrimTexts();

                ChrAsm.LeftWeaponModel0?.DummyPolyMan.DrawAllHitPrimTexts();
                ChrAsm.LeftWeaponModel1?.DummyPolyMan.DrawAllHitPrimTexts();
                ChrAsm.LeftWeaponModel2?.DummyPolyMan.DrawAllHitPrimTexts();
                ChrAsm.LeftWeaponModel3?.DummyPolyMan.DrawAllHitPrimTexts();
            }

            DrawRagdollTexts(CurrentTransform.WorldMatrix);
        }

        public void UpdateSkeleton()
        {
            if (IS_REMO_DUMMY)
            {
                var mainMat = SkeletonFlver.HavokSkeletonThisIsMappedTo.HkxSkeleton.FirstOrDefault(b => b.Name == Name)?.CurrentHavokTransform.GetMatrix().ToXna() ?? Matrix.Identity;
                foreach (var b in SkeletonFlver.FlverSkeleton)
                {
                    b.CurrentMatrix = mainMat;
                }
                StartTransform = CurrentTransform = new Transform(mainMat);
                return;
            }

            if (AnimContainer != null && AnimContainer.Skeleton != null && AnimContainer.Skeleton.HkxSkeleton.Count > 0 && !IsRemoModel)
            {
                if (SkeletonFlver.HavokSkeletonThisIsMappedTo == null)
                    SkeletonFlver.MapToSkeleton(AnimContainer.Skeleton, IsRemoModel);
            }

            SkeletonFlver.CopyFromHavokSkeleton();
        }

        
        public void DrawRemoPrims()
        {
            if (IS_REMO_DUMMY && RemoDummyTransformPrim != null)
            {
                RemoDummyTransformPrim.Transform = CurrentTransform;
                RemoDummyTransformPrim.Name = Name;
                RemoDummyTransformPrim.Draw(null, Matrix.Identity);
                //RemoDummyTransformTextPrint.Clear();
                //RemoDummyTransformTextPrint.AppendLine(Name);
                RemoDummyTransformTextPrint.Position3D = CurrentTransformPosition;
                RemoDummyTransformTextPrint.Draw();
            }
        }

        public void Draw(int lod = 0, bool motionBlur = false, bool forceNoBackfaceCulling = false, bool isSkyboxLol = false)
        {
            if (IS_REMO_DUMMY)
            {
                return;
            }

            GFX.FlverShader.Effect.Opacity = Opacity;

            GFX.CurrentWorldView.ApplyViewToShader(GFX.FlverShader, CurrentTransform);

            if (isSkyboxLol)
            {
                //((FlverShader)shader).Bones0 = new Matrix[] { Matrix.Identity };
                GFX.FlverShader.Effect.IsSkybox = true;
            }
            else if (SkeletonFlver != null)
            {
                

                if (ChrAsm != null)
                {
                    GFX.FlverShader.Effect.Bones0 = SkeletonFlver.ShaderMatrices0;

                    if (SkeletonFlver.FlverSkeleton.Count >= FlverShader.MaxBonePerMatrixArray)
                    {
                        GFX.FlverShader.Effect.Bones1 = SkeletonFlver.ShaderMatrices1;

                        if (SkeletonFlver.FlverSkeleton.Count >= FlverShader.MaxBonePerMatrixArray * 2)
                        {
                            GFX.FlverShader.Effect.Bones2 = SkeletonFlver.ShaderMatrices2;

                            if (SkeletonFlver.FlverSkeleton.Count >= FlverShader.MaxBonePerMatrixArray * 3)
                            {
                                GFX.FlverShader.Effect.Bones3 = SkeletonFlver.ShaderMatrices3;

                                if (SkeletonFlver.FlverSkeleton.Count >= FlverShader.MaxBonePerMatrixArray * 4)
                                {
                                    GFX.FlverShader.Effect.Bones4 = SkeletonFlver.ShaderMatrices4;

                                    if (SkeletonFlver.FlverSkeleton.Count >= FlverShader.MaxBonePerMatrixArray * 5)
                                    {
                                        GFX.FlverShader.Effect.Bones5 = SkeletonFlver.ShaderMatrices5;
                                    }
                                }
                            }
                        }
                    }
                }

                

                GFX.FlverShader.Effect.IsSkybox = false;
            }
            else
            {
                GFX.FlverShader.Effect.Bones0 = NewAnimSkeleton_FLVER.IDENTITY_MATRICES;
                GFX.FlverShader.Effect.Bones1 = NewAnimSkeleton_FLVER.IDENTITY_MATRICES;
                GFX.FlverShader.Effect.Bones2 = NewAnimSkeleton_FLVER.IDENTITY_MATRICES;
                GFX.FlverShader.Effect.Bones3 = NewAnimSkeleton_FLVER.IDENTITY_MATRICES;
                GFX.FlverShader.Effect.Bones4 = NewAnimSkeleton_FLVER.IDENTITY_MATRICES;
                GFX.FlverShader.Effect.Bones5 = NewAnimSkeleton_FLVER.IDENTITY_MATRICES;
            }

            GFX.FlverShader.Effect.EnableSkinning = EnableSkinning;

            GFX.FlverShader.Effect.DebugAnimWeight = DebugAnimWeight_Deprecated;

            if (IsVisible && MainMesh != null)
            {
                MainMesh.DrawMask = DrawMask;
                MainMesh.Draw(lod, motionBlur, forceNoBackfaceCulling, isSkyboxLol, this, SkeletonFlver, onDrawFail: (ex) =>
                {
                    //ImGuiDebugDrawer.DrawText3D($"{Name} failed to draw:\n\n{ex}", CurrentTransformPosition, Color.Red, Color.Black, 20);
                    if (!DialogManager.AnyDialogsShowing)
                    {
                        DialogManager.DialogOK("DRAW ERROR", $"{Name} failed to draw:\n\n{ex}");
                    }
                });
            }
            
            if (ChrAsm != null)
            {
                ChrAsm.Draw(DrawMask, lod, motionBlur, forceNoBackfaceCulling, isSkyboxLol);
            }
        }

        public void DrawRagdoll(Matrix WorldMatrix)
        {
            if (!DBG.GetCategoryEnableDraw(DebugPrimitives.DbgPrimCategory.Ragdoll))
				return;

			DrawRagdollBodies(WorldMatrix);
            DrawRagdollConstraints(WorldMatrix);
        }

        private static StatusPrinter RagdollTextDrawer = new StatusPrinter(null);

        public void DrawRagdollTexts(Matrix WorldMatrix)
        {
			if (!DBG.GetCategoryEnableNameDraw(DebugPrimitives.DbgPrimCategory.Ragdoll))
				return;

			List<IDbgPrim> primitives = new List<IDbgPrim>(RagdollBodies);
            primitives.AddRange(RagdollConstraints);
            for (int i = 0; i < primitives.Count; ++i)
            {
                var p = primitives[i];
                if (p == null)
                    continue;

                if (DBG.ShowPrimitiveNametags)
                {
					RagdollTextDrawer.Clear();

					RagdollTextDrawer.AppendLine(
						p.Name,
						i < RagdollBodies.Count ? Main.Colors.ColorHelperRagdoll : Main.Colors.ColorHelperRagdollConstraint);

					RagdollTextDrawer.Position3D =
						Vector3.Transform(Vector3.Zero,
						p.Transform.WorldMatrix
						* WorldMatrix);

					GFX.SpriteBatchBeginForText();
					RagdollTextDrawer.Draw();
					GFX.SpriteBatchEnd();
				}
            }
        }

        public void DrawRagdollBodies(Matrix WorldMatrix)
        {
            if (RagdollLevelContainer == null)
                return;

            hknpRagdollData ragdollData = GetHavokObject<hknpRagdollData>(RagdollLevelContainer);
            if (ragdollData == null)
                return;

			hkaSkeletonMapper skeletonMapper = GetHavokObject<hkaSkeletonMapper>(RagdollLevelContainer);
			if (skeletonMapper == null)
				return;

			var simpleMapping = skeletonMapper.m_mapping.m_simpleMappings;
            if (simpleMapping == null)
                return;

            if (skeletonMapper.m_mapping.m_chainMappings != null)
                skeletonMapper.m_mapping.m_chainMappings = skeletonMapper.m_mapping.m_chainMappings;

            var hkxSkeleton = SkeletonFlver.HavokSkeletonThisIsMappedTo.HkxSkeleton;

			hkaSkeleton ragdollSkeleton = ragdollData.m_skeleton;
            List<int> boneToBodyMap = ragdollData.m_boneToBodyMap;
            var bodyInfos = ragdollData.m_bodyCinfos;
            for (int i = 0; i < RagdollBodies.Count; ++i)
            {
                IDbgPrim prim = RagdollBodies[i];
                if (prim == null)
                    continue;
                
                int ragdollBoneIndex = boneToBodyMap.Find(e => e == i);
                if (ragdollBoneIndex < 0)
                    continue;

                int mapIndex = simpleMapping.FindIndex(e => e.m_boneB == ragdollBoneIndex);
                if (mapIndex < 0)
                    continue;

                int referenceBoneIndex = simpleMapping[mapIndex].m_boneA;

				string boneName = skeletonMapper.m_mapping.m_skeletonA.m_bones[referenceBoneIndex].m_name;
                int boneIndex = hkxSkeleton.FindIndex(e => e.Name == boneName);
				Matrix boneMatrix = hkxSkeleton[boneIndex].CurrentMatrix;
                Matrix poseMatrix = RagdollPoseMatrices[ragdollBoneIndex];

                hknpBodyCinfo body = bodyInfos[i];

                DbgPrimWireCapsule capsule = prim as DbgPrimWireCapsule;
                hknpCapsuleShape hkcapsule = body.m_shape as hknpCapsuleShape;
                if (capsule != null && hkcapsule != null)
                {
                    Matrix objectTransform = Matrix.CreateFromQuaternion(Microsoft.Xna.Framework.Quaternion.Normalize(new Microsoft.Xna.Framework.Quaternion(
                        body.m_orientation.X,
                        body.m_orientation.Y,
                        body.m_orientation.Z,
                        body.m_orientation.W)))
                    * Matrix.CreateTranslation(new Vector3(
                        body.m_position.X,
                        body.m_position.Y,
                        body.m_position.Z));

                    Matrix transform = objectTransform * poseMatrix * boneMatrix;

                    Vector4 a = hkcapsule.m_a;
                    Vector4 b = hkcapsule.m_b;
                    float radius = a.W;

                    Vector3 a3 = new Vector3(a.X, a.Y, a.Z);
                    a3 = Vector3.Transform(a3, transform);
                    Vector3 b3 = new Vector3(b.X, b.Y, b.Z);
                    b3 = Vector3.Transform(b3, transform);

                    capsule.UpdateCapsuleEndPoints(a3, b3, new ParamData.AtkParam.Hit() {DummyPolySourceSpawnedOn = ParamData.AtkParam.DummyPolySource.Body, Radius = radius}, null, null, null);

                    for (int index = 0; index < capsule.UnparentedChildren.Count; ++index)
                    {
                        capsule.UnparentedChildren[index].Transform = new Transform(Matrix.CreateScale(Vector3.One * 0.2f) * boneMatrix);
                    }
                }

                
                DbgPrimWireBox box = prim as DbgPrimWireBox;
                hknpBoxShape hkBox = body.m_shape as hknpBoxShape;
                if (box != null && hkBox != null)
                {
                    
                }

                prim.Draw(null, WorldMatrix);
            }
        }

        public void DrawRagdollConstraints(Matrix WorldMatrix)
        {
            if (RagdollLevelContainer == null)
                return;

            hknpRagdollData ragdollData = GetHavokObject<hknpRagdollData>(RagdollLevelContainer);
            if (ragdollData == null)
                return;

			hkaSkeletonMapper skeletonMapper = GetHavokObject<hkaSkeletonMapper>(RagdollLevelContainer);
			if (skeletonMapper == null)
				return;

			var simpleMapping = skeletonMapper.m_mapping.m_simpleMappings;
            if (simpleMapping == null)
                return;

            var hkxSkeleton = SkeletonFlver.HavokSkeletonThisIsMappedTo.HkxSkeleton;

			hkaSkeleton ragdollSkeleton = ragdollData.m_skeleton;
            List<int> boneToBodyMap = ragdollData.m_boneToBodyMap;
            var bodyInfos = ragdollData.m_bodyCinfos;
            List<hknpConstraintCinfo> constraintInfos = ragdollData.m_constraintCinfos;
            for (int i = 0; i < RagdollConstraints.Count; ++i)
            {
                IDbgPrim prim = RagdollConstraints[i];
                if (prim == null)
                    continue;

                hknpConstraintCinfo constraintInfo = constraintInfos[i];
                if (constraintInfo == null)
                    continue;

                int bodyAIndex = (int)(constraintInfo.m_bodyA.m_serialAndIndex & 0x00FFFFFF);
				int bodyBIndex = (int)(constraintInfo.m_bodyB.m_serialAndIndex & 0x00FFFFFF);

				int ragdollBoneAIndex = boneToBodyMap.Find(e => e == bodyAIndex);
                if (ragdollBoneAIndex < 0)
                    continue;

				int ragdollBoneBIndex = boneToBodyMap.Find(e => e == bodyBIndex);
				if(ragdollBoneBIndex < 0)
					continue;

				int mapAIndex = simpleMapping.FindIndex(e => e.m_boneB == ragdollBoneAIndex);
                if (mapAIndex < 0)
                    continue;

				int mapBIndex = simpleMapping.FindIndex(e => e.m_boneB == ragdollBoneBIndex);
				if (mapBIndex < 0)
					continue;

				int referenceBoneAIndex = simpleMapping[mapAIndex].m_boneA;
				int referenceBoneBIndex = simpleMapping[mapBIndex].m_boneA;

				string boneAName = skeletonMapper.m_mapping.m_skeletonA.m_bones[referenceBoneAIndex].m_name;
				string boneBName = skeletonMapper.m_mapping.m_skeletonA.m_bones[referenceBoneBIndex].m_name;

				int boneAIndex = hkxSkeleton.FindIndex(e => e.Name == boneAName);
                int boneBIndex = hkxSkeleton.FindIndex(e => e.Name == boneBName);

				Matrix boneAMatrix = hkxSkeleton[boneAIndex].CurrentMatrix;
                Matrix boneBMatrix = hkxSkeleton[boneBIndex].CurrentMatrix;

                Matrix poseAMatrix = RagdollPoseMatrices[ragdollBoneAIndex];
                Matrix poseBMatrix = RagdollPoseMatrices[ragdollBoneBIndex];

                hknpBodyCinfo bodyAInfo = bodyInfos[bodyAIndex];
				hknpBodyCinfo bodyBInfo = bodyInfos[bodyBIndex];

				Matrix objectATransform = Matrix.CreateFromQuaternion(Microsoft.Xna.Framework.Quaternion.Normalize(new Microsoft.Xna.Framework.Quaternion(
					bodyAInfo.m_orientation.X,
					bodyAInfo.m_orientation.Y,
					bodyAInfo.m_orientation.Z,
					bodyAInfo.m_orientation.W)))
				* Matrix.CreateTranslation(new Vector3(
					bodyAInfo.m_position.X,
					bodyAInfo.m_position.Y,
					bodyAInfo.m_position.Z));

				Matrix objectBTransform = Matrix.CreateFromQuaternion(Microsoft.Xna.Framework.Quaternion.Normalize(new Microsoft.Xna.Framework.Quaternion(
					bodyBInfo.m_orientation.X,
					bodyBInfo.m_orientation.Y,
					bodyBInfo.m_orientation.Z,
					bodyBInfo.m_orientation.W)))
				* Matrix.CreateTranslation(new Vector3(
					bodyBInfo.m_position.X,
					bodyBInfo.m_position.Y,
					bodyBInfo.m_position.Z));

				var hing = prim as DbgPrimWireHing;

                hkpSetLocalTransformsConstraintAtom transforms = null;
                hkpRagdollConstraintData ragdollConstraintData = constraintInfo.m_constraintData as hkpRagdollConstraintData;
                if (ragdollConstraintData != null)
                    transforms = ragdollConstraintData.m_atoms.m_transforms;

                hkpLimitedHingeConstraintData limitedHingeConstraintData = null;
                if (transforms == null)
                {
					limitedHingeConstraintData = constraintInfo.m_constraintData as hkpLimitedHingeConstraintData;
                    if (limitedHingeConstraintData != null)
                    {
                        transforms = limitedHingeConstraintData.m_atoms.m_transforms;
                    }
				}

				if (transforms != null)
                {
                    Matrix localTransformA = transforms.m_transformA;
                    Matrix localTransformB = transforms.m_transformB;
                    
                    Vector3 translateA = localTransformA.Translation;
					Vector3 translateB = localTransformB.Translation;

					Matrix transformA = objectATransform * poseAMatrix * boneAMatrix;
                    Matrix transformB = objectBTransform * poseBMatrix * boneBMatrix;

                    if (ragdollConstraintData != null)
                    {
						prim.Transform = new Transform(Matrix.CreateTranslation(translateB) * transformB);
					}

					if (limitedHingeConstraintData != null)
                    {
						prim.Transform = new Transform(Matrix.CreateTranslation(translateB) * transformB);

						for(int index = 0; index < prim.UnparentedChildren.Count; ++index)
						{
							IDbgPrim child = prim.UnparentedChildren[index];
							child.Transform = new Transform(Matrix.CreateTranslation(translateA) * transformA);
						}
					}

					//prim.Draw(null, WorldMatrix);
				}

                prim.Draw(null, WorldMatrix);
            }
        }

        public void Dispose()
        {
            DbgPrimDrawer?.Dispose();
            ChrAsm?.Dispose();
            SkeletonFlver = null;
            AnimContainer = null;
            MainMesh?.Dispose();
            // Do not need to dispose DummyPolyMan because it goes 
            // stores its primitives in the model's DbgPrimDrawer
        }

        void UpdateRagdollPose()
        {
            if (RagdollLevelContainer == null)
                return;

            hknpRagdollData ragdollData = GetHavokObject<hknpRagdollData>(RagdollLevelContainer);
            if (ragdollData == null)
                return;

            hkaSkeleton skeleton = ragdollData.m_skeleton;
            List<hkQsTransform> transforms = skeleton.m_referencePose;
            Matrix[] referenceMatrices = new Matrix[transforms.Count];
            for (int i = 0; i < referenceMatrices.Length; ++i)
            {
                hkQsTransform transform = transforms[i];
                referenceMatrices[i] = Matrix.CreateScale(new Vector3(
						transform.m_scale.X,
						transform.m_scale.Y,
						transform.m_scale.Z))
					* Matrix.CreateFromQuaternion(Microsoft.Xna.Framework.Quaternion.Normalize(new Microsoft.Xna.Framework.Quaternion(
						transform.m_rotation.X,
						transform.m_rotation.Y,
						transform.m_rotation.Z,
						transform.m_rotation.W)))
					* Matrix.CreateTranslation(new Vector3(
						transform.m_translation.X,
						transform.m_translation.Y,
						transform.m_translation.Z));
			}

            RagdollPoseMatrices = new Matrix[referenceMatrices.Length];
            for (int i = 0; i < referenceMatrices.Length; ++i)
            {
				RagdollPoseMatrices[i] = referenceMatrices[i];

				int parentIndex = skeleton.m_parentIndices[i];

				while (parentIndex >= 0)
				{
					RagdollPoseMatrices[i] = RagdollPoseMatrices[i] * referenceMatrices[parentIndex];
					parentIndex = skeleton.m_parentIndices[parentIndex];
				}
			}

            for (int i = 0; i < RagdollPoseMatrices.Length; ++i)
            {
                RagdollPoseMatrices[i] = Matrix.Invert(RagdollPoseMatrices[i]);
            }
		}

        void CreateRagdollPrimitives()
        {
            CreateRagdollBodies();
            CreateRagdollConstraints();
        }

        void CreateRagdollBodies()
        {
            RagdollBodies = new List<IDbgPrim>();
            if (RagdollLevelContainer == null)
                return;

            hknpRagdollData ragdollData = GetHavokObject<hknpRagdollData>(RagdollLevelContainer);
            if (ragdollData == null)
                return;

            var bodyInfos = ragdollData.m_bodyCinfos;
            for (int i = 0; i < bodyInfos.Count; ++i)
            {
                var bodyInfo = bodyInfos[i];
                IDbgPrim primitive = CreateRigbodyPrimtive(bodyInfo);
                RagdollBodies.Add(primitive);
            }
        }

        void CreateRagdollConstraints()
        {
			RagdollConstraints = new List<IDbgPrim>();
			if (RagdollLevelContainer == null)
				return;

			hknpRagdollData ragdollData = GetHavokObject<hknpRagdollData>(RagdollLevelContainer);
			if(ragdollData == null)
				return;

			var constraintInfos = ragdollData.m_constraintCinfos;
			for( int i = 0; i < constraintInfos.Count; ++i)
			{
				var constraintInfo = constraintInfos[i];
				IDbgPrim primitive = CreateConstraintPrimitive(constraintInfo, ragdollData);
				RagdollConstraints.Add(primitive);
			}
		}

		static T GetHavokObject<T>(hkRootLevelContainer container) where T : class
        {
            var element = container.m_namedVariants.Find(e => e.m_variant is T);
            return element.m_variant as T;
        }

        IDbgPrim CreateRigbodyPrimtive(hknpBodyCinfo bodyInfo)
        {
            IDbgPrim primitive = null;
            Color ragdollColor = Main.Colors.ColorHelperRagdoll;

            hknpShape shape = bodyInfo.m_shape;
            switch (shape.m_type)
            {
                case hknpShapeType.Enum.CAPSULE:
                    {
                        primitive = new DbgPrimWireCapsule(ragdollColor);
                    }
                    break;

                case hknpShapeType.Enum.BOX:
                    {
                        hknpBoxShape box = shape as hknpBoxShape;
                        System.Numerics.Matrix4x4 transform = box.m_obb;
                        System.Numerics.Matrix4x4.Decompose(transform, out System.Numerics.Vector3 scale, out System.Numerics.Quaternion rotation, out System.Numerics.Vector3 translation);
                        Transform t = new Transform(translation, rotation, scale);
                        primitive = new DbgPrimWireBox(t, - Vector3.One / 2, Vector3.One / 2, ragdollColor);
                    }
                    break;

                default:
                    break;
            }

            if (primitive != null)
            {
				primitive.Name = bodyInfo.m_name;
                primitive.NameColor = ragdollColor;
				primitive.OverrideColor = ragdollColor;
				primitive.Category = DbgPrimCategory.Ragdoll;

                //primitive.UnparentedChildren.Add(new DbgPrimWireFrame(new Transform(Vector3.Zero, Microsoft.Xna.Framework.Quaternion.Identity, Vector3.One * .2f)));
			}

			return primitive;
        }

        IDbgPrim CreateConstraintPrimitive(hknpConstraintCinfo constraintInfo, hknpRagdollData ragdollData)
        {
            IDbgPrim primitive = null;
            Color constraintColor = Main.Colors.ColorHelperRagdollConstraint;
            hkpConstraintData constraintData = constraintInfo.m_constraintData;
            if (constraintData is hkpRagdollConstraintData)
            {
				hkpRagdollConstraintData ragdollConstraintData = constraintData as hkpRagdollConstraintData;
				hkpSetLocalTransformsConstraintAtom transforms = ragdollConstraintData.m_atoms.m_transforms;
				hkpTwistLimitConstraintAtom twistLimit = ragdollConstraintData.m_atoms.m_twistLimit;
                hkpConeLimitConstraintAtom planeLimit = ragdollConstraintData.m_atoms.m_planesLimit;

				Vector3 twist = GetColumn(ref transforms.m_transformB, twistLimit.m_twistAxis);
                Vector3 normal = GetColumn(ref transforms.m_transformB, planeLimit.m_refAxisInB);
				primitive = new DbgPrimWireRagdoll(twist, twistLimit.m_minAngle, twistLimit.m_maxAngle, normal);
                DbgPrimWireCone cone = new DbgPrimWireCone(twist, ragdollConstraintData.m_atoms.m_coneLimit.m_maxAngle, Color.Yellow);
                primitive.Children.Add(cone);
				DbgPrimWireCone planeMin = new DbgPrimWireCone(-normal, MathHelper.PiOver2 - Math.Abs(planeLimit.m_minAngle), Color.Bisque);
				primitive.Children.Add(planeMin);
				DbgPrimWireCone planeMax = new DbgPrimWireCone(normal, MathHelper.PiOver2 - Math.Abs(planeLimit.m_maxAngle), Color.Tan);
				primitive.Children.Add(planeMax);
			}
			else if (constraintData is hkpLimitedHingeConstraintData)
            {
				hkpLimitedHingeConstraintData limitedHingeConstraintData = constraintData as hkpLimitedHingeConstraintData;
				hkpSetLocalTransformsConstraintAtom transforms = limitedHingeConstraintData.m_atoms.m_transforms;
                hkpAngLimitConstraintAtom limit = limitedHingeConstraintData.m_atoms.m_angLimit;

                Vector3 axis = GetColumn(ref transforms.m_transformB, 0);
				Vector3 prep = GetColumn(ref transforms.m_transformB, 2);
                Vector3 limitAxis = GetColumn(ref transforms.m_transformA, 2);
				primitive = new DbgPrimWireHing(axis, prep, limit.m_minAngle, limit.m_maxAngle);
                DbgPrimWire limitAxisPrimitive = new DbgPrimWire();
                limitAxisPrimitive.AddLine(Vector3.Zero, limitAxis * 0.1f, Color.White);
                primitive.UnparentedChildren.Add(limitAxisPrimitive);

            }
            else
            {
                primitive = null;
            }

            if (primitive != null)
            {
				int bodyAIndex = (int)(constraintInfo.m_bodyA.m_serialAndIndex & 0x00FFFFFF);
				int bodyBIndex = (int)(constraintInfo.m_bodyB.m_serialAndIndex & 0x00FFFFFF);

                string bodyAName = ragdollData.m_bodyCinfos[bodyAIndex].m_name;
				string bodyBName = ragdollData.m_bodyCinfos[bodyBIndex].m_name;

				primitive.Name = $"{constraintInfo.m_name}\n[{bodyBName}->{bodyAName}]{(constraintData as hkpRagdollConstraintData)?.m_atoms.m_coneLimit.m_maxAngle}";
                primitive.NameColor = constraintColor;
				primitive.Category = DbgPrimCategory.Ragdoll;
			}

            return primitive;
        }

        static Vector3 GetColumn(ref System.Numerics.Matrix4x4 m, int index)
        {
            if (index == 1)
            {
                return new Vector3(m.M21, m.M22, m.M23);
            }
			else if (index == 2)
			{
				return new Vector3(m.M31, m.M32, m.M33);
			}
            else
            {
                return new Vector3(m.M11, m.M12, m.M13);
            }
		}
    }
}
