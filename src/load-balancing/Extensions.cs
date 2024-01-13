using System.Collections.Generic;
using Godot;

public static class Extensions
{
    public static IEnumerable<string> GetItemsText(this ItemList list)
    {
        int count = list.ItemCount;
        for(int i = 0; i < count; i++)
        {
            yield return list.GetItemText(i);
        }
    }
}
