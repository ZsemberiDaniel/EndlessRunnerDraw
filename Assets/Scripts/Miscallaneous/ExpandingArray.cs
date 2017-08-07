public class ExpandingArray<T> {

    private T[] array;
    public T this[int key] {
        get {
            return array[key];
        }
        set {
            Set(key, value);
        }
    }
    public int Length { get { return array.Length; } }

    public ExpandingArray() : this(32) { }

    public ExpandingArray(int initialCount) {
        array = new T[initialCount];
    }

    public void Set(int at, T obj) {
        if (at >= array.Length) {
            System.Array.Resize(ref array, array.Length * 2);
        }

        array[at] = obj;
    }
	
}
