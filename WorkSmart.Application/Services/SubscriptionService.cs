using AutoMapper;
using WorkSmart.Core.Dto.PackageDtos;
using WorkSmart.Core.Dto.SubscriptionDtos;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Interface;

namespace WorkSmart.Application.Services
{
    public class SubscriptionService
    {
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly IPackageRepository _packageRepository;
        private readonly IMapper _mapper;

        public SubscriptionService(ISubscriptionRepository subscriptionRepository, IPackageRepository packageRepository, IMapper mapper)
        {
            _subscriptionRepository = subscriptionRepository;
            _packageRepository = packageRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<SubscriptionDto>> GetAll()
        {
            var subscriptions = await _subscriptionRepository.GetAll();
            return _mapper.Map<IEnumerable<SubscriptionDto>>(subscriptions);
        }

        public async Task<List<(SubscriptionDto subscription, GetPackageDto package)>> GetByUserId(int id)
        {
            var subscriptions = await _subscriptionRepository.GetByUserId(id);
            var result = new List<(SubscriptionDto, GetPackageDto)>();

            foreach (var subscription in subscriptions)
            {
                var package = await _packageRepository.GetById(subscription.PackageID);
                if (package != null)
                {
                    result.Add((_mapper.Map<SubscriptionDto>(subscription), _mapper.Map<GetPackageDto>(package)));
                }
            }

            return result;
        }

        public async Task Add(SubscriptionDto subscriptionDto)
        {
            var subscription = _mapper.Map<Subscription>(subscriptionDto);
            await _subscriptionRepository.Add(subscription);
        }

        public void Update(SubscriptionDto subscriptionDto)
        {
            var subscription = _mapper.Map<Subscription>(subscriptionDto);
            _subscriptionRepository.Update(subscription);
        }

        public void Delete(int id)
        {
            _subscriptionRepository.Delete(id);
        }

        public async Task<IEnumerable<object>> SubscriptionRevenueDashboard()
        {
            return await _subscriptionRepository.SubscriptionRevenueDashboard();
        }
    }
}
