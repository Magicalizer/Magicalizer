using System.Collections;
using System.Reflection;
using Magicalizer.Filters.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Magicalizer;

public class EnumerableFilterBinder : IModelBinder
{
  private readonly IModelBinderFactory binderFactory;

  public EnumerableFilterBinder(IModelBinderFactory binderFactory)
  {
    this.binderFactory = binderFactory;
  }

  public async Task BindModelAsync(ModelBindingContext context)
  {
    object enumerableFilter = Activator.CreateInstance(context.ModelType)!;
    Type filterType = context.ModelType.GetGenericArguments()[0];

    await this.BindSingleFilterOrFilterList(context, enumerableFilter, filterType, nameof(IEnumerableFilter.Any));
    await this.BindSingleFilterOrFilterList(context, enumerableFilter, filterType, nameof(IEnumerableFilter.None));
    context.Result = ModelBindingResult.Success(enumerableFilter);
  }

  private async Task BindSingleFilterOrFilterList(ModelBindingContext context, object enumerableFilter, Type filterType, string propertyName)
  {
    string prefix = ModelNames.CreatePropertyModelName(context.ModelName, propertyName);

    if (!context.ValueProvider.ContainsPrefix(prefix)) return;

    PropertyInfo? property = enumerableFilter.GetType().GetProperty(propertyName);

    if (property == null) return;

    IList? filterList = null;
    Type filterListType = typeof(List<>).MakeGenericType(filterType);
    object? filter = await TryBind(context, filterType, prefix);

    if (filter == null)
      filterList = await TryBind(context, filterListType, prefix) as IList;

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
    IModelBinder binder = binderFactory.CreateBinder(new ModelBinderFactoryContext { Metadata = metadata, BindingInfo = new BindingInfo { BinderModelName = prefix } });

    using (context.EnterNestedScope(metadata, prefix, prefix, null))
    {
      await binder.BindModelAsync(context);
      return context.Result.IsModelSet ? context.Result.Model : null;
    }
  }
}
