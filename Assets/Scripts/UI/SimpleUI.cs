using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace EmpireOfHonor.UI
{
    /// <summary>
    /// Populates the UI with control instructions at runtime.
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class SimpleUI : MonoBehaviour
    {
        [SerializeField, TextArea] private string customMessage;

        private void Awake()
        {
            var textComponent = GetComponent<Text>();
            if (textComponent == null)
            {
                return;
            }

            textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComponent.alignment = TextAnchor.UpperLeft;
            textComponent.supportRichText = false;
            textComponent.horizontalOverflow = HorizontalWrapMode.Wrap;
            textComponent.verticalOverflow = VerticalWrapMode.Overflow;

            textComponent.text = string.IsNullOrWhiteSpace(customMessage) ? BuildDefaultMessage() : customMessage;
        }

        private static string BuildDefaultMessage()
        {
            var builder = new StringBuilder();
            builder.AppendLine("TPS MODE");
            builder.AppendLine("WASD - Move | Shift - Sprint | Space - Jump | LMB / RT - Attack");
            builder.AppendLine("C / Select - Switch to Tactical");
            builder.AppendLine();
            builder.AppendLine("TACTICAL MODE");
            builder.AppendLine("WASD / Left Stick - Pan | Q/E - Rotate | Scroll / LT/RT - Zoom");
            builder.AppendLine("Hold Alt + LMB - Move/Attack | Hold Alt + RMB - Hold Position");
            builder.AppendLine("1..4 / D-Pad - Select Groups");
            return builder.ToString();
        }
    }
}
