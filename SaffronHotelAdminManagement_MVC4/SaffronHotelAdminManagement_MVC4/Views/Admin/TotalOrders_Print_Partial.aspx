<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage" %>
<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=11.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>
<!DOCTYPE html>

<html>
<head runat="server">
    <meta name="viewport" content="width=device-width" />
    <title>Saffron Hotel | Admin</title>
    <script runat="server">
        void page_Load(object sender, EventArgs args)
        {
            if (!IsPostBack)
            {
                String Title = ViewData["TitleData_TotalOrders_Print"] as String;
                using (SaffronHotelAdminManagement_MVC4.SaffronModel db = new SaffronHotelAdminManagement_MVC4.SaffronModel())
                {
                    System.Data.DataTable dt = new System.Data.DataTable();
                    dt.Columns.Add("OrderType", typeof(string));
                    dt.Columns.Add("TableNo", typeof(string));
                    dt.Columns.Add("CustomerName", typeof(string));
                    dt.Columns.Add("Date", typeof(DateTime));
                    dt.Columns.Add("BillNo", typeof(int));
                    dt.Columns.Add("Amount", typeof(double));
                    dt.Columns.Add("TotalItem", typeof(int));
                    dt.Columns.Add("Title", typeof(string));

                    string orderstatus_TotalOrders_Print = Convert.ToString(Session["OrderStatus_TotalOrder"]);
                    var list1 = db.OrderMasters.OrderBy(a => new { a.Date, a.BillNo }).ToList();
                    if (orderstatus_TotalOrders_Print == "Panding Order")
                    {
                        list1 = list1.Where(a => a.IsProcess == false && a.IsComplete == false && a.IsCancle == false).ToList();
                    }
                    else if (orderstatus_TotalOrders_Print == "Process")
                    {
                        list1 = list1.Where(a => a.IsProcess == true && a.IsComplete == false && a.IsCancle == false).ToList();
                    }
                    else if (orderstatus_TotalOrders_Print == "Complete")
                    {
                        list1 = list1.Where(a => a.IsProcess == false && a.IsComplete == true && a.IsCancle == false).ToList();
                    }
                    else if (orderstatus_TotalOrders_Print == "Cancel")
                    {
                        list1 = list1.Where(a => a.IsProcess == false && a.IsComplete == false && a.IsCancle == true).ToList();
                    }
                    
                    
                    foreach (var item in list1)
                    {
                        System.Data.DataRow dr = dt.NewRow();

                        dr["OrderType"] = item.OrderType;
                        dr["TableNo"] = (item.Table_ID == null) ? "-" : item.TableMaster.TableNo;
                        dr["CustomerName"] = item.CustomerDetail.CustomerName;
                        dr["Date"] = item.Date.Value.ToString("dd-MM-yyyy");
                        dr["BillNo"] = item.BillNo;
                        dr["Amount"] = item.OrderDetailMasters.Sum(am => am.Amount.Value);
                        dr["TotalItem"] = item.OrderDetailMasters.Count;
                        dr["Title"] = Title;
                        dr.EndEdit();
                        dt.Rows.Add(dr);
                    }
                    Session["stDate"] = Session["enDate"] = Session["OrderStatus_TotalOrder"] =Session["OrderStatus_TotalOrder_Count"] =Session["OrderStatus"] = null;
                    TotalOrders_Print_ReportViewer.LocalReport.ReportPath = Server.MapPath("~/Views/Reports/TotalOrders_Print.rdlc");
                    TotalOrders_Print_ReportViewer.LocalReport.DataSources.Clear();
                    ReportDataSource rds = new ReportDataSource("DataSet1", dt);
                    TotalOrders_Print_ReportViewer.LocalReport.DataSources.Add(rds);
                    TotalOrders_Print_ReportViewer.LocalReport.Refresh();
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
                <rsweb:ReportViewer ID="TotalOrders_Print_ReportViewer" runat="server" Height="800px" Width="800px" AsyncRendering="false" BackColor="#99CCFF"></rsweb:ReportViewer>
                <%--<rsweb:ReportViewer ID="ReportViewer1" runat="server" Height="800px" Width="800px" AsyncRendering="false"></rsweb:ReportViewer>--%>
            </div>
        </div>
    </form>
</body>
</html>
