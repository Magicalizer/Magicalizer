using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using Magicalizer.Filters.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Magicalizer;

public class EnumerableFilterBinder : IModelBinder
{
  private record EnumerableFilterPropertyCache(
    PropertyInfo? IsEmpty,
    PropertyInfo? IsNotEmpty,
    PropertyInfo? Any,
    PropertyInfo? None
  );

  private static readonly ConcurrentDictionary<Type, EnumerableFilterPropertyCache> propertiesByTypes = new();
  private readonly IModelBinderFactory binderFactory;

  public EnumerableFilterBinder(IModelBinderFactory binderFactory)
  {
    this.binderFactory = binderFactory;
  }

  public async Task BindModelAsync(ModelBindingContext context)
  {
    object enumerableFilter = Activator.CreateInstance(context.ModelType)!;
    Type filterType = context.ModelType.GetGenericArguments()[0];
    EnumerableFilterPropertyCache properties = propertiesByTypes.GetOrAdd(context.ModelType, t => new(
      t.GetProperty(nameof(IEnumerableFilter.IsEmpty)),
      t.GetProperty(nameof(IEnumerableFilter.IsNotEmpty)),
      t.GetProperty(nameof(IEnumerableFilter.Any)),
      t.GetProperty(nameof(IEnumerableFilter.None))
    ));

    this.BindBoolProperty(context, enumerableFilter, properties.IsEmpty, nameof(IEnumerableFilter.IsEmpty));
    this.BindBoolProperty(context, enumerableFilter, properties.IsNotEmpty, nameof(IEnumerableFilter.IsNotEmpty));
    await this.BindSingleFilterOrFilterList(context, enumerableFilter, filterType, properties.Any, nameof(IEnumerableFilter.Any));
    await this.BindSingleFilterOrFilterList(context, enumerableFilter, filterType, properties.None, nameof(IEnumerableFilter.None));
    context.Result = ModelBindingResult.Success(enumerableFilter);
  }

  private void BindBoolProperty(ModelBindingContext context, object enumerableFilter, PropertyInfo? property, string propertyName)
  {
    if (property == null) return;

    string prefix = ModelNames.CreatePropertyModelName(context.ModelName, propertyName);
    ValueProviderResult result = context.ValueProvider.GetValue(prefix);

    if (result == ValueProviderResult.None) return;

    if (bool.TryParse(result.FirstValue, out bool value))
      property.SetValue(enumerableFilter, value);
  }

  private async Task BindSingleFilterOrFilterList(ModelBindingContext context, object enumerableFilter, Type filterType, PropertyInfo? property, string propertyName)
  {
    if (property == null) return;

    string prefix = ModelNames.CreatePropertyModelName(context.ModelName, propertyName);

    if (!context.ValueProvider.ContainsPrefix(prefix)) return;

    IList? filterList = null;
    Type filterListType = typeof(List<>).MakeGenericType(filterType);
    object? filter = await this.TryBind(context, filterType, prefix);

    if (filter == null)
      filterList = await this.TryBind(context, filterListType, prefix) as IList;

    else
    {
      filterList = (IList)Activator.CreateInstance(filterListType)!;
      filterList.Add(filter);
    }

    if (filterList != null && filterList.Count != 0)
      property.SetValue(enumerableFilter, filterList);
  }

  private async Task<object?> TryBind(ModelBindingContext context, Type filterType, string prefix)
  {
    context.Result = ModelBindingResult.Failed();

    ModelMetadata metadata = context.ModelMetadata.GetMetadataForType(filterType);
    IModelBinder binder = this.binderFactory.CreateBinder(new ModelBinderFactoryContext { Metadata = metadata, BindingInfo = new BindingInfo { BinderModelName = prefix } });

    using (context.EnterNestedScope(metadata, prefix, prefix, null))
    {
      await binder.BindModelAsync(context);
      return context.Result.IsModelSet ? context.Result.Model : null;
    }
  }
}
