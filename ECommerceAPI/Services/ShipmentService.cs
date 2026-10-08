using AutoMapper;
using ECommerceAPI.Data;
using ECommerceAPI.DTOs;
using ECommerceAPI.Exceptions;
using ECommerceAPI.Models;
using ECommerceAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.Services
{

    public class ShipmentService : IShipmentService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public ShipmentService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<ShipmentDto?> GetByOrderIdAsync(int orderId, int userId, bool isAdmin)
        {
            var order = await _db.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == orderId)
                ?? throw new NotFoundException("Order not found.");

            if (!isAdmin && order.UserId != userId)
                throw new UnauthorizedException("You do not have access to this order.");

            var shipment = await _db.Shipments
                .AsNoTracking()
                .Include(s => s.StatusHistory.OrderBy(h => h.ChangedAt))
                .FirstOrDefaultAsync(s => s.OrderId == orderId);

            return shipment == null ? null : _mapper.Map<ShipmentDto>(shipment);
        }

        public async Task<ShipmentDto> CreateForOrderAsync(int orderId, CreateShipmentDto dto)
        {
            var orderExists = await _db.Orders.AnyAsync(o => o.Id == orderId);
            if (!orderExists)
                throw new NotFoundException("Order not found.");

            var shipmentAlreadyExists = await _db.Shipments.AnyAsync(s => s.OrderId == orderId);
            if (shipmentAlreadyExists)
                throw new BadRequestException("Shipment already exists for this order.");

            var shipment = new Shipment
            {
                OrderId = orderId,
                Carrier = dto.Carrier,
                TrackingNumber = dto.TrackingNumber,
                Status = ShipmentStatus.Processing
            };
            shipment.StatusHistory.Add(new ShipmentStatusHistory { Status = ShipmentStatus.Processing, ChangedAt = DateTime.UtcNow });

            _db.Shipments.Add(shipment);
            await _db.SaveChangesAsync();
            return _mapper.Map<ShipmentDto>(shipment);
        }

        public async Task<ShipmentDto> UpdateStatusAsync(int orderId, UpdateShipmentStatusDto dto)
        {
            var shipment = await _db.Shipments
                .Include(s => s.StatusHistory)
                .FirstOrDefaultAsync(s => s.OrderId == orderId)
                ?? throw new NotFoundException("Shipment not found for this order.");

            if (!Enum.TryParse<ShipmentStatus>(dto.Status, true, out var newStatus))
                throw new BadRequestException("Invalid shipment status.");

            shipment.Status = newStatus;
            shipment.UpdatedAt = DateTime.UtcNow;
            if (dto.Carrier != null) shipment.Carrier = dto.Carrier;
            if (dto.TrackingNumber != null) shipment.TrackingNumber = dto.TrackingNumber;

            if (newStatus == ShipmentStatus.Shipped && shipment.ShippedAt == null)
                shipment.ShippedAt = DateTime.UtcNow;
            if (newStatus == ShipmentStatus.Delivered && shipment.DeliveredAt == null)
                shipment.DeliveredAt = DateTime.UtcNow;

            shipment.StatusHistory.Add(new ShipmentStatusHistory
            {
                Status = newStatus,
                Note = dto.Note,
                ChangedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            return _mapper.Map<ShipmentDto>(shipment);
        }
    }
}