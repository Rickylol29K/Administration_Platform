using DAL;

namespace Logic;

public partial class ApplicationLogic : ILogicService
{
    private readonly IDataRepository _repository;

    public ApplicationLogic(IDataRepository repository)
    {
        if (repository == null)
        {
            throw new ArgumentNullException(nameof(repository));
        }

        _repository = repository;
    }
}
