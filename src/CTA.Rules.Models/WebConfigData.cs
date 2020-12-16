
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
// NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
public partial class configuration
{

    private configurationAdd[] appSettingsField;

    private configurationSystemweb systemwebField;

    private configurationSystemwebServer systemwebServerField;

    private configurationRuntime runtimeField;

    private configurationAdd1[] connectionStringsField;

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("add", IsNullable = false)]
    public configurationAdd[] appSettings
    {
        get
        {
            return this.appSettingsField;
        }
        set
        {
            this.appSettingsField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("system.web")]
    public configurationSystemweb systemweb
    {
        get
        {
            return this.systemwebField;
        }
        set
        {
            this.systemwebField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("system.webServer")]
    public configurationSystemwebServer systemwebServer
    {
        get
        {
            return this.systemwebServerField;
        }
        set
        {
            this.systemwebServerField = value;
        }
    }

    /// <remarks/>
    public configurationRuntime runtime
    {
        get
        {
            return this.runtimeField;
        }
        set
        {
            this.runtimeField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("add", IsNullable = false)]
    public configurationAdd1[] connectionStrings
    {
        get
        {
            return this.connectionStringsField;
        }
        set
        {
            this.connectionStringsField = value;
        }
    }
}

[ExcludeFromCodeCoverage]
/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class configurationAdd
{

    private string keyField;

    private string valueField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string key
    {
        get
        {
            return this.keyField;
        }
        set
        {
            this.keyField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string value
    {
        get
        {
            return this.valueField;
        }
        set
        {
            this.valueField = value;
        }
    }
}

[ExcludeFromCodeCoverage]
/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class configurationSystemweb
{

    private configurationSystemwebCompilation compilationField;

    private configurationSystemwebAuthentication authenticationField;

    private configurationSystemwebPages pagesField;

    private configurationSystemwebMembership membershipField;

    private configurationSystemwebRoleManager roleManagerField;

    /// <remarks/>
    public configurationSystemwebCompilation compilation
    {
        get
        {
            return this.compilationField;
        }
        set
        {
            this.compilationField = value;
        }
    }

    /// <remarks/>
    public configurationSystemwebAuthentication authentication
    {
        get
        {
            return this.authenticationField;
        }
        set
        {
            this.authenticationField = value;
        }
    }

    /// <remarks/>
    public configurationSystemwebPages pages
    {
        get
        {
            return this.pagesField;
        }
        set
        {
            this.pagesField = value;
        }
    }

    /// <remarks/>
    public configurationSystemwebMembership membership
    {
        get
        {
            return this.membershipField;
        }
        set
        {
            this.membershipField = value;
        }
    }

    /// <remarks/>
    public configurationSystemwebRoleManager roleManager
    {
        get
        {
            return this.roleManagerField;
        }
        set
        {
            this.roleManagerField = value;
        }
    }
}

[ExcludeFromCodeCoverage]
/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class configurationSystemwebCompilation
{

    private configurationSystemwebCompilationAdd[] assembliesField;

    private bool debugField;

    private decimal targetFrameworkField;

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("add", IsNullable = false)]
    public configurationSystemwebCompilationAdd[] assemblies
    {
        get
        {
            return this.assembliesField;
        }
        set
        {
            this.assembliesField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool debug
    {
        get
        {
            return this.debugField;
        }
        set
        {
            this.debugField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public decimal targetFramework
    {
        get
        {
            return this.targetFrameworkField;
        }
        set
        {
            this.targetFrameworkField = value;
        }
    }
}

[ExcludeFromCodeCoverage]
/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class configurationSystemwebCompilationAdd
{

    private string assemblyField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string assembly
    {
        get
        {
            return this.assemblyField;
        }
        set
        {
            this.assemblyField = value;
        }
    }
}

[ExcludeFromCodeCoverage]
/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class configurationSystemwebAuthentication
{

    private configurationSystemwebAuthenticationForms formsField;

    private string modeField;

    /// <remarks/>
    public configurationSystemwebAuthenticationForms forms
    {
        get
        {
            return this.formsField;
        }
        set
        {
            this.formsField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string mode
    {
        get
        {
            return this.modeField;
        }
        set
        {
            this.modeField = value;
        }
    }
}

[ExcludeFromCodeCoverage]
/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class configurationSystemwebAuthenticationForms
{

    private string loginUrlField;

    private ushort timeoutField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string loginUrl
    {
        get
        {
            return this.loginUrlField;
        }
        set
        {
            this.loginUrlField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public ushort timeout
    {
        get
        {
            return this.timeoutField;
        }
        set
        {
            this.timeoutField = value;
        }
    }
}

[ExcludeFromCodeCoverage]
/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class configurationSystemwebPages
{

    private configurationSystemwebPagesAdd[] namespacesField;

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("add", IsNullable = false)]
    public configurationSystemwebPagesAdd[] namespaces
    {
        get
        {
            return this.namespacesField;
        }
        set
        {
            this.namespacesField = value;
        }
    }
}

[ExcludeFromCodeCoverage]
/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class configurationSystemwebPagesAdd
{

    private string namespaceField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string @namespace
    {
        get
        {
            return this.namespaceField;
        }
        set
        {
            this.namespaceField = value;
        }
    }
}

[ExcludeFromCodeCoverage]
/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class configurationSystemwebMembership
{

    private configurationSystemwebMembershipProviders providersField;

    /// <remarks/>
    public configurationSystemwebMembershipProviders providers
    {
        get
        {
            return this.providersField;
        }
        set
        {
            this.providersField = value;
        }
    }
}

[ExcludeFromCodeCoverage]
/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class configurationSystemwebMembershipProviders
{

    private object clearField;

    private configurationSystemwebMembershipProvidersAdd addField;

    /// <remarks/>
    public object clear
    {
        get
        {
            return this.clearField;
        }
        set
        {
            this.clearField = value;
        }
    }

    /// <remarks/>
    public configurationSystemwebMembershipProvidersAdd add
    {
        get
        {
            return this.addField;
        }
        set
        {
            this.addField = value;
        }
    }
}

[ExcludeFromCodeCoverage]
/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class configurationSystemwebMembershipProvidersAdd
{

    private string nameField;

    private string typeField;

    private string connectionStringNameField;

    private bool enablePasswordRetrievalField;

    private bool enablePasswordResetField;

    private bool requiresQuestionAndAnswerField;

    private string applicationNameField;

    private bool requiresUniqueEmailField;

    private string passwordFormatField;

    private byte maxInvalidPasswordAttemptsField;

    private byte minRequiredPasswordLengthField;

    private byte minRequiredNonalphanumericCharactersField;

    private byte passwordAttemptWindowField;

    private string passwordStrengthRegularExpressionField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string name
    {
        get
        {
            return this.nameField;
        }
        set
        {
            this.nameField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string type
    {
        get
        {
            return this.typeField;
        }
        set
        {
            this.typeField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string connectionStringName
    {
        get
        {
            return this.connectionStringNameField;
        }
        set
        {
            this.connectionStringNameField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool enablePasswordRetrieval
    {
        get
        {
            return this.enablePasswordRetrievalField;
        }
        set
        {
            this.enablePasswordRetrievalField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool enablePasswordReset
    {
        get
        {
            return this.enablePasswordResetField;
        }
        set
        {
            this.enablePasswordResetField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool requiresQuestionAndAnswer
    {
        get
        {
            return this.requiresQuestionAndAnswerField;
        }
        set
        {
            this.requiresQuestionAndAnswerField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string applicationName
    {
        get
        {
            return this.applicationNameField;
        }
        set
        {
            this.applicationNameField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool requiresUniqueEmail
    {
        get
        {
            return this.requiresUniqueEmailField;
        }
        set
        {
            this.requiresUniqueEmailField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string passwordFormat
    {
        get
        {
            return this.passwordFormatField;
        }
        set
        {
            this.passwordFormatField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public byte maxInvalidPasswordAttempts
    {
        get
        {
            return this.maxInvalidPasswordAttemptsField;
        }
        set
        {
            this.maxInvalidPasswordAttemptsField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public byte minRequiredPasswordLength
    {
        get
        {
            return this.minRequiredPasswordLengthField;
        }
        set
        {
            this.minRequiredPasswordLengthField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public byte minRequiredNonalphanumericCharacters
    {
        get
        {
            return this.minRequiredNonalphanumericCharactersField;
        }
        set
        {
            this.minRequiredNonalphanumericCharactersField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public byte passwordAttemptWindow
    {
        get
        {
            return this.passwordAttemptWindowField;
        }
        set
        {
            this.passwordAttemptWindowField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string passwordStrengthRegularExpression
    {
        get
        {
            return this.passwordStrengthRegularExpressionField;
        }
        set
        {
            this.passwordStrengthRegularExpressionField = value;
        }
    }
}

[ExcludeFromCodeCoverage]
/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class configurationSystemwebRoleManager
{

    private configurationSystemwebRoleManagerProviders providersField;

    private string defaultProviderField;

    private bool enabledField;

    private bool cacheRolesInCookieField;

    private string cookieNameField;

    private byte cookieTimeoutField;

    private string cookiePathField;

    private bool cookieRequireSSLField;

    private bool cookieSlidingExpirationField;

    private string cookieProtectionField;

    /// <remarks/>
    public configurationSystemwebRoleManagerProviders providers
    {
        get
        {
            return this.providersField;
        }
        set
        {
            this.providersField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string defaultProvider
    {
        get
        {
            return this.defaultProviderField;
        }
        set
        {
            this.defaultProviderField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool enabled
    {
        get
        {
            return this.enabledField;
        }
        set
        {
            this.enabledField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool cacheRolesInCookie
    {
        get
        {
            return this.cacheRolesInCookieField;
        }
        set
        {
            this.cacheRolesInCookieField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string cookieName
    {
        get
        {
            return this.cookieNameField;
        }
        set
        {
            this.cookieNameField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public byte cookieTimeout
    {
        get
        {
            return this.cookieTimeoutField;
        }
        set
        {
            this.cookieTimeoutField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string cookiePath
    {
        get
        {
            return this.cookiePathField;
        }
        set
        {
            this.cookiePathField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool cookieRequireSSL
    {
        get
        {
            return this.cookieRequireSSLField;
        }
        set
        {
            this.cookieRequireSSLField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool cookieSlidingExpiration
    {
        get
        {
            return this.cookieSlidingExpirationField;
        }
        set
        {
            this.cookieSlidingExpirationField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string cookieProtection
    {
        get
        {
            return this.cookieProtectionField;
        }
        set
        {
            this.cookieProtectionField = value;
        }
    }
}

[ExcludeFromCodeCoverage]
/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class configurationSystemwebRoleManagerProviders
{

    private configurationSystemwebRoleManagerProvidersAdd addField;

    /// <remarks/>
    public configurationSystemwebRoleManagerProvidersAdd add
    {
        get
        {
            return this.addField;
        }
        set
        {
            this.addField = value;
        }
    }
}

[ExcludeFromCodeCoverage]
/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class configurationSystemwebRoleManagerProvidersAdd
{

    private string nameField;

    private string typeField;

    private string connectionStringNameField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string name
    {
        get
        {
            return this.nameField;
        }
        set
        {
            this.nameField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string type
    {
        get
        {
            return this.typeField;
        }
        set
        {
            this.typeField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string connectionStringName
    {
        get
        {
            return this.connectionStringNameField;
        }
        set
        {
            this.connectionStringNameField = value;
        }
    }
}

[ExcludeFromCodeCoverage]
/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class configurationSystemwebServer
{

    private configurationSystemwebServerValidation validationField;

    private configurationSystemwebServerModules modulesField;

    /// <remarks/>
    public configurationSystemwebServerValidation validation
    {
        get
        {
            return this.validationField;
        }
        set
        {
            this.validationField = value;
        }
    }

    /// <remarks/>
    public configurationSystemwebServerModules modules
    {
        get
        {
            return this.modulesField;
        }
        set
        {
            this.modulesField = value;
        }
    }
}

[ExcludeFromCodeCoverage]
/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class configurationSystemwebServerValidation
{

    private bool validateIntegratedModeConfigurationField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool validateIntegratedModeConfiguration
    {
        get
        {
            return this.validateIntegratedModeConfigurationField;
        }
        set
        {
            this.validateIntegratedModeConfigurationField = value;
        }
    }
}

[ExcludeFromCodeCoverage]
/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class configurationSystemwebServerModules
{

    private bool runAllManagedModulesForAllRequestsField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool runAllManagedModulesForAllRequests
    {
        get
        {
            return this.runAllManagedModulesForAllRequestsField;
        }
        set
        {
            this.runAllManagedModulesForAllRequestsField = value;
        }
    }
}

[ExcludeFromCodeCoverage]
/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class configurationRuntime
{

    private assemblyBinding assemblyBindingField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Namespace = "urn:schemas-microsoft-com:asm.v1")]
    public assemblyBinding assemblyBinding
    {
        get
        {
            return this.assemblyBindingField;
        }
        set
        {
            this.assemblyBindingField = value;
        }
    }
}

[ExcludeFromCodeCoverage]
/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:schemas-microsoft-com:asm.v1")]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "urn:schemas-microsoft-com:asm.v1", IsNullable = false)]
public partial class assemblyBinding
{

    private assemblyBindingDependentAssembly dependentAssemblyField;

    /// <remarks/>
    public assemblyBindingDependentAssembly dependentAssembly
    {
        get
        {
            return this.dependentAssemblyField;
        }
        set
        {
            this.dependentAssemblyField = value;
        }
    }
}

[ExcludeFromCodeCoverage]
/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:schemas-microsoft-com:asm.v1")]
public partial class assemblyBindingDependentAssembly
{

    private assemblyBindingDependentAssemblyAssemblyIdentity assemblyIdentityField;

    private assemblyBindingDependentAssemblyBindingRedirect bindingRedirectField;

    /// <remarks/>
    public assemblyBindingDependentAssemblyAssemblyIdentity assemblyIdentity
    {
        get
        {
            return this.assemblyIdentityField;
        }
        set
        {
            this.assemblyIdentityField = value;
        }
    }

    /// <remarks/>
    public assemblyBindingDependentAssemblyBindingRedirect bindingRedirect
    {
        get
        {
            return this.bindingRedirectField;
        }
        set
        {
            this.bindingRedirectField = value;
        }
    }
}

[ExcludeFromCodeCoverage]
/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:schemas-microsoft-com:asm.v1")]
public partial class assemblyBindingDependentAssemblyAssemblyIdentity
{

    private string nameField;

    private string publicKeyTokenField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string name
    {
        get
        {
            return this.nameField;
        }
        set
        {
            this.nameField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string publicKeyToken
    {
        get
        {
            return this.publicKeyTokenField;
        }
        set
        {
            this.publicKeyTokenField = value;
        }
    }
}

[ExcludeFromCodeCoverage]
/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:schemas-microsoft-com:asm.v1")]
public partial class assemblyBindingDependentAssemblyBindingRedirect
{

    private string oldVersionField;

    private string newVersionField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string oldVersion
    {
        get
        {
            return this.oldVersionField;
        }
        set
        {
            this.oldVersionField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string newVersion
    {
        get
        {
            return this.newVersionField;
        }
        set
        {
            this.newVersionField = value;
        }
    }
}

[ExcludeFromCodeCoverage]
/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class configurationAdd1
{

    private string nameField;

    private string connectionStringField;

    private string providerNameField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string name
    {
        get
        {
            return this.nameField;
        }
        set
        {
            this.nameField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string connectionString
    {
        get
        {
            return this.connectionStringField;
        }
        set
        {
            this.connectionStringField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string providerName
    {
        get
        {
            return this.providerNameField;
        }
        set
        {
            this.providerNameField = value;
        }
    }
}

