namespace Magicalizer.Filters.Abstractions;

public interface IEnumerableFilter
{
  IEnumerable<IFilter>? Any { get; }

  IEnumerable<IFilter>? None { get; }
}