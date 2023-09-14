﻿// Copyright (c) Jason Ma
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace LWGUI
{
	/// <summary>
	/// Helpers for drawing Unreal Style Revertable Shader GUI 
	/// </summary>
	public class RevertableHelper
	{
		public static readonly float revertButtonWidth = 15f;
		public static          float fieldWidth;
		public static          float labelWidth;

		#region GUI Setting

		public static void IndentRect(ref Rect rect)
		{
			rect.xMax -= RevertableHelper.revertButtonWidth;
		}

		public static Rect SplitRevertButtonRect(ref Rect rect, bool isCallInDrawer = false)
		{
			// TODO: use Reflection
			float defaultHeightWithoutDrawers = EditorGUIUtility.singleLineHeight;
			var revertButtonRect = GetRevertButtonRect(defaultHeightWithoutDrawers, rect, isCallInDrawer);
			IndentRect(ref rect);
			return revertButtonRect;
		}

		public static Rect GetRevertButtonRect(float propHeight, Rect rect, bool isCallInDrawer = false)
		{
			if (isCallInDrawer) rect.xMax += revertButtonWidth;
			var revertButtonRect = new Rect(rect.xMax - revertButtonWidth + 2f,
											rect.yMax - propHeight * 0.5f - revertButtonWidth * 0.5f,
											revertButtonWidth - 2f,
											revertButtonWidth - 3f);
			return revertButtonRect;
		}

		public static void InitRevertableGUIWidths()
		{
			EditorGUIUtility.fieldWidth += RevertableHelper.revertButtonWidth;
			EditorGUIUtility.labelWidth -= RevertableHelper.revertButtonWidth;
			RevertableHelper.fieldWidth = EditorGUIUtility.fieldWidth;
			RevertableHelper.labelWidth = EditorGUIUtility.labelWidth;
		}

		public static void SetRevertableGUIWidths()
		{
			EditorGUIUtility.fieldWidth = RevertableHelper.fieldWidth;
			EditorGUIUtility.labelWidth = RevertableHelper.labelWidth;
		}

		public static void FixGUIWidthMismatch(MaterialProperty.PropType propType, MaterialEditor materialEditor)
		{
			switch (propType)
			{
				case MaterialProperty.PropType.Texture:
				case MaterialProperty.PropType.Range:
					materialEditor.SetDefaultGUIWidths();
					break;
				default:
					RevertableHelper.SetRevertableGUIWidths();
					break;
			}
		}

		#endregion


		#region Property Handle

		public static void SetPropertyToDefault(MaterialProperty defaultProp, MaterialProperty prop)
		{
			prop.vectorValue = defaultProp.vectorValue;
			prop.colorValue = defaultProp.colorValue;
			prop.floatValue = defaultProp.floatValue;
			prop.textureValue = defaultProp.textureValue;
#if UNITY_2021_1_OR_NEWER
			prop.intValue = defaultProp.intValue;
#endif
		}

		public static string GetPropertyDefaultValueText(MaterialProperty defaultProp)
		{
			string defaultText = String.Empty;
			switch (defaultProp.type)
			{
				case MaterialProperty.PropType.Color:
					defaultText += defaultProp.colorValue;
					break;
				case MaterialProperty.PropType.Float:
				case MaterialProperty.PropType.Range:
					defaultText += defaultProp.floatValue;
					break;
#if UNITY_2021_1_OR_NEWER
				case MaterialProperty.PropType.Int:
					defaultText += defaultProp.intValue;
					break;
#endif
				case MaterialProperty.PropType.Texture:
					defaultText += defaultProp.textureValue != null ? defaultProp.textureValue.name : "None";
					break;
				case MaterialProperty.PropType.Vector:
					defaultText += defaultProp.vectorValue;
					break;
			}
			return defaultText;
		}

		#endregion


		#region Draw revert button

		public static bool DrawRevertableProperty(Rect position, MaterialProperty prop, LWGUI lwgui)
		{
			bool hasModified = prop.hasMixedValue;

			var propDynamicData = lwgui.perFrameData.propertyDatas[prop.name];
			if (!hasModified)
				hasModified = !Helper.PropertyValueEquals(prop, propDynamicData.defualtProperty);

			var extraPropNames = lwgui.perShaderData.propertyDatas[prop.name].extraPropNames;
			if (!hasModified && extraPropNames.Count > 0)
			{
				hasModified = extraPropNames.Any((extraPropName =>
													 !Helper.PropertyValueEquals(lwgui.perFrameData.propertyDatas[extraPropName].property,
																				 lwgui.perFrameData.propertyDatas[extraPropName].defualtProperty)));
			}

			if (!hasModified)
				return false;

			Rect rect = position;
			if (DrawRevertButton(rect))
			{
				// GUI.changed = true;
				propDynamicData.changed = true;
				SetPropertyToDefault(propDynamicData.defualtProperty, prop);
				foreach (var extraPropName in extraPropNames)
				{
					var extraPropDynamicData = lwgui.perFrameData.propertyDatas[extraPropName];
					extraPropDynamicData.changed = true;
					SetPropertyToDefault(extraPropDynamicData.defualtProperty, extraPropDynamicData.property);
				}
				// refresh keywords
				MaterialEditor.ApplyMaterialPropertyDrawers(lwgui.materialEditor.targets);
				return true;
			}
			return false;
		}

		private static readonly Texture _icon = AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GUIDToAssetPath("e7bc1130858d984488bca32b8512ca96"));

		public static bool DrawRevertButton(Rect rect)
		{
			if (_icon == null) Debug.LogError("RevertIcon.png + meta is missing!");
			GUI.DrawTexture(rect, _icon);
			var e = Event.current;
			if (e.type == UnityEngine.EventType.MouseDown && rect.Contains(e.mousePosition))
			{
				e.Use();
				return true;
			}
			return false;
		}

		#endregion
	}
}