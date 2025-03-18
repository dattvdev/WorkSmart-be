using System.Xml.Serialization;
using WorkSmart.Core.Entity;

namespace WorkSmart.Core.Interface
{
    public interface ITagRepository : IBaseRepository<Tag>
    {
        Task<IEnumerable<Tag>> GetByCategory(string category);
        Task<IEnumerable<Tag>> GetTagByListID(List<int> tagID);
    }
}
