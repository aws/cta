using System;
using System.Collections.Generic;
using System.Linq;
using CTA.WebForms.Helpers.TagConversion;
using CTA.WebForms.Services;
using CTA.WebForms.TagCodeBehindHandlers;
using CTA.WebForms.TagConverters.TagTemplateConditions;
using CTA.WebForms.TagConverters.TagTemplateInvokables;
using HtmlAgilityPack;

namespace CTA.WebForms.TagConverters
{
    /// <summary>
    /// A converter that utilizes a series of templates denoting
    /// optional alternate configurations to port view layer tags
    /// of a given type.
    /// </summary>
    public class TemplateTagConverter : TagConverter
    {
        public IEnumerable<TemplateCondition> Conditions { get; set; }
        /// <summary>
        /// The set of actions to be taken outside of normal view or code
        /// behind conversion.
        /// </summary>
        public IEnumerable<TemplateInvokable> Invocations { get; set; }
        /// <summary>
        /// The set of temlate options to be used for source tags of the
        /// given type.
        /// </summary>
        public IDictionary<string, string> Templates { get; set; }

        /// <inheritdoc/>
        public override void Initialize(
            CodeBehindReferenceLinkerService codeBehindLinkerService,
            ViewImportService viewImportService)
        {
            base.Initialize(codeBehindLinkerService, viewImportService);

            foreach (var invokable in Invocations ?? Enumerable.Empty<TemplateInvokable>())
            {
                invokable.Initialize(viewImportService);
            }

            // TODO: Interactions with code behind linker service to register converter
        }

        /// <inheritdoc/>
        public override void Validate()
        {
            if (string.IsNullOrEmpty(TagName))
            {
                throw new ConfigValidationException($"{Rules.Config.Constants.WebFormsErrorTag}Failed to validate template tag converter, " +
                    $"expected TagName to have a value but was null or empty");
            }

            if (CodeBehindHandler != null && GetCodeBehindHandlerType() == null)
            {
                throw new ConfigValidationException($"{Rules.Config.Constants.WebFormsErrorTag}Failed to validate template tag converter, " +
                    $"CodeBehindHandler type {CodeBehindHandler} could not be found");
            }

            foreach (var condition in Conditions ?? Enumerable.Empty<TemplateCondition>())
            {
                condition.Validate(true);
            }

            foreach (var invocation in Invocations ?? Enumerable.Empty<TemplateInvokable>())
            {
                invocation.Validate();
            }
        }

        /// <inheritdoc/>
        public override void MigrateTag(HtmlNode node)
        {
            var template = SelectTemplate(node);
            if (template == null)
            {
                // No suitable template was found for replacing this node
                // so we do nothing here
                return;
            }

            var templateParser = new TagTemplateParser(_codeBehindLinkerService);
            var replacementText = templateParser.ParseTemplate(template, node);

            ReplaceNode(node, replacementText);

            var validInvocations = Invocations?.Where(invokable => invokable.ShouldInvoke(template));
            foreach (var invocation in validInvocations ?? Enumerable.Empty<TemplateInvokable>())
            {
                invocation.Invoke();
            }
        }

        /// <summary>
        /// Uses condition collection to determine which template should be
        /// used to convert the given node, then returns it.
        /// </summary>
        /// <param name="node">The node to be replaced.</param>
        /// <returns>The template that should be used for conversion.</returns>
        private string SelectTemplate(HtmlNode node)
        {
            foreach (var kvp in Templates)
            {
                var shouldUseTemplate = Conditions?
                    .Where(condition => condition.ShouldCheckCondition(kvp.Key))
                    .All(condition => condition.ConditionIsMet(node)) ?? true;

                if (shouldUseTemplate)
                {
                    return kvp.Value;
                }
            }

            return null;
        }

        /// <summary>
        /// Retrieves an instance of the specified code behind handler class
        /// if one exists.
        /// </summary>
        /// <returns>An instance of the converter's code behind handler class if
        /// one was specified, otherwise null.</returns>
        /// <exception cref="InvalidOperationException">Throws if code behind handler
        /// is specified but class couldn't be found.</exception>
        private ITagCodeBehindHandler GetCodeBehindHandlerInstance()
        {
            var handlerType = GetCodeBehindHandlerType();

            if (handlerType == null)
            {
                throw new InvalidOperationException($"Code behind handler type {CodeBehindHandler} could not be found");
            }

            return (ITagCodeBehindHandler)Activator.CreateInstance(handlerType);
        }

        /// <summary>
        /// Retrieves the type of the specified code behind handler class
        /// if one exists.
        /// </summary>
        /// <returns>The code behind handler type if it can be found, null otherwise</returns>
        private Type GetCodeBehindHandlerType()
        {
            if (CodeBehindHandler == null)
            {
                return null;
            }

            var handlerClassName = CodeBehindHandler.Equals("Default", StringComparison.InvariantCultureIgnoreCase)
                ? "DefaultTagCodeBehindHandler" : CodeBehindHandler;

            var webFormsAssembly = AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(a => a.FullName.StartsWith("CTA.WebForms") && !a.FullName.Contains("Test"))
                .FirstOrDefault();

            var handlerType = webFormsAssembly
                .GetTypes()
                .Where(t => t.Name.Equals(handlerClassName) && !t.IsAbstract && !t.IsInterface)
                .FirstOrDefault();

            return handlerType;
        }

        /// <summary>
        /// Replaces <paramref name="node"/> with the contents of <paramref name="nodeReplacementText"/>.
        /// </summary>
        /// <param name="node">The node to be replaced.</param>
        /// <param name="nodeReplacementText">The text to replace the node with</param>
        private void ReplaceNode(HtmlNode node, string nodeReplacementText)
        {
            var parent = node.ParentNode;
            var current = node;

            // Can't create multiple nodes at once, so instead we load the replacement
            // text into a new temporary html document and let HtmlAgilityPack generate
            // the nodes as children.
            var tempDoc = new HtmlDocument();

            tempDoc.OptionOutputOriginalCase = true;
            tempDoc.LoadHtml(nodeReplacementText);

            foreach (var child in tempDoc.DocumentNode.ChildNodes)
            {
                child.OwnerDocument.OptionOutputOriginalCase = true;

                parent.InsertAfter(child, current);
                current = child;
            }

            parent.RemoveChild(node);
        }
    }
}
