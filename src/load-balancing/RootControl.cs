using Godot;

using System;
using System.IO.Hashing;
using System.Text.Json;

public partial class RootControl : Control
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    private void _on_ready()
    {
        this._on_refresh_pressed();
    }

    private void _on_refresh_pressed()
    {
        var hasherList = this.GetNode<LineEdit>("%Hasher");
        NonCryptographicHashAlgorithm hasher;
        if (string.IsNullOrEmpty(hasherList.Text))
        {
            hasher = new Crc32();
        }
        else
        {
            string item = "System.IO.Hashing." + hasherList.Text + ", System.IO.Hashing";
            GD.Print($"Using {item}");
            hasher = Activator.CreateInstance(Type.GetType(item, true)) as NonCryptographicHashAlgorithm;
        }

        Globals.Ring = new ConsistentHashRing<string, string>(hasher);

        var instances = this.GetNode<SpinBox>("%Instances");
        var serverInstances = (int)instances.Value;

        var servers = this.GetNode<ItemList>("%Servers");
        // get all godot ItemList items
        var machines = servers.GetItemsText();
        var workItems = this.GetNode<CodeEdit>("%WorkItems");
        var work = JsonSerializer.Deserialize<string[]>(workItems.Text);

        if (Globals.Ring != null)
        {
            foreach (var item in machines)
            {
                Globals.Ring.AddWorkAgent(item, (ushort)serverInstances);
                GD.Print($"Added {item} agent {serverInstances} times");
            }

            foreach (var item in work)
            {
                Globals.Ring.AddWork(item);
                // GD.Print($"Added {item} work");
            }

        }
        this.GetNode<Control>("%Canvas").QueueRedraw();
        GD.Print("Refreshed");
    }

    private void _on_add_server_pressed()
    {
        var servers = this.GetNode<ItemList>("%Servers");
        var server = this.GetNode<LineEdit>("%Server");
        var serverName = server.Text;

        servers.AddItem(serverName);
    }


    private void _on_remove_server_pressed()
    {
        var servers = this.GetNode<ItemList>("%Servers");
        var items = servers.GetSelectedItems();
        if (items != null && items.Length > 0)
        {
            servers.RemoveItem(items[0]);
        }
    }
}
