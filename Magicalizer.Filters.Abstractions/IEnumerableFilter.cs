namespace Magicalizer.Filters.Abstractions;

public interface IEnumerableFilter
{
  bool? IsEmpty { get; }
  bool? IsNotEmpty { get; }
  IEnumerable<IFilter>? Any { get; }

  IEnumerable<IFilter>? None { get; }
}