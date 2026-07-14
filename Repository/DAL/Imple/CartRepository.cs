using Dapper;
using DTO.Models.Master.Cart;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using OFMS_API.DAL.Interface;
using System.Data;

namespace OFMS_API.DAL.Imple
{
    /// <summary>
    /// Dapper-based implementation of ICartRepository.
    /// Uses inline parameterized SQL — no stored procedures.
    /// </summary>
    public class CartRepository : ICartRepository
    {
        private readonly string _connStr;

        public CartRepository(IConfiguration configuration)
        {
            _connStr = configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        // ── Helpers ────────────────────────────────────────────────
        private SqlConnection CreateConnection() => new(_connStr);

        // ── Write operations ───────────────────────────────────────

        public async Task<int> AddToCartAsync(
            AddCartRequest request, int customerId, decimal unitPrice, int createdBy)
        {
            using var conn = CreateConnection();
            const string sql = @"
                INSERT INTO tblCart
                    (CustomerId, IdItemMaster, Quantity, UnitPrice, TotalPrice,
                     Remarks, IsActive, CreatedOn, CreatedBy)
                VALUES
                    (@CustomerId, @IdItemMaster, @Quantity, @UnitPrice, @TotalPrice,
                     @Remarks, 1, GETDATE(), @CreatedBy);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            return await conn.ExecuteScalarAsync<int>(sql, new
            {
                CustomerId   = customerId,
                request.IdItemMaster,
                request.Quantity,
                UnitPrice    = unitPrice,
                TotalPrice   = request.Quantity * unitPrice,
                request.Remarks,
                CreatedBy    = createdBy
            });
        }

        public async Task UpdateCartAsync(UpdateCartRequest request, decimal unitPrice, int updatedBy)
        {
            using var conn = CreateConnection();
            const string sql = @"
                UPDATE tblCart
                SET
                    Quantity  = @Quantity,
                    TotalPrice = @TotalPrice,
                    UpdatedOn = GETDATE(),
                    UpdatedBy = @UpdatedBy
                WHERE IdCart = @IdCart
                  AND IsActive = 1;";

            await conn.ExecuteAsync(sql, new
            {
                request.IdCart,
                request.Quantity,
                TotalPrice = request.Quantity * unitPrice,
                UpdatedBy  = updatedBy
            });
        }

        public async Task RemoveCartItemAsync(int idCart, int customerId)
        {
            using var conn = CreateConnection();
            const string sql = @"
                UPDATE tblCart
                SET IsActive  = 0,
                    UpdatedOn = GETDATE()
                WHERE IdCart     = @IdCart
                  AND CustomerId = @CustomerId
                  AND IsActive   = 1;";

            await conn.ExecuteAsync(sql, new { IdCart = idCart, CustomerId = customerId });
        }

        public async Task ClearCartAsync(int customerId)
        {
            using var conn = CreateConnection();
            const string sql = @"
                UPDATE tblCart
                SET IsActive  = 0,
                    UpdatedOn = GETDATE()
                WHERE CustomerId = @CustomerId
                  AND IsActive   = 1;";

            await conn.ExecuteAsync(sql, new { CustomerId = customerId });
        }

        public async Task ClearCartWithConnectionAsync(int customerId, SqlConnection conn, SqlTransaction tran)
        {
            const string sql = @"
                UPDATE tblCart
                SET IsActive  = 0,
                    UpdatedOn = GETDATE()
                WHERE CustomerId = @CustomerId
                  AND IsActive   = 1;";

            await conn.ExecuteAsync(sql, new { CustomerId = customerId }, transaction: tran);
        }

        public async Task<int> IncreaseQuantityAsync(int idCart, int addedQuantity, decimal unitPrice, int updatedBy)
        {
            using var conn = CreateConnection();
            const string sql = @"
                UPDATE tblCart
                SET
                    Quantity   = Quantity + @Added,
                    TotalPrice = (Quantity + @Added) * @UnitPrice,
                    UpdatedOn  = GETDATE(),
                    UpdatedBy  = @UpdatedBy
                OUTPUT INSERTED.Quantity
                WHERE IdCart   = @IdCart
                  AND IsActive = 1;";

            return await conn.ExecuteScalarAsync<int>(sql, new
            {
                IdCart     = idCart,
                Added      = addedQuantity,
                UnitPrice  = unitPrice,
                UpdatedBy  = updatedBy
            });
        }

        // ── Read operations ────────────────────────────────────────

        public async Task<List<CartItemResponse>> GetCartByCustomerAsync(int customerId)
        {
            using var conn = CreateConnection();
            const string sql = @"
                SELECT
                    C.IdCart,
                    C.IdItemMaster,
                    IM.ItemName,
                    (SELECT TOP 1 ImageUrl
                     FROM tblItemMasterImage
                     WHERE ReferenceId = C.IdItemMaster
                       AND ImageTypeId = 4   -- Item image type
                     ORDER BY IdItemMasterImage) AS ItemImage,
                    C.Quantity,
                    C.UnitPrice,
                    C.TotalPrice,
                    C.Remarks
                FROM tblCart C
                INNER JOIN tblItemMaster IM ON IM.IdItemMaster = C.IdItemMaster
                WHERE C.CustomerId = @CustomerId
                  AND C.IsActive   = 1
                ORDER BY C.CreatedOn DESC;";

            var rows = await conn.QueryAsync<CartItemResponse>(sql, new { CustomerId = customerId });
            return rows.AsList();
        }

        public async Task<int> GetCartCountAsync(int customerId)
        {
            using var conn = CreateConnection();
            const string sql = @"
                SELECT COUNT(1)
                FROM tblCart
                WHERE CustomerId = @CustomerId
                  AND IsActive   = 1;";

            return await conn.ExecuteScalarAsync<int>(sql, new { CustomerId = customerId });
        }

        public async Task<int> IsItemAlreadyExistsAsync(int customerId, int idItemMaster)
        {
            using var conn = CreateConnection();
            const string sql = @"
                SELECT ISNULL(
                    (SELECT TOP 1 IdCart
                     FROM tblCart
                     WHERE CustomerId  = @CustomerId
                       AND IdItemMaster = @IdItemMaster
                       AND IsActive    = 1), 0);";

            return await conn.ExecuteScalarAsync<int>(sql, new
            {
                CustomerId   = customerId,
                IdItemMaster = idItemMaster
            });
        }

        public async Task<CartSummaryResponse> CalculateCartSummaryAsync(int customerId)
        {
            // Reuse the item list query so we don't round-trip twice
            var items = await GetCartByCustomerAsync(customerId);

            return new CartSummaryResponse
            {
                Items      = items,
                TotalItems = items.Sum(i => i.Quantity),
                SubTotal   = items.Sum(i => i.TotalPrice),
                GrandTotal = items.Sum(i => i.TotalPrice)  // extend here for tax/delivery
            };
        }
    }
}
