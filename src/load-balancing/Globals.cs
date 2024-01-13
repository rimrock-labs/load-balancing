using Godot;
using System;

public partial class Globals : Node2D
{
    public static ConsistentHashRing<string, string> Ring { get; set; }
}
