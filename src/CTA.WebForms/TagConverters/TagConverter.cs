using System;
using System.Linq;
using System.Threading.Tasks;
using CTA.WebForms.Services;
using CTA.WebForms.TagCodeBehindHandlers;
using HtmlAgilityPack;

namespace CTA.WebForms.TagConverters
{
    /// <summary>
    /// An abstract converter capable of porting view layer tags of a given
    /// type to an alternate representation. This includes porting related
    /// to code behind references.
    /// </summary>
    public abstract class TagConverter
    {
        private protected TaskManagerService _taskManagerService;
        private protected CodeBehindReferenceLinkerService _codeBehindLinkerService;
        private protected ViewImportService _viewImportService;

        /// <summary>
        /// The name of the tag to apply this converter to.
        /// </summary>
        public string TagName { get; set; }
        /// <summary>
        /// The type representation of the source tag in code behind
        /// files, if such a type exists.
        /// </summary>
        public string CodeBehindType { get; set; }
        /// <summary>
        /// The name of the type to be used for conversion of code behind
        /// references to the source tag. In most cases <see cref="DefaultTagCodeBehindHandler"/>
        /// will be sufficient.
        /// </summary>
        public string CodeBehindHandler { get; set; }
        /// <summary>
        /// The set of conditions used to decide which template to use on any
        /// given source tag.
        /// </summary>

        /// <summary>
        /// Used to inject any necessary services and perform other initialization
        /// steps required before tag migration.
        /// </summary>
        /// <param name="taskManagerService">The service instance that is used
        /// to perform managed calls to other services.</param>
        /// <param name="codeBehindLinkerService">The service instance that will
        /// be used to convert tag code behind references.</param>
        /// <param name="viewImportService">The service instance that will be used
        /// for adding any view imports needed for the conversion.</param>
        public virtual void Initialize(
            TaskManagerService taskManagerService,
            CodeBehindReferenceLinkerService codeBehindLinkerService,
            ViewImportService viewImportService)
        {
            _taskManagerService = taskManagerService;
            _codeBehindLinkerService = codeBehindLinkerService;
            _viewImportService = viewImportService;
        }

        /// <summary>
        /// Retrieves an instance of the specified code behind handler class
        /// if one exists.
        /// </summary>
        /// <param name="idValue">The value of the ID attribute for the node which this
        /// handler will be used on.</param>
        /// <returns>An instance of the converter's code behind handler class if
        /// one was specified, otherwise null.</returns>
        /// <exception cref="InvalidOperationException">Throws if code behind handler
        /// is specified but class couldn't be found.</exception>
        public TagCodeBehindHandler GetCodeBehindHandlerInstance(string idValue)
        {
            if (CodeBehindHandler == null || idValue == null)
            {
                return null;
            }

            var handlerType = GetCodeBehindHandlerType();

            if (handlerType == null)
            {
                throw new InvalidOperationException($"Code behind handler type {CodeBehindHandler} could not be found");
            }

            return (TagCodeBehindHandler)Activator.CreateInstance(handlerType, CodeBehindType, idValue);
        }

        /// <summary>
        /// Retrieves the type of the specified code behind handler class
        /// if one exists.
        /// </summary>
        /// <returns>The code behind handler type if it can be found, null otherwise</returns>
        private protected Type GetCodeBehindHandlerType()
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

            var handlerType = webFormsAssembly?
                .GetTypes()
                .Where(t => t.Name.Equals(handlerClassName) && !t.IsAbstract && !t.IsInterface)
                .FirstOrDefault();

            return handlerType;
        }

        /// <summary>
        /// Replaces the provided <see cref="HtmlNode"/> with its Blazor equivalent
        /// as specified by the given converter.
        /// </summary>
        /// <param name="node">The tag to be replaced.</param>
        /// <param name="viewFilePath">The path of the view file being modified.</param>
        /// <param name="handler">The code behind handler to be used on this node, if one exists.</param>
        /// <param name="taskId">The task id of the view file converter that called this method.</param>
        public abstract Task MigrateTag(HtmlNode node, string viewFilePath, TagCodeBehindHandler handler, int taskId);

        /// <summary>
        /// Checks whether the properties of this converter form a valid configuration.
        /// </summary>
        public abstract void Validate();
    }
}
