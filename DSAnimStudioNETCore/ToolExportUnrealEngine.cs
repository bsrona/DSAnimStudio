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
using static DSAnimStudio.ToolExportUnrealEngine;
using static SoulsAssetPipeline.Animation.TAE.EventGroup;
using static SoulsAssetPipeline.Animation.TAE.Animation;
using System.Data;
using Behavior;
using Havoc.Objects;

namespace Behavior
{
	public class Variable
	{
		public string Name;
		public VariableType Type;
		public Role Role;
		public short RoleFlags;
	}

	public class VariableBound
	{
		public int Min;
		public int Max;
	}

	public class VariablesValues
	{
		public List<int> Values = new List<int>();
		public List<System.Numerics.Vector4> VectorValues = new List<System.Numerics.Vector4>();
		public List<hkReferencedObject> ObjectValues = new List<hkReferencedObject>();
	}

	public class Variables
	{
		public List<Variable> VariableInfos = new List<Variable>();
		public VariablesValues Values = new VariablesValues();
		public List<VariableBound> Bounds = new List<VariableBound>();

		public static Variables Create(List<string> names, List<hkbVariableInfo> infos, hkbVariableValueSet hkValues, List<hkbVariableBounds> hkBounds = null)
		{
			List<Variable> variables = new List<Variable>();

			if (infos != null)
			{
				for (int i = 0; i < infos.Count; ++i)
				{
					hkbVariableInfo characterPropertyInfo = infos[i];

					Variable variable = new Variable();

					variable.Name = names[i];
					variable.Type = characterPropertyInfo.m_type;
					variable.Role = characterPropertyInfo.m_role.m_role;
					variable.RoleFlags = characterPropertyInfo.m_role.m_flags;

					variables.Add(variable);
				}
			}

			VariablesValues values = new VariablesValues();
			if (hkValues != null)
			{
				List<int> baseValues = new List<int>();

				List<hkbVariableValue> hkbValues = hkValues.m_wordVariableValues;
				if (hkbValues != null)
				{
					for (int i = 0; i < hkbValues.Count; ++i)
					{
						hkbVariableValue hkValue = hkbValues[i];
						baseValues.Add(hkValue.m_value);
					}
				}
				values.Values = baseValues;
				values.VectorValues = hkValues.m_quadVariableValues?.ToList<System.Numerics.Vector4>();
				values.VectorValues ??= new List<System.Numerics.Vector4>();
				values.ObjectValues = hkValues.m_variantVariableValues ?? new List<hkReferencedObject>();
			}

			List<VariableBound> bounds = new List<VariableBound>();
			if (hkBounds != null)
			{
				for (int i = 0; i < bounds.Count; ++i)
				{
					hkbVariableBounds hkBound = hkBounds[i];

					VariableBound bound = new VariableBound();
					bound.Min = hkBound.m_min.m_value;
					bound.Max = hkBound.m_max.m_value;
					bounds.Add(bound);
				}
			}

			Variables result = new Variables();

			result.VariableInfos = variables;
			result.Values = values;
			result.Bounds = bounds;

			return result;
		}
	}

	public class Bind
	{
		public string MemberPath;
		public int VariableIndex;
		public sbyte BitIndex;
		public BindingType BindingType;
		public int OffsetInObjectPlusOne;
		public int OffsetInArrayPlusOne;
		public int RootVariableIndex;
		public sbyte VariableType;
		public sbyte Flags;

		public static Bind CreateBind(hkbVariableBindingSetBinding hkBind)
		{
			Bind bind = new Bind();

			bind.MemberPath = hkBind.m_memberPath;
			bind.VariableIndex = hkBind.m_variableIndex;
			bind.BitIndex = hkBind.m_bitIndex;
			bind.BindingType = hkBind.m_bindingType;
			bind.OffsetInObjectPlusOne = hkBind.m_offsetInObjectPlusOne;
			bind.OffsetInArrayPlusOne = hkBind.m_offsetInObjectPlusOne;
			bind.RootVariableIndex = hkBind.m_rootVariableIndex;
			bind.VariableType = hkBind.m_variableType;
			bind.Flags = hkBind.m_flags;

			if(bind.BitIndex > 0)
			{
				int fuck = 0;
				int fuck1 = fuck;
			}
			if(bind.OffsetInObjectPlusOne > 0)
			{
				int fuck = 0;
				int fuck1 = fuck;
			}
			if(bind.OffsetInArrayPlusOne > 0)
			{
				int fuck = 0;
				int fuck1 = fuck;
			}
			if(bind.RootVariableIndex > 0)
			{
				int fuck = 0;
				int fuck1 = fuck;
			}
			if(bind.Flags != 0)
			{
				int fuck = 0;
				int fuck1 = fuck;
			}
			if(bind.VariableType > 0)
			{
				int fuck = 0;
				int fuck1 = fuck;
			}
			return bind;
		}
	}

	public class Binds
	{
		public List<Bind> Bindings;
		public int IndexOfBindingToEnable;
		public bool HasOutputBinding;
		public bool InitializedOffsets;

		public static Binds CreateBinds(hkbVariableBindingSet hkBinds)
		{
			if (hkBinds == null)
				return null;

			List<Bind> bindings = new List<Bind>();

			List<hkbVariableBindingSetBinding> hkBindSets = hkBinds.m_bindings;
			for (int i = 0; i < hkBindSets.Count; i++)
			{
				hkbVariableBindingSetBinding hkBindSet = hkBindSets[i];
				Bind bind = Bind.CreateBind(hkBindSet);
				bindings.Add(bind);
			}

			Binds binds = new Binds();
			binds.Bindings = bindings;
			binds.IndexOfBindingToEnable = hkBinds.m_indexOfBindingToEnable;
			binds.HasOutputBinding = hkBinds.m_hasOutputBinding;
			binds.InitializedOffsets = hkBinds.m_initializedOffsets;

			if (binds.IndexOfBindingToEnable > 0)
			{
				int fuck = 0;
				int fuck1 = fuck;
			}
			if (binds.HasOutputBinding)
			{
				int fuck = 0;
				int fuck1 = fuck;
			}
			if (binds.InitializedOffsets)
			{
				int fuck = 0;
				int fuck1 = fuck;
			}
			return binds;
		}
	}

	public class Project
	{
		public List<string> CharacterFiles;
		public List<string> BehaviorFiles;
		public string SourceFile;
		public string ScriptPath;
		public System.Numerics.Vector4 Up;

		public static Project Create(hkbProjectData hkProjectData)
		{
			hkbProjectStringData hkStringData = hkProjectData.m_stringData;

			Project project = new Project();

			project.Up = hkProjectData.m_worldUpWS;
			project.CharacterFiles = hkStringData.m_characterFilenames;
			project.BehaviorFiles = hkStringData.m_behaviorFilenames;
			project.SourceFile = hkStringData.m_fullPathToSource;
			project.ScriptPath = hkStringData.m_scriptsPath;

			return project;
		}
	}

	public class Character
	{
		public Vector4 AxisUp;
		public Vector4 AxisForward;
		public Vector4 AxisRight;

		public float Scale;

		public string Rig;
		public string Ragdoll;
		public string Behavior;

		public List<string> LuaFiles;

		public Variables Properties;

		public static Character Create(hkbCharacterData hkCharacterData)
		{
			hkbCharacterStringData hkStringData = hkCharacterData.m_stringData;
			List<hkbVariableInfo> propertyInfos = hkCharacterData.m_characterPropertyInfos;
			hkbVariableValueSet propertyValues = hkCharacterData.m_characterPropertyValues;

			Variables variables = Variables.Create(hkStringData.m_characterPropertyNames, propertyInfos, propertyValues);

			Character character = new Character();

			character.AxisUp = hkCharacterData.m_modelUpMS;
			character.AxisForward = hkCharacterData.m_modelForwardMS;
			character.AxisRight = hkCharacterData.m_modelRightMS;

			character.Scale = hkCharacterData.m_scale;

			character.Rig = hkStringData.m_rigName;
			character.Ragdoll = hkStringData.m_ragdollName;
			character.Behavior = hkStringData.m_behaviorFilename;

			character.LuaFiles = hkStringData.m_luaFiles;

			character.Properties = variables;

			return character;
		}
	}

	public class Bindable
	{
		public string _ClassName;
		public Binds Binds;

		public static Bindable Create(hkbBindable hkBindable)
		{
			Bindable bindable = null;

			Constructors.TryGetValue(hkBindable.GetType(), out ConstructorInfo Constructor);
			if (Constructor != null)
			{
				bindable = Constructor?.Invoke(null) as Bindable;
				bindable.From(hkBindable);
			}

			if (bindable == null)
				bindable = null;

			return bindable;
		}

		public static T Create<T>(hkbBindable hkBindable) where T : Bindable
		{
			return Create(hkBindable) as T;
		}

		public static List<T> Creates<HKT, T>(List<HKT> hkBindables) where T : Bindable where HKT : hkbBindable
		{
			List<T> bindables = new List<T>();

			for (int i = 0; i < hkBindables.Count; ++i)
			{
				HKT hkBindable = hkBindables[i];
				T bindable = Create(hkBindable) as T;
				bindables.Add(bindable);
			}

			return bindables;
		}

		public virtual void From(hkbBindable hkBindable)
		{
			_ClassName = "Beh" + GetType().Name;
			Binds = Binds.CreateBinds(hkBindable.m_variableBindingSet);
		}

		static Bindable()
		{
			Type thisType = typeof(Bindable);
			Type[] subTypes = thisType.Assembly.GetTypes().Where(t => t.IsSubclassOf(thisType)).ToArray();
			for (int i = 0; i < subTypes.Length; ++i)
			{
				Type subType = subTypes[i];
				System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(subType.TypeHandle);
			}
		}

		protected static void Register<HKT, T>()
		{
			Constructors.Add(typeof(HKT), typeof(T).GetConstructor(Arguments));
		}

		static Dictionary<Type, ConstructorInfo> Constructors = new Dictionary<Type, ConstructorInfo>();
		static Type[] Arguments = new Type[0];
	}

	public class Node : Bindable
	{
		public string Name;

		public static Node Create(hkbNode hkNode)
		{
			var result = Bindable.Create(hkNode) as Node;
			return result;
		}

		public override void From(hkbBindable hkBindable)
		{
			base.From(hkBindable);

			var hkNode = hkBindable as hkbNode;

			Name = hkNode.m_name;
		}
	}

	public class Generator : Node
	{
		public static Generator Create(hkbGenerator hkNode)
		{
			var result = Node.Create(hkNode) as Generator;
			return result;
		}
	}

	public class Modifier : Node
	{
		public bool Enable;

		public static Modifier Create(hkbModifier hkNode)
		{
			var result = Node.Create(hkNode) as Modifier;
			return result;
		}

		public override void From(hkbBindable hkBindable)
		{
			base.From(hkBindable);

			hkbModifier hkModifier = hkBindable as hkbModifier;

			Enable = hkModifier.m_enable;
		}
	}

	public class Event
	{
		public string Name;
		public uint Flags;
	}

	public class Graph : Generator
	{
		public List<string> Animations;
		public Variables Variables;
		public Variables CharacterProperties;
		public List<Event> Events;
		public Generator Generator;

		static Graph()
		{
			Register<hkbBehaviorGraph, Graph>();
		}

		public override void From(hkbBindable hkBindable)
		{
			base.From(hkBindable);

			hkbBehaviorGraph hkBehaviorGraph = hkBindable as hkbBehaviorGraph;

			hkbBehaviorGraphData graphData = hkBehaviorGraph.m_data;
			hkbBehaviorGraphStringData stringData = graphData.m_stringData;
			List<hkbVariableInfo> variableInfos = graphData.m_variableInfos;
			hkbVariableValueSet variableValues = graphData.m_variableInitialValues;
			List<hkbVariableBounds> virableValueBounds = graphData.m_variableBounds;
			List<hkbEventInfo> eventInfos = graphData.m_eventInfos;

			List<string> animations = stringData.m_animationNames ?? new List<string>();

			Variables variables = Variables.Create(stringData.m_variableNames, variableInfos, variableValues, virableValueBounds);
			Variables properties = Variables.Create(stringData.m_characterPropertyNames, graphData.m_characterPropertyInfos, null);

			List<Event> events = new List<Event>();
			if (eventInfos != null)
			{
				for (int i = 0; i < eventInfos.Count; ++i)
				{
					hkbEventInfo hkEventInfo = eventInfos[i];
					Event behaviorEvent = new Event();
					behaviorEvent.Name = stringData.m_eventNames[i];
					behaviorEvent.Flags = hkEventInfo.m_flags;
					events.Add(behaviorEvent);
				}
			}

			Generator generator = Create(hkBehaviorGraph.m_rootGenerator);

			Animations = animations;
			Variables = variables;
			CharacterProperties = properties;
			Events = events;
			Generator = generator;
		}
	}

	public class StateMachine : Generator
	{
		public class State : Bindable
		{
			public string Name;
			public int StateId;
			public Generator Generator;
			public TransitionList Transitions;

			static State()
			{
				Register<hkbStateMachineStateInfo, State>();
			}

			public override void From(hkbBindable hkBindable)
			{
				base.From(hkBindable);

				var hkState = hkBindable as hkbStateMachineStateInfo;

				TransitionList transitions = new TransitionList();
				transitions.From(hkState.m_transitions);

				Name = hkState.m_name;
				StateId = hkState.m_stateId;
				Generator = Generator.Create(hkState.m_generator);
				Transitions = transitions;
			}
		}

		public class Transition
		{
			//public hkbStateMachineTimeInterval m_triggerInterval;
			//public hkbStateMachineTimeInterval m_initiateInterval;
			//public hkbTransitionEffect m_transition;
			//public hkbCondition m_condition;
			public int EventId;
			public int ToStateId;
			public int FromNestedStateId;
			public int ToNestedStateId;
			public short Priority;
			public short Flags;

			public void From(hkbStateMachineTransitionInfo hkTransition)
			{
				EventId = hkTransition.m_eventId;
				ToStateId = hkTransition.m_toStateId;
				FromNestedStateId = hkTransition.m_fromNestedStateId;
				ToNestedStateId = hkTransition.m_toNestedStateId;
				Priority = hkTransition.m_priority;
				Flags = hkTransition.m_flags;

				if ((hkTransition.m_flags & (int)TransitionFlags.FLAG_FROM_NESTED_STATE_ID_IS_VALID) != 0
					|| (hkTransition.m_flags & (int)TransitionFlags.FLAG_TO_NESTED_STATE_ID_IS_VALID) != 0)
				{
					Flags = hkTransition.m_flags;
				}

				if (hkTransition.m_condition != null)
				{
					Flags = hkTransition.m_flags;
				}
			}
		}

		public class TransitionList
		{
			//public hkbStateMachineTimeInterval m_triggerInterval;
			//public hkbStateMachineTimeInterval m_initiateInterval;
			//public hkbTransitionEffect m_transition;
			//public hkbCondition m_condition;
			public List<Transition> Transitions = new List<Transition>();

			public bool HasEventlessTransitions;
			public bool HasTimeBoundedTransitions;

			public void From(hkbStateMachineTransitionInfoArray hkTransitionList)
			{
				if (hkTransitionList == null)
					return;

				List<Transition> transitions = new List<Transition>();

				List<hkbStateMachineTransitionInfo> hkTransitions = hkTransitionList.m_transitions;
				for (int i = 0; i < hkTransitions.Count; ++i)
				{
					hkbStateMachineTransitionInfo hkTransition = hkTransitions[i];
					Transition transition = new Transition();
					transition.From(hkTransition);
					transitions.Add(transition);
				}

				Transitions = transitions;
				HasEventlessTransitions = hkTransitionList.m_hasEventlessTransitions;
				HasTimeBoundedTransitions = hkTransitionList.m_hasTimeBoundedTransitions;
			}
		}

		public List<State> States = new List<State>();
		public TransitionList WildcardTransitions = new TransitionList();

		public StartStateMode StartStateMode;
		public int StartStateID;

		static StateMachine()
		{
			Register<hkbStateMachine, StateMachine>();
		}

		public override void From(hkbBindable hkBindable)
		{
			base.From(hkBindable);

			hkbStateMachine hkStateMachine = hkBindable as hkbStateMachine;

			List<State> states = Creates<hkbStateMachineStateInfo, State>(hkStateMachine.m_states);
			TransitionList transitions = new TransitionList();
			transitions.From(hkStateMachine.m_wildcardTransitions);

			States = states;
			WildcardTransitions = transitions;

			StartStateMode = hkStateMachine.m_startStateMode;
			StartStateID = hkStateMachine.m_startStateId;
		}
	}

	public class ReferenceGenerator : Generator
	{
		public string BehaviorName;

		static ReferenceGenerator()
		{
			Register<hkbBehaviorReferenceGenerator, ReferenceGenerator>();
		}

		public override void From(hkbBindable hkBindable)
		{
			base.From(hkBindable);

			var hkGenerator = hkBindable as hkbBehaviorReferenceGenerator;

			BehaviorName = hkGenerator.m_behaviorName;
		}
	}

	public class LayerGenerator : Generator
	{
		public class Layer : Bindable
		{
			public Generator Generator;

			static Layer()
			{
				Register<hkbLayer, Layer>();
			}

			public override void From(hkbBindable hkBindable)
			{
				base.From(hkBindable);

				var hkLayer = hkBindable as hkbLayer;

				Generator = Generator.Create(hkLayer.m_generator);
			}
		}

		public List<Layer> Layers;
		public short IndexOfSyncMasterChild;
		public ushort Flags;
		public int NumActiveLayers;
		public bool InitSync;

		static LayerGenerator()
		{
			Register<hkbLayerGenerator, LayerGenerator>();
		}

		public override void From(hkbBindable hkBindable)
		{
			base.From(hkBindable);

			var hkGenerator = hkBindable as hkbLayerGenerator;

			List<Layer> layers = Creates<hkbLayer, Layer>(hkGenerator.m_layers);

			Layers = layers;
			IndexOfSyncMasterChild = hkGenerator.m_indexOfSyncMasterChild;
			Flags = hkGenerator.m_flags;
			NumActiveLayers = hkGenerator.m_numActiveLayers;
			InitSync = hkGenerator.m_initSync;
		}
	}

	public class BlenderGenerator : Generator
	{
		public class Child : Bindable
		{
			public Generator Generator;
			public float Weight;

			static Child()
			{
				Register<hkbBlenderGeneratorChild, Child>();
			}

			public override void From(hkbBindable hkBindable)
			{
				base.From(hkBindable);

				var hkChild = hkBindable as hkbBlenderGeneratorChild;

				Generator = Generator.Create(hkChild.m_generator);
				Weight = hkChild.m_weight;
			}
		}

		public List<Child> Children;
		public float ReferencePoseWeightThreshold;
		public float BlendParameter;

		static BlenderGenerator()
		{
			Register<hkbBlenderGenerator, BlenderGenerator>();
		}

		public override void From(hkbBindable hkBindable)
		{
			base.From(hkBindable);

			var hkGenerator = hkBindable as hkbBlenderGenerator;

			if (hkGenerator.m_flags != 0)
			{
				int flags = hkGenerator.m_flags;
				int flags1 = flags;
			}

			if (hkGenerator.m_doSubtractiveBlend)
			{
				bool subtractive = hkGenerator.m_doSubtractiveBlend;
				subtractive = hkGenerator.m_subtractLastChild;
			}

			if (hkGenerator.m_subtractLastChild)
			{
				bool subtractive = hkGenerator.m_doSubtractiveBlend;
				subtractive = hkGenerator.m_subtractLastChild;
			}

			List<Child> children = Creates<hkbBlenderGeneratorChild, Child>(hkGenerator.m_children);

			Children = children;
			ReferencePoseWeightThreshold = hkGenerator.m_referencePoseWeightThreshold;
			BlendParameter = hkGenerator.m_blendParameter;
		}
	}

	public class ModifierGenerator : Generator
	{
		public Modifier Modifier;
		public Generator Generator;

		static ModifierGenerator()
		{
			Register<hkbModifierGenerator, ModifierGenerator>();
		}

		public override void From(hkbBindable hkBindable)
		{
			base.From(hkBindable);

			var hkGenerator = hkBindable as hkbModifierGenerator;

			Modifier = Modifier.Create(hkGenerator.m_modifier);
			Generator = Create(hkGenerator.m_generator);
		}
	}

	public class ScriptGenerator : Generator
	{
		public Generator Generator;

		public string OnActivateScript;
		public string OnPreUpdateScript;
		public string OnGenerateScript;
		public string OnHandleEventScript;
		public string OnDeactivateScript;


		static ScriptGenerator()
		{
			Register<hkbScriptGenerator, ScriptGenerator>();
		}

		public override void From(hkbBindable hkBindable)
		{
			base.From(hkBindable);

			var hkGenerator = hkBindable as hkbScriptGenerator;

			Generator = Create(hkGenerator.m_child);

			OnActivateScript = hkGenerator.m_onActivateScript;
			OnPreUpdateScript = hkGenerator.m_onPreUpdateScript;
			OnGenerateScript = hkGenerator.m_onGenerateScript;
			OnHandleEventScript = hkGenerator.m_onHandleEventScript;
			OnDeactivateScript = hkGenerator.m_onDeactivateScript;
		}
	}

	public class ManualSelectorGenerator : Generator
	{
		public List<Generator> Generators;

		static ManualSelectorGenerator()
		{
			Register<hkbManualSelectorGenerator, ManualSelectorGenerator>();
		}

		public override void From(hkbBindable hkBindable)
		{
			base.From(hkBindable);

			var hkGenerator = hkBindable as hkbManualSelectorGenerator;

			List<Generator> generators = Creates<hkbGenerator, Generator>(hkGenerator.m_generators);

			Generators = generators;
		}
	}

	public class CustomManualSelectorGenerator : Generator
	{
		public List<Generator> Generators;

		static CustomManualSelectorGenerator()
		{
			Register<HKX2.CustomManualSelectorGenerator, CustomManualSelectorGenerator>();
		}

		public override void From(hkbBindable hkBindable)
		{
			base.From(hkBindable);

			var hkGenerator = hkBindable as HKX2.CustomManualSelectorGenerator;

			List<Generator> generators = Creates<hkbGenerator, Generator>(hkGenerator.m_generators);

			Generators = generators;
		}
	}

	public class ClipGenerator : Generator
	{
		public string AnimationName;

		static ClipGenerator()
		{
			Register<hkbClipGenerator, ClipGenerator>();
		}

		public override void From(hkbBindable hkBindable)
		{
			base.From(hkBindable);

			var hkGenerator = hkBindable as hkbClipGenerator;

			AnimationName = hkGenerator.m_animationName;
		}
	}

	public class ModifierList : Modifier
	{
		public List<Modifier> Modifiers;

		static ModifierList()
		{
			Register<hkbModifierList, ModifierList>();
		}

		public override void From(hkbBindable hkBindable)
		{
			base.From(hkBindable);

			var hkObject = hkBindable as hkbModifierList;

			List<Modifier> modifiers = Creates<hkbModifier, Modifier>(hkObject.m_modifiers);

			Modifiers = modifiers;
		}
	}

	public class FootIkControlsModifier : Modifier
	{
		static FootIkControlsModifier()
		{
			Register<hkbFootIkControlsModifier, FootIkControlsModifier>();
		}

		public override void From(hkbBindable hkBindable)
		{
			base.From(hkBindable);

			var hkObject = hkBindable as hkbFootIkControlsModifier;
		}
	}

	public class GetHandleOnBoneModifier : Modifier
	{
		static GetHandleOnBoneModifier()
		{
			Register<hkbGetHandleOnBoneModifier, GetHandleOnBoneModifier>();
		}

		public override void From(hkbBindable hkBindable)
		{
			base.From(hkBindable);

			var hkObject = hkBindable as hkbGetHandleOnBoneModifier;
		}
	}

	public class HandIkControlsModifier : Modifier
	{
		static HandIkControlsModifier()
		{
			Register<hkbHandIkControlsModifier, HandIkControlsModifier>();
		}

		public override void From(hkbBindable hkBindable)
		{
			base.From(hkBindable);

			var hkObject = hkBindable as hkbHandIkControlsModifier;
		}
	}

	public class KeyframeBonesModifier : Modifier
	{
		static KeyframeBonesModifier()
		{
			Register<hkbKeyframeBonesModifier, HandIkControlsModifier>();
		}

		public override void From(hkbBindable hkBindable)
		{
			base.From(hkBindable);

			var hkObject = hkBindable as hkbKeyframeBonesModifier;
		}
	}

	public class CustomLookAtTwistModifier : Modifier
	{
		static CustomLookAtTwistModifier()
		{
			Register<HKX2.CustomLookAtTwistModifier, CustomLookAtTwistModifier>();
		}

		public override void From(hkbBindable hkBindable)
		{
			base.From(hkBindable);

			var hkObject = hkBindable as HKX2.CustomLookAtTwistModifier;
		}
	}
}

namespace DSAnimStudio
{
    public class ToolExportUnrealEngine
    {
        public enum ExportFileType
        {
			All,
            SkeletalMesh_Fbx,
			PhysicsAsset_Json,
			Materials_Json,
			Mtds_Json,
			Textures,
			AnimationSkeleton_Fbx,
            AnimationSequence_Fbx,
			AnimationSequences_Fbx,
			Taes_Json,
			Behavior_Json,
		}

		HavokSplineFixer splineFixer = null;
        public void InitForAnimContainer(NewAnimationContainer animContainer)
        {
            if (splineFixer == null)
                splineFixer = new HavokSplineFixer(animContainer.Skeleton);

            
        }

		public static readonly Vector3D UnitScale = new Vector3D(100, 100, 100); // Unit from FromSoftware Meter to UnrealEngine Centimeter and mirror
		public static readonly Vector3D Mirror = new Vector3D(1, 1, -1);

		public struct Part
		{
			public FLVER2 flver;
			public string flverPath;
			public NewAnimationContainer aniContainer;
			public TaeFileContainer taeContainer;
			public hkRootLevelContainer ragdollContainer;
			public Matrix[] ragdollPoseMatrices;
			public IBinder behbnd;
		}

		public struct PartFile
		{
			public string Mesh;
			public string Skeleton;
			public string PhysicsAsset;
			public List<string> Animations;
			public List<string> AdditiveAnimations;
			public List<string> Taes;
			public List<string> Behaviors;
			public List<string> Materials;
			public List<string> Mtds;
			public List<string> Textures;
		}

		public enum CombineMode
		{
			/** Uses the average value of the materials touching: (a+b)/2 */
			Average = 0,
			/** Uses the minimum value of the materials touching: min(a,b) */
			Min = 1,
			/** Uses the product of the values of the materials touching: a*b */
			Multiply = 2,
			/** Uses the maximum value of materials touching: max(a,b) */
			Max = 3
		}

		public class PhysicsMaterial
		{
			public string Name;
			//public uint m_isExclusive;
			//public int m_flags;
			//public TriggerType m_triggerType;
			//public hkUFloat8 m_triggerManifoldTolerance;
			public float DynamicFriction;
			public float StaticFriction;
			public float Restitution;
			public CombineMode FrictionCombineMode;
			public CombineMode RestitutionCombineMode;
			//public float m_weldingTolerance;
			//public float m_maxContactImpulse;
			//public float m_fractionOfClippedImpulseToApply;
			//public MassChangerCategory m_massChangerCategory;
			//public hknpHalf m_massChangerHeavyObjectFactor;
			//public hknpHalf m_softContactForceFactor;
			//public hknpHalf m_softContactDampFactor;
			//public hkUFloat8 m_softContactSeparationVelocity;
			//public hknpSurfaceVelocity m_surfaceVelocity;
			//public hknpHalf m_disablingCollisionsBetweenCvxCvxDynamicObjectsDistance;
			//public ulong m_userData;
			//public bool m_isShared;
		}

		public class Shape
		{
			public hknpShapeType.Enum Type;
		}

		public class Capsule : Shape
		{
			public float Radius;
			public float Length;
			public System.Numerics.Vector3 Center;
			public System.Numerics.Quaternion Rotation;
		}

		public struct Body
		{
			public string Name;
			public string BoneName;
			public hknpMotionType.Enum MotionType;

			public float LinearDamping;
			public float AngularDamping;

			public float Mass;
			public float Volume;
			public System.Numerics.Vector4 CenterOfMass;
			public System.Numerics.Vector4 IntertiaTensor;
			public System.Numerics.Quaternion MajorAxisSpace;

			public int MaterialIndex;

			public Shape Shape;
		}

		public enum EAngularConstraintMotion
		{
			/** No constraint against this axis. */
			Free,
			/** Limited freedom along this axis. */
			Limited,
			/** Fully constraint against this axis. */
			Locked,

			Count,
		};

		public struct Constraint
		{
			public string Name;

			public string BoneAName;
			public string BoneBName;

			public int BodyAIndex;
			public int BodyBIndex;

			public System.Numerics.Vector3 Pos1;
			public System.Numerics.Vector3 Pos2;

			public System.Numerics.Vector3 PriAxis1;
			public System.Numerics.Vector3 PriAxis2;

			public System.Numerics.Vector3 SecAxis1;
			public System.Numerics.Vector3 SecAxis2;

			public float Swing1LimitDegrees;
			public float Swing2LimitDegrees;
			public float TwistLimitDegrees;

			public EAngularConstraintMotion Swing1Motion;
			public EAngularConstraintMotion Swing2Motion;
			public EAngularConstraintMotion TwistMotion;
		}

		public struct Ragdoll
		{
			public List<PhysicsMaterial> Materials;
			public List<Body> Bodies;
			public List<Constraint> Constraints;
		}

		public struct Event
		{
			public float StartTime;
			public float EndTime;
			public int Type;
			public IReadOnlyDictionary<string, object> Parameters;
		}

		public struct EventGroup
		{
			public long GroupType;
			public EventGroupDataStruct GroupData;
			public List<int> Events;
		}

		public struct Action
		{
			public long ID;
			public string Name;
			public List<Event> Events;
			public List<EventGroup> EventGroups;
			public AnimMiniHeader MiniHeader;
			public string FileName;
			public string Reference;
			public string AnimationFile;
		}

		public struct Tae
		{
			public int ID;
			public byte[] Flags;
			public string SkeletonName;
			public string SibName;
			public List<Action> Actions;
			public long EventBank;
		}

		public void Export(ExportFileType fileType, string path, string filename, out bool userRequestCancel)
		{
			bool requestCancel = false;
			try
			{
				if (fileType == ExportFileType.All)
				{
					ExportAll(path);
				}
				else if (fileType == ExportFileType.SkeletalMesh_Fbx)
				{
					ExportSkeletalMeshes(path);
				}
				else if (fileType == ExportFileType.PhysicsAsset_Json)
				{
					ExportPhysicsAssets(path);
				}
				else if (fileType == ExportFileType.Materials_Json)
				{
					ExportMaterials(path);
				}
				else if (fileType == ExportFileType.Mtds_Json)
				{
					ExportMtds(path);
				}
				else if (fileType == ExportFileType.Textures)
				{
					ExportTextures(path);
				}
				else if (fileType == ExportFileType.AnimationSkeleton_Fbx)
				{
					ExportSkeletons(path);
				}
				else if (fileType == ExportFileType.AnimationSequence_Fbx)
				{
					NewAnimationContainer animContainer = Scene.MainModel.AnimContainer;
					ExportAnimation(animContainer, animContainer.CurrentAnimationName, Path.ChangeExtension($"{path}\\{animContainer.CurrentAnimationName}", "fbx"));
				}
				else if (fileType == ExportFileType.AnimationSequences_Fbx)
				{
					ExportAnimations(path);
				}
				else if (fileType == ExportFileType.Taes_Json)
				{
					ExportTaes(path);
				}
				else if (fileType == ExportFileType.Behavior_Json)
				{
					ExportBehaviors(path);
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
			List<Part> parts = GetParts();

			for (int i = 0; i < parts.Count; ++i)
			{
				Part part = parts[i];

				PartFile partFile = new PartFile();

				partFile.Textures = ExportTextures(path, part);
				partFile.Mtds = ExportMtds(path, part);
				partFile.Materials = ExportMaterials(path, part);
				partFile.Skeleton = ExportSkeleton(path, part);
				partFile.PhysicsAsset = ExportPhysicsAsset(path, part);
				partFile.Animations = ExportAnimations(path, part, false);
				partFile.AdditiveAnimations = ExportAnimations(path, part, true);
				partFile.Taes = ExportTaes(path, part);
				partFile.Behaviors = ExportBehaviors(path, part);
				partFile.Mesh = ExportSkeletalMesh(path, part);

				var json = Newtonsoft.Json.JsonConvert.SerializeObject(partFile, Newtonsoft.Json.Formatting.Indented);
				string relativePath = Path.ChangeExtension(partFile.Mesh, "dsc");
				WriteTextFile(json, path + relativePath);
			}
		}

		public void ExportSkeletalMeshes(string path)
		{
			List<Part> parts = GetParts();

			for (int i = 0; i < parts.Count; ++i)
			{
				Part part = parts[i];

				ExportSkeletalMesh(path, part);
			}
		}

		public void ExportPhysicsAssets(string path)
		{
			List<Part> parts = GetParts();

			for (int i = 0; i < parts.Count; ++i)
			{
				Part part = parts[i];

				ExportPhysicsAsset(path, part);
			}
		}

		public void ExportTaes(string path)
		{
			List<Part> parts = GetParts();

			for (int i = 0; i < parts.Count; ++i)
			{
				Part part = parts[i];

				ExportTaes(path, part);
			}
		}

		public void ExportBehaviors(string path)
		{
			List<Part> parts = GetParts();

			for (int i = 0; i < parts.Count; ++i)
			{
				Part part = parts[i];

				ExportBehaviors(path, part);
			}
		}

		public void ExportAnimations(string path)
		{
			List<Part> parts = GetParts();

			for (int i = 0; i < parts.Count; ++i)
			{
				Part part = parts[i];

				ExportAnimations(path, part, false);
				ExportAnimations(path, part, true);
			}
		}

		public void ExportSkeletons(string path)
		{
			List<Part> parts = GetParts();

			for (int i = 0; i < parts.Count; ++i)
			{
				Part part = parts[i];

				ExportSkeleton(path, part);
			}
		}

		public void ExportMaterials(string path)
		{
			List<Part> parts = GetParts();

			for (int i = 0; i < parts.Count; ++i)
			{
				Part part = parts[i];
				ExportMaterials(path, part);
			}
		}

		public void ExportMtds(string path)
		{
			List<Part> parts = GetParts();

			for (int i = 0; i < parts.Count; ++i)
			{
				Part part = parts[i];
				ExportMtds(path, part);
			}
		}

		public void ExportTextures(string path)
		{
			List<Part> parts = GetParts();

			for (int i = 0; i < parts.Count; ++i)
			{
				Part part = parts[i];
				ExportTextures(path, part);
			}
		}

		public string ExportSkeletalMesh(string path, Part part)
		{
			FLVER2 flver = part.flver;
			string flverPath = part.flverPath;

			string originPath = flverPath;

			string relativePath = ToRelativePath(originPath);
			relativePath = Path.ChangeExtension(relativePath, "fbx");

			string relativeToRoot = RelativeToRoot(relativePath);

			string fullPath = path + relativePath;

			CreateDirectory(fullPath);

			using (var context = new AssimpContext())
			{
				Assimp.Scene scene = CreateScene(flver, relativeToRoot);
				if (context.ExportFile(scene, fullPath, ExportFormatID))
					return relativePath;
				else
					return null;
			}
		}

		public string ExportPhysicsAsset(string path, Part part)
		{
			FLVER2 flver = part.flver;
			string flverPath = part.flverPath;

			string originPath = flverPath;

			string relativePath = ToRelativePath(originPath);
			relativePath = Path.ChangeExtension(relativePath, "rag");

			string fullPath = path + relativePath;

			CreateDirectory(fullPath);

			List<PhysicsMaterial> physicsMaterials = ExportPhysicsMaterials(part);
			List<Body> bodies = ExportBodies(part);
			List<Constraint> constraints = ExportConstraints(part);

			physicsMaterials = RemapPhysicsMaterials(physicsMaterials, bodies);

			Ragdoll ragdoll = new Ragdoll();
			ragdoll.Materials = physicsMaterials;
			ragdoll.Bodies = bodies;
			ragdoll.Constraints = constraints;

			var json = Newtonsoft.Json.JsonConvert.SerializeObject(ragdoll, Newtonsoft.Json.Formatting.Indented);
			WriteTextFile(json, fullPath);

			return relativePath;
		}

		public List<string> ExportTaes(string path, Part part)
		{
			List<string> taePaths = new List<string>();

			TaeFileContainer container = part.taeContainer;
			if (container == null)
				return taePaths;

			IBinder binder = container.containerANIBND;
			IReadOnlyDictionary<string, TAE> taes = container.AllTAEDict;
			List<string> taeOriginalPath = taes.Keys.ToList();

			NewAnimationContainer animationContainer = part.aniContainer;
			var mainChrSolver = new TaeAnimRefChainSolver(taes, animationContainer.Animations);

			foreach (var taePair in taes)
			{
				string originalPath = taePair.Key;
				string relativePath = ToRelativePath(originalPath);
				string fullPath = path + relativePath;

				List<Action> actions = new List<Action>();

				TAE tae = taePair.Value;
				List<TAE.Animation> taeAnimations = tae.Animations;

				for (int i = 0; i < taeAnimations.Count; ++i)
				{
					TAE.Animation taeAnimation = taeAnimations[i];

					string reference = null;

					if (taeAnimation.MiniHeader is TAE.Animation.AnimMiniHeader.ImportOtherAnim asImportOtherAnim)
					{
						if (mainChrSolver.GetCompositeAnimIDOfAnimInTAE(tae, taeAnimation) == asImportOtherAnim.ImportFromAnimID)
						{
							reference = relativePath;
						}
						else
						{
							var id = mainChrSolver.GetSplitAnimID(asImportOtherAnim.ImportFromAnimID);
							string referenceOrignalPath = taeOriginalPath.Find(e => e.ToUpper().EndsWith($"{id.Upper:D2}.TAE"));
							if (referenceOrignalPath != null)
								reference = ToRelativePath(referenceOrignalPath);
						}
					}

					string name = mainChrSolver.GetHKXNameIgnoreReferences(tae, taeAnimation);
					name = Path.GetFileNameWithoutExtension(name);

					string animationFile = null;

					string animationName = mainChrSolver.GetHKXName(tae, taeAnimation);
					if (!string.IsNullOrEmpty(animationName))
					{
						string aniOriginalPath = binder.Files.Find(e => e.Name.Contains(animationName))?.Name;
						if (string.IsNullOrEmpty(aniOriginalPath))
							aniOriginalPath = animationContainer.GetAnimationPath(animationName);
						if (!string.IsNullOrEmpty(aniOriginalPath))
						{
							animationFile = ToRelativePath(aniOriginalPath);
							animationFile = Path.ChangeExtension(animationFile, "fbx");
						}
					}

					List<Event> events = new List<Event>();

					List<TAE.Event> taeEvents = taeAnimation.Events;
					for (int j = 0; j < taeEvents.Count; ++j)
					{
						TAE.Event taeEvent = taeEvents[j];

						Event e = new Event();

						e.StartTime = taeEvent.StartTime;
						e.EndTime = taeEvent.EndTime;
						e.Type = taeEvent.Type;
						e.Parameters = taeEvent.Parameters?.Values;

						events.Add(e);
					}

					List<EventGroup> eventGroups = new List<EventGroup>();

					List<TAE.EventGroup> taeEventGroups = taeAnimation.EventGroups;
					for (int j = 0; j < taeEventGroups.Count; ++j)
					{
						TAE.EventGroup taeEventGroup = taeEventGroups[j];

						EventGroup e = new EventGroup();

						e.GroupType = taeEventGroup.GroupType;
						e.GroupData = taeEventGroup.GroupData;
						e.Events = taeEventGroup.indices;

						eventGroups.Add(e);
					}

					Action action = new Action();

					action.ID = taeAnimation.ID;
					action.Name = name;
					action.MiniHeader = taeAnimation.MiniHeader;
					action.FileName = taeAnimation.AnimFileName;
					action.Reference = reference;
					action.AnimationFile = animationFile;
					action.Events = events;
					action.EventGroups = eventGroups;

					actions.Add(action);
				}

				Tae aTae = new Tae();

				aTae.ID = tae.ID;
				aTae.Flags = tae.Flags;
				aTae.SkeletonName = tae.SkeletonName;
				aTae.SibName = tae.SibName;
				aTae.EventBank = tae.EventBank;
				aTae.Actions = actions;

				var json = Newtonsoft.Json.JsonConvert.SerializeObject(aTae, Newtonsoft.Json.Formatting.Indented);
				WriteTextFile(json, fullPath);

				taePaths.Add(relativePath);
			}

			return taePaths;
		}

		public List<string> ExportBehaviors(string path, Part part)
		{
			List<string> behaviorPaths = new List<string>();

			IBinder behbnd = part.behbnd;
			if (behbnd == null)
				return behaviorPaths;

			for (int i = 0; i < behbnd.Files.Count; ++i)
			{
				BinderFile benFile = behbnd.Files[i];
				string originalPath = benFile.Name;
				string relativePath = ToRelativePath(originalPath);
				string fullPath = path + relativePath;

				hkRootLevelContainer container = HKX.Load(benFile.Bytes) as hkRootLevelContainer;
				if (container == null)
					continue;

				fullPath = ExportBehavior(fullPath, container);

				if (fullPath == null)
					continue;

				behaviorPaths.Add(fullPath);
			}

			return behaviorPaths;
		}

		public string ExportSkeleton(string path, Part part)
		{
			NewAnimationContainer animationContainer = part.aniContainer;
			IBinder anibnd = animationContainer.baseANIBND;
			if (anibnd == null)
				return null;

			string originPath = anibnd.Files.Find(e => e.Name.ToLower().Contains("skeleton"))?.Name;
			string relativePath = ToRelativePath(originPath);
			relativePath = Path.ChangeExtension(relativePath, "fbx");
			string fullpath = path + relativePath;
			CreateDirectory(fullpath);

			using (var context = new AssimpContext())
			{
				Assimp.Scene scene = CreateScene(animationContainer);
				if (context.ExportFile(scene, fullpath, ExportFormatID))
					return relativePath;
				else
					return null;
			}
		}

		public List<string> ExportAnimations(string path, Part part, bool isAdditive)
		{
			List<string> animations = new List<string>();

			NewAnimationContainer animationContainer = part.aniContainer;
			if (animationContainer == null)
				return animations;

			List<IBinder> binders = new List<IBinder>();

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
					relativePath = Path.ChangeExtension(relativePath, "fbx");
					string fullPath = path + relativePath;

					try
					{
						NewHavokAnimation hkAnimation = animationContainer.FindAnimation(filename);
						if (hkAnimation == null)
							continue;

						if (isAdditive != hkAnimation.IsAdditiveBlend)
							continue;

						if (ExportAnimation(animationContainer, filename, fullPath))
							animations.Add(relativePath);
					}
					catch (Exception ex)
					{
						ErrorLog.LogWarning($"Unable to export {fullPath}. exception: {ex.Message}");
					}
				}
			}

			return animations;
		}

		public List<string> ExportMaterials(string path, Part part)
		{
			FLVER2 flver = part.flver;
			string flverPath = part.flverPath;
			string originPath = flverPath;
			string relativePath = ToRelativePath(originPath);
			string relativeDirectory = Path.GetDirectoryName(relativePath);

			List<string> flverMaterialRelativePaths = new List<string>();

			List<FLVER2.Material> flverMaterials = flver.Materials;

			for (int i = 0; i < flverMaterials.Count; ++i)
			{
				FLVER2.Material flverMaterial = flverMaterials[i];

				string name = GetIndexName(flverMaterial.Name, i);
				string flverMaterialRelativePath = $"{relativeDirectory}\\mat\\{name}.mat";
				flverMaterialRelativePaths.Add(flverMaterialRelativePath);

				string jsonFlverMaterialPath = path + flverMaterialRelativePath;
				ExportFlverMaterial(flverMaterial, name, jsonFlverMaterialPath);
			}

			return flverMaterialRelativePaths;
		}

		public List<string> ExportMtds(string path, Part part)
		{
			List<string> mtdPaths = new List<string>();

			FLVER2 flver = part.flver;

			List<FLVER2.Material> flverMaterials = flver.Materials;
			List<FLVER2.Mesh> flverMeshes = flver.Meshes;

			for (int i = 0; i < flverMaterials.Count; ++i)
			{
				FLVER2.Material flverMaterial = flverMaterials[i];

				FlverMaterialDefInfo flverMaterialDefInfo = FlverMaterialDefInfo.Lookup(flverMaterial.MTD);
				if (flverMaterialDefInfo == null)
					continue;

				FLVER2.Mesh mesh = flverMeshes.Find(e => e.MaterialIndex == i);
				FLVER2.FaceSet faceSet = mesh.FaceSets.First();
				bool isTwoSide = faceSet == null ? false : !faceSet.CullBackfaces;

				string mtdPath = ToRelativePath(flverMaterial.MTD);
				string jsonMtdPath = path + mtdPath;

				ExportMTD(flverMaterialDefInfo, jsonMtdPath, isTwoSide);
				mtdPaths.Add(mtdPath);
			}

			return mtdPaths;
		}

		public List<string> ExportTextures(string path, Part part)
		{
			List<string> texturePaths = new List<string>();

			FLVER2 flver = part.flver;
			List<string> filePaths = GetReferenceTexturePaths(flver);
			for (int i = 0; i < filePaths.Count; ++i)
			{
				string filePath = filePaths[i];
				string relativePath = ToRelativePath(filePath);
				string exportPath = path + relativePath;
				if (!ExportTexture(exportPath))
					continue;

				texturePaths.Add(relativePath);
			}

			return texturePaths;
		}

		public string ExportBehavior(string path, hkRootLevelContainer container)
		{
			hkbProjectData hkProjectData = GetHavokObject<hkbProjectData>(container);
			hkbCharacterData hkCharacterData = GetHavokObject<hkbCharacterData>(container);
			hkbBehaviorGraph hkBehaviorGraph = GetHavokObject<hkbBehaviorGraph>(container);

			string extension = null;
			object behaviorObject = null;

			if (hkProjectData != null)
			{
				extension = "hkp";

				behaviorObject = CreateBehaviorProject(hkProjectData);
			}
			else if (hkCharacterData != null)
			{
				extension = "hkc";

				behaviorObject = CreateBehaviorCharacter(hkCharacterData);
			}
			else if (hkBehaviorGraph != null)
			{
				extension = "hkb";

				behaviorObject = CreateBehaviorGraph(hkBehaviorGraph);
			}

			if (behaviorObject == null)
				return null;

			var json = Newtonsoft.Json.JsonConvert.SerializeObject(behaviorObject, Newtonsoft.Json.Formatting.Indented);

			path = Path.ChangeExtension(path, extension);

			WriteTextFile(json, path);

			return path;
		}

		public bool ExportAnimation(NewAnimationContainer animContainer, string name, string path)
		{
			CreateDirectory(path);
			return ExportAnimationFBX(animContainer, name, path);
		}

		public void ExportFlverMaterial(FLVER2.Material material, string name, string path)
		{
			List<FLVER2.Texture> textures = new List<FLVER2.Texture>(material.Textures.Count);
			for (int i = 0; i < material.Textures.Count; ++i)
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

		public void ExportMTD(FlverMaterialDefInfo flverMaterialDefInfo, string path, bool isTwoSide)
		{
			MTD material = flverMaterialDefInfo.mtd;
			if (material == null)
				return;

			var samplerConfigs = flverMaterialDefInfo.SamplerConfigs;

			List<MTD.Texture> textures = new List<MTD.Texture>(material.Textures.Count);
			for (int i = 0; i < material.Textures.Count; ++i)
			{
				MTD.Texture texture = material.Textures[i];
				String texturePath = texture.Path;

				if (string.IsNullOrEmpty(texturePath))
				{
					if (samplerConfigs.ContainsKey(texture.Type))
					{
						var samplerConfig = samplerConfigs[texture.Type];
						texturePath = samplerConfig.DefaultTexPath;
					}
				}

				MTD.Texture jsonTexture = new MTD.Texture();

				jsonTexture.Type = texture.Type;
				jsonTexture.Extended = texture.Extended;
				jsonTexture.UVNumber = texture.UVNumber;
				jsonTexture.ShaderDataIndex = texture.ShaderDataIndex;
				jsonTexture.Path = ToRelativePath(ToTexturePath(texturePath));
				jsonTexture.UnkFloats = texture.UnkFloats;

				textures.Add(jsonTexture);
			}

			MTD jsonMaterial = new MTD();

			jsonMaterial.ShaderPath = ToRelativePath(material.ShaderPath);
			jsonMaterial.Description = material.Description;
			jsonMaterial.Params = material.Params;
			jsonMaterial.Textures = textures;

			if (isTwoSide)
			{
				List<MTD.Param> newParams = new List<MTD.Param>(material.Params.Count);
				for (int i = 0; i < material.Params.Count; ++i)
				{
					MTD.Param param = material.Params[i];
					MTD.Param newParam = new MTD.Param(param.Name, param.Type, param.Value);
					newParams.Add(newParam);
				}
				jsonMaterial.Params = newParams;
				jsonMaterial.Params.Add(new MTD.Param("g_bTwoSide", MTD.ParamType.Bool, true));
			}

			var json = Newtonsoft.Json.JsonConvert.SerializeObject(jsonMaterial, Newtonsoft.Json.Formatting.Indented);

			WriteTextFile(json, path);
		}

		public bool ExportTexture(string path)
		{
			var shortName = Utils.GetShortIngameFileName(path).ToLower();
			if (!TexturePool.Fetches.ContainsKey(shortName))
				return false;

			TextureFetchRequest request = TexturePool.Fetches[shortName];
			byte[] ddsBytes = request?.TexInfo?.DDSBytes;
			if (ddsBytes == null)
				return false;

			return ExportTexture(ddsBytes, path);
		}

		public bool ExportAnimationFBX(NewAnimationContainer animContainer, string name, string path)
		{
			using (var context = new AssimpContext())
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
				for (int frameIndex = 0; frameIndex < rootMotionFrames.Length; ++frameIndex)
				{
					var frame = rootMotionFrames[frameIndex];

					rootMotion += "\t";
					rootMotion += frame.ToString();
				}

				writer.WriteLine(rootMotion);

				for (int hkxBoneIndex = 0; hkxBoneIndex < hkxBoneInfos.Count; ++hkxBoneIndex)
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

		bool ExportTexture(byte[] ddsBytes, string path)
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
						return true;
					}
					catch (Exception e)
					{
						return false;
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
			Assimp.Node root = new Assimp.Node("RootNode");

			List<Assimp.Node> boneNodes = CreateBoneNodes(flver, root);
			List<Material> materials = CreateMaterials(flver, relativeToRoot);
			List<Mesh> meshes = CreateMeshes(flver, boneNodes);

			if (meshes.Count > 0)
			{
				for (int i = 0; i < meshes.Count; ++i)
				{
					Mesh mesh = meshes[i];
					FLVER2.Mesh flverMesh = flver.Meshes[i];

					Assimp.Node parentNode = boneNodes[flverMesh.DefaultBoneIndex];
					Assimp.Node meshNode = new Assimp.Node(mesh.Name);

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
			Assimp.Node root = new Assimp.Node("RootNode");

			HKX.HKASkeleton skeleton = animContainer.Skeleton.OriginalHavokSkeleton;
			List<Assimp.Node> boneNodes = CreateBoneNodes(skeleton, root);

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

		List<Assimp.Node> CreateBoneNodes(FLVER2 flver, Assimp.Node root)
		{
			List<FLVER.Bone> bones = flver.Bones;

			List<Assimp.Node> nodes = new List<Assimp.Node>(bones.Count);
			for (int i = 0; i < bones.Count; ++i)
			{
				FLVER.Bone bone = bones[i];
				Assimp.Node node = CreateBoneNode(bone);
				nodes.Add(node);
			}

			for (int i = 0; i < bones.Count; ++i)
			{
				FLVER.Bone bone = bones[i];
				Assimp.Node parent = root;

				int parentIndex = bone.ParentIndex;
				if (parentIndex >= 0)
					parent = nodes[parentIndex];

				Assimp.Node node = nodes[i];
				node.Parent = parent;
				if (parent != null)
					parent.Children.Add(node);
			}

			return nodes;
		}

		List<Assimp.Node> CreateBoneNodes(HKASkeleton skeleton, Assimp.Node root)
		{
			HKArray<HKX.Bone> bones = skeleton.Bones;
			HKArray<HKX.Transform> transforms = skeleton.Transforms;

			int count = (int)bones.Size;
			List<Assimp.Node> nodes = new List<Assimp.Node>(count);
			for (int i = 0; i < count; ++i)
			{
				HKX.Bone bone = bones[i];
				HKX.Transform transform = transforms[i];
				Assimp.Node node = CreateBoneNode(bone, transform);
				nodes.Add(node);
			}

			HKArray<HKShort> parentIndices = skeleton.ParentIndices;
			for (int i = 0; i < count; ++i)
			{
				Assimp.Node parent = root;

				int parentIndex = parentIndices[i].data;
				if (parentIndex >= 0)
					parent = nodes[parentIndex];

				Assimp.Node node = nodes[i];
				node.Parent = parent;

				if (parent != null)
					parent.Children.Add(node);
			}

			return nodes;
		}

		Assimp.Node CreateBoneNode(FLVER.Bone bone)
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


			Assimp.Node node = CreateNode(bone.Name, transform);

			return node;
		}

		Assimp.Node CreateBoneNode(HKX.Bone bone, HKX.Transform t)
		{
			System.Numerics.Vector3 scale = new System.Numerics.Vector3(t.Scale.Vector.X, t.Scale.Vector.Y, t.Scale.Vector.Z);
			System.Numerics.Quaternion rotation = new System.Numerics.Quaternion(t.Rotation.Vector.X, t.Rotation.Vector.Y, t.Rotation.Vector.Z, t.Rotation.Vector.W);
			System.Numerics.Vector3 translation = new System.Numerics.Vector3(t.Position.Vector.X, t.Position.Vector.Y, t.Position.Vector.Z);

			rotation = new System.Numerics.Quaternion(-Mirror.X * rotation.X, -Mirror.Y * rotation.Y, -Mirror.Z * rotation.Z, rotation.W);
			translation = System.Numerics.Vector3.Multiply(translation, To(UnitScale * Mirror));

			System.Numerics.Matrix4x4 transform = System.Numerics.Matrix4x4.CreateScale(scale)
				* System.Numerics.Matrix4x4.CreateFromQuaternion(rotation)
				* System.Numerics.Matrix4x4.CreateTranslation(translation);

			Assimp.Node node = CreateNode(bone.Name.GetString(), transform);

			return node;
		}

		Assimp.Node CreateNode(string name, System.Numerics.Matrix4x4 transform)
		{
			Assimp.Node node = new Assimp.Node();

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

		List<Assimp.Bone> CreateBones(List<Assimp.Node> nodes)
		{
			List<Assimp.Bone> bones = new List<Assimp.Bone>(nodes.Count);

			for (int i = 0; i < nodes.Count; ++i)
			{
				Assimp.Node node = nodes[i];

				Assimp.Bone bone = new Assimp.Bone();
				bone.Name = node.Name;
				bone.Node = node;

				bones.Add(bone);
			}

			return bones;
		}


		List<Mesh> CreateMeshes(FLVER2 flver, List<Assimp.Node> boneNodes)
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
				if (havokAnimationData.IsAdditiveBlend)
				{
					var skeleTransform = havokAnimationData.hkaSkeleton.Transforms[hkxBoneIndex];

					NewBlendableTransform referencePos = NewBlendableTransform.FromHKXTransform(skeleTransform);

					System.Numerics.Matrix4x4 transform;
					if (havokAnimationData.BlendHint == HKX.AnimationBlendHint.ADDITIVE_DEPRECATED)
						transform = referencePos.GetMatrixScale() * referencePos.GetMatrix() * frame.GetMatrixScale() * frame.GetMatrix();
					else
						transform = frame.GetMatrixScale() * frame.GetMatrix() * referencePos.GetMatrixScale() * referencePos.GetMatrix();

					System.Numerics.Matrix4x4.Decompose(transform, out frame.Scale, out frame.Rotation, out frame.Translation);
				}

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
			Assimp.Node master = scene.RootNode.FindNode("Master");
			if (master == null)
				return;

			List<Assimp.Node> boneNodes = new List<Assimp.Node>();
			Queue<Assimp.Node> queue = new Queue<Assimp.Node>();
			queue.Enqueue(master);
			while (queue.Count > 0)
			{
				Assimp.Node node = queue.Dequeue();
				for (int i = 0; i < node.ChildCount; ++i)
				{
					Assimp.Node child = node.Children[i];
					queue.Enqueue(child);
				}
				boneNodes.Add(node);
			}

			AddDummySkin(scene, boneNodes);
		}

		void AddDummySkin(Assimp.Scene scene, List<Assimp.Node> boneNodes)
		{
			Assimp.Node node = new Assimp.Node("DummySkin");
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

		Mesh CreateDummySkin(List<Assimp.Node> boneNodes)
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

		List<PhysicsMaterial> ExportPhysicsMaterials(Part part)
		{
			List<PhysicsMaterial> physicsMaterials = new List<PhysicsMaterial>();

			hkRootLevelContainer ragdollLevelContainer = part.ragdollContainer;
			if (ragdollLevelContainer == null)
				return physicsMaterials;

			hknpRagdollData ragdollData = GetHavokObject<hknpRagdollData>(ragdollLevelContainer);
			var materials = ragdollData.m_materials;

			for (int i = 0; i < materials.Count; ++i)
			{
				hknpMaterial hkMapterial = materials[i];

				PhysicsMaterial physicsMateiral = CreatePhysicsMaterial(hkMapterial);

				physicsMaterials.Add(physicsMateiral);
			}

			return physicsMaterials;
		}

		List<Body> ExportBodies(Part part)
		{
			List<Body> bodies = new List<Body>();

			hkRootLevelContainer ragdollLevelContainer = part.ragdollContainer;
			if (ragdollLevelContainer == null)
				return bodies;

			hknpRagdollData ragdollData = GetHavokObject<hknpRagdollData>(ragdollLevelContainer);
			hkaSkeletonMapper skeletonMapper = GetHavokObject<hkaSkeletonMapper>(ragdollLevelContainer);
			var simpleMapping = skeletonMapper.m_mapping.m_simpleMappings;

			hkaSkeleton ragdollSkeleton = ragdollData.m_skeleton;
			List<hknpMotionProperties> motionProperties = ragdollData.m_motionProperties;
			List<int> boneToBodyMap = ragdollData.m_boneToBodyMap;
			var bodyInfos = ragdollData.m_bodyCinfos;

			for (int i = 0; i < bodyInfos.Count; ++i)
			{
				hknpBodyCinfo bodyInfo = bodyInfos[i];
				if (bodyInfo == null)
					continue;

				int ragdollBoneIndex = boneToBodyMap.Find(e => e == i);
				if (ragdollBoneIndex < 0)
					continue;

				int mapIndex = simpleMapping.FindIndex(e => e.m_boneB == ragdollBoneIndex);
				if (mapIndex < 0)
					continue;

				int referenceBoneIndex = simpleMapping[mapIndex].m_boneA;

				string boneName = skeletonMapper.m_mapping.m_skeletonA.m_bones[referenceBoneIndex].m_name;

				Matrix poseMatrix = part.ragdollPoseMatrices[ragdollBoneIndex];

				hknpMotionProperties motionProperty = motionProperties[bodyInfo.m_motionPropertiesId];
				Body body = CreateBody(bodyInfo, motionProperty, boneName, poseMatrix);

				bodies.Add(body);
			}

			return bodies;
		}

		List<Constraint> ExportConstraints(Part part)
		{
			List<Constraint> constraints = new List<Constraint>();
			hkRootLevelContainer ragdollLevelContainer = part.ragdollContainer;
			if (ragdollLevelContainer == null)
				return constraints;

			Matrix[] poseMatrices = part.ragdollPoseMatrices;

			hknpRagdollData ragdollData = GetHavokObject<hknpRagdollData>(ragdollLevelContainer);
			hkaSkeletonMapper skeletonMapper = GetHavokObject<hkaSkeletonMapper>(ragdollLevelContainer);
			var simpleMapping = skeletonMapper.m_mapping.m_simpleMappings;

			hkaSkeleton ragdollSkeleton = ragdollData.m_skeleton;
			List<int> boneToBodyMap = ragdollData.m_boneToBodyMap;

			var bodyInfos = ragdollData.m_bodyCinfos;
			var constraintInfos = ragdollData.m_constraintCinfos;

			for (int i = 0; i < constraintInfos.Count; ++i)
			{
				hknpConstraintCinfo constraintInfo = constraintInfos[i];
				if (constraintInfo == null)
					continue;

				int bodyAIndex = (int)(constraintInfo.m_bodyA.m_serialAndIndex & 0x00FFFFFF);
				int bodyBIndex = (int)(constraintInfo.m_bodyB.m_serialAndIndex & 0x00FFFFFF);

				int ragdollBoneAIndex = boneToBodyMap.Find(e => e == bodyAIndex);
				if (ragdollBoneAIndex < 0)
					continue;

				int ragdollBoneBIndex = boneToBodyMap.Find(e => e == bodyBIndex);
				if (ragdollBoneBIndex < 0)
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

				Matrix poseAMatrix = poseMatrices[ragdollBoneAIndex];
				Matrix poseBMatrix = poseMatrices[ragdollBoneBIndex];

				hknpBodyCinfo bodyAInfo = bodyInfos[bodyAIndex];
				hknpBodyCinfo bodyBInfo = bodyInfos[bodyBIndex];

				Constraint constraint = CreateConstraint(constraintInfo, bodyAInfo, bodyBInfo, boneAName, boneBName, poseAMatrix, poseBMatrix);

				constraints.Add(constraint);
			}

			return constraints;
		}

		List<PhysicsMaterial> RemapPhysicsMaterials(List<PhysicsMaterial> physicsMaterials, List<Body> bodies)
		{
			List<PhysicsMaterial> remapPhysicsMaterials = new List<PhysicsMaterial>();
			List<int> remapIndices = new List<int>(physicsMaterials.Count);

			for (int i = 0; i < physicsMaterials.Count; ++i)
			{
				PhysicsMaterial physicsMaterial = physicsMaterials[i];
				int remapIndex = remapPhysicsMaterials.FindIndex(e => Equal(e, physicsMaterial));
				if (remapIndex < 0)
				{
					remapIndex = remapPhysicsMaterials.Count;
					remapPhysicsMaterials.Add(physicsMaterial);
				}
				remapIndices.Add(remapIndex);
			}

			for (int i = 0; i < bodies.Count; ++i)
			{
				Body body = bodies[i];
				body.MaterialIndex = remapIndices[body.MaterialIndex];
				bodies[i] = body;
			}

			return remapPhysicsMaterials;
		}

		PhysicsMaterial CreatePhysicsMaterial(hknpMaterial hkMaterial)
		{
			PhysicsMaterial material = new PhysicsMaterial();

			material.Name = hkMaterial.m_name;
			material.DynamicFriction = Unpack(hkMaterial.m_dynamicFriction);
			material.StaticFriction = Unpack(hkMaterial.m_staticFriction);
			material.Restitution = Unpack(hkMaterial.m_restitution);
			material.FrictionCombineMode = ToCombineMode(hkMaterial.m_frictionCombinePolicy);
			material.RestitutionCombineMode = ToCombineMode(hkMaterial.m_restitutionCombinePolicy);

			return material;
		}

		Body CreateBody(hknpBodyCinfo bodyInfo, hknpMotionProperties motionProperty, string boneName, Matrix poseMatrix)
		{
			Matrix objectTransform = Matrix.CreateFromQuaternion(Microsoft.Xna.Framework.Quaternion.Normalize(new Microsoft.Xna.Framework.Quaternion(
					bodyInfo.m_orientation.X,
					bodyInfo.m_orientation.Y,
					bodyInfo.m_orientation.Z,
					bodyInfo.m_orientation.W)))
				* Matrix.CreateTranslation(new Vector3(
					bodyInfo.m_position.X,
					bodyInfo.m_position.Y,
					bodyInfo.m_position.Z));

			Matrix transform = objectTransform * poseMatrix;

			Shape shape = null;
			if (bodyInfo.m_shape.m_type == hknpShapeType.Enum.CAPSULE)
			{
				hknpCapsuleShape capsuleShape = bodyInfo.m_shape as hknpCapsuleShape;
				Vector4 a = capsuleShape.m_a;
				Vector4 b = capsuleShape.m_b;
				float radius = a.W;

				Vector3 a3 = new Vector3(a.X, a.Y, a.Z);
				a3 = Vector3.Transform(a3, transform);
				Vector3 b3 = new Vector3(b.X, b.Y, b.Z);
				b3 = Vector3.Transform(b3, transform);

				Vector3 axis = a3 - b3;
				Vector3 center = (a3 + b3) * 0.5f;
				float length = axis.Length();
				axis.Normalize();

				System.Numerics.Quaternion rotation = System.Numerics.Quaternion.Identity;

				if ((axis - Vector3.UnitZ).LengthSquared() > 0.000001f)
				{
					Vector3 rotateAxis = Vector3.Cross(Vector3.UnitZ, axis);
					float angle = MathF.Acos(axis.Z);
					rotation = System.Numerics.Quaternion.CreateFromAxisAngle(To(rotateAxis), angle);
				}

				System.Numerics.Quaternion mirror = System.Numerics.Quaternion.CreateFromAxisAngle(To(Vector3.UnitX), MathF.PI);

				Capsule capsule = new Capsule();

				capsule.Center = To(Vector3.Transform(center, mirror)) * To(UnitScale);
				capsule.Length = length * UnitScale.X;
				capsule.Radius = radius * UnitScale.X;
				capsule.Rotation = mirror * rotation;

				shape = capsule;
			}

			if (shape != null)
				shape.Type = bodyInfo.m_shape.m_type;

			hkCompressedMassProperties compressedMassProperties = (bodyInfo.m_shape.m_properties.m_entries[0].m_object as hknpShapeMassProperties)?.m_compressedMassProperties;
			System.Numerics.Vector4 inertia = Unpack(compressedMassProperties.m_inertia);
			System.Numerics.Vector4 centerOfMass = Unpack(compressedMassProperties.m_centerOfMass);
			System.Numerics.Vector4 majorAxisSpace = Unpack(compressedMassProperties.m_majorAxisSpace);

			Body body = new Body();

			body.Name = bodyInfo.m_name;
			body.BoneName = boneName;
			body.MotionType = bodyInfo.m_motionType;
			body.LinearDamping = motionProperty.m_linearDamping;
			body.AngularDamping = motionProperty.m_angularDamping;
			body.Mass = bodyInfo.m_mass < 0 ? compressedMassProperties.m_mass * 0.1f : bodyInfo.m_mass;
			body.Volume = compressedMassProperties.m_volume;
			body.CenterOfMass = centerOfMass;
			body.IntertiaTensor = inertia;
			body.MajorAxisSpace = new System.Numerics.Quaternion(majorAxisSpace.X, majorAxisSpace.Y, majorAxisSpace.Z, majorAxisSpace.W);
			body.MaterialIndex = bodyInfo.m_materialId;
			body.Shape = shape;

			return body;
		}

		Constraint CreateConstraint(hknpConstraintCinfo constraintInfo, hknpBodyCinfo bodyAInfo, hknpBodyCinfo bodyBInfo, string boneAName, string boneBName, Matrix poseAMatrix, Matrix poseBMatrix)
		{
			Vector3 translateA = Vector3.Zero;
			Vector3 translateB = Vector3.Zero;

			Vector3 axisXA = System.Numerics.Vector3.UnitX;
			Vector3 axisYA = System.Numerics.Vector3.UnitY;
			Vector3 axisZA = System.Numerics.Vector3.UnitZ;

			Vector3 axisXB = System.Numerics.Vector3.UnitX;
			Vector3 axisYB = System.Numerics.Vector3.UnitY;
			Vector3 axisZB = System.Numerics.Vector3.UnitZ;

			float Swing1LimitDegrees = 0.0f;
			float Swing2LimitDegrees = 0.0f;
			float TwistLimitDegrees = 0.0f;

			EAngularConstraintMotion Swing1Motion = EAngularConstraintMotion.Limited;
			EAngularConstraintMotion Swing2Motion = EAngularConstraintMotion.Limited;
			EAngularConstraintMotion TwistMotion = EAngularConstraintMotion.Limited;

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

				Matrix transformA = objectATransform * poseAMatrix;
				Matrix transformB = objectBTransform * poseBMatrix;

				Matrix localTransformA = transforms.m_transformA;
				Matrix localTransformB = transforms.m_transformB;

				Vector3 localTranslateA = localTransformA.Translation;
				Vector3 localTranslateB = localTransformB.Translation;

				translateA = Vector3.Transform(localTranslateA, transformA);
				translateB = Vector3.Transform(localTranslateB, transformB);

				if (ragdollConstraintData != null)
				{
					hkpTwistLimitConstraintAtom twistLimit = ragdollConstraintData.m_atoms.m_twistLimit;
					hkpConeLimitConstraintAtom coneLimit = ragdollConstraintData.m_atoms.m_coneLimit;
					hkpConeLimitConstraintAtom planesLimit = ragdollConstraintData.m_atoms.m_planesLimit;

					Vector3 localAxisXA = GetColumn(ref localTransformA, twistLimit.m_twistAxis);
					Vector3 localAxisYA = GetColumn(ref localTransformA, twistLimit.m_refAxis);
					Vector3 localAxisZA = GetColumn(ref localTransformA, planesLimit.m_refAxisInB);

					Vector3 localAxisXB = GetColumn(ref localTransformB, twistLimit.m_twistAxis);
					Vector3 localAxisYB = GetColumn(ref localTransformB, twistLimit.m_refAxis);
					Vector3 localAxisZB = GetColumn(ref localTransformB, planesLimit.m_refAxisInB);

					axisXA = Vector3.TransformNormal(localAxisXA, transformA);
					axisXA.Normalize();
					axisYA = Vector3.TransformNormal(localAxisYA, transformA);
					axisYA.Normalize();
					axisZA = Vector3.Cross(axisXA, axisYA);

					axisXB = Vector3.TransformNormal(localAxisXB, transformB);
					axisXB.Normalize();
					axisYB = Vector3.TransformNormal(localAxisYB, transformB);
					axisYB.Normalize();
					axisZB = Vector3.Cross(axisXB, axisYB);

					Swing1LimitDegrees = MathF.Min((planesLimit.m_maxAngle - planesLimit.m_minAngle) * 0.5f, coneLimit.m_maxAngle);
					Swing2LimitDegrees = coneLimit.m_maxAngle;
					TwistLimitDegrees = (twistLimit.m_maxAngle - twistLimit.m_minAngle) * 0.5f;

					float deltaAngle = twistLimit.m_minAngle + TwistLimitDegrees;
					if (MathF.Abs(deltaAngle) > 0.000001f)
					{
						Microsoft.Xna.Framework.Quaternion rotate = Microsoft.Xna.Framework.Quaternion.CreateFromAxisAngle(axisXB, deltaAngle);
						axisYB = Vector3.Transform(axisYB, rotate);
						axisZB = Vector3.Transform(axisZB, rotate);
					}
				}
				else if (limitedHingeConstraintData != null)
				{
					Vector3 localAxisZA = GetColumn(ref localTransformA, 0);
					Vector3 localAxisXA = GetColumn(ref localTransformA, 2);

					Vector3 localAxisZB = GetColumn(ref localTransformB, 0);
					Vector3 localAxisXB = GetColumn(ref localTransformB, 2);

					axisZA = Vector3.TransformNormal(localAxisZA, transformA);
					axisZA.Normalize();
					axisXA = Vector3.TransformNormal(localAxisXA, transformA);
					axisXA.Normalize();
					axisYA = Vector3.Cross(axisZA, axisXA);

					axisZB = Vector3.TransformNormal(localAxisZB, transformB);
					axisZB.Normalize();
					axisXB = Vector3.TransformNormal(localAxisXB, transformB);
					axisXB.Normalize();
					axisYB = Vector3.Cross(axisZB, axisXB);

					hkpAngLimitConstraintAtom limit = limitedHingeConstraintData.m_atoms.m_angLimit;
					Swing1LimitDegrees = (limit.m_maxAngle - limit.m_minAngle) * 0.5f;

					float deltaAngle = limit.m_minAngle + Swing1LimitDegrees;
					if (MathF.Abs(deltaAngle) > 0.000001f)
					{
						Microsoft.Xna.Framework.Quaternion rotate = Microsoft.Xna.Framework.Quaternion.CreateFromAxisAngle(axisZB, deltaAngle);
						axisXB = Vector3.Transform(axisXB, rotate);
						axisYB = Vector3.Transform(axisYB, rotate);
					}

					Swing2Motion = EAngularConstraintMotion.Locked;
					TwistMotion = EAngularConstraintMotion.Locked;
				}
			}

			int BodyAIndex = (int)(constraintInfo.m_bodyA.m_serialAndIndex & 0x00FFFFFF);
			int BodyBIndex = (int)(constraintInfo.m_bodyB.m_serialAndIndex & 0x00FFFFFF);

			System.Numerics.Quaternion mirror = System.Numerics.Quaternion.CreateFromAxisAngle(To(Vector3.UnitX), MathF.PI);

			Constraint constraint = new Constraint();

			constraint.Name = constraintInfo.m_name;

			constraint.BoneAName = boneAName;
			constraint.BoneBName = boneBName;

			constraint.BodyAIndex = BodyAIndex;
			constraint.BodyBIndex = BodyBIndex;

			constraint.Pos1 = To(Vector3.Transform(translateA, mirror)) * To(UnitScale);
			constraint.Pos2 = To(Vector3.Transform(translateB, mirror)) * To(UnitScale);

			constraint.PriAxis1 = To(Vector3.Transform(axisXA, mirror));
			constraint.PriAxis2 = To(Vector3.Transform(axisXB, mirror));

			constraint.SecAxis1 = To(Vector3.Transform(axisYA, mirror));
			constraint.SecAxis2 = To(Vector3.Transform(axisYB, mirror));

			constraint.Swing1LimitDegrees = Swing1LimitDegrees / MathF.PI * 180;
			constraint.Swing2LimitDegrees = Swing2LimitDegrees / MathF.PI * 180;
			constraint.TwistLimitDegrees = TwistLimitDegrees / MathF.PI * 180;

			constraint.Swing1Motion = Swing1Motion;
			constraint.Swing2Motion = Swing2Motion;
			constraint.TwistMotion = TwistMotion;

			return constraint;
		}

		Project CreateBehaviorProject(hkbProjectData hkProjectData)
		{
			var project = Project.Create(hkProjectData);

			project.SourceFile = ToRelativePath(project.SourceFile);
			project.ScriptPath = ToRelativePath(project.ScriptPath);

			return project;
		}

		Character CreateBehaviorCharacter(hkbCharacterData hkCharacterData)
		{
			var character = Character.Create(hkCharacterData);

			return character;
		}

		Graph CreateBehaviorGraph(hkbBehaviorGraph hkBehaviorGraph)
		{
			var graph = Graph.Create<Graph>(hkBehaviorGraph);

			return graph;
		}

		public Assimp.Scene CreateTestScene()
		{
			using (var context = new AssimpContext())
			{
				Assimp.Scene testScene = context.ImportFile("C:\\Users\\chypy\\Documents\\3dsMax\\export\\test.FBX");
				List<Mesh> testMeshes = testScene.Meshes;
				Assimp.Node rootNode = testScene.RootNode;
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
			for (int i = 0; i < (indices.Count / 3); i++)
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

			Assimp.Node node = new Assimp.Node("Node");
			node.MeshIndices.Add(0);

			Assimp.Scene scene = new Assimp.Scene();
			scene.RootNode = node;
			scene.Meshes.Add(mesh);

			Material material = new Material();
			material.Name = "Material";
			scene.Materials.Add(material);

			return scene;
		}

		List<Part> GetParts()
		{
			List<Part> parts = new List<Part>();

			Part mainPart = new Part();

			Model main = Scene.MainModel;
			mainPart.flver = main.flver;
			mainPart.flverPath = main.flverName;
			mainPart.aniContainer = main.AnimContainer;
			mainPart.taeContainer = Main.TAE_EDITOR.FileContainer;
			mainPart.ragdollContainer = main.RagdollLevelContainer;
			mainPart.ragdollPoseMatrices = main.RagdollPoseMatrices;
			mainPart.behbnd = main.behaviorBinder;

			parts.Add(mainPart);

			NewChrAsm charAsm = main.ChrAsm;
			if (charAsm == null)
				return parts;

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

				Part part = new Part();

				part.flver = flver;
				part.flverPath = mesh.flverName;

				parts.Add(part);
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
				if (flver == null)
					continue;

				Part part = new Part();

				part.flver = flver;
				part.flverPath = model.flverName;
				part.aniContainer = model.AnimContainer;
				part.ragdollContainer = model.RagdollLevelContainer;
				part.ragdollPoseMatrices = model.RagdollPoseMatrices;
				part.behbnd = model.behaviorBinder;

				parts.Add(part);
			}

			return parts;
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
				if (!string.IsNullOrEmpty(defaultPath) && !texturePaths.Contains(defaultPath))
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
			int index = keys.FindIndex(e => formatName.Contains(e));
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

		BoundingBox CalculateAABB(List<Assimp.Node> boneNodes)
		{
			List<Vector3> positions = new List<Vector3>(boneNodes.Count);

			for (int i = 0; i < boneNodes.Count; ++i)
			{
				Assimp.Node node = boneNodes[i];
				Matrix4x4 transform = node.Transform;

				Assimp.Node parent = node.Parent;
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

		System.Numerics.Vector3 To(Vector3 v)
		{
			System.Numerics.Vector3 result = new System.Numerics.Vector3(v.X, v.Y, v.Z);

			return result;
		}

		System.Numerics.Quaternion To(Assimp.Quaternion q)
		{
			System.Numerics.Quaternion result = new System.Numerics.Quaternion(q.X, q.Y, q.Z, q.W);

			return result;
		}

		System.Numerics.Quaternion To(Microsoft.Xna.Framework.Quaternion q)
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

		public static void WriteTextFile(string content, string path)
		{
			CreateDirectory(path);
			File.WriteAllText(path, content);
		}

		public static void CreateDirectory(string path)
		{
			var dir = Path.GetDirectoryName(path);
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);
		}

		string ToRelativePath(string path)
		{
			string relativePath = RemoveRootPath(path);
			relativePath = ReplacePaths(relativePath);
			return relativePath;
		}

		static readonly string[] Directories = new string[]
		{
			"INTERROOT_win64",
			"Model",
			"Material",
			"data",
		};

		public static string RemoveRootPath(string path)
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

		string ToTexturePath(string path, bool isCheckTransparent = true, bool isTransparent = false)
		{
			if (string.IsNullOrEmpty(path))
				return path;

			string extension = Path.GetExtension(path);
			if (extension.ToLower().Contains("tif"))
			{
				if ((isCheckTransparent && IsHasAlpha(path)) || (!isCheckTransparent && isTransparent))
					path = Path.ChangeExtension(path, ".bmp");
			}

			return path;
		}

		static T GetHavokObject<T>(hkRootLevelContainer container) where T : class
		{
			var element = container.m_namedVariants.Find(e => e.m_variant is T);
			return element?.m_variant as T;
		}

		static Vector3 GetColumn(ref Matrix m, int index)
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

		static float Unpack(hknpHalf v)
		{
			int i = v.m_value << 16;
			float r = BitConverter.ToSingle(BitConverter.GetBytes(i));
			return r;
		}

		static System.Numerics.Vector4 Unpack(hkPackedVector3 v)
		{
			List<short> svalues = v.m_values;
			List<int> ivalues = new List<int>(svalues.Count);

			for (int i = 0; i < svalues.Count; ++i)
			{
				short svalue = svalues[i];
				ivalues.Add((int)svalue << 16);
			}

			float exp = BitConverter.ToSingle(BitConverter.GetBytes(ivalues[3]));
			System.Numerics.Vector4 result = new System.Numerics.Vector4((float)(ivalues[0]), (float)(ivalues[1]), (float)(ivalues[2]), (float)(ivalues[3]));
			return result * exp;
		}

		static System.Numerics.Vector4 Unpack(List<short> v)
		{
			double HK_QUADREAL_UNPACK16_UNIT_VEC = 1.0 / (30000.0 * 0x10000);

			int hkPackedUnitVector_m_offset;
			unchecked
			{
				hkPackedUnitVector_m_offset = (int)0x80000000;
			}

			List<short> svalues = v;
			List<int> ivalues = new List<int>(svalues.Count);

			for (int i = 0; i < svalues.Count; ++i)
			{
				short svalue = svalues[i];
				ivalues.Add(((int)svalue << 16) + hkPackedUnitVector_m_offset);
			}

			System.Numerics.Vector4 result = new System.Numerics.Vector4((float)(ivalues[0]), (float)(ivalues[1]), (float)(ivalues[2]), (float)(ivalues[3]));
			return result * (float)HK_QUADREAL_UNPACK16_UNIT_VEC;
		}

		CombineMode ToCombineMode(CombinePolicy c)
		{
			CombineMode mode = CombineMode.Average;

			switch (c)
			{
				case CombinePolicy.COMBINE_MIN:
					mode = CombineMode.Min;
					break;
				case CombinePolicy.COMBINE_MAX:
					mode = CombineMode.Max;
					break;
				case CombinePolicy.COMBINE_GEOMETRIC_MEAN:
				case CombinePolicy.COMBINE_ARITHMETIC_MEAN:
				default:
					mode = CombineMode.Average;
					break;
			}

			return mode;
		}

		public static bool Equal(PhysicsMaterial lhs, PhysicsMaterial rhs)
		{
			return lhs.DynamicFriction == rhs.DynamicFriction
				&& lhs.StaticFriction == rhs.StaticFriction
				&& lhs.Restitution == rhs.Restitution
				&& lhs.FrictionCombineMode == rhs.FrictionCombineMode
				&& lhs.RestitutionCombineMode == rhs.RestitutionCombineMode;
		}

		const string ExportFormatID = "fbxa";
	}
}
