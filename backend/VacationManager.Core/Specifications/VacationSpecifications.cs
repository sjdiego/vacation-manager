using VacationManager.Core.Entities;

namespace VacationManager.Core.Specifications;

/// <summary>
/// Specification to filter vacations by date range
/// </summary>
public class DateRangeSpecification : Specification<Vacation>
{
    private readonly DateTime _startDate;
    private readonly DateTime _endDate;

    public DateRangeSpecification(DateTime startDate, DateTime endDate)
    {
        _startDate = startDate;
        _endDate = endDate;
    }

    public override bool IsSatisfiedBy(Vacation vacation)
    {
        return vacation.StartDate <= _endDate && vacation.EndDate >= _startDate;
    }
}

/// <summary>
/// Specification to filter vacations by status
/// </summary>
public class VacationStatusSpecification : Specification<Vacation>
{
    private readonly VacationStatus _status;

    public VacationStatusSpecification(VacationStatus status)
    {
        _status = status;
    }

    public override bool IsSatisfiedBy(Vacation vacation)
    {
        return vacation.Status == _status;
    }
}

/// <summary>
/// Specification to filter vacations by team
/// </summary>
public class TeamSpecification : Specification<Vacation>
{
    private readonly Guid _teamId;
    private readonly Dictionary<Guid, Guid?> _userTeamMapping;

    public TeamSpecification(Guid teamId, Dictionary<Guid, Guid?> userTeamMapping)
    {
        _teamId = teamId;
        _userTeamMapping = userTeamMapping;
    }

    public override bool IsSatisfiedBy(Vacation vacation)
    {
        return _userTeamMapping.TryGetValue(vacation.UserId, out var teamId) && teamId == _teamId;
    }
}

/// <summary>
/// Specification to filter vacations by user
/// </summary>
public class UserSpecification : Specification<Vacation>
{
    private readonly Guid _userId;

    public UserSpecification(Guid userId)
    {
        _userId = userId;
    }

    public override bool IsSatisfiedBy(Vacation vacation)
    {
        return vacation.UserId == _userId;
    }
}

/// <summary>
/// Specification to filter vacations by type
/// </summary>
public class VacationTypeSpecification : Specification<Vacation>
{
    private readonly VacationType _type;

    public VacationTypeSpecification(VacationType type)
    {
        _type = type;
    }

    public override bool IsSatisfiedBy(Vacation vacation)
    {
        return vacation.Type == _type;
    }
}

/// <summary>
/// Specification for pending vacations (shortcut)
/// </summary>
public class PendingVacationsSpecification : VacationStatusSpecification
{
    public PendingVacationsSpecification() : base(VacationStatus.Pending)
    {
    }
}

/// <summary>
/// Specification for approved vacations (shortcut)
/// </summary>
public class ApprovedVacationsSpecification : VacationStatusSpecification
{
    public ApprovedVacationsSpecification() : base(VacationStatus.Approved)
    {
    }
}
