using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
public enum DescriptionTypeEnum { NONE, ERROR, INFO, WARNING, }

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
public class DescriptionAttribute : PropertyAttribute
{

	public string Text { get; private set; }

    public DescriptionAttribute(string text, DescriptionTypeEnum descriptionType = DescriptionTypeEnum.NONE, int order = 0)
    {
        Text = text;
        this.order = order;

        switch (descriptionType)
        {
            case DescriptionTypeEnum.ERROR:
                messageType = MessageType.Error;
                break;
            case DescriptionTypeEnum.INFO:
                messageType = MessageType.Info;
                break;
            case DescriptionTypeEnum.NONE:
                messageType = MessageType.None;
                break;
            case DescriptionTypeEnum.WARNING:
                messageType = MessageType.Warning;
                break;
            default:
                break;
        }
    }

    public MessageType messageType
    {
        get;
        private set;
    }
}
#endif

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(DescriptionAttribute))]
public class DescriptionDrawer : DecoratorDrawer
{
    private DescriptionAttribute descriptionAttribute { get { return ((DescriptionAttribute)attribute); } }

    public override void OnGUI(Rect position)
    {
        Rect textPosition = EditorGUI.IndentedRect(position);
        textPosition.height = CalculateTextHeight();
        DrawBox(textPosition, descriptionAttribute.Text);
    }

    public override float GetHeight()
    {
        return CalculateTextHeight();
    }

    public float CalculateTextHeight()
    {
        GUIStyle guiStyle = "HelpBox";
        return Mathf.Max(guiStyle.CalcHeight(new GUIContent(descriptionAttribute.Text),
            Screen.width - (descriptionAttribute.messageType != MessageType.None ? 53 : 21)),
            40);
    }

    private void DrawBox(Rect position, string text)
    {
        EditorGUI.HelpBox(position, text, descriptionAttribute.messageType);
    }
}

#endif