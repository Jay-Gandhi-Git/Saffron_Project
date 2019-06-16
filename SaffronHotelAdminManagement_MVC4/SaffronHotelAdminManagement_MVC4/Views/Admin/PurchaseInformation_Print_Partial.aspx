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
                String Title = ViewData["TitleData_PurchaseInformationPrint"] as String;
                using (SaffronHotelAdminManagement_MVC4.SaffronModel db = new SaffronHotelAdminManagement_MVC4.SaffronModel())
                {
                    System.Data.DataTable dt = new System.Data.DataTable();
                    //dt.Columns.Add("OrderType", typeof(string));
                    //dt.Columns.Add("TableNo", typeof(string));
                    //dt.Columns.Add("CustomerName", typeof(string));
                    dt.Columns.Add("Date", typeof(DateTime));
                    dt.Columns.Add("BillNo", typeof(int));
                    dt.Columns.Add("Amount", typeof(double));
                    //dt.Columns.Add("TotalItem", typeof(int));
                    dt.Columns.Add("Title", typeof(string));

                    DateTime stDate = Convert.ToDateTime(Session["stDate_PurchaseInformation_Print"]);
                    DateTime enDate = Convert.ToDateTime(Session["enDate_PurchaseInformation_Print"]);
                    int itemCategory_Id = Convert.ToInt32(Session["itemCategory_Id_PurchaseInformation_Print"]);

                    var list = db.Purchases.Where(a => a.PurchaseDetails.Select(m1 => m1.ItemMaster.ItemCategory_ID).Contains(itemCategory_Id) && a.Date >= stDate && a.Date <= enDate).OrderBy(a => a.Date).ToList();
                    
                    foreach (var item in list)
                    {
                        System.Data.DataRow dr = dt.NewRow();
                       
                        dr["Date"] = item.Date.Value.ToString("dd-MM-yyyy");
                        dr["BillNo"] = item.BillNo;
                        dr["Amount"] = item.PurchaseDetails.Sum(m => m.Amount);
                        dr["Title"] = Title;
                        dr.EndEdit();
                        dt.Rows.Add(dr);
                    }
                    Session["stDate"] = Session["enDate"] = Session["OrderStatus"] = null;
                    PurchaseInformation_Print_ReportViewer.LocalReport.ReportPath = Server.MapPath("~/Views/Reports/PurchaseInformation_Print.rdlc");
                    PurchaseInformation_Print_ReportViewer.LocalReport.DataSources.Clear();
                    ReportDataSource rds = new ReportDataSource("DataSet1", dt);
                    PurchaseInformation_Print_ReportViewer.LocalReport.DataSources.Add(rds);
                    PurchaseInformation_Print_ReportViewer.LocalReport.Refresh();
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
                <rsweb:ReportViewer ID="PurchaseInformation_Print_ReportViewer" runat="server" Height="800px" Width="800px" AsyncRendering="false" BackColor="#99CCFF"></rsweb:ReportViewer>
                <%--<rsweb:ReportViewer ID="ReportViewer1" runat="server" Height="800px" Width="800px" AsyncRendering="false"></rsweb:ReportViewer>--%>
            </div>
        </div>
    </form>
</body>
</html>
