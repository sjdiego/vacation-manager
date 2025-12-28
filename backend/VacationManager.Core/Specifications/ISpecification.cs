namespace VacationManager.Core.Specifications;

/// <summary>
/// Defines a specification pattern for querying entities
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
public interface ISpecification<T>
{
    /// <summary>
    /// Determines whether an entity satisfies the specification
    /// </summary>
    bool IsSatisfiedBy(T entity);
}
