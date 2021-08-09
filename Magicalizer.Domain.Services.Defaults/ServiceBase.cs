// Copyright © 2021 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using FluentValidation;
using Magicalizer.Data.Entities.Abstractions;
using Magicalizer.Data.Repositories.Abstractions;
using Magicalizer.Domain.Models.Abstractions;
using Magicalizer.Filters.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Magicalizer.Domain.Services.Defaults
{
  public abstract class ServiceBase<TEntity, TModel, TFilter>
    where TEntity : class, IEntity, new()
    where TModel : class, IModel
    where TFilter : class, IFilter
  {
    protected readonly IStorage storage;
    protected readonly IServiceProvider serviceProvider;

    public ServiceBase(IStorage storage, IServiceProvider serviceProvider)
    {
      this.storage = storage;
      this.serviceProvider = serviceProvider;
    }

    protected Inclusion<TEntity>[] ToInclusions(string[] includes)
    {
      return includes?
        .Select(i => this.AutofixInclusion(typeof(TEntity), i))
        .Where(i => !string.IsNullOrEmpty(i))
        .Distinct()
        .Select(i => new Inclusion<TEntity>(i)).ToArray();
    }

    protected string AutofixInclusion(Type type, string include)
    {
      string result = string.Empty;
      string propertyName = include.Split('.')[0];
      PropertyInfo property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

      if (property == null)
        return result;

      result += property.Name;

      if (include.Contains('.'))
      {
        string subResult = this.AutofixInclusion(
          property.PropertyType.IsGenericType ?
            property.PropertyType.GetGenericArguments().First() : property.PropertyType,
            include.Substring(include.IndexOf('.') + 1)
        );

        result += string.IsNullOrEmpty(subResult) ? string.Empty : "." + subResult;
      }

      return result;
    }

    protected virtual IValidator<TModel> GetValidator()
    {
      return this.serviceProvider.GetService<IValidator<TModel>>();
    }

    protected virtual TModel EntityToModel(TEntity entity)
    {
      if (entity == null)
        return null;

      return Activator.CreateInstance(typeof(TModel), entity) as TModel;
    }
  }
}