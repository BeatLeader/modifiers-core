using System;

namespace ModifiersCore;

internal static class ArrayUtils {
    public static void ShrinkArray<T>(ref T[] array, T item) {
        var index = Array.IndexOf(array, item);
        if (index == -1) return;
        //
        var size = array.Length;
        var length = size - index - 1;
        Array.ConstrainedCopy(array, index + 1, array, index, length);
        Array.Resize(ref array, size - 1);
    }
    
    public static void ExpandArray<T>(ref T[] array, T item) {
        var index = Array.IndexOf(array, item);
        if (index != -1) return;
        //
        var size = array.Length;
        Array.Resize(ref array, size + 1);
        array[size] = item;
    }
}