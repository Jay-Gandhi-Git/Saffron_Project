<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage" %>
<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=11.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>
<!DOCTYPE html>

<html>
<head runat="server">
    <meta name="viewport" content="width=device-width" />
    <title>Saffron | Admin</title>
     <link rel="shortcut icon" type="image/x-icon" href="~/images/Saffron_ICO.ico" />
    <script runat="server">
        void page_Load(object sender, EventArgs args)
        {
            if (!IsPostBack)
            {
                String Title = ViewData["TitleData_ItemUsedStockPrint"] as String;
                using (SaffronHotelAdminManagement_MVC4.SaffronModel db = new SaffronHotelAdminManagement_MVC4.SaffronModel())
                {
                    System.Data.DataTable dt = new System.Data.DataTable();
                    dt.Columns.Add("ItemName", typeof(string));
                    //dt.Columns.Add("TableNo", typeof(string));
                    //dt.Columns.Add("CustomerName", typeof(string));
                    dt.Columns.Add("Date", typeof(DateTime));
                    dt.Columns.Add("Quantity", typeof(int));
                    //dt.Columns.Add("TotalAmount", typeof(double));
                    //dt.Columns.Add("TotalItem", typeof(int));
                    dt.Columns.Add("Title", typeof(string));

                    DateTime stDate = Convert.ToDateTime(Session["sDate_ItemUsedStock"]);
                    DateTime enDate = Convert.ToDateTime(Session["eDate_ItemUsedStock"]);
                    int itemcategory_Id = Convert.ToInt32(Session["itemcategory_Id"]);
                    var list = db.ItemUsedStocks.Where(a => a.ItemMaster.ItemCategory_ID == itemcategory_Id && a.Date >= stDate && a.Date <= enDate).OrderBy(a => a.Date).ToList();
                    
                    foreach (var item in list)
                    {
                        System.Data.DataRow dr = dt.NewRow();
                        dr["ItemName"] = item.ItemMaster.ItemName;
                        //dr["ItemName"] = "";
                        dr["Quantity"] = item.Quantity;
                        //dr["CustomerName"] = item.CustomerDetail.CustomerName;
                        dr["Date"] = item.Date.Value.ToString("dd-MM-yyyy");
                        //dr["BillNo"] = item.BillNo;
                        //dr["TotalItem"] = item.OrderDetailMasters.Count;
                        //dr["TotalAmount"] = item.OrderDetailMasters.Sum(am => am.Amount.Value);
                        dr["Title"] = Title;
                        dr.EndEdit();
                        dt.Rows.Add(dr);
                    }
                    Session["eDate_ItemUsedStock"] = Session["sDate_ItemUsedStock"] = Session["itemcategory_Id"] = null;
                    ItemUsedStock_Print_ReportViewer.LocalReport.ReportPath = Server.MapPath("~/Views/Reports/ItemUsedStockInfromation_Print.rdlc");
                    ItemUsedStock_Print_ReportViewer.LocalReport.DataSources.Clear();
                    ReportDataSource rds = new ReportDataSource("DataSet1", dt);
                    ItemUsedStock_Print_ReportViewer.LocalReport.DataSources.Add(rds);
                    ItemUsedStock_Print_ReportViewer.LocalReport.Refresh();
                }
            }
        }
    </script>
</head>
<body>
    <form runat="server">

       <%-- <script type="text/javascript">
            jQuery(window).load(function () {
                //jQuery("#status").fadeOut();
                jQuery("#preloader").delay(1800).fadeOut("slow");
            })
        </script>

        <div id="preloader">
            <div id="status">&nbsp;</div>
        </div>--%>
        <div>
            <div align="center">
                <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
                <rsweb:ReportViewer ID="ItemUsedStock_Print_ReportViewer" runat="server" Height="800px" Width="800px" AsyncRendering="false" BackColor="#99CCFF"></rsweb:ReportViewer>
                <%--<rsweb:ReportViewer ID="ReportViewer1" runat="server" Height="800px" Width="800px" AsyncRendering="false"></rsweb:ReportViewer>--%>
            </div>
        </div>
    </form>
</body>
</html>
