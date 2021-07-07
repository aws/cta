<div class="esh-pager">
    <div class="container">
        <article class="esh-pager-wrapper row">
            <nav>
                <asp:HyperLink ID="PaginationPrevious" runat="server" CssClass="esh-pager-item esh-pager-item--navigable">
                    Previous
                </asp:HyperLink>

                <asp:HyperLink ID="PaginationNext" runat="server" CssClass="esh-pager-item esh-pager-item--navigable">
                    Next
                </asp:HyperLink>
            </nav>
            <td>
                <asp:HyperLink NavigateUrl='<%# GetRouteUrl("EditProductRoute", new {id =Item.Id}) %>' runat="server" CssClass="esh-table-link">
                    Edit
                </asp:HyperLink>
                |
                <asp:HyperLink NavigateUrl='<%# GetRouteUrl("ProductDetailsRoute", new {id =Item.Id}) %>' runat="server" CssClass="esh-table-link">
                    Details
                </asp:HyperLink>
                |
                <asp:HyperLink NavigateUrl='<%# GetRouteUrl("DeleteProductRoute", new {id =Item.Id}) %>' runat="server" CssClass="esh-table-link">
                    Delete
                </asp:HyperLink>
            </td>
        </article>
    </div>
</div>