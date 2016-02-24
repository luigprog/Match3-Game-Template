using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

class PropertyDrawersUtils
{
#if UNITY_EDITOR
    public static FieldInfo ReflectionInfoRecursive(SerializedProperty propert, out object target, string varName = "")
    {
        string propName = propert.name;
        string fullpath = propert.propertyPath;
        if (!string.IsNullOrEmpty(varName))
        {
            fullpath = fullpath.Replace(propert.name, varName);
            propName = varName;
        }
        List<string> pathList = fullpath.Split('.').ToList();
        if (propName == "data")
        {
            propName = pathList[pathList.LastIndexOf("Array") - 1];
        }
        pathList.Remove("Array");
        target = propert.serializedObject.targetObject;
        if ((propert.serializedObject.targetObject as MonoBehaviour) != null)
        {
            return ReflectionFieldInfRecursive(propert.serializedObject.targetObject, (propert.serializedObject.targetObject as MonoBehaviour).GetType().GetField((propert.propertyPath.Split('.').Length == 1 && !string.IsNullOrEmpty(propName) ? propName : propert.propertyPath.Split('.')[0]), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public), propName, pathList, out target);
        }

        if ((propert.serializedObject.targetObject as ScriptableObject) != null)
        {
            return ReflectionFieldInfRecursive(propert.serializedObject.targetObject, (propert.serializedObject.targetObject as ScriptableObject).GetType().GetField((propert.propertyPath.Split('.').Length == 1 && !string.IsNullOrEmpty(propName) ? propName : propert.propertyPath.Split('.')[0]), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public), propName, pathList, out target);
        }

        return null;
    }

    public static FieldInfo ReflectionFieldInfRecursive(object targetObject, FieldInfo info, string propertName, List<string> paths, out object objectOut)
    {
        if (paths.Count > 1 && paths[0] != propertName)
        {
            paths.RemoveAt(0);

            if (paths[0].Contains("data["))
            {
                int pathIndex = int.Parse(paths[0].Replace("data[", "").Replace("]", ""));
                paths.RemoveAt(0);
                try
                {
                    if ((info.GetValue(targetObject) as IList).Count <= pathIndex)
                    {
                        objectOut = null;
                        return null;
                    }
                }
                catch
                {
                    objectOut = null;
                    return null;
                }
                return ReflectionFieldInfRecursive((info.GetValue(targetObject) as IList)[pathIndex], info.FieldType.GetGenericArguments()[0].GetField(paths[0], BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public), propertName, paths, out objectOut);
            }
            return ReflectionFieldInfRecursive(info.GetValue(targetObject), info.FieldType.GetField(paths[0], BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public), propertName, paths, out objectOut);
        }
        else
        {
            objectOut = targetObject;
            return info;
        }
    }

    public static T ReflectionFieldRecursive<T>(SerializedProperty propert, string varName = "")
    {
        object targetObject = null;
        FieldInfo fieldInfo = ReflectionInfoRecursive(propert, out targetObject, varName);
        T temp = default(T);
        try
        {
            temp = (T)(fieldInfo.GetValue(targetObject));
        }
        catch
        {
            temp = (T)(fieldInfo.GetValue(targetObject) as IList)[int.Parse(propert.propertyPath.Substring(propert.propertyPath.LastIndexOf("data[") + 5).Split(']')[0])];
        }
        return temp;
    }
#endif
}