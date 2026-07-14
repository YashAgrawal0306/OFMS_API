using DTO.Models.Master.Cart;
using OFMS_API.BL.Interface;
using OFMS_API.DAL.Interface;
using Repository.DAL.Interface.Master.ItemMaster;

namespace OFMS_API.BL.Imple
{
    /// <summary>
    /// Business logic for the Cart module.
    /// Validates customers, items, and quantities before delegating to the repository.
    /// UnitPrice is ALWAYS fetched from ItemMaster — never accepted from the frontend.
    /// </summary>
    public class CartBL : ICartBL
    {
        private readonly ICartRepository _cartRepo;
        private readonly IItemMasterDAL  _itemMasterDAL;
        private readonly IuserDAL        _userDAL;

        public CartBL(
            ICartRepository  cartRepo,
            IItemMasterDAL   itemMasterDAL,
            IuserDAL         userDAL)
        {
            _cartRepo      = cartRepo;
            _itemMasterDAL = itemMasterDAL;
            _userDAL       = userDAL;
        }

        // ── Add to Cart ────────────────────────────────────────────
        public async Task<(bool Success, string Message)> AddToCartAsync(
            AddCartRequest request, int customerId)
        {
            // 1. Quantity validation
            if (request.Quantity <= 0)
                return (false, "Quantity must be greater than zero.");

            // 2. Item validation
            var item = await _itemMasterDAL.GetItemMasterById(request.IdItemMaster);
            if (item == null || item.IdItemMaster == 0)
                return (false, "Item not found.");

            if (!item.IsActive)
                return (false, "Item is not available for ordering.");

            decimal unitPrice = item.Price;

            // 3. Duplicate check: if already in cart, increase quantity
            int existingIdCart = await _cartRepo.IsItemAlreadyExistsAsync(customerId, request.IdItemMaster);
            if (existingIdCart > 0)
            {
                await _cartRepo.IncreaseQuantityAsync(existingIdCart, request.Quantity, unitPrice, customerId);
                return (true, $"Quantity updated for '{item.ItemName}' in your cart.");
            }

            // 4. Add new row
            await _cartRepo.AddToCartAsync(request, customerId, unitPrice, customerId);
            return (true, $"'{item.ItemName}' added to your cart successfully.");
        }

        // ── Update Cart ────────────────────────────────────────────
        public async Task<(bool Success, string Message)> UpdateCartAsync(
            UpdateCartRequest request, int customerId)
        {
            // If quantity becomes 0, remove the item automatically
            if (request.Quantity == 0)
            {
                await _cartRepo.RemoveCartItemAsync(request.IdCart, customerId);
                return (true, "Item removed from cart.");
            }

            if (request.Quantity < 0)
                return (false, "Quantity cannot be negative.");

            // We need the current unit price from the item
            // First, get the current cart item to find IdItemMaster
            var cart = await _cartRepo.GetCartByCustomerAsync(customerId);
            var cartItem = cart.FirstOrDefault(x => x.IdCart == request.IdCart);
            if (cartItem == null)
                return (false, "Cart item not found.");

            var item = await _itemMasterDAL.GetItemMasterById(cartItem.IdItemMaster);
            if (item == null || !item.IsActive)
                return (false, "Item is no longer available.");

            await _cartRepo.UpdateCartAsync(request, item.Price, customerId);
            return (true, "Cart updated successfully.");
        }

        // ── Remove single item ─────────────────────────────────────
        public async Task<(bool Success, string Message)> RemoveCartItemAsync(
            int idCart, int customerId)
        {
            await _cartRepo.RemoveCartItemAsync(idCart, customerId);
            return (true, "Item removed from cart.");
        }

        // ── Clear entire cart ──────────────────────────────────────
        public async Task<(bool Success, string Message)> ClearCartAsync(int customerId)
        {
            await _cartRepo.ClearCartAsync(customerId);
            return (true, "Cart cleared successfully.");
        }

        // ── Get full cart ──────────────────────────────────────────
        public async Task<CartSummaryResponse> GetCartAsync(int customerId)
        {
            return await _cartRepo.CalculateCartSummaryAsync(customerId);
        }

        // ── Count for badge ────────────────────────────────────────
        public async Task<int> GetCartCountAsync(int customerId)
        {
            return await _cartRepo.GetCartCountAsync(customerId);
        }
    }
}
