// Copyright (c) Jason Ma

using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LWGUI
{
	public static class UnityEditorExtension
	{

		#region MaterialEditor

		// For Developers: Call this after a material has modified in code
		public static void ApplyMaterialPropertyAndDecoratorDrawers(Material material)
		{
			var objs = new Object[] { material };
			ApplyMaterialPropertyAndDecoratorDrawers(objs);
		}

		// Called after edit or undo
		public static void ApplyMaterialPropertyAndDecoratorDrawers(Object[] targets)
		{
			if (!EditorMaterialUtility.disableApplyMaterialPropertyDrawers)
			{
				if (targets == null || targets.Length == 0)
					return;
				var target = targets[0] as Material;
				if (target == null)
					return;

				var shader = target.shader;
				string[] propNames = MaterialEditor.GetMaterialPropertyNames(targets);
				for (int i = 0; i < propNames.Length; i++)
				{
					var prop = MaterialEditor.GetMaterialProperty(targets, i);
					var drawer = ReflectionHelper.GetPropertyDrawer(shader, prop, out var decoratorDrawers);

					if (drawer != null)
					{
						drawer.Apply(prop);
					}
					if (decoratorDrawers != null)
					{
						foreach (var decoratorDrawer in decoratorDrawers)
						{
							decoratorDrawer.Apply(prop);
						}
					}
				}
			}
		}

		#endregion

	}
}