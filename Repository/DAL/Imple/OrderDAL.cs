using Dapper;
using DTO.Models.CommonModel;
using DTO.Models.Master.AddressMaster;
using DTO.Models.Master.OrderMaster;
using Helper.Helper.Common;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using OFMS_API.DAL.Interface;
using OFMS_API.Helper.Hub;
using OFMS_API.Helper.Hub.Service;
using OFMS_API.Models;
using OFMS_API.Models.DTO;

namespace OFMS_API.DAL.Imple
{
    public class OrderDAL : IOrderDAL
    {
        private readonly string connq;
        public OrderDAL(IConfiguration configuration)
        {
            connq = configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        public async Task<int> AddOrderMaster(OrderMasterTO order, SqlConnection conn, SqlTransaction tran)
        { 
            try
            {
                const string insertHeaderSql = @"
                            INSERT INTO tblOrderMaster
                            (
                                OrderNo, CustomerId, IdStatus, SubTotal, TaxAmount,
                                DeliveryCharge, DiscountAmount, GrandTotal, Remarks,idAddressMapping,
                                IsActive, CreatedOn, CreatedBy
                            )
                            VALUES
                            (
                                @OrderNo, @CustomerId, @IdStatus, @SubTotal, @TaxAmount,
                                @DeliveryCharge, @DiscountAmount, @GrandTotal, @Remarks,@idAddressMapping,
                                1, GETDATE(), @CreatedBy
                            );
                            SELECT CAST(SCOPE_IDENTITY() AS INT);";

                int newId = conn.QuerySingle<int>(insertHeaderSql, new
                {
                    order.OrderNo,
                    order.CustomerId,
                    order.IdStatus,
                    order.SubTotal,
                    order.TaxAmount,
                    order.DeliveryCharge,
                    order.DiscountAmount,
                    order.GrandTotal,
                    order.Remarks,
                    order.IdAddressMapping,
                    order.CreatedBy
                }, transaction: tran);

                if (order.OrderItems != null && order.OrderItems.Any())
                {
                    const string insertDetailSql = @"
                                INSERT INTO tblOrderDetails
                                (
                                    IdOrderMaster, IdItemMaster, Quantity, UnitPrice, TotalPrice, CreatedOn
                                )
                                VALUES
                                (
                                    @IdOrderMaster, @IdItemMaster, @Quantity, @UnitPrice, @TotalPrice, GETDATE()
                                );";

                    var detailParams = order.OrderItems.Select(item => new
                    {
                        IdOrderMaster = newId,
                        item.IdItemMaster,
                        item.Quantity,
                        item.UnitPrice,
                        item.TotalPrice
                    });

                    await conn.ExecuteAsync(insertDetailSql, detailParams, transaction: tran);
                }

                return newId;
            }
            catch
            {
                throw;
            }
        }
        public async Task<int> AddPaymentData(TblPaymentTO tblPaymentTO, SqlConnection conn, SqlTransaction tran)
        {
            const string sql = @"
                                INSERT INTO tblPayment
                                (
                                    IdOrderMaster,
                                    Amount,
                                    PaymentMethod,
                                    TransactionNo,
                                    TransactionTypeId,
                                    IdStatus,
                                    IsActive,
                                    CreatedOn,
                                    CreatedBy
                                )
                                VALUES
                                (
                                    @IdOrderMaster,
                                    @Amount,
                                    @PaymentMethod,
                                    @TransactionNo,
                                    @TransactionTypeId,
                                    @IdStatus,
                                    1,
                                    GETDATE(),
                                    @CreatedBy
                                );

                                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            int paymentId = await conn.QuerySingleAsync<int>(
                sql,
                new
                {
                    tblPaymentTO.IdOrderMaster,
                    tblPaymentTO.Amount,
                    tblPaymentTO.PaymentMethod,
                    tblPaymentTO.TransactionNo,
                    tblPaymentTO.TransactionTypeId,
                    tblPaymentTO.IdStatus,
                    tblPaymentTO.CreatedBy
                },
                transaction: tran);

            return paymentId;
        }

        public async Task<OutPutClass<OrderListResponseTO>> GetOrderMasterList(OrderListFilter filter)
        {
            try
            {
                using var conn = new SqlConnection(connq);

                int pageNo = filter.PageNo ?? 1;
                int pageSize = filter.PageSize ?? 10;
                int offset = (pageNo - 1) * pageSize;

                string sortColumn = string.IsNullOrWhiteSpace(filter.SortColumn)
                    ? "OM.CreatedOn"
                    : filter.SortColumn;

                string sortOrder = string.IsNullOrWhiteSpace(filter.SortOrder)
                    ? "DESC"
                    : filter.SortOrder;
                DateTime fromdate = DateTime.Now;
                DateTime toDate = DateTime.Now;
                if ((filter.Fromdate != "" && filter.Fromdate != null) && (filter.Todate != "" && filter.Todate != null))
                {
                    fromdate = Convert.ToDateTime(filter.Fromdate);
                    toDate = Convert.ToDateTime(filter.Todate).AddDays(1).AddSeconds(-1);
                }
                List<int> orderStatus = Enum.GetValues(typeof(Enums.OrderStatus)).Cast<Enums.OrderStatus>()
                                    .Select(x => (int)x)
                                    .ToList();
                if (filter.OrderStatus != null && filter.OrderStatus != "")
                {
                    orderStatus.Clear();
                    orderStatus.Add(Convert.ToInt32(filter.OrderStatus));
                }
                var output = new OutPutClass<OrderListResponseTO>();

                string countQuery = @"
        SELECT COUNT(1)
        FROM tblOrderMaster OM
        INNER JOIN tblUser U ON U.userid = OM.CustomerId
        WHERE
            (@OrderNo IS NULL OR OM.OrderNo LIKE '%' + @OrderNo + '%')
            AND (@CustomerName IS NULL OR U.UserName LIKE '%' + @CustomerName + '%')
            AND (@IsActive IS NULL OR OM.IsActive = @IsActive);";

                int totalRecords = await conn.ExecuteScalarAsync<int>(
                    countQuery,
                    new
                    {
                        filter.OrderNo,
                        filter.CustomerName,
                        IsActive = filter.isActive
                    });

                string orderQuery = $@"
        SELECT
            OM.*,
            U.UserName AS CustomerName,
            DS.StatusName
        FROM tblOrderMaster OM
        INNER JOIN tblUser U ON U.userid = OM.CustomerId
        LEFT JOIN dimStatus DS ON DS.IdStatus = OM.IdStatus
        WHERE
            (@OrderNo IS NULL OR OM.OrderNo LIKE '%' + @OrderNo + '%')
            AND (OM.IdStatus in @orderStatus)
            AND (@CustomerName IS NULL OR U.UserName LIKE '%' + @CustomerName + '%')
            AND (@IsActive IS NULL OR OM.IsActive = @IsActive) AND OM.CreatedOn BETWEEN @FromDate AND @ToDate
        ORDER BY {sortColumn} {sortOrder}
        OFFSET @Offset ROWS
        FETCH NEXT @PageSize ROWS ONLY";

                var orders = (await conn.QueryAsync<OrderListResponseTO>(
                    orderQuery,
                    new
                    {
                        filter.OrderNo,
                        filter.CustomerName,
                        IsActive = filter.isActive,
                        fromdate = fromdate,
                        toDate = toDate,
                        orderStatus = orderStatus,
                        Offset = offset,
                        PageSize = pageSize
                    })).ToList();

                if (!orders.Any())
                {
                    output.List = new List<OrderListResponseTO>();
                    output.TotalCount = 0;
                    return output;
                }

                var orderIds = orders.Select(x => x.IdOrderMaster).ToList();

                //---------------------------------------------------------
                // Payment Details
                //---------------------------------------------------------

                string paymentSql = @"
         SELECT *,dimStatus.StatusName,dimStatus.Description
  FROM tblPayment tblPayment LEFT JOIN dimStatus ON tblPayment.IdStatus = dimStatus.IdStatus
        WHERE IdOrderMaster IN @OrderIds";

                var payments = (await conn.QueryAsync<TblPaymentResponseTO>(
                    paymentSql,
                    new { OrderIds = orderIds }))
                    .ToList();





                //---------------------------------------------------------
                // Delivery Assignment
                //---------------------------------------------------------

                string deliverySql = @"
        SELECT
            DA.*,
            U.UserName AS DeliveryBoyName
        FROM tblDeliveryAssignment DA
        LEFT JOIN tblUser U
            ON U.userid = DA.DeliveryBoyUserId
        WHERE DA.IdOrderMaster IN @OrderIds";

                var deliveries = (await conn.QueryAsync<DeliveryAssignmentResponseTO>(
                    deliverySql,
                    new { OrderIds = orderIds }))
                    .ToList();

                //---------------------------------------------------------
                // Order Items + Cook Assignment
                //---------------------------------------------------------

                string itemSql = @"
        SELECT
            OD.IdOrderMaster,
            OD.IdOrderDetails,
            OD.IdItemMaster,
            IM.ItemName,
            OD.Quantity,
            OD.UnitPrice,
            OD.TotalPrice,

            CA.IdCookAssignment,
            CA.CookUserId,
            CU.UserName AS CookName,
            CA.IdStatus,
            CA.AssignedOn

        FROM tblOrderDetails OD

        LEFT JOIN tblItemMaster IM
            ON IM.IdItemMaster = OD.IdItemMaster

        LEFT JOIN tblCookAssignment CA
            ON CA.IdOrderDetails = OD.IdOrderDetails

        LEFT JOIN tblUser CU
            ON CU.userid = CA.CookUserId

        WHERE OD.IdOrderMaster IN @OrderIds";

                var itemLookup = new Dictionary<int, List<OrderItemResponseTO>>();

                var itemData = await conn.QueryAsync<dynamic>(
                    itemSql,
                    new { OrderIds = orderIds });

                foreach (var row in itemData)
                {
                    int orderId = row.IdOrderMaster;

                    if (!itemLookup.ContainsKey(orderId))
                        itemLookup[orderId] = new List<OrderItemResponseTO>();

                    itemLookup[orderId].Add(new OrderItemResponseTO
                    {
                        IdOrderDetails = row.IdOrderDetails,
                        IdItemMaster = row.IdItemMaster,
                        ItemName = row.ItemName,
                        Quantity = row.Quantity,
                        UnitPrice = row.UnitPrice,
                        TotalPrice = row.TotalPrice,

                        CookAssignment = row.IdCookAssignment == null
                            ? null
                            : new CookAssignmentTO
                            {
                                IdCookAssignment = row.IdCookAssignment,
                                CookUserId = row.CookUserId,
                                CookName = row.CookName,
                                IdStatus = row.IdStatus,
                                AssignedOn = row.AssignedOn
                            }
                    });
                }

                //---------------------------------------------------------
                // Mapping
                //---------------------------------------------------------

                foreach (var order in orders)
                {
                    order.TblPaymentResponseTO =
                        payments.FirstOrDefault(x =>
                            x.IdOrderMaster == order.IdOrderMaster);

                    order.DeliveryAssignment =
                        deliveries.FirstOrDefault(x =>
                            x.IdOrderMaster == order.IdOrderMaster);

                    order.orderItemResponseTO =
                        itemLookup.ContainsKey(order.IdOrderMaster)
                            ? itemLookup[order.IdOrderMaster]
                            : new List<OrderItemResponseTO>();
                }

                output.List = orders;
                output.TotalCount = totalRecords;

                return output;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<tblAddressResponseTO> GetAddressByIdAddressMapping(int idAddressMapping)
        {
            var conn =new SqlConnection(connq);
            string AddressSql = @"
              SELECT tblAddressMapping.IdAddressMapping,tblAddress.*,
          dimCountry.CountryName,dimCountry.CountryCode,dimState.StateName,dimState.StateCode,dimCity.CityName,dimCity.CityCode
FROM tblAddressMapping tblAddressMapping 
LEFT JOIN tblAddress ON tblAddressMapping.IdAddress = tblAddress.IdAddress
LEFT JOIN dimCountry dimCountry ON dimCountry.IdCountry = tblAddress.IdCountry
LEFT JOIN dimState dimState ON dimState.IdState = tblAddress.IdState
LEFT JOIN dimCity dimCity ON dimCity.IdCity = tblAddress.IdCity
      WHERE tblAddressMapping.IdAddressMapping = @idAddressMapping";

            var Addess = (await conn.QueryFirstOrDefaultAsync<tblAddressResponseTO>(
                AddressSql,
                new { idAddressMapping = idAddressMapping }));
                
            return Addess;
        }

        public async Task<OrderListResponseTO> GetOrderMasterListByIdOrder(int IdOrderMaster)
        {
            using var conn = new SqlConnection(connq);

            string orderSql = @"
        SELECT 
            OM.*,
            U.UserName AS CustomerName,
            DS.StatusName
        FROM tblOrderMaster OM
        INNER JOIN tblUser U ON U.userId = OM.CustomerId
        LEFT JOIN dimStatus DS ON DS.IdStatus = OM.IdStatus
        WHERE OM.IdOrderMaster = @IdOrderMaster";

            var order = await conn.QueryFirstOrDefaultAsync<OrderListResponseTO>(
                orderSql,
                new { IdOrderMaster });

            if (order == null)
                return null;

            // Payment Detail
            string paymentSql = @"
        SELECT *
        FROM tblPayment
        WHERE IdOrderMaster = @IdOrderMaster";

            order.TblPaymentResponseTO = await conn.QueryFirstOrDefaultAsync<TblPaymentResponseTO>(
                paymentSql,
                new { IdOrderMaster });

            // Delivery Assignment
            string deliverySql = @"
        SELECT
            DA.*,
            U.UserName AS DeliveryBoyName
        FROM tblDeliveryAssignment DA
        LEFT JOIN tblUser U
            ON U.userId = DA.DeliveryBoyId
        WHERE DA.IdOrderMaster = @IdOrderMaster";

            order.DeliveryAssignment = await conn.QueryFirstOrDefaultAsync<DeliveryAssignmentResponseTO>(
                deliverySql,
                new { IdOrderMaster });

            // Order Items + Cook Assignment
            string itemSql = @"
        SELECT
            OD.IdOrderMaster,
            OD.IdOrderDetails,
            OD.IdItemMaster,
            IM.ItemName,
            OD.Quantity,
            OD.UnitPrice,
            OD.TotalPrice,

            CA.IdCookAssignment,
            CA.CookUserId,
            CU.UserName AS CookName,
            CA.IdStatus,
            CA.AssignedOn

        FROM tblOrderDetails OD

        LEFT JOIN tblItemMaster IM
            ON IM.IdItemMaster = OD.IdItemMaster

        LEFT JOIN tblCookAssignment CA
            ON CA.IdOrderDetails = OD.IdOrderDetails

        LEFT JOIN tblUser CU
            ON CU.userId = CA.CookUserId

        WHERE OD.IdOrderMaster = @IdOrderMaster";

            var itemData = await conn.QueryAsync<dynamic>(
                itemSql,
                new { IdOrderMaster });

            order.orderItemResponseTO = itemData.Select(row => new OrderItemResponseTO
            {
                IdOrderDetails = row.IdOrderDetails,
                IdItemMaster = row.IdItemMaster,
                ItemName = row.ItemName,
                Quantity = row.Quantity,
                UnitPrice = row.UnitPrice,
                TotalPrice = row.TotalPrice,

                CookAssignment = row.IdCookAssignment == null
                    ? null
                    : new CookAssignmentTO
                    {
                        IdCookAssignment = row.IdCookAssignment,
                        CookUserId = row.CookUserId,
                        CookName = row.CookName,
                        IdStatus = row.IdStatus,
                        AssignedOn = row.AssignedOn
                    }
            }).ToList();

            return order;
        }

        public async Task<bool> UpdateOrderMaster(OrderMasterTO order)
        {
            using var conn = new SqlConnection(connq);

            await conn.OpenAsync();

            using var tran = conn.BeginTransaction();

            try
            {
                string updateOrderSql = @"
            UPDATE tblOrderMaster
            SET
                CustomerId = @CustomerId,
                IdStatus = @IdStatus,
                SubTotal = @SubTotal,
                TaxAmount = @TaxAmount,
                DeliveryCharge = @DeliveryCharge,
                DiscountAmount = @DiscountAmount,
                GrandTotal = @GrandTotal,
                Remarks = @Remarks,
                UpdatedOn = GETDATE(),
                UpdatedBy = @UpdatedBy
            WHERE IdOrderMaster = @IdOrderMaster";

                await conn.ExecuteAsync(
                    updateOrderSql,
                    order,
                    transaction: tran);

                // Remove Existing Items
                await conn.ExecuteAsync(
                    "DELETE FROM tblOrderDetails WHERE IdOrderMaster = @IdOrderMaster",
                    new { order.IdOrderMaster },
                    transaction: tran);

                // Add New Items
                if (order.OrderItems?.Any() == true)
                {
                    string detailSql = @"
                INSERT INTO tblOrderDetails
                (
                    IdOrderMaster,
                    IdItemMaster,
                    Quantity,
                    UnitPrice,
                    TotalPrice,
                    CreatedOn
                )
                VALUES
                (
                    @IdOrderMaster,
                    @IdItemMaster,
                    @Quantity,
                    @UnitPrice,
                    @TotalPrice,
                    GETDATE()
                )";

                    var details = order.OrderItems.Select(x => new
                    {
                        IdOrderMaster = order.IdOrderMaster,
                        x.IdItemMaster,
                        x.Quantity,
                        x.UnitPrice,
                        x.TotalPrice
                    });

                    await conn.ExecuteAsync(detailSql, details, transaction: tran);
                }

                // Update Payment
                if (order.PaymentDetail != null)
                {
                    string paymentSql = @"
                UPDATE tblPayment
                SET
                    Amount = @Amount,
                    PaymentMethod = @PaymentMethod,
                    TransactionNo = @TransactionNo,
                    TransactionTypeId = @TransactionTypeId,
                    IdStatus = @IdStatus,
                    UpdatedOn = GETDATE(),
                    UpdatedBy = @UpdatedBy
                WHERE IdOrderMaster = @IdOrderMaster";

                    await conn.ExecuteAsync(
                        paymentSql,
                        new
                        {
                            order.IdOrderMaster,
                            order.PaymentDetail.Amount,
                            order.PaymentDetail.PaymentMethod,
                            order.PaymentDetail.TransactionNo,
                            order.PaymentDetail.TransactionTypeId,
                            order.PaymentDetail.IdStatus,
                            UpdatedBy = order.UpdatedBy
                        },
                        transaction: tran);
                }

                tran.Commit();

                return true;
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }
    }
}
