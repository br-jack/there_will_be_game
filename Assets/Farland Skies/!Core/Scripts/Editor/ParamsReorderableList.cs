using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Borodar.FarlandSkies.Core.Editor
{
    public class ParamsReorderableList
    {
        private readonly ReorderableList _reorderableList;
        private static GUIStyle _header;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public static GUIStyle Title
        {
            get
            {
                if (_header != null) return _header;

                _header = (GUIStyle)"DD Background";
                _header.margin = new RectOffset(5, 5, 5, 0);
                _header.padding = new RectOffset(5, 5, 3, 3);
                _header.alignment = TextAnchor.MiddleLeft;
                return _header;
            }
        }

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public ParamsReorderableList(SerializedProperty elements, PropertyDrawer drawer)
        {
            var so = elements.serializedObject;

            _reorderableList = new ReorderableList(
                elements.serializedObject,
                elements,
                true,
                false,
                true,
                true)
            {
                headerHeight = 0f,
                elementHeight = drawer.GetPropertyHeight(elements, GUIContent.none),
                drawElementCallback = (rect, index, active, focused) =>
                {
                    drawer.OnGUI(rect, elements.GetArrayElementAtIndex(index), GUIContent.none);
                }
            };
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void DoLayoutList()
        {
            _reorderableList.DoLayoutList();
        }
    }
}