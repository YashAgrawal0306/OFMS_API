using DTO.Models.CommonModel;
using DTO.Models.CookModule;
using OFMS_API.Repository.DAL.Interface.CookModule;
using OFMS_API.Services.BL.Interface.CookModule;
using Services.BL.Interface.Notification;
using OFMS_API.DAL.Interface;
using System.Linq;
using System.Threading.Tasks;

namespace OFMS_API.Services.BL.Imple.CookModule
{
    public class CookModuleBL : ICookModuleBL
    {
        private readonly ICookModuleDAL _cookModuleDAL;
        private readonly INotificationService _notificationService;
        private readonly IuserDAL _userDAL;
        private readonly IOrderDAL _orderDal;

        public CookModuleBL(ICookModuleDAL cookModuleDAL, INotificationService notificationService, IuserDAL userDAL, IOrderDAL orderDal)
        {
            _cookModuleDAL = cookModuleDAL;
            _notificationService = notificationService;
            _userDAL = userDAL;
            _orderDal = orderDal;
        }

        public async Task<CookDashboardCountsTO> GetCookDashboardCounts(int cookUserId)
        {
            return await _cookModuleDAL.GetCookDashboardCounts(cookUserId);
        }

        public async Task<OutPutClass<CookOrderListResponseTO>> GetMyAssignedOrders(int cookUserId, FilterModelTO filter, bool completedHistory)
        {
            return await _cookModuleDAL.GetMyAssignedOrders(cookUserId, filter, completedHistory);
        }

        public async Task<CookOrderDetailResponseTO> GetOrderDetailsForCook(int cookUserId, int orderId)
        {
            return await _cookModuleDAL.GetOrderDetailsForCook(cookUserId, orderId);
        }

        public async Task<bool> AcceptOrder(int cookUserId, AcceptOrderRequestTO request)
        {
            bool success = await _cookModuleDAL.AcceptOrder(cookUserId, request);
            if (success)
            {
                await NotifyAdmins($"Cook ID {cookUserId} has accepted Order ID {request.IdOrderMaster}.", "COOK_ACCEPT");
            }
            return success;
        }

        public async Task<int> UpdateCookingStatus(int cookUserId, UpdateCookingStatusRequestTO request)
        {
            int orderId = await _cookModuleDAL.UpdateCookingStatus(cookUserId, request);
            if (orderId > 0)
            {
                await _orderDal.RecalculateOrderStatusDAL(orderId);
                await NotifyAdmins($"Cook ID {cookUserId} has updated status to ID '{request.NewStatusId}' for Order ID {request.IdOrderMaster}.", "COOK_STATUS");
            }
            return orderId;
        }

        public async Task<bool> UpdateEstimatedTime(int cookUserId, UpdateEstimatedTimeRequestTO request)
        {
            return await _cookModuleDAL.UpdateEstimatedTime(cookUserId, request);
        }

        public async Task<byte[]> GenerateCookHistoryExcel(CookReportFilterTO filter)
        {
            var data = await _cookModuleDAL.GetCookHistoryReportData(filter);

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var ws = workbook.Worksheets.Add("Cook History Report");

            // Enable gridlines
            ws.ShowGridLines = true;

            // 1. Title Banner (Rows 1 & 2)
            var titleRange = ws.Range("A1:M2");
            titleRange.Merge();
            titleRange.Value = "COOK KITCHEN HISTORY & PERFORMANCE REPORT";
            titleRange.Style.Font.Bold = true;
            titleRange.Style.Font.FontSize = 16;
            titleRange.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
            titleRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#0F172A");
            titleRange.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
            titleRange.Style.Alignment.Vertical = ClosedXML.Excel.XLAlignmentVerticalValues.Center;

            // 2. Metadata Info Block (Rows 4 & 5)
            ws.Cell(4, 1).Value = "Exported Date:";
            ws.Cell(4, 1).Style.Font.Bold = true;
            ws.Cell(4, 2).Value = System.DateTime.Now.ToString("dd-MMM-yyyy hh:mm tt");

            ws.Cell(4, 4).Value = "Filter Period:";
            ws.Cell(4, 4).Style.Font.Bold = true;
            
            string periodStr = "All Time";
            if (filter.FilterType == "Day" && filter.FromDate.HasValue)
                periodStr = $"Day ({filter.FromDate.Value:dd-MMM-yyyy})";
            else if (filter.FilterType == "Month" && filter.SelectedMonth.HasValue && filter.SelectedYear.HasValue)
                periodStr = $"Month ({new System.DateTime(filter.SelectedYear.Value, filter.SelectedMonth.Value, 1):MMMM yyyy})";
            else if (filter.FilterType == "Year" && filter.SelectedYear.HasValue)
                periodStr = $"Year ({filter.SelectedYear.Value})";
            else if (filter.FilterType == "Range" && (filter.FromDate.HasValue || filter.ToDate.HasValue))
                periodStr = $"Range ({filter.FromDate:dd-MMM-yyyy} to {filter.ToDate:dd-MMM-yyyy})";

            ws.Cell(4, 5).Value = periodStr;

            ws.Cell(5, 1).Value = "Filtered Cook:";
            ws.Cell(5, 1).Style.Font.Bold = true;
            string cookStr = (filter.CookUserId.HasValue && filter.CookUserId.Value > 0)
                ? (data.FirstOrDefault()?.CookName ?? $"Cook #{filter.CookUserId}")
                : "All Cooks";
            ws.Cell(5, 2).Value = cookStr;

            ws.Cell(5, 4).Value = "Total Records:";
            ws.Cell(5, 4).Style.Font.Bold = true;
            ws.Cell(5, 5).Value = data.Count;

            // 3. Table Column Headers (Row 7)
            string[] headers = {
                "S.No", "Order No", "Item Name", "Qty", "Customer Name", 
                "Cook Name", "Assigned On", "Completed On", "Est. Time (Mins)", 
                "Actual Time (Mins)", "Status", "Order Type", "Remarks"
            };

            for (int col = 0; col < headers.Length; col++)
            {
                var cell = ws.Cell(7, col + 1);
                cell.Value = headers[col];
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
                cell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#F97316"); // Orange Header
                cell.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                cell.Style.Alignment.Vertical = ClosedXML.Excel.XLAlignmentVerticalValues.Center;
                cell.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
            }
            ws.Row(7).Height = 24;

            // 4. Data Rows
            int currentRow = 8;
            int totalQty = 0;

            for (int i = 0; i < data.Count; i++)
            {
                var item = data[i];
                totalQty += item.Quantity;

                ws.Cell(currentRow, 1).Value = i + 1;
                ws.Cell(currentRow, 2).Value = item.OrderNo ?? "-";
                ws.Cell(currentRow, 3).Value = item.ItemName ?? "-";
                ws.Cell(currentRow, 4).Value = item.Quantity;
                ws.Cell(currentRow, 5).Value = item.CustomerName ?? "-";
                ws.Cell(currentRow, 6).Value = item.CookName ?? "-";
                ws.Cell(currentRow, 7).Value = item.AssignedOn.ToString("dd-MMM-yyyy hh:mm tt");
                ws.Cell(currentRow, 8).Value = item.ReadyOn.HasValue ? item.ReadyOn.Value.ToString("dd-MMM-yyyy hh:mm tt") : "-";
                ws.Cell(currentRow, 9).Value = item.EstimatedPreparationTime.HasValue ? item.EstimatedPreparationTime.Value : "-";
                ws.Cell(currentRow, 10).Value = item.ActualPreparationTime.HasValue ? item.ActualPreparationTime.Value : "-";
                ws.Cell(currentRow, 11).Value = item.StatusName ?? "Completed";
                ws.Cell(currentRow, 12).Value = item.IsMerged ? "Merged Batch" : "Standard";
                ws.Cell(currentRow, 13).Value = item.Remarks ?? "";

                // Formatting
                ws.Cell(currentRow, 1).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                ws.Cell(currentRow, 2).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                ws.Cell(currentRow, 4).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Right;
                ws.Cell(currentRow, 7).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                ws.Cell(currentRow, 8).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                ws.Cell(currentRow, 9).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                ws.Cell(currentRow, 10).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                ws.Cell(currentRow, 11).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

                // Alternate row background shading
                if (i % 2 == 1)
                {
                    ws.Range(currentRow, 1, currentRow, 13).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#F8FAFC");
                }

                currentRow++;
            }

            // 5. Total Summary Row
            var summaryRow = currentRow;
            ws.Cell(summaryRow, 1).Value = "TOTAL";
            ws.Cell(summaryRow, 1).Style.Font.Bold = true;
            ws.Range(summaryRow, 1, summaryRow, 3).Merge();
            ws.Cell(summaryRow, 1).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Right;

            ws.Cell(summaryRow, 4).Value = totalQty;
            ws.Cell(summaryRow, 4).Style.Font.Bold = true;
            ws.Cell(summaryRow, 4).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Right;

            var summaryRange = ws.Range(summaryRow, 1, summaryRow, 13);
            summaryRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#E2E8F0");
            summaryRange.Style.Font.Bold = true;
            summaryRange.Style.Border.TopBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
            summaryRange.Style.Border.BottomBorder = ClosedXML.Excel.XLBorderStyleValues.Double;

            // Auto-fit columns for nice display
            ws.Columns().AdjustToContents();

            using var ms = new System.IO.MemoryStream();
            workbook.SaveAs(ms);
            return ms.ToArray();
        }

        private async Task NotifyAdmins(string message, string notificationCode)
        {
            var admins = await _userDAL.GetAllCustomer(new FilterModelTO { RoleId = 1, PageNo = 1, PageSize = 1000 });
            var managers = await _userDAL.GetAllCustomer(new FilterModelTO { RoleId = 2, PageNo = 1, PageSize = 1000 });

            var targets = new System.Collections.Generic.List<int>();
            if (admins?.List != null) targets.AddRange(admins.List.Select(u => u.UserId));
            if (managers?.List != null) targets.AddRange(managers.List.Select(u => u.UserId));

            foreach (var userId in targets)
            {
                await _notificationService.SendNotificationAsync(userId, message, notificationCode);
            }
        }
    }
}
