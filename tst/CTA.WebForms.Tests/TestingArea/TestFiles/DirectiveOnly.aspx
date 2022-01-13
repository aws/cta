<%@ Register Src="~/CustomControls/Counter.ascx" TagName="Counter" TagPrefix="TCounter" %>
<%@ Register Src="eShopOnBlazor/Foobar.ascx" TagName="Footer" TagPrefix="TFooter" %>
<%@ Register Src="Footer.ascx" TagName="Footer1" TagPrefix="TFooter1" %>
<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="eShopLegacyWebForms._Default" %>
<%@ Master language="C#" autoeventwireup="true" codebehind="Site.master.cs" inherits="eShopLegacyWebForms.SiteMaster" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Counter.ascx.cs" Inherits="eShopLegacyWebForms.Counter" %>

<div>
    <TCounter:Counter ID="counter1" runat="server" IncrementAmount="10" />
    <Tfooter:Footer ID="footer1" runat="server" />
</div>