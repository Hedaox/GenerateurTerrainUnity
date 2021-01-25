using System;
using UnityEngine;

[System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
public class ListMaskAttribute : PropertyAttribute
{
    public bool alwaysFoldOut;
    public ListMaskLayout layout = ListMaskLayout.Vertical;
}

public enum ListMaskLayout
{
    Vertical,
    Horizontal
}