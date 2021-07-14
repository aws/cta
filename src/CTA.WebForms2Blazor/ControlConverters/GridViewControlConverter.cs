using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace CTA.WebForms2Blazor.ControlConverters
{
    public class GridViewControlConverter : ControlConverter
    {
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
            node.OwnerDocument.OptionOutputOriginalCase = true;
            
            var itemTypeAttr = node.Attributes.AttributesWithName("itemtype").FirstOrDefault();
            var itemType  = itemTypeAttr?.Value ?? "PleaseReplaceWithItemTypeHere";

            IEnumerable<String> newItemTypeAttr = new List<String>()
            {
                "ItemType=" + itemType
            };
            foreach (string subControl in SubControls)
            {
                UpdateInnerHtmlNode(node, "asp:" + subControl, newName: subControl, newAttributes: newItemTypeAttr);
            }

            IEnumerable<String> itemTemplateContextAttr = new List<String>()
            {
                "Context=Item"
            };
            UpdateInnerHtmlNode(node, "ItemTemplate", newAttributes: itemTemplateContextAttr);

            return base.Convert2Blazor(node);
        }   
    }
}
