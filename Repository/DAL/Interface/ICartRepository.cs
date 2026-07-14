using DTO.Models.Master.Cart;

namespace OFMS_API.DAL.Interface
{
    /// <summary>
    /// Data-access contract for the Cart module.
    /// All methods are customer-scoped; the caller is responsible for
    /// passing the correct customerId extracted from the JWT token.
    /// </summary>
    public interface ICartRepository
    {
        /// <summary>Insert a new cart row. Returns the new IdCart.</summary>
        Task<int> AddToCartAsync(AddCartRequest request, int customerId, decimal unitPrice, int createdBy);

        /// <summary>Update quantity and recalculate TotalPrice for an existing row.</summary>
        Task UpdateCartAsync(UpdateCartRequest request, decimal unitPrice, int updatedBy);

        /// <summary>Soft-delete (set IsActive = 0) one cart item belonging to this customer.</summary>
        Task RemoveCartItemAsync(int idCart, int customerId);

        /// <summary>Soft-delete ALL active cart items for this customer.</summary>
        Task ClearCartAsync(int customerId);

        /// <summary>
        /// Clears the cart inside an existing open transaction (called from OrderBL).
        /// </summary>
        Task ClearCartWithConnectionAsync(int customerId, Microsoft.Data.SqlClient.SqlConnection conn, Microsoft.Data.SqlClient.SqlTransaction tran);

        /// <summary>Return full cart details (joined with item master) for one customer.</summary>
        Task<List<CartItemResponse>> GetCartByCustomerAsync(int customerId);

        /// <summary>Return total number of active cart items for the badge.</summary>
        Task<int> GetCartCountAsync(int customerId);

        /// <summary>
        /// Returns the IdCart if the item already exists in the cart, 0 otherwise.
        /// </summary>
        Task<int> IsItemAlreadyExistsAsync(int customerId, int idItemMaster);

        /// <summary>Add extra quantity to an existing cart row; returns new quantity.</summary>
        Task<int> IncreaseQuantityAsync(int idCart, int addedQuantity, decimal unitPrice, int updatedBy);

        /// <summary>Compute totals across all active cart items for this customer.</summary>
        Task<CartSummaryResponse> CalculateCartSummaryAsync(int customerId);
    }
}
