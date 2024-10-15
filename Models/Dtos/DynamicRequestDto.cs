using System;

public class DynamicRequestDto
{
    public string ListType { get; set; }
    public string Name { get; set; }
    public Array Data { get; set; }  // Use JArray for dynamic arrays
}
