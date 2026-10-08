using AutoMapper;
using ECommerceAPI.Data;
using ECommerceAPI.DTOs;
using ECommerceAPI.Exceptions;
using ECommerceAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using ECommerceAPI.Models;

namespace ECommerceAPI.Services
{
    public class AddressService : IAddressService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public AddressService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<List<AddressDto>> GetMyAddressesAsync(int userId)
        {
            var addresses = await _db.Addresses
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.IsDefault)
                .ThenByDescending(a => a.CreatedAt)
                .ToListAsync();
            return _mapper.Map<List<AddressDto>>(addresses);
        }

        public async Task<AddressDto> CreateAsync(int userId, CreateAddressDto dto)
        {
            var address = _mapper.Map<Address>(dto);
            address.UserId = userId;

            if (dto.IsDefault || !await _db.Addresses.AnyAsync(a => a.UserId == userId))
            {
                await ClearDefaultAsync(userId);
                address.IsDefault = true;
            }

            _db.Addresses.Add(address);
            await _db.SaveChangesAsync();
            return _mapper.Map<AddressDto>(address);
        }

        public async Task<AddressDto> UpdateAsync(int userId, int addressId, UpdateAddressDto dto)
        {
            var address = await _db.Addresses.FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId)
                ?? throw new NotFoundException("Address not found.");

            _mapper.Map(dto, address);

            if (dto.IsDefault)
            {
                await ClearDefaultAsync(userId);
                address.IsDefault = true;
            }

            await _db.SaveChangesAsync();
            return _mapper.Map<AddressDto>(address);
        }

        public async Task DeleteAsync(int userId, int addressId)
        {
            var address = await _db.Addresses.FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId)
                ?? throw new NotFoundException("Address not found.");

            _db.Addresses.Remove(address);
            await _db.SaveChangesAsync();

            if (address.IsDefault)
            {
                var next = await _db.Addresses.Where(a => a.UserId == userId).OrderByDescending(a => a.CreatedAt).FirstOrDefaultAsync();
                if (next != null)
                {
                    next.IsDefault = true;
                    await _db.SaveChangesAsync();
                }
            }
        }

        public async Task SetDefaultAsync(int userId, int addressId)
        {
            var address = await _db.Addresses.FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId)
                ?? throw new NotFoundException("Address not found.");

            await ClearDefaultAsync(userId);
            address.IsDefault = true;
            await _db.SaveChangesAsync();
        }

        private async Task ClearDefaultAsync(int userId)
        {
            var current = await _db.Addresses.Where(a => a.UserId == userId && a.IsDefault).ToListAsync();
            foreach (var a in current) a.IsDefault = false;
        }
    }
}
