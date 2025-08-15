using Godot;

public static class NodeExt
{
    public static void FreeChildren(this Node node)
    {
        foreach (Node item in node.GetChildren())
        {
            item.QueueFree();
        }
    }

    public static void RemoveChildren(this Node node)
    {
        while (node.GetChildCount() > 0)
        {
            var child = node.GetChild(0);
            child.QueueFree();
            node.RemoveChild(child);
        }
    }
}
