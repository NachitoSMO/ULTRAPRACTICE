using JetBrains.Annotations;

namespace ULTRAPRACTICE.Interfaces;
[UsedImplicitly]
public interface ITypeProperties<T, out TSelf>
    where TSelf : ITypeProperties<T, TSelf>
{
    T BackupObject { get; set; }
    
    public TSelf CopyFrom(T other);
    public void Restore();
}