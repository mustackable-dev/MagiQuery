using System.Linq.Expressions;

namespace MagiQuery.Models;

/// <summary>
/// Represents a collection of property selectors for a specific entity type <typeparamref name="TEntity"/>.
/// </summary>
/// <typeparam name="TEntity">The entity type whose properties are being selected</typeparam>
public class PropertySelectors<TEntity>
{
    internal readonly List<Expression> Selectors = [];
    /// <summary>
    /// Defines a property to be selected from a <typeparamref name="TEntity"/>
    /// </summary>
    /// <typeparam name="TProperty">The type of the property being selected</typeparam>
    /// <param name="selector">An expression selecting the property on <typeparamref name="TEntity"/></param>
    /// <returns>The current <see cref="PropertySelectors&lt;TEntity&gt;"/> instance</returns>
    public PropertySelectors<TEntity> SelectProperty<TProperty>(
        Expression<Func<TEntity, TProperty>> selector) 
    {
        Selectors.Add(selector);
        return this;
    }
}