<div>
    <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="false" ItemType="People" SelectMethod="People.GetPeople">
        <Columns>
            <asp:BoundField  DataField="Name" HeaderText="First Name"/>
            <asp:BoundField  DataField="LastName" HeaderText="Last Name" />
            <asp:BoundField  DataField="Position" HeaderText="Person Type" />
        </Columns>
    </asp:GridView>
</div>
<div>
    <asp:GridView runat=server AutoGenerateColumns="false" DataKeyNames="CustomerID" SelectMethod="GetCustomers" EmptyDataText="No data available">
        <Columns>
            <asp:BoundField DataField="CustomerID" HeaderText="ID" />
            <asp:BoundField DataField="CompanyName" HeaderText="CompanyName" />
            <asp:BoundField DataField="FirstName" HeaderText="FirstName"/>
            <asp:BoundField DataField="LastName" HeaderText="LastName"/>
            <asp:TemplateField>
                <ItemTemplate>
                    <button type="button">Click Me! <%# Item.Name %></button>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:ButtonField ButtonType="Button" DataTextField="CompanyName" DataTextFormatString="{0}" CommandName="Customer"/>
        </Columns>
    </asp:GridView>
</div>