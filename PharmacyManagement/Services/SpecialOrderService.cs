using AutoMapper;
using PharmacyManagement.DTO;
using PharmacyManagement.Interface;
using PharmacyManagement.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PharmacyManagement.Services
{
    public class SpecialOrderService : ISpecialOrderService
    {
        private readonly ISpecialOrderRepository _repository;
        private readonly IMapper _mapper;

        public SpecialOrderService(ISpecialOrderRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<SpecialOrderDto?> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            return _mapper.Map<SpecialOrderDto>(entity);
        }

        public async Task<IEnumerable<SpecialOrderDto>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<SpecialOrderDto>>(entities);
        }

        public async Task<IEnumerable<SpecialOrderDto>> GetByDoctorIdAsync(string doctorId)
        {
            var entities = await _repository.GetByDoctorIdAsync(doctorId);
            return _mapper.Map<IEnumerable<SpecialOrderDto>>(entities);
        }

        public async Task<SpecialOrderDto> CreateAsync(CreateSpecialOrderDto dto, string userId)
        {
            var model = _mapper.Map<SpecialOrder>(dto);
            model.DateRequested = System.DateTime.UtcNow;
            model.Status = "Pending";
            model.DoctorId = userId; // Set DoctorId from authenticated user

            var created = await _repository.CreateAsync(model);
            return _mapper.Map<SpecialOrderDto>(created);
        }

        public async Task<bool> UpdateStatusAsync(int id, UpdateSpecialOrderStatusDto dto)
        {
            return await _repository.UpdateStatusAsync(id, dto.Status, dto.AdminNotes);
        }
    }
}