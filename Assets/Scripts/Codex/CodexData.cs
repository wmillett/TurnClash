using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CodexEntry
{
    public string id;
    public string title;
    public string description;
    public string imagePath;
    public string category;
    public string subcategory;
}

[Serializable]
public class CodexData
{
    public List<CodexEntry> entries;
}

public enum CodexCategory
{
    Characters,
    History,
    Locations,
    Organizations
} 