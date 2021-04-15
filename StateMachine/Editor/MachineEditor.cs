using System.Text;
using UnityEditor;
using UnityEngine;

namespace Phuntasia.Fsm
{
    [CustomEditor(typeof(Machine), true)]
    public class MachineEditor : Editor
    {
    }

    public static class MachineGizmos
    {
        static readonly GUIContent _content;
        static readonly GUIStyle _style;
        static readonly StringBuilder _sb;

        static MachineGizmos()
        {
            _content = new GUIContent();

            _style = new GUIStyle
            {
                fontSize = 8,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(4, 4, 2, 2)
            };
            _style.normal.background = Texture2D.whiteTexture;
            _style.normal.textColor = Color.white;

            _sb = new StringBuilder();
        }


        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        static void OnDrawGizmos(Machine machine, GizmoType gizmoType)
        {
            if (!machine.IsRunning)
            {
                GUI.backgroundColor = new Color(0.4f, 0.1f, 0.1f, 0.5f);
                _content.text = "Not Running";
            }
            else
            {
                GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);
                _content.text = machine.State.GetType().Name.Replace("State", "").Beautify();
            }
            

            Handles.BeginGUI();
            {
                var pos = HandleUtility.WorldToGUIPoint(machine.transform.position + new Vector3(0, 2.25f, 0));
                var size = _style.CalcSize(_content);    
                
                GUI.Label(new Rect(pos.x - size.x/2f, pos.y, size.x, size.y), _content, _style);                
            }
            Handles.EndGUI();

            GUI.backgroundColor = Color.white;
        }
    }
}