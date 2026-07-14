using DTO.Models.Master.Cart;

namespace OFMS_API.BL.Interface
{
    /// <summary>
    /// Business-logic contract for the Cart module.
    /// Every method receives customerId from the authenticated JWT token —
    /// it must never be supplied by the frontend.
    /// </summary>
    public interface ICartBL
    {
        /// <summary>Add an item to the cart, or increase quantity if already present.</summary>
        Task<(bool Success, string Message)> AddToCartAsync(AddCartRequest request, int customerId);

        /// <summary>Update the quantity of an existing cart row. Removes the row if qty == 0.</summary>
        Task<(bool Success, string Message)> UpdateCartAsync(UpdateCartRequest request, int customerId);

        /// <summary>Remove a single cart item owned by this customer.</summary>
        Task<(bool Success, string Message)> RemoveCartItemAsync(int idCart, int customerId);

        /// <summary>Remove all items from this customer's cart.</summary>
        Task<(bool Success, string Message)> ClearCartAsync(int customerId);

        /// <summary>Return the full cart summary (items + totals) for this customer.</summary>
        Task<CartSummaryResponse> GetCartAsync(int customerId);

        /// <summary>Return the count of active cart items (for the navbar badge).</summary>
        Task<int> GetCartCountAsync(int customerId);
    }
}
