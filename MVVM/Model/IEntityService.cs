using ZabgcExamsDesktop.API;
using ZabgcExamsDesktop.API.Models;

namespace ZabgcExamsDesktop.MVVM.Model
{
    public interface IEntityService<T>
    {
        Task<List<T>> GetAllAsync();
        Task<bool> CreateAsync(T item);
        Task<bool> UpdateAsync(T item);
        Task<bool> DeleteAsync(int id);
        T CreateNewItem();
        string GetEntityName();
        string GetItemName(T item);
    }

    public class DepartmentService : IEntityService<DepartmentDto>
    {
        private readonly ApiService _apiService;
        public DepartmentService(ApiService apiService) => _apiService = apiService;

        public DepartmentDto CreateNewItem() => new DepartmentDto { IsNew = true, IsEditing = true };
        public Task<List<DepartmentDto>> GetAllAsync() => _apiService.GetDepartmentsAsync();
        public Task<bool> CreateAsync(DepartmentDto item) => _apiService.CreateDepartmentAsync(item);
        public Task<bool> UpdateAsync(DepartmentDto item) => _apiService.UpdateDepartmentAsync(item);
        public Task<bool> DeleteAsync(int id) => _apiService.DeleteDepartmentAsync(id);
        public string GetEntityName() => "кафедру";
        public string GetItemName(DepartmentDto item) => item.NameOfDepartment;
    }
    public class GroupService : IEntityService<GroupDto>
    {
        private readonly ApiService _apiService;
        public GroupService(ApiService apiService) => _apiService = apiService;
        public GroupDto CreateNewItem() => new GroupDto { IsNew = true, IsEditing = true };
        public Task<List<GroupDto>> GetAllAsync() => _apiService.GetGroupsAsync();
        public Task<bool> CreateAsync(GroupDto item) => _apiService.CreateGroupAsync(item);
        public Task<bool> UpdateAsync(GroupDto item) => _apiService.UpdateGroupAsync(item);
        public Task<bool> DeleteAsync(int id) => _apiService.DeleteGroupAsync(id);
        public string GetEntityName() => "группу";
        public string GetItemName(GroupDto item) => item.NameOfGroup;
    }
    public class TeacherService : IEntityService<TeacherDto>
    {
        private readonly ApiService _apiService;
        public TeacherService(ApiService apiService) => _apiService = apiService;
        public TeacherDto CreateNewItem() => new TeacherDto { IsNew = true, IsEditing = true };
        public Task<List<TeacherDto>> GetAllAsync() => _apiService.GetTeachersAsync();
        public Task<bool> CreateAsync(TeacherDto item) => _apiService.CreateTeacherAsync(item);
        public Task<bool> UpdateAsync(TeacherDto item) => _apiService.UpdateTeacherAsync(item);
        public Task<bool> DeleteAsync(int id) => _apiService.DeleteTeacherAsync(id);
        public string GetEntityName() => "преподавателя";
        public string GetItemName(TeacherDto item) => item.FullName;
    }
    public class DepartmentOwnerService : IEntityService<DepartmentOwnerDto>
    {
        private readonly ApiService _apiService;
        public DepartmentOwnerService(ApiService apiService) => _apiService = apiService;
        public DepartmentOwnerDto CreateNewItem() => new DepartmentOwnerDto { IsNew = true, IsEditing = true };
        public Task<List<DepartmentOwnerDto>> GetAllAsync() => _apiService.GetDepartmentOwnersAsync();
        public Task<bool> CreateAsync(DepartmentOwnerDto item) => _apiService.CreateDepartmentOwnerAsync(item);
        public Task<bool> UpdateAsync(DepartmentOwnerDto item) => _apiService.UpdateDepartmentOwnerAsync(item);
        public Task<bool> DeleteAsync(int id) => _apiService.DeleteDepartmentOwnerAsync(id);
        public string GetEntityName() => "зав. отделения";
        public string GetItemName(DepartmentOwnerDto item) => item.OwnerName;
    }
    public class ManagerService : IEntityService<ManagerDto>
    {
        private readonly ApiService _apiService;
        public ManagerService(ApiService apiService) => _apiService = apiService;
        public ManagerDto CreateNewItem() => new ManagerDto { IsNew = true, IsEditing = true };
        public Task<List<ManagerDto>> GetAllAsync() => _apiService.GetManagersAsync();
        public Task<bool> CreateAsync(ManagerDto item) => _apiService.CreateManagerAsync(item);
        public Task<bool> UpdateAsync(ManagerDto item) => _apiService.UpdateManagerAsync(item);
        public Task<bool> DeleteAsync(int id) => _apiService.DeleteManagerAsync(id);
        public string GetEntityName() => "менеджера";
        public string GetItemName(ManagerDto item) => item.FullName;
    }
    public class AudienceService : IEntityService<AudienceDto>
    {
        private readonly ApiService _apiService;
        public AudienceService(ApiService apiService) => _apiService = apiService;
        public AudienceDto CreateNewItem() => new AudienceDto { IsNew = true, IsEditing = true };
        public Task<List<AudienceDto>> GetAllAsync() => _apiService.GetAudiencesAsync();
        public Task<bool> CreateAsync(AudienceDto item) => _apiService.CreateAudienceAsync(item);
        public Task<bool> UpdateAsync(AudienceDto item) => _apiService.UpdateAudienceAsync(item);
        public Task<bool> DeleteAsync(int id) => _apiService.DeleteAudienceAsync(id);
        public string GetEntityName() => "аудитории";
        public string GetItemName(AudienceDto item) => item.NumberAudience.ToString();
    }
    public class DisciplineService : IEntityService<DisciplineDto>
    {
        private readonly ApiService _apiService;
        public DisciplineService(ApiService apiService) => _apiService = apiService;
        public DisciplineDto CreateNewItem() => new DisciplineDto { IsNew = true, IsEditing = true };
        public Task<List<DisciplineDto>> GetAllAsync() => _apiService.GetDisciplinesAsync();
        public Task<bool> CreateAsync(DisciplineDto item) => _apiService.CreateDisciplineAsync(item);
        public Task<bool> UpdateAsync(DisciplineDto item) => _apiService.UpdateDisciplineAsync(item);
        public Task<bool> DeleteAsync(int id) => _apiService.DeleteDisciplineAsync(id);
        public string GetEntityName() => "дисциплину";
        public string GetItemName(DisciplineDto item) => item.NameDiscipline;
    }
}
