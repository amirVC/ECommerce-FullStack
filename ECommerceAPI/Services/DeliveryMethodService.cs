
using AutoMapper;
using ECommerceAPI.Data;
using ECommerceAPI.DTOs;
using ECommerceAPI.Exceptions;
using ECommerceAPI.Models;
using ECommerceAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

public class DeliveryMethodService : IDeliveryMethodService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;

    public DeliveryMethodService(AppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<List<DeliveryMethodDto>> GetActiveAsync()
    {
        var methods = await _db.DeliveryMethods
            .Where(m => m.IsActive)
            .OrderBy(m => m.SortOrder)
            .ToListAsync();
        return _mapper.Map<List<DeliveryMethodDto>>(methods);
    }

    public async Task<List<DeliveryMethodDto>> GetAllAsync()
    {
        var methods = await _db.DeliveryMethods.OrderBy(m => m.SortOrder).ToListAsync();
        return _mapper.Map<List<DeliveryMethodDto>>(methods);
    }

    public async Task<DeliveryMethodDto> CreateAsync(CreateDeliveryMethodDto dto)
    {
        var method = _mapper.Map<DeliveryMethod>(dto);
        _db.DeliveryMethods.Add(method);
        await _db.SaveChangesAsync();
        return _mapper.Map<DeliveryMethodDto>(method);
    }

    public async Task<DeliveryMethodDto> UpdateAsync(int id, UpdateDeliveryMethodDto dto)
    {
        var method = await _db.DeliveryMethods.FindAsync(id) ?? throw new NotFoundException("Delivery method not found.");
        _mapper.Map(dto, method);
        await _db.SaveChangesAsync();
        return _mapper.Map<DeliveryMethodDto>(method);
    }

    public async Task DeleteAsync(int id)
    {
        var method = await _db.DeliveryMethods.FindAsync(id) ?? throw new NotFoundException("Delivery method not found.");
        _db.DeliveryMethods.Remove(method);
        await _db.SaveChangesAsync();
    }
}