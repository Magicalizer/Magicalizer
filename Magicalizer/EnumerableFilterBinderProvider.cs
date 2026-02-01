using Magicalizer.Filters.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

namespace Magicalizer;

public class EnumerableFilterBinderProvider : IModelBinderProvider
{
  public IModelBinder? GetBinder(ModelBinderProviderContext context)
  {
    if (context == null) throw new ArgumentNullException(nameof(context));

    if (context.Metadata.ModelType.IsGenericType && context.Metadata.ModelType.GetGenericTypeDefinition() == typeof(EnumerableFilter<>))
      return new EnumerableFilterBinder(context.Services.GetRequiredService<IModelBinderFactory>());

    return null;
  }
}