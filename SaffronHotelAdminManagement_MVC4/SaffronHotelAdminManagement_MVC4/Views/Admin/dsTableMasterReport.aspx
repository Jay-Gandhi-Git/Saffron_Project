<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=11.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>

<!DOCTYPE html>

<html>
<head runat="server">
    <meta name="viewport" content="width=device-width" />
    <title>Saffron | Admin</title>
    <script runat="server">

    void page_Load(object sender, EventArgs args)
        {
            if(!IsPostBack)
            {
                System.Data.DataTable dt = new System.Data.DataTable();
                dt.Columns.Add("TableCapacity");
                dt.Columns.Add("TableNo");

                dt.Rows.Add("5", "No 1");
                dt.Rows.Add("5", "No 2");
                dt.Rows.Add("5", "No 3");
                dt.Rows.Add("5", "No 4");
                dt.Rows.Add("5", "No 5");

                ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/Views/Reports/rptTableMaster.rdlc");
                ReportViewer1.LocalReport.DataSources.Clear();
                ReportDataSource rds = new ReportDataSource("DataSet1", dt);
                ReportViewer1.LocalReport.DataSources.Add(rds);
                ReportViewer1.LocalReport.Refresh();
            }
        }
    
    </script>
</head>
<body>
    <div>
        <form runat="server">
            <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
            <rsweb:ReportViewer ID="ReportViewer1" runat="server" AsyncRendering="False"></rsweb:ReportViewer>
        </form>
    </div>
</body>
</html>
