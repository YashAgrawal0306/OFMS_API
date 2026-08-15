using DTO.Models.CommonModel;
using DTO.Models.Master.OrderMaster;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using OFMS_API.BL.Interface;
using OFMS_API.DAL.Imple;
using OFMS_API.DAL.Interface;
using OFMS_API.Models;
using OFMS_API.Models.DTO;
using Services.BL.Interface.Notification;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace OFMS_API.BL.Imple
{
    public class OrderBL : IOrderBL
    {
        private readonly string connq;
        private readonly IOrderDAL _iOrderDAL;
        private readonly IuserDAL _iuserDAL;
        private readonly INotificationService _notificationService;
        private readonly ICartRepository _cartRepo;

        public OrderBL(IOrderDAL iOrderDAL, IuserDAL iuserDAL, INotificationService notificationService, IConfiguration configuration, ICartRepository cartRepo)
        {
            _iOrderDAL = iOrderDAL;
            _iuserDAL = iuserDAL;
            _notificationService = notificationService;
            connq = configuration.GetConnectionString("DefaultConnection") ?? "";
            _cartRepo = cartRepo;
        }
        public async Task<ResultMessage> AddOrderMaster(OrderMasterTO orderMasterTO)
        {
            ResultMessage resultMessage = new();
            using var conn = new SqlConnection(connq);
            await conn.OpenAsync();
            SqlTransaction tran = conn.BeginTransaction();
            int result = await _iOrderDAL.AddOrderMaster(orderMasterTO,conn,tran);
            if (result > 0)
            {
                orderMasterTO.PaymentDetail.IdOrderMaster = result;
                int idPaymentDeatil =await _iOrderDAL.AddPaymentData(orderMasterTO.PaymentDetail,conn,tran);
                if (idPaymentDeatil > 0)
                {
                    // Clear the cart inside the same transaction
                    await _cartRepo.ClearCartWithConnectionAsync(orderMasterTO.CustomerId, conn, tran);

                    tran.Commit();
                    resultMessage.IsSuccess = true;
                    resultMessage.Message = "Order Added Successfully";

                    // Notify Admin (RoleId=1) and Manager (RoleId=2)
                    var admins = await _iuserDAL.GetAllCustomer(new FilterModelTO { RoleId = 1, PageNo = 1, PageSize = 1000 });
                    var managers = await _iuserDAL.GetAllCustomer(new FilterModelTO { RoleId = 2, PageNo = 1, PageSize = 1000 });

                    var adminAndManagers = new List<int>();
                    if (admins?.List != null) adminAndManagers.AddRange(admins.List.Select(u => u.UserId));
                    if (managers?.List != null) adminAndManagers.AddRange(managers.List.Select(u => u.UserId));

                    foreach (var userId in adminAndManagers)
                    {
                        await _notificationService.SendNotificationAsync(userId, $"New Order Placed: {orderMasterTO.OrderNo}", "NEW_ORDER");
                    }
                }
                else
                {
                    tran.Rollback();
                    resultMessage.IsSuccess = false;
                    resultMessage.Message = "Payment Detail Not Added";
                }
            }
            else
            {
                tran.Rollback();
                resultMessage.IsSuccess = false;
                resultMessage.Message = "Order Not Added";
            }
            return resultMessage;

        }

        public async Task<OutPutClass<OrderListResponseTO>> GetOrderMasterList(OrderListFilter orderListFilter)
        {
            var data = await _iOrderDAL.GetOrderMasterList(orderListFilter);
            if (data != null)
            {
                foreach(var item in data.List)
                {
                    if (item.IdAddressMapping > 0)
                    {
                        item.tblAddressResponseTO = await _iOrderDAL.GetAddressByIdAddressMapping(item.IdAddressMapping);
                    }
                }
            }
            return data;
        }
        public async Task<OrderListResponseTO> GetOrderMasterListByIdOrder(int IdOrderMaster)
        {
            var data =  await _iOrderDAL.GetOrderMasterListByIdOrder(IdOrderMaster);
            if(data != null)
            {
                data.tblAddressResponseTO = await _iOrderDAL.GetAddressByIdAddressMapping(data.IdAddressMapping);
            }
            return data;
        }

        public async Task<bool> UpdateOrderMaster(OrderMasterTO order)
        {
            return await _iOrderDAL.UpdateOrderMaster(order);
        }

        public async Task<ResultMessage> UpdateOrderStatus(UpdateOrderStatusRequest request)
        {
            var result = await _iOrderDAL.UpdateOrderStatus(request);
            if (result.IsSuccess)
            {
                var order = await GetOrderMasterListByIdOrder(request.IdOrderMaster);
                if (order != null)
                {
                    string statusMsg = request.IdStatus switch
                    {
                        1 => "placed successfully",
                        2 => "accepted by Admin",
                        3 => "assigned to a cook",
                        4 => "prepared and is ready for pickup",
                        5 => "assigned to a delivery executive",
                        6 => "delivered successfully",
                        7 => "cancelled",
                        _ => "updated"
                    };

                    await _notificationService.SendNotificationAsync(order.CustomerId, $"Your order #{order.OrderNo} has been {statusMsg}.");
                }
            }
            return result;
        }

        public async Task<byte[]> GenerateOrderInvoiceAsync(int IdOrderMaster)
        {
            var order = await GetOrderMasterListByIdOrder(IdOrderMaster);
            if (order == null) throw new Exception("Order not found");

            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
            
            var darkBlue = "#103667";
            var lightBlue = "#f0f4f8";
            var textDark = "#333333";

            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(QuestPDF.Helpers.PageSizes.A4);
                    page.Margin(1.5f, QuestPDF.Infrastructure.Unit.Centimetre);
                    page.PageColor(QuestPDF.Helpers.Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontColor(textDark));

                    // Header
                    page.Header().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            // Left: Logo and Address
                            row.RelativeItem().Row(innerRow =>
                            {
                                innerRow.ConstantItem(70).Height(70).Background(darkBlue).AlignCenter().AlignMiddle()
                                    .Text("OFMS").FontColor(QuestPDF.Helpers.Colors.White).FontSize(16).SemiBold();
                                
                                innerRow.RelativeItem().PaddingLeft(15).Column(logoText =>
                                {
                                    logoText.Item().Text("OFMS RESTAURANT").FontSize(18).SemiBold().FontColor(darkBlue);
                                    logoText.Item().PaddingTop(2).Text("123 Main Street, Food City, FC 12345");
                                    logoText.Item().Text("Phone: (123) 456-7890   |   Email: info@ofms.com");
                                });
                            });

                            // Right: Invoice badge and meta
                            row.ConstantItem(200).Column(rightCol =>
                            {
                                rightCol.Item().Background(darkBlue).PaddingVertical(8).AlignCenter()
                                    .Text("INVOICE").FontSize(22).SemiBold().FontColor(QuestPDF.Helpers.Colors.White);
                                
                                rightCol.Item().PaddingTop(10).Table(metaTable =>
                                {
                                    metaTable.ColumnsDefinition(c =>
                                    {
                                        c.RelativeColumn();
                                        c.ConstantColumn(10);
                                        c.RelativeColumn();
                                    });

                                    metaTable.Cell().Text("Invoice No.").SemiBold().FontSize(9);
                                    metaTable.Cell().Text(":").FontSize(9);
                                    metaTable.Cell().Text($"INV-{order.OrderNo}").FontSize(9);

                                    metaTable.Cell().Text("Invoice Date").SemiBold().FontSize(9);
                                    metaTable.Cell().Text(":").FontSize(9);
                                    metaTable.Cell().Text(DateTime.Now.ToString("dd MMM yyyy")).FontSize(9);

                                    metaTable.Cell().Text("Order No.").SemiBold().FontSize(9);
                                    metaTable.Cell().Text(":").FontSize(9);
                                    metaTable.Cell().Text(order.OrderNo).FontSize(9);

                                    metaTable.Cell().Text("Order Date").SemiBold().FontSize(9);
                                    metaTable.Cell().Text(":").FontSize(9);
                                    metaTable.Cell().Text(order.CreatedOn.ToString("dd MMM yyyy")).FontSize(9);
                                });
                            });
                        });

                        col.Item().PaddingVertical(15).LineHorizontal(1).LineColor(QuestPDF.Helpers.Colors.Grey.Lighten2);

                        // Bill To Section
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(billCol =>
                            {
                                billCol.Item().Width(70).Background(darkBlue).PaddingVertical(3).AlignCenter()
                                    .Text("BILL TO").FontColor(QuestPDF.Helpers.Colors.White).SemiBold().FontSize(9);
                                
                                billCol.Item().PaddingTop(10).Text(order.CustomerName).SemiBold().FontSize(12);
                                
                                if (order.tblAddressResponseTO != null)
                                {
                                    var addr = order.tblAddressResponseTO;
                                    billCol.Item().Text($"Address Line 1: {addr.AddressLine1}").FontSize(9);
                                    billCol.Item().Text(addr.Area ?? "").FontSize(9);
                                    billCol.Item().Text($"{addr.CityName}, {addr.StateName} - {addr.Pincode}").FontSize(9);
                                }
                                // billCol.Item().Text($"Phone: {order.CustomerMobile ?? ""}").FontSize(9);
                            });

                            row.ConstantItem(200).AlignRight().AlignMiddle().Container()
                                .Background(lightBlue).Border(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).Padding(15)
                                .Row(thxRow =>
                                {
                                    thxRow.RelativeItem().PaddingLeft(10).Column(thxCol =>
                                    {
                                        thxCol.Item().Text("Thank you!").SemiBold().FontColor(darkBlue).FontSize(12).Italic();
                                        thxCol.Item().Text("We appreciate\nyour business.").FontSize(9);
                                    });
                                });
                        });

                        col.Item().PaddingVertical(15).LineHorizontal(1).LineColor(QuestPDF.Helpers.Colors.Grey.Lighten2);
                    });

                    // Items Table
                    page.Content().Column(col =>
                    {
                        col.Item().Border(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(40); // No
                                columns.RelativeColumn(); // Item Name
                                columns.ConstantColumn(100); // Price
                                columns.ConstantColumn(60); // Qty
                                columns.ConstantColumn(100); // Total
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(darkBlue).Padding(8).Text("#").FontColor(QuestPDF.Helpers.Colors.White).SemiBold();
                                header.Cell().Background(darkBlue).Padding(8).Text("Item Name").FontColor(QuestPDF.Helpers.Colors.White).SemiBold();
                                header.Cell().Background(darkBlue).Padding(8).AlignCenter().Text("Price").FontColor(QuestPDF.Helpers.Colors.White).SemiBold();
                                header.Cell().Background(darkBlue).Padding(8).AlignCenter().Text("Qty").FontColor(QuestPDF.Helpers.Colors.White).SemiBold();
                                header.Cell().Background(darkBlue).Padding(8).AlignCenter().Text("Total").FontColor(QuestPDF.Helpers.Colors.White).SemiBold();
                            });

                            int index = 1;
                            if (order.orderItemResponseTO != null)
                            {
                                foreach (var item in order.orderItemResponseTO)
                                {
                                    table.Cell().BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten3).Padding(8).Text(index.ToString());
                                    table.Cell().BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten3).Padding(8).Text(item.ItemName);
                                    table.Cell().BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten3).Padding(8).AlignCenter().Text($"Rs. {item.UnitPrice:0.00}");
                                    table.Cell().BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten3).Padding(8).AlignCenter().Text(item.Quantity.ToString());
                                    table.Cell().BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten3).Padding(8).AlignCenter().Text($"Rs. {item.TotalPrice:0.00}");
                                    index++;
                                }
                            }
                        });

                        col.Item().PaddingVertical(15).LineHorizontal(1).LineColor(QuestPDF.Helpers.Colors.Grey.Lighten2);

                        // Payment Info and Summary
                        col.Item().Row(row =>
                        {
                            // Left: Payment Info
                            row.RelativeItem().PaddingRight(20).Column(payCol =>
                            {
                                payCol.Item().PaddingBottom(10).Text("Payment Info").SemiBold().FontColor(darkBlue).FontSize(11);
                                
                                payCol.Item().Row(r => {
                                    r.RelativeItem().Text(t => {
                                        t.Span("Payment Method : ").SemiBold().FontSize(9);
                                        t.Span("Cash on Delivery").FontSize(9);
                                    });
                                });

                                payCol.Item().PaddingTop(5).Row(r => {
                                    r.RelativeItem().Text(t => {
                                        t.Span("Payment Status : ").SemiBold().FontSize(9);
                                        t.Span("Paid").FontSize(9);
                                    });
                                });

                                payCol.Item().PaddingTop(10).Row(r => {
                                    r.RelativeItem().Column(c => {
                                        c.Item().Text("Notes :").SemiBold().FontSize(9);
                                        c.Item().Text("Thank you for ordering with us!\nEnjoy your meal.").FontSize(9);
                                    });
                                });
                            });

                            // Right: Summary Table
                            row.ConstantItem(250).Border(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).Column(sumCol =>
                            {
                                sumCol.Item().Padding(10).Table(sumTable =>
                                {
                                    sumTable.ColumnsDefinition(c =>
                                    {
                                        c.RelativeColumn();
                                        c.ConstantColumn(10);
                                        c.RelativeColumn();
                                    });

                                    sumTable.Cell().PaddingBottom(5).Text("SubTotal").SemiBold().FontSize(9);
                                    sumTable.Cell().PaddingBottom(5).Text(":").FontSize(9);
                                    sumTable.Cell().PaddingBottom(5).AlignRight().Text($"Rs. {order.SubTotal:0.00}").FontSize(9);

                                    sumTable.Cell().PaddingBottom(5).Text("Tax").SemiBold().FontSize(9);
                                    sumTable.Cell().PaddingBottom(5).Text(":").FontSize(9);
                                    sumTable.Cell().PaddingBottom(5).AlignRight().Text($"Rs. {order.TaxAmount:0.00}").FontSize(9);

                                    sumTable.Cell().PaddingBottom(5).Text("Delivery Charges").SemiBold().FontSize(9);
                                    sumTable.Cell().PaddingBottom(5).Text(":").FontSize(9);
                                    sumTable.Cell().PaddingBottom(5).AlignRight().Text($"Rs. {order.DeliveryCharge:0.00}").FontSize(9);
                                    
                                    if (order.DiscountAmount > 0)
                                    {
                                        sumTable.Cell().PaddingBottom(5).Text("Discount").SemiBold().FontSize(9);
                                        sumTable.Cell().PaddingBottom(5).Text(":").FontSize(9);
                                        sumTable.Cell().PaddingBottom(5).AlignRight().Text($"-Rs. {order.DiscountAmount:0.00}").FontSize(9);
                                    }
                                });

                                sumCol.Item().Background(darkBlue).Padding(10).Row(r =>
                                {
                                    r.RelativeItem().Text("Grand Total").SemiBold().FontColor(QuestPDF.Helpers.Colors.White);
                                    r.ConstantItem(10).Text(":").FontColor(QuestPDF.Helpers.Colors.White);
                                    r.RelativeItem().AlignRight().Text($"Rs. {order.GrandTotal:0.00}").SemiBold().FontColor(QuestPDF.Helpers.Colors.White);
                                });

                                string words = $"({ConvertNumberToWords((int)order.GrandTotal)} Rupees Only)";
                                sumCol.Item().Padding(10).AlignCenter().Text(words).FontSize(8);
                            });
                        });
                    });

                    page.Footer().Column(col =>
                    {
                        col.Item().AlignCenter().Text("For any queries, please contact us. We are here to help!").FontSize(9);
                        col.Item().PaddingVertical(5).AlignCenter().Text("----------------------------------------").FontColor(QuestPDF.Helpers.Colors.Grey.Lighten1);
                        col.Item().AlignCenter().Text("Thank you for your order!").SemiBold().FontColor(darkBlue);
                        col.Item().PaddingTop(5).AlignCenter().Text(x =>
                        {
                            x.Span("Page ").FontSize(8);
                            x.CurrentPageNumber().FontSize(8);
                            x.Span(" of ").FontSize(8);
                            x.TotalPages().FontSize(8);
                        });
                    });
                });
            });

            return document.GeneratePdf();
        }

        private string ConvertNumberToWords(int number)
        {
            if (number == 0) return "Zero";
            if (number < 0) return "Minus " + ConvertNumberToWords(Math.Abs(number));
            string words = "";
            if ((number / 1000000) > 0)
            {
                words += ConvertNumberToWords(number / 1000000) + " Million ";
                number %= 1000000;
            }
            if ((number / 1000) > 0)
            {
                words += ConvertNumberToWords(number / 1000) + " Thousand ";
                number %= 1000;
            }
            if ((number / 100) > 0)
            {
                words += ConvertNumberToWords(number / 100) + " Hundred ";
                number %= 100;
            }
            if (number > 0)
            {
                if (words != "") words += "and ";
                var unitsMap = new[] { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
                var tensMap = new[] { "Zero", "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };
                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0) words += " " + unitsMap[number % 10];
                }
            }
            return words;
        }
    }
}
