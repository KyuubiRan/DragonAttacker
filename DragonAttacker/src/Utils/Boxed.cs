namespace DragonAttacker.Utils;

public class Boxed<T>
{
    public T? value;

    public Boxed(T? value)
    {
        this.value = value;
    }

    public Boxed()
    {
    }

    public static implicit operator T?(Boxed<T> thiz)
    {
        return thiz.value;
    }
    
    public static implicit operator Boxed<T> (T? value)
    {
        return new Boxed<T>(value);
    }
}