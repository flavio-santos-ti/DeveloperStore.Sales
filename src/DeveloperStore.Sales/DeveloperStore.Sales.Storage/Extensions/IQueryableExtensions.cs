using System.Linq.Expressions;

namespace DeveloperStore.Sales.Storage.Extensions;

public static class IQueryableExtensions
{
    public static IQueryable<T> OrderByDynamic<T>(this IQueryable<T> source, string propertyName, bool descending)
    {
        var parameter = Expression.Parameter(typeof(T), "p");

        var propertyParts = propertyName.Split('.');
        Expression property = parameter;

        foreach (var part in propertyParts)
        {
            property = Expression.PropertyOrField(property, part);
        }

        if (property.Type.IsGenericType && property.Type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            property = Expression.Convert(property, Nullable.GetUnderlyingType(property.Type));
        }

        var lambda = Expression.Lambda(property, parameter);

        var methodName = descending ? "OrderByDescending" : "OrderBy";
        var result = Expression.Call(
            typeof(Queryable),
            methodName,
            new[] { typeof(T), property.Type },
            source.Expression,
            Expression.Quote(lambda));

        return source.Provider.CreateQuery<T>(result);
    }

    public static IQueryable<T> ThenByDynamic<T>(this IQueryable<T> source, string propertyName, bool descending)
    {
        var parameter = Expression.Parameter(typeof(T), "p");

        var propertyParts = propertyName.Split('.');
        Expression property = parameter;

        foreach (var part in propertyParts)
        {
            property = Expression.Property(property, part);
        }

        if (property.Type.IsGenericType && property.Type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            property = Expression.Convert(property, Nullable.GetUnderlyingType(property.Type));
        }

        var lambda = Expression.Lambda(property, parameter);

        var methodName = descending ? "ThenByDescending" : "ThenBy";
        var result = Expression.Call(
            typeof(Queryable),
            methodName,
            new[] { typeof(T), property.Type },
            source.Expression,
            Expression.Quote(lambda));

        return source.Provider.CreateQuery<T>(result);
    }

}
