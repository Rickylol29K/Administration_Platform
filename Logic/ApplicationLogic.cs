using DAL;

namespace Logic;

public partial class ApplicationLogic : ILogicService
{
    private readonly IDataRepository _repository;

    public ApplicationLogic(IDataRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }
}
