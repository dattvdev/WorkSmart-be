using WorkSmart.Core.Entity;

namespace WorkSmart.Core.Interface
{
    public interface ICVTemplateRepository : IBaseRepository<CV_Template>
    {
        Task<IEnumerable<CV_Template>> GetTemplatesByNameAsync(string name);
    }
}
