<div class="row">
    <dl class="col-md-6 dl-horizontal">
        <dt>Name
        </dt>

        <dd>
            <asp:Label runat="server" Text='<%#product.Name%>' />
        </dd>

        <dt>Description
        </dt>

        <dd>
            <asp:Label runat="server" Text='<%#product.Description%>' />
        </dd>

        <dt>Brand
        </dt>

        <dd>
            <asp:Label runat="server" Text='<%#product.CatalogBrand.Brand%>' />
        </dd>

        <dt>Type
        </dt>

        <dd>
            <asp:Label runat="server" Text='<%#product.CatalogType.Type%>' />
        </dd>
        <dt>Price
        </dt>

        <dd>
            <asp:Label CssClass="esh-price" runat="server" Text='<%#product.Price%>' />
        </dd>

        <dt>Picture name
        </dt>

        <dd>
            <asp:Label runat="server" Text='<%#product.PictureFileName%>' />
        </dd>

        <dt>Stock
        </dt>

        <dd>
            <asp:Label runat="server" Text='<%#product.AvailableStock%>' />
        </dd>

        <dt>Restock
        </dt>

        <dd>
            <asp:Label runat="server" Text='<%#product.RestockThreshold%>' />
        </dd>

        <dt>Max stock
        </dt>

        <dd>
            <asp:Label id="label1" runat="server" Text='<%#product.MaxStockThreshold%>' />
        </dd>

    </dl>
</div>