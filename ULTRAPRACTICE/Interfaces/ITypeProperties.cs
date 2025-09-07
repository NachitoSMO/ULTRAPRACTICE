namespace ULTRAPRACTICE.Interfaces;

public interface ITypeProperties<in T>
{
    public void CopyFrom(T other);
    public void CopyTo(T other);
}