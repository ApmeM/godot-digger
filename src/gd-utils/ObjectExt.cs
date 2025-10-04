using System;
using System.Threading.Tasks;

public static class ObjectExt
{
    public static async Task ToMySignal(this Godot.Object obj, string signal)
    {
        try
        {
            await obj.ToSignal(obj, signal);
        }
        catch (ObjectDisposedException)
        {
            return;
        }
    }

    public static async Task<T> ToMySignal<T>(this Godot.Object obj, string signal)
    {
        try
        {
            var result = await obj.ToSignal(obj, signal);
            return (T)Convert.ChangeType(result[0], typeof(T));
        }
        catch (ObjectDisposedException)
        {
            return default;
        }
    }

    public static async Task<(T1, T2)> ToMySignal<T1, T2>(this Godot.Object obj, string signal)
    {
        try
        {
            var result = await obj.ToSignal(obj, signal);
            return (
                (T1)Convert.ChangeType(result[0], typeof(T1)),
                (T2)Convert.ChangeType(result[1], typeof(T2))
            );
        }
        catch (ObjectDisposedException)
        {
            return default;
        }
    }
}
