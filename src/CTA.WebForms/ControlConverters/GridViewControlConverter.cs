using System;
using System.Collections.Generic;
using System.Linq;
using CTA.WebForms.Helpers.ControlHelpers;
using HtmlAgilityPack;

namespace CTA.WebForms.ControlConverters
{
    public class GridViewControlConverter : ControlConverter
    {
        private const string ItemTypeAttributeName = "ItemType";
        private const string ItemTemplateNodeName = "ItemTemplate";
        private const string TempItemType = "PleaseReplaceWithActualItemTypeHere";
        protected override Dictionary<string, string> AttributeMap { 
            get
            {
                return new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
                {
                    ["AutoGenerateColumns"]="AutoGenerateColumns",
                    ["CssClass"]="CssClass",
                    ["DataKeyNames"]="DataKeyNames",
                    ["DataSource"]="DataSource",
                    ["EmptyDataText"]="EmptyDataText",
                    ["Enabled"]="Enabled",
                    ["ID"]="@ref",
                    ["Items"]="Items",
                    ["ItemType"]="ItemType",
                    ["OnDataBinding"]="OnDataBinding",
                    ["OnDataBound"]="OnDataBound",
                    ["OnItemDataBound"]="OnItemDataBound",
                    ["OnInit"]="OnInit",
                    ["OnLoad"]="OnLoad",
                    ["OnPreRender"]="OnPreRender",
                    ["OnUnload"]="OnUnload",
                    ["OnDisposed"]="OnDisposed",
                    ["SelectMethod"]="SelectMethod",
                    ["TabIndex"]="TabIndex",
                    ["Visible"]="Visible",
                    ["DataField"]="DataField",
                    ["DataFormatString"]="DataFormatString",
                    ["HeaderText"]="HeaderText",
                    ["Visible"]="Visible",
                    ["AccessibleHeaderText"]="AccessibleHeaderText",
                    ["DataNavigateUrlFields"]="DataNavigateUrlFields",
                    ["DataNavigateUrlFormatString"]="DataNavigateUrlFormatString",
                    ["DataTextField"]="DataTextField",
                    ["DataTextFormatString"]="DataTextFormatString",
                    ["HeaderText"]="HeaderText",
                    ["NavigateUrl"]="NavigateUrl",
                    ["Target"]="Target",
                    ["Text"]="Text",
                    ["ButtonType"]="ButtonType",
                    ["CommandName"]="CommandName",
                };
            } 
        }

        private List<String> SubControls
        {
            get
            {
                return new List<string>()
                {
                    "BoundField", "ButtonField", "TemplateField", "HyperLinkField", "CommandField", "DynamicField", 
                    "CheckBoxField", "ImageField",
                };
            }
        }

        protected override string BlazorName { get {return "GridView"; } }
        
        public GridViewControlConverter() : base()
        {
            
        }

        public override HtmlNode Convert2Blazor(HtmlNode node)
        {
            var itemTypeAttr = node.Attributes.AttributesWithName(ItemTypeAttributeName).FirstOrDefault();
            bool hasItemType = itemTypeAttr != null;
            var itemType  = itemTypeAttr?.Value ?? TempItemType;

            IEnumerable<ViewLayerControlAttribute> newItemTypeAttr = new List<ViewLayerControlAttribute>()
            {
                new ViewLayerControlAttribute(ItemTypeAttributeName, itemType)
            };
            
            foreach (string subControl in SubControls)
            {
                var aspControlName = Constants.AspControlTag + subControl;
                UpdateInnerHtmlNode(node, aspControlName, newName: subControl, addedAttributes: newItemTypeAttr);
            }

            IEnumerable<ViewLayerControlAttribute> itemTemplateContextAttr = new List<ViewLayerControlAttribute>()
            {
                new ViewLayerControlAttribute("Context", "Item")
            };
            UpdateInnerHtmlNode(node, ItemTemplateNodeName, addedAttributes: itemTemplateContextAttr);

            var attrToBeAdded = hasItemType ? null : newItemTypeAttr;
            var joinedAttributesString = JoinAllAttributes(node.Attributes, attrToBeAdded);
            return Convert2BlazorFromParts(NodeTemplate, BlazorName, joinedAttributesString, node.InnerHtml);
        }   
    }
}
