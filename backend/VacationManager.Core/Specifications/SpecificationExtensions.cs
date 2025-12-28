namespace VacationManager.Core.Specifications;

/// <summary>
/// Extension methods for applying specifications to collections
/// </summary>
public static class SpecificationExtensions
{
    /// <summary>
    /// Filters a collection using a specification
    /// </summary>
    public static IEnumerable<T> Where<T>(this IEnumerable<T> source, ISpecification<T> specification)
    {
        return source.Where(item => specification.IsSatisfiedBy(item));
    }

    /// <summary>
    /// Checks if any item in the collection satisfies the specification
    /// </summary>
    public static bool Any<T>(this IEnumerable<T> source, ISpecification<T> specification)
    {
        return source.Any(item => specification.IsSatisfiedBy(item));
    }

    /// <summary>
    /// Checks if all items in the collection satisfy the specification
    /// </summary>
    public static bool All<T>(this IEnumerable<T> source, ISpecification<T> specification)
    {
        return source.All(item => specification.IsSatisfiedBy(item));
    }

    /// <summary>
    /// Gets the first item that satisfies the specification, or default if none found
    /// </summary>
    public static T? FirstOrDefault<T>(this IEnumerable<T> source, ISpecification<T> specification)
    {
        return source.FirstOrDefault(item => specification.IsSatisfiedBy(item));
    }

    /// <summary>
    /// Counts how many items satisfy the specification
    /// </summary>
    public static int Count<T>(this IEnumerable<T> source, ISpecification<T> specification)
    {
        return source.Count(item => specification.IsSatisfiedBy(item));
    }
}
