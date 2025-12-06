using System.Collections.Generic;

[System.Serializable]
public class Component
{
    public string name;
    public string description;
    public dimension dimension;
}

[System.Serializable]

public class dimension
{
    public string length;
    public string width;
    public string height;
}

[System.Serializable]
public class ComponentInfo
{
    public List<Component> components;
}