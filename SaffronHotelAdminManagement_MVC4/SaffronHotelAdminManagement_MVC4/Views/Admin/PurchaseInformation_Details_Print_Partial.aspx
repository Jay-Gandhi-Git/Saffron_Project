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
                String Title = ViewData["TitleData_PurchaseInformation_Details_Print"] as String;
                using (SaffronHotelAdminManagement_MVC4.SaffronModel db = new SaffronHotelAdminManagement_MVC4.SaffronModel())
                {
                    System.Data.DataTable dt = new System.Data.DataTable();
                    //dt.Columns.Add("OrderType", typeof(string));
                    //dt.Columns.Add("TableNo", typeof(string));
                    dt.Columns.Add("ItemName", typeof(string));
                    //dt.Columns.Add("Date", typeof(DateTime));
                    dt.Columns.Add("Quantity", typeof(int));
                    dt.Columns.Add("Rate", typeof(double));
                    dt.Columns.Add("Amount", typeof(double));
                    //dt.Columns.Add("TotalItem", typeof(int));
                    dt.Columns.Add("Title", typeof(string));

                    
                    int id=Convert.ToInt32(Session["Purchase_Id_PurchaseInformation_Details_Print"]);
                    var list = db.PurchaseDetails.Where(a => a.Purchase_ID == id).ToList();
                    
                    
                    foreach (var item in list)
                    {
                        System.Data.DataRow dr = dt.NewRow();
                       
                        //dr["Date"] = item.Date.Value.ToString("dd-MM-yyyy");
                        //dr["BillNo"] = item.BillNo;
                        dr["ItemName"]=item.ItemMaster.ItemName;
                        dr["Quantity"]=item.Quantity;
                        dr["Rate"]=item.Rate;
                        dr["Amount"] = item.Amount;
                        dr["Title"] = Title;
                        dr.EndEdit();
                        dt.Rows.Add(dr);
                    }

                    Session["BillNo_PurchaseInformation_Details_Print"] = Session["vendorname_PurchaseInformation_Details_Print"] = Session["city_PurchaseInformation_Details_Print"] = Session["Purchase_Id_PurchaseInformation_Details_Print"] = Session["Date__PurchaseInformation_Details_Print"] = null;
                    
                    
                    PurchaseInformation_Details_Print_ReportViewer.LocalReport.ReportPath = Server.MapPath("~/Views/Reports/PurchaseInformation_Details_Print.rdlc");
                    PurchaseInformation_Details_Print_ReportViewer.LocalReport.DataSources.Clear();
                    ReportDataSource rds = new ReportDataSource("DataSet1", dt);
                    PurchaseInformation_Details_Print_ReportViewer.LocalReport.DataSources.Add(rds);
                    PurchaseInformation_Details_Print_ReportViewer.LocalReport.Refresh();
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
                <rsweb:ReportViewer ID="PurchaseInformation_Details_Print_ReportViewer" runat="server" Height="800px" Width="800px" AsyncRendering="false" BackColor="#99CCFF"></rsweb:ReportViewer>
                <%--<rsweb:ReportViewer ID="ReportViewer1" runat="server" Height="800px" Width="800px" AsyncRendering="false"></rsweb:ReportViewer>--%>
            </div>
        </div>
    </form>
</body>
</html>
