using ECommerce.Web.DTOs;
using ECommerce.Web.Interfaces;
using ECommerce.Web.Options;
using ECommerce.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
namespace ECommerce.Web.Controllers;
public class CheckoutController : Controller
{
    private readonly ICartService _cartService;
    private readonly IOrderService _orderService;
    private readonly IPaymentService _paymentService;
    private readonly IAuthService _authService;
    private readonly IAddressService _addressService;
    private readonly IDeliveryMethodService _deliveryMethodService;
    private readonly ShippingOptions _shippingOptions;
    private readonly StripeOptions _stripeOptions;
    public CheckoutController(
        ICartService cartService,
        IOrderService orderService,
        IPaymentService paymentService,
        IAuthService authService,
        IAddressService addressService,
        IDeliveryMethodService deliveryMethodService,
        IOptions<ShippingOptions> shippingOptions,
        IOptions<StripeOptions> stripeOptions)
    {
        _cartService = cartService;
        _orderService = orderService;
        _paymentService = paymentService;
        _authService = authService;
        _addressService = addressService;
        _deliveryMethodService = deliveryMethodService;
        _shippingOptions = shippingOptions.Value;
        _stripeOptions = stripeOptions.Value;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var cart = await _cartService.GetCartAsync();
        if (!cart.Items.Any())
            return RedirectToAction("Index", "Cart");
        if (!_authService.IsLoggedIn())
        {
            return RedirectToAction(
                "Login",
                "Account",
                new
                {
                    returnUrl = Url.Action("Index", "Cart")
                });
        }

        var addresses = await _addressService.GetMyAddressesAsync();
        var deliveryMethods = await _deliveryMethodService.GetActiveAsync();
        var defaultAddress = addresses.FirstOrDefault(a => a.IsDefault) ?? addresses.FirstOrDefault();
        // OrderBy(SortOrder) makes this explicit: Standard (SortOrder=1) wins over Express (SortOrder=2)
        var defaultMethod = deliveryMethods.OrderBy(m => m.SortOrder).FirstOrDefault();

        ViewBag.Cart = cart;
        // Without these two, the view's shippingCost/freeShippingThreshold default to 0,
        // which makes the "Free" branch fire on first paint no matter what's actually selected.
        ViewBag.ShippingCost = defaultMethod != null ? CalculateShippingCost(cart, defaultMethod) : 0m;
        ViewBag.FreeShippingThreshold = _shippingOptions.FreeShippingThreshold;

        var model = new CheckoutViewModel
        {
            PaymentMethod = "Cash",
            AddressId = defaultAddress?.Id ?? 0,
            DeliveryMethodId = defaultMethod?.Id ?? 0,
            AvailableAddresses = addresses,
            AvailableDeliveryMethods = deliveryMethods
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Index(CheckoutViewModel model)
    {
        var cart = await _cartService.GetCartAsync();
        if (!cart.Items.Any())
        {
            ModelState.AddModelError("", "Your cart is empty.");
            return View(model);
        }

        var addresses = await _addressService.GetMyAddressesAsync();
        var deliveryMethods = await _deliveryMethodService.GetActiveAsync();

        var selectedAddress = addresses.FirstOrDefault(a => a.Id == model.AddressId);
        var selectedMethod = deliveryMethods.FirstOrDefault(m => m.Id == model.DeliveryMethodId);

        if (selectedAddress == null)
            ModelState.AddModelError(nameof(model.AddressId), "Please select a shipping address.");
        if (selectedMethod == null)
            ModelState.AddModelError(nameof(model.DeliveryMethodId), "Please select a delivery method.");

        if (!ModelState.IsValid)
        {
            ViewBag.Cart = cart;
            ViewBag.ShippingCost = selectedMethod != null ? CalculateShippingCost(cart, selectedMethod) : 0m;
            ViewBag.FreeShippingThreshold = _shippingOptions.FreeShippingThreshold;
            model.AvailableAddresses = addresses;
            model.AvailableDeliveryMethods = deliveryMethods;
            return View(model);
        }

        var dto = new CheckoutDto
        {
            FullName = selectedAddress!.FullName,
            PhoneNumber = selectedAddress.Phone,
            Address = selectedAddress.Line2 != null
                ? $"{selectedAddress.Line1}, {selectedAddress.Line2}"
                : selectedAddress.Line1,
            City = selectedAddress.City,
            PostalCode = selectedAddress.PostalCode,
            Country = selectedAddress.Country,
            Notes = model.Notes,
            PaymentMethod = model.PaymentMethod,
            AddressId = selectedAddress.Id,
            DeliveryMethodId = selectedMethod!.Id,
            // The API has final say: it zeroes this out if a FreeShipping
            // coupon is validly applied, regardless of what's sent here.
            ShippingCost = CalculateShippingCost(cart, selectedMethod),
            Items = cart.Items.Select(c => new OrderItemDto
            {
                ProductId = c.ProductId,
                Quantity = c.Quantity
            }).ToList()
        };

        var order = await _orderService.CheckoutAsync(dto);
        if (order == null)
        {
            ModelState.AddModelError("", "Checkout failed.");
            ViewBag.Cart = cart;
            ViewBag.ShippingCost = CalculateShippingCost(cart, selectedMethod);
            ViewBag.FreeShippingThreshold = _shippingOptions.FreeShippingThreshold;
            model.AvailableAddresses = addresses;
            model.AvailableDeliveryMethods = deliveryMethods;
            return View(model);
        }

        await _cartService.ClearCartAsync();

        if (string.Equals(model.PaymentMethod, "Card", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(Pay), new { orderId = order.Id });
        }

        return RedirectToAction(nameof(Success));
    }

    [HttpGet]
    public async Task<IActionResult> Pay(int orderId)
    {
        if (!_authService.IsLoggedIn())
        {
            return RedirectToAction("Login", "Account");
        }

        var order = await _orderService.GetOrderByIdAsync(orderId);
        if (order == null)
            return NotFound();

        if (string.Equals(order.PaymentStatus, "Succeeded", StringComparison.OrdinalIgnoreCase))
            return RedirectToAction(nameof(Success));
        var intent = await _paymentService.CreatePaymentIntentAsync(orderId);
        if (intent == null)
        {
            TempData["PaymentError"] = "Could not start payment. Please try again or contact support.";
            return RedirectToAction(nameof(Success));
        }

        ViewBag.ClientSecret = intent.ClientSecret;
        ViewBag.PublishableKey = _stripeOptions.PublishableKey;
        ViewBag.OrderId = orderId;
        ViewBag.Amount = intent.Amount;
        ViewBag.Currency = intent.Currency;

        return View();
    }

    public IActionResult Success()
    {
        return View();
    }

    private decimal CalculateShippingCost(CartViewModel cart, DTOs.DeliveryMethodDto method)
        => cart.Total >= _shippingOptions.FreeShippingThreshold ? 0m : method.Price;
}