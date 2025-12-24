using Logic.Contracts;

namespace Logic;

public interface ILogicService :
    IUserLogic,
    IClassLogic,
    IAttendanceLogic,
    IGradeLogic,
    IEventLogic,
    IAnnouncementLogic
{
}
