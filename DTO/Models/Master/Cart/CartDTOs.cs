using System;
using System.Collections.Generic;

namespace DTO.Models.Master.Cart
{
    // ─── Request DTOs ──────────────────────────────────────────────
    public class AddCartRequest
    {
        /// <summary>Item to add to the cart.</summary>
        public int IdItemMaster { get; set; }

        /// <summary>How many units to add (must be > 0).</summary>
        public int Quantity { get; set; }

        /// <summary>Optional notes for this cart line.</summary>
        public string? Remarks { get; set; }
    }

    public class UpdateCartRequest
    {
        /// <summary>Primary key of the cart row to update.</summary>
        public int IdCart { get; set; }

        /// <summary>New quantity (0 ⟹ remove the row automatically).</summary>
        public int Quantity { get; set; }
    }

    // ─── Response DTOs ─────────────────────────────────────────────
    public class CartItemResponse
    {
        public int IdCart { get; set; }
        public int IdItemMaster { get; set; }
        public string? ItemName { get; set; }
        public string? ItemImage { get; set; }   // full URL returned by API
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string? Remarks { get; set; }
    }

    public class CartSummaryResponse
    {
        public List<CartItemResponse> Items { get; set; } = [];
        public int TotalItems { get; set; }
        public decimal SubTotal { get; set; }
        public decimal GrandTotal { get; set; }
    }
}
