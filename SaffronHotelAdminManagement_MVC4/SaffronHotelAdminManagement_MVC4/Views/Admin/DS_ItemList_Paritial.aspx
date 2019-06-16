<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage" %>

<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=11.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>

<!DOCTYPE html>

<html>
<head runat="server">
    <meta name="viewport" content="width=device-width" />
    <title>Saffron | Admin</title>
    <script runat="server">
        void page_Load(object sender, EventArgs args)
        {
            if (!IsPostBack)
            {
                using (SaffronHotelAdminManagement_MVC4.SaffronModel db = new SaffronHotelAdminManagement_MVC4.SaffronModel())
                {
                    System.Data.DataTable dt = new System.Data.DataTable();
                    dt.Columns.Add("ItemCategoryName", typeof(string));
                    dt.Columns.Add("ItemName", typeof(string));
                    dt.Columns.Add("Rate", typeof(double));
                    dt.Columns.Add("ReOrderLevel", typeof(int));

                    var list = db.ItemMasters.ToList();
                    foreach (var item in list)
                    {
                        System.Data.DataRow dr = dt.NewRow();
                        dr["ItemCategoryName"] = item.ItemCategory.ItemCategoryName;
                        dr["ItemName"] = item.ItemName;
                        dr["Rate"] = item.Rate.Value;
                        dr["ReOrderLevel"] = (item.ReOrderLevel.HasValue) ? item.ReOrderLevel.Value : 0;
                        dr.EndEdit();
                        dt.Rows.Add(dr);
                    }

                    ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/Views/Reports/Item.rdlc");
                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportDataSource rds = new ReportDataSource("DS_Saffron", dt);
                    ReportViewer1.LocalReport.DataSources.Add(rds);
                    ReportViewer1.LocalReport.Refresh();

                }
            }
        }
    </script>
</head>
<body>
    
    <form runat="server" style="border: none; background: none; background-color: none;">
        <div align="center">
            <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
            <rsweb:ReportViewer ID="ReportViewer1" runat="server" Height="800px" Width="800px" AsyncRendering="false"></rsweb:ReportViewer>
        </div>
    </form>


</body>
</html>
