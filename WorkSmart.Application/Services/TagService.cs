using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Interface;

namespace WorkSmart.Application.Services
{
    public class TagService
    {
        private readonly ITagRepository _tagRepository;
        public TagService(ITagRepository tagRepository)
        {
            _tagRepository = tagRepository;
        }

        public async Task<IEnumerable<Tag>> GetAll()
        {
            return await _tagRepository.GetAll();
        }
        public async Task<Tag> GetById(int id)
        {
            return await _tagRepository.GetById(id);
        }
        public async Task Add(Tag tag)
        {
            await _tagRepository.Add(tag);
        }
        public void Update(Tag tag)
        {
            _tagRepository.Update(tag);
        }
        public void Delete(int id)
        {
            _tagRepository.Delete(id);
        }

        public async Task<IEnumerable<Tag>> GetByCategory(string category)
        {
            return await _tagRepository.GetByCategory(category);
        }
    }
}
