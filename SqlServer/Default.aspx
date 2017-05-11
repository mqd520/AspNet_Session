<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SqlServer._Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
&nbsp;&nbsp; key:&nbsp;&nbsp;&nbsp;
        <asp:TextBox ID="TextBox1" runat="server"></asp:TextBox>
        <br />
        <br />
        value:&nbsp;&nbsp;&nbsp;
        <asp:TextBox ID="TextBox2" runat="server"></asp:TextBox>
        <br />
        <br />
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
        <asp:Button ID="Button1" runat="server" OnClick="Button1_Click" Text="创   建" />
&nbsp;<asp:Button ID="Button2" runat="server" OnClick="Button2_Click" Text="读   取" />
&nbsp;<asp:Button ID="Button3" runat="server" OnClick="Button3_Click" Text="删   除" />
&nbsp;<asp:Button ID="Button4" runat="server" OnClick="Button4_Click" Text="清   空" />
        <br />
        <br />
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
        <asp:Label ID="lb_error" runat="server" Font-Bold="True" Font-Italic="False" Font-Names="微软雅黑" Font-Overline="False" Font-Size="Large" Font-Underline="False" ForeColor="Red" Visible="False"></asp:Label>
    </form>
</body>
</html>
