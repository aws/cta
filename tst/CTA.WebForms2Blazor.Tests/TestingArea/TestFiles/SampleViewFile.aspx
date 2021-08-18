<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="eShopLegacyWebForms._Default" %>
<%@ Register Src="~/footer.ascx" TagName="footer" TagPrefix="Tfooter" %>
<%@ Register Src="~/Counter.ascx" TagName="Counter" TagPrefix="TCounter" %>

<asp:Content ID="CatalogList" ContentPlaceHolderID="MainContent" runat="server">
    <div>
        <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="false">
            <Columns>
                <asp:BoundField  DataField="Name" HeaderText="First Name" />
                <asp:BoundField  DataField="LastName" HeaderText="Last Name" />
                <asp:BoundField  DataField="Position" HeaderText="Person Type" />
            </Columns>
        </asp:GridView>
    </div>

    <div class="esh-table">
        <p class="esh-link-wrapper">
            <a runat="server" href="<%$RouteUrl:RouteName=CreateProductRoute%>" class="btn esh-button esh-button-primary">
                Create New
            </a>
        </p>

        <asp:ListView ID="productList" ItemPlaceholderID="itemPlaceHolder" runat="server" ItemType="eShopLegacyWebForms.Models.CatalogItem">
            <EmptyDataTemplate>
                <table>
                    <tr>
                        <td>No data was returned.</td>
                    </tr>
                </table>
            </EmptyDataTemplate>
            <LayoutTemplate>
                <table class="table">
                    <thead>
                        <tr class="esh-table-header">
                            <th></th>
                            <th>Name</th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:PlaceHolder runat="server" ID="itemPlaceHolder"></asp:PlaceHolder>
                    </tbody>
                </table>
            </LayoutTemplate>
            <ItemTemplate>
                <tr>
                    <td>
                        <image class="esh-thumbnail" src='/Pics/<%#:Item.PictureFileName%>' />
                    </td>
                    <td>
                        <p><%#:Item.Name%></p>
                    </td>
                    <td>
                        <asp:HyperLink NavigateUrl='<%# GetRouteUrl("EditProductRoute", new {id =Item.Id}) %>' runat="server" CssClass="esh-table-link">
                            Edit
                        </asp:HyperLink>
                    </td>
                </tr>
            </ItemTemplate>
        </asp:ListView>
    </div>

    <div class="esh-pager">
        <TCounter:Counter ID="counter1" runat="server" IncrementAmount="10" />
        <Tfooter:footer ID="footer1" runat="server" />
    </div>
</asp:Content>
