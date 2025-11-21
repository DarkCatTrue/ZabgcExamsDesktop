using ZabgcExamsDesktop.API;
using ZabgcExamsDesktop.API.Models;

namespace ZabgcExamsDesktop.MVVM.Model
{
    public class EntityServiceFactory
    {
        private readonly Dictionary<Type, object> _services;

        public EntityServiceFactory(ApiService apiService)
        {
            _services = new Dictionary<Type, object>
            {
                [typeof(DepartmentDto)] = new DepartmentService(apiService),
                [typeof(GroupDto)] = new GroupService(apiService),
                [typeof(TeacherDto)] = new TeacherService(apiService),
                [typeof(AudienceDto)] = new AudienceService(apiService),
                [typeof(DisciplineDto)] = new DisciplineService(apiService)
            };
        }

        public IEntityService<T> GetService<T>() => (IEntityService<T>)_services[typeof(T)];
    }
}
