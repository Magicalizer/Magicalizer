// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Magicalizer.Data.Entities.Abstractions;

namespace Magicalizer.Data.Repositories.Abstractions
{
  public class Inclusion<TEntity> where TEntity : class, IEntity
  {
    public Expression<Func<TEntity, object>> Property { get; }
    public string PropertyPath { get; }

    public Inclusion(Expression<Func<TEntity, object>> property)
    {
      this.Property = property;
    }

    public Inclusion(string propertyPath)
    {
      this.PropertyPath = propertyPath;
    }
  }
}