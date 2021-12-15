using System;
using System.Collections.Generic;
using System.Linq;
using CTA.WebForms.Helpers.ControlHelpers;
using HtmlAgilityPack;

namespace CTA.WebForms.ControlConverters
{
    public class ListViewControlConverter : ControlConverter
    {
        private const string ItemPlaceHolderIdAttributeName = "ItemPlaceHolderID";
        private const string LayoutTemplateNodeName = "LayoutTemplate";
        private const string AspPlaceHolderNodeName = "asp:PlaceHolder";
        protected override Dictionary<string, string> AttributeMap { 
            get
            {
                return new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
                {
                    ["ID"] = "@ref",
                    ["ItemType"] = "ItemType",
                    ["AdditionalAttributes"] = "AdditionalAttributes",
                    ["AlternatingItemTemplate"] = "AlternatingItemTemplate",
                    ["ChildComponents"] = "ChildComponents",
                    ["ChildContent"] = "ChildContent",
                    ["DataKeys"] = "DataKeys",
                    ["DataMember"] = "DataMember",
                    ["DataSource"] = "DataSource",
                    ["DataSourceID"] = "DataSourceID",
                    ["EmptyDataTemplate"] = "EmptyDataTemplate",
                    ["Enabled"] = "Enabled",
                    ["EnableTheming"] = "EnableTheming",
                    ["EnableViewState"] = "EnableViewState",
                    ["GroupItemCount"] = "GroupItemCount",
                    ["GroupSeparatorTemplate"] = "GroupSeparatorTemplate",
                    ["GroupTemplate"] = "GroupTemplate",
                    ["InsertItemPosition"] = "InsertItemPosition",
                    ["ItemPlaceHolder"] = "ItemPlaceHolder",
                    ["ItemPlaceholderID"] = "ItemPlaceholderID",
                    ["Items"] = "Items",
                    ["ItemSeparatorTemplate"] = "ItemSeparatorTemplate",
                    ["ItemTemplate"] = "ItemTemplate",
                    ["LayoutTemplate"] = "LayoutTemplate",
                    ["SelectedIndex"] = "SelectedIndex",
                    ["SelectMethod"] = "SelectMethod",
                    ["SkinID"] = "SkinID",
                    ["Style"] = "Style",
                    ["TabIndex"] = "TabIndex",
                    ["Visible"] = "Visible",
                };
            } 
        }

        protected override IEnumerable<ViewLayerControlAttribute> NewAttributes
        {
            get
            {
                return new List<ViewLayerControlAttribute>()
                {
                    new ViewLayerControlAttribute("Context", "Item")
                };
            }
        }
        
        protected override string BlazorName { get { return "ListView"; } }
        
        public ListViewControlConverter() : base()
        {
            
        }

        public override HtmlNode Convert2Blazor(HtmlNode node)
        {
            var itemPlaceHolderValue = node.Attributes
                .AttributesWithName(ItemPlaceHolderIdAttributeName)
                .FirstOrDefault()
                ?.Value;
            
            if (!string.IsNullOrEmpty(itemPlaceHolderValue))
            {
                IEnumerable<ViewLayerControlAttribute> layoutContextAttr = new List<ViewLayerControlAttribute>()
                {
                    new ViewLayerControlAttribute("Context", itemPlaceHolderValue)
                };
            
                var success = UpdateInnerHtmlNode(node, LayoutTemplateNodeName, addedAttributes: layoutContextAttr);
                if (success)
                {
                    //Could accidentally replace placeholders for other stuff with different IDs,
                    //might need to add ID identifier as well
                    UpdateInnerHtmlNode(node, AspPlaceHolderNodeName, id: itemPlaceHolderValue, template: "@{2}", newBody: itemPlaceHolderValue);
                }
            }
            
            var joinedAttributesString = JoinAllAttributes(node.Attributes, NewAttributes);
            return Convert2BlazorFromParts(NodeTemplate, BlazorName, joinedAttributesString, node.InnerHtml);
        }
    }
}
