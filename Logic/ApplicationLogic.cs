using DAL;

namespace Logic;

public partial class ApplicationLogic : ILogicService
{
    private readonly IDataRepository _repository;
    private readonly Contracts.IUserLogic _users;
    private readonly Contracts.IClassLogic _classes;
    private readonly Contracts.IAttendanceLogic _attendance;
    private readonly Contracts.IGradeLogic _grades;
    private readonly Contracts.IEventLogic _events;

    public ApplicationLogic(IDataRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _users = new Services.UserLogic(_repository);
        _classes = new Services.ClassLogic(_repository);
        _attendance = new Services.AttendanceLogic(_repository);
        _grades = new Services.GradeLogic(_repository);
        _events = new Services.EventLogic(_repository);
    }
}
