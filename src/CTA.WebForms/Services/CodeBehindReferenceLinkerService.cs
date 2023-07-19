using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CTA.Rules.Config;
using CTA.WebForms.Helpers.TagConversion;
using CTA.WebForms.TagCodeBehindHandlers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CTA.WebForms.Services
{
    /// <summary>
    /// Service class used to link code behind files to their corresponding
    /// view files. This makes simultaneous conversion of tags in the view
    /// layer and code behind references possible.
    /// </summary>
    public class CodeBehindReferenceLinkerService
    {
        private IDictionary<string, TagCodeBehindLink> _tagCodeBehindLinks;

        /// <summary>
        /// Initializes a new <see cref="CodeBehindReferenceLinkerService"/> instance.
        /// </summary>
        public CodeBehindReferenceLinkerService()
        {
            _tagCodeBehindLinks = new Dictionary<string, TagCodeBehindLink>();
        }

        /// <summary>
        /// Interacts with appropriate code behind converter to replace references
        /// to the given tag as specified by the provided code behind handler and
        /// retrieves a binding to the generated property if it exists.
        /// </summary>
        /// <param name="viewFilePath">The path of the view file being modified.</param>
        /// <param name="idValue">The id of the node being converted.</param>
        /// <param name="codeBehindName">The name of the attribute being converted as it will
        /// appear in the code behind.</param>
        /// <param name="convertedSourceValue">The converted source attribute value, should no
        /// code behind conversions be necessary.</param>
        /// <param name="targetAttribute">The attribute that the conversion result will be assigned to,
        /// if one exists, otherwise null.</param>
        /// <param name="handler">The code behind handler to be used on this node, if one exists.</param>
        /// <param name="token">A cancellation token for stopping processes if stall occurs.</param>
        /// <exception cref="InvalidOperationException">Throws if view file with path <paramref name="viewFilePath"/>
        /// has not been registered.</exception>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="handler"/> is null.</exception>
        /// <returns>Property binding text if a bindable property was generated, null otherwise.</returns>
        public async Task<string> HandleCodeBehindForAttributeAsync(
            string viewFilePath,
            string codeBehindName,
            string convertedSourceValue,
            string targetAttribute,
            TagCodeBehindHandler handler,
            CancellationToken token)
        {
            if (handler == null)
            {
                throw new ArgumentNullException($"{Rules.Config.Constants.WebFormsErrorTag}Failed to handle codebehind for attribute, " +
                    $"argument {nameof(handler)} was null");
            }

            if (!IsCodeBehindLinkCreated(viewFilePath))
            {
                throw new InvalidOperationException($"{Rules.Config.Constants.WebFormsErrorTag}Failed to handle code behind conversions for " +
                    $"attribute {codeBehindName} of element with ID {handler.IdValue}, missing view file registration for path {viewFilePath}");
            }

            if (!IsCodeBehindLinkValid(viewFilePath))
            {
                // If either the view file or code behind file doesn't
                // exist then do nothing, since no code behind conversions
                // can take place
                return null;
            }

            await WaitForClassDeclarationRegistered(viewFilePath, token);

            var semanticModel = _tagCodeBehindLinks[viewFilePath].SemanticModel;
            var classDeclaration = _tagCodeBehindLinks[viewFilePath].ClassDeclaration;

            handler.StageCodeBehindConversionsForAttribute(
                semanticModel,
                classDeclaration,
                codeBehindName,
                convertedSourceValue);

            return handler.GetBindingIfExists(codeBehindName, targetAttribute);
        }

        /// <summary>
        /// Stages operations to comment out any code behind references that are to be left
        /// unconverted by handlers pertaining to the given view file. Assumes that the
        /// view file at <paramref name="viewFilePath"/> has already verified as been registered.
        /// </summary>
        /// <param name="viewFilePath">The path of the view file whose code behind is to be cleaned up.</param>
        private void StageCleanUpForUnconvertableReferences(string viewFilePath)
        {
            // Not checking contains key here because we assume it was checked beforehand,
            // failure to do this may result in an exception

            foreach (var handler in _tagCodeBehindLinks[viewFilePath].Handlers)
            {
                handler.StageCleanUpForUnconvertableReferences(
                    _tagCodeBehindLinks[viewFilePath].SemanticModel,
                    _tagCodeBehindLinks[viewFilePath].ClassDeclaration);
            }
        }

        /// <summary>
        /// Performs all staged operations in any handlers pertaining to the given view/code behind
        /// pairing.
        /// </summary>
        /// <param name="viewFilePath">The path of the view file whose code behind is being converted.</param>
        private void PerformAllStagedOperations(string viewFilePath)
        {
            // Not checking contains key here because we assume it was checked beforehand,
            // failure to do this may result in an exception

            var currentLink = _tagCodeBehindLinks[viewFilePath];

            var allStagedConversions = currentLink.Handlers
                .SelectMany(handler => handler.StagedConversions)
                .DistinctBy(conversion => conversion.input)
                .ToDictionary(conversion => conversion.input, conversion => conversion.replacement);

            var alteredClass = currentLink.ClassDeclaration
                .ReplaceNodes(allStagedConversions.Keys, (original, _) => allStagedConversions[original]);

            foreach (var handler in currentLink.Handlers)
            {
                alteredClass = handler.PerformMemberAdditions(alteredClass);
            }

            _tagCodeBehindLinks[viewFilePath].ClassDeclaration = alteredClass;
        }

        /// <summary>
        /// Checks if a code behind link for files at the specified path has been
        /// created in the service.
        /// </summary>
        /// <param name="viewFilePath">The path of the corresponding view file.</param>
        /// <returns><c>true</c> if the code behind link exists, <c>false</c> otherwise.</returns>
        private bool IsCodeBehindLinkCreated(string viewFilePath)
        {
            return _tagCodeBehindLinks.ContainsKey(viewFilePath);
        }

        /// <summary>
        /// Checks if a code behind link for files at the specified path validly
        /// links a code behind file and a view file.
        /// </summary>
        /// <param name="viewFilePath">The path of the corresponding view file.</param>
        /// <returns><c>true</c> if the code behind link exists, and both files have been registered <c>false</c> otherwise.</returns>
        private bool IsCodeBehindLinkValid(string viewFilePath)
        {
            return IsCodeBehindLinkCreated(viewFilePath)
                   && _tagCodeBehindLinks[viewFilePath].ViewFileExists
                   && _tagCodeBehindLinks[viewFilePath].CodeBehindFileExists;
        }

        /// <summary>
        /// Registers a view file for use by the service.
        /// </summary>
        /// <param name="viewFilePath">The full path of the view file.</param>
        public void RegisterViewFile(string viewFilePath)
        {
            if (!IsCodeBehindLinkCreated(viewFilePath))
            {
                _tagCodeBehindLinks.Add(viewFilePath, new TagCodeBehindLink());
            }

            _tagCodeBehindLinks[viewFilePath].ViewFileExists = true;
        }

        /// <summary>
        /// Registers a code behind file for use by the service.
        /// </summary>
        /// <param name="viewFilePath">The full path of the view file.</param>
        public void RegisterCodeBehindFile(string viewFilePath)
        {
            if (!IsCodeBehindLinkCreated(viewFilePath))
            {
                _tagCodeBehindLinks.Add(viewFilePath, new TagCodeBehindLink());
            }

            _tagCodeBehindLinks[viewFilePath].CodeBehindFileExists = true;
        }

        /// <summary>
        /// Registers a code behind handler that will be used in conversions for the given
        /// view file.
        /// </summary>
        /// <param name="viewFilePath">The path of the view file that <paramref name="handler"/> applies to.</param>
        /// <param name="handler">The code behind handler that is to be used for the file at <paramref name="viewFilePath"/>.</param>
        /// <exception cref="InvalidOperationException">Throws if view file with path <paramref name="viewFilePath"/>
        /// has not been registered.</exception>
        public void RegisterCodeBehindHandler(string viewFilePath, TagCodeBehindHandler handler)
        {
            if (!IsCodeBehindLinkCreated(viewFilePath))
            {
                throw new InvalidOperationException($"{Rules.Config.Constants.WebFormsErrorTag}Failed to register code behind handler, " +
                    $"missing view file registration for path {viewFilePath}");
            }

            _tagCodeBehindLinks[viewFilePath].Handlers.Add(handler);
        }

        /// <summary>
        /// Registers a code behind class declaration that will be used by code behind handlers.
        /// </summary>
        /// <param name="viewFilePath">The full path of the view file that this code behind is linked to.</param>
        /// <param name="semanticModel">The semantic model that the <paramref name="classDeclaration"/> belongs to.</param>
        /// <param name="classDeclaration">The code behind class declaration</param>
        /// <exception cref="InvalidOperationException">Throws if view file with path <paramref name="viewFilePath"/>
        /// has not been registered.</exception>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="classDeclaration"/> is null.</exception>
        public void RegisterClassDeclaration(string viewFilePath, SemanticModel semanticModel, ClassDeclarationSyntax classDeclaration)
        {
            if (semanticModel == null)
            {
                throw new ArgumentNullException($"{Rules.Config.Constants.WebFormsErrorTag}Failed to register code behind class declaration, " +
                    $"argument {nameof(semanticModel)} was null");
            }

            if (classDeclaration == null)
            {
                throw new ArgumentNullException($"{Rules.Config.Constants.WebFormsErrorTag}Failed to register code behind class declaration, " +
                    $"argument {nameof(classDeclaration)} was null");
            }

            if (!IsCodeBehindLinkCreated(viewFilePath))
            {
                throw new InvalidOperationException($"{Rules.Config.Constants.WebFormsErrorTag}Failed to register type declaration, " +
                    $"missing view file registration for path {viewFilePath}");
            }

            _tagCodeBehindLinks[viewFilePath].SemanticModel = semanticModel;
            _tagCodeBehindLinks[viewFilePath].ClassDeclaration = classDeclaration;
            var classRegisteredSource = _tagCodeBehindLinks[viewFilePath].ClassDeclarationRegisteredTaskSource;

            if (!classRegisteredSource.Task.IsCompleted)
            {
                classRegisteredSource.SetResult(true);
            }
        }

        /// <summary>
        /// Notifies the service that the given view file has fully staged all of the code behind
        /// conversions available to its handlers, unblocking other processes that rely on this predicate.
        /// </summary>
        /// <param name="viewFilePath">The full path of the view file that is being converted.</param>
        /// <exception cref="InvalidOperationException">Throws if view file with path <paramref name="viewFilePath"/>
        /// has not been registered.</exception>
        public void NotifyAllHandlerConversionsStaged(string viewFilePath)
        {
            if (!IsCodeBehindLinkCreated(viewFilePath))
            {
                throw new InvalidOperationException($"{Rules.Config.Constants.WebFormsErrorTag}Failed to raise handler conversions staged notification, " +
                    $"missing view file registration for path {viewFilePath}");
            }

            var handlerStagingSource = _tagCodeBehindLinks[viewFilePath].HandlersStagingTaskSource;

            if (!handlerStagingSource.Task.IsCompleted)
            {
                handlerStagingSource.SetResult(true);
            }
        }

        /// <summary>
        /// Converts tag code behind references in <paramref name="classDeclaration"/> using code behind
        /// handlers necessary for the tags found in the view file at <paramref name="viewFilePath"/>.
        /// </summary>
        /// <param name="viewFilePath">The view file whose tag references are to be converted.</param>
        /// <param name="semanticModel">The semantic model that the <paramref name="classDeclaration"/> belongs to.</param>
        /// <param name="classDeclaration">The class declaration where the code behind references will be replaced.</param>
        /// <param name="token">A cancellation token for stopping processes if stall occurs.</param>
        /// <returns>The modified class declaration if modifications needed to be made, otherwise
        /// the original value of <paramref name="classDeclaration"/> is returned.</returns>
        public async Task<ClassDeclarationSyntax> ExecuteTagCodeBehindHandlersAsync(
            string viewFilePath,
            SemanticModel semanticModel,
            ClassDeclarationSyntax classDeclaration,
            CancellationToken token)
        {
            if (IsCodeBehindLinkCreated(viewFilePath))
            {
                RegisterClassDeclaration(viewFilePath, semanticModel, classDeclaration);
                await WaitForAllHandlerConversionsStaged(viewFilePath, token);
                StageCleanUpForUnconvertableReferences(viewFilePath);
                PerformAllStagedOperations(viewFilePath);

                return _tagCodeBehindLinks[viewFilePath].ClassDeclaration;
            }

            return classDeclaration;
        }

        public void CancelRemainingTagCodeBehindLinks()
        {
            LogHelper.LogWarning("Cancelling remaining tasks in TagCodeBehindLinks...");

            LogHelper.LogInformation("Cancelling remaining TagCodeBehindLink ClassDeclaration tasks...");
            foreach (var (_, tagCodeBehindLink) in _tagCodeBehindLinks)
            {
                if (!tagCodeBehindLink.ClassDeclarationRegisteredTaskSource.Task.IsCompleted)
                {
                    tagCodeBehindLink.ClassDeclarationRegisteredTaskSource.SetCanceled();
                }
            }

            LogHelper.LogInformation("Cancelling remaining TagCodeBehindLink Handler tasks...");
            foreach (var (_, tagCodeBehindLink) in _tagCodeBehindLinks)
            {
                if (!tagCodeBehindLink.HandlersStagingTaskSource.Task.IsCompleted)
                {
                    tagCodeBehindLink.HandlersStagingTaskSource.SetCanceled();
                }
            }

            LogHelper.LogInformation("Remaining tasks in TagCodeBehindLinks have been cancelled.");
        }

        /// <summary>
        /// A process used to block execution until all handlers for a given view file have staged their available
        /// conversions. Assumes that the view file at <paramref name="viewFilePath"/> has already been verified as
        /// registered.
        /// </summary>
        /// <param name="viewFilePath">The view file whose handlers are executing.</param>
        /// <param name="token">A cancellation token for stopping processes if stall occurs.</param>
        /// <returns>A task that completes once all handlers for a given view file have completed execution.</returns>
        private Task<bool> WaitForAllHandlerConversionsStaged(string viewFilePath, CancellationToken token)
        {
            // Not checking contains key here because we assume it was checked beforehand,
            // failure to do this may result in an exception

            var handlerStagingSource = _tagCodeBehindLinks[viewFilePath].HandlersStagingTaskSource;

            if (!handlerStagingSource.Task.IsCompleted)
            {
                token.Register(() => {
                    if (!handlerStagingSource.Task.IsCompleted)
                    {
                        handlerStagingSource.SetCanceled();
                    }
                });
            }

            return handlerStagingSource.Task;
        }

        /// <summary>
        /// A process used to block execution the code behind class declaration of a given view file
        /// has been registered. Assumes that the view file at <paramref name="viewFilePath"/> has already
        /// been verified as registered.
        /// </summary>
        /// <param name="viewFilePath">The view file whose code behind requires registration.</param>
        /// <param name="token">A cancellation token for stopping processes if stall occurs.</param>
        /// <returns>A task that completes once the code behind class declaration of a given view
        /// file has been registered.</returns>
        private Task<bool> WaitForClassDeclarationRegistered(string viewFilePath, CancellationToken token)
        {
            // Not checking contains key here because we assume it was checked beforehand,
            // failure to do this may result in an exception

            var classRegisteredSource = _tagCodeBehindLinks[viewFilePath].ClassDeclarationRegisteredTaskSource;

            if (!classRegisteredSource.Task.IsCompleted)
            {
                token.Register(() => {
                    if (!classRegisteredSource.Task.IsCompleted)
                    {
                        classRegisteredSource.SetCanceled();
                    }
                });
            }

            return classRegisteredSource.Task;
        }
    }
}
