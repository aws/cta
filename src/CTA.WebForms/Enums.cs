using System;
using System.Collections.Generic;
using System.Text;

namespace CTA.WebForms
{
    public enum WebFormsAppLifecycleEvent
    {
        BeginRequest,
        AuthenticateRequest,
        PostAuthenticateRequest,
        AuthorizeRequest,
        PostAuthorizeRequest,
        ResolveRequestCache,
        PostResolveRequestCache,
        MapRequestHandler,
        PostMapRequestHandler,
        AcquireRequestState,
        PostAcquireRequestState,
        PreRequestHandlerExecute,
        RequestHandlerExecute,
        PostRequestHandlerExecute,
        ReleaseRequestState,
        PostReleaseRequestState,
        UpdateRequestCache,
        PostUpdateRequestCache,
        LogRequest,
        PostLogRequest,
        EndRequest,
        PreSendRequestHeaders,
        PreSendRequestContent
    }

    public enum WebFormsPageLifecycleEvent
    {
        PreInit,
        Init,
        InitComplete,
        PreLoad,
        Load,
        LoadComplete,
        PreRender,
        PreRenderComplete,
        SaveStateComplete,
        Render,
        Unload
    }

    public enum BlazorComponentLifecycleEvent
    {
        SetParametersAsync,
        OnInitialized,
        OnParametersSet,
        OnAfterRender,
        Dispose
    }

    public enum DirectiveMigrationResultType
    {
        UsingDirective,
        GeneralDirective,
        Comment,
        HTMLNode
    }

    public enum AttributeTargetTypes
    {
        String,
        HtmlBoolean,
        ComponentBoolean,
        InvertedHtmlBoolean,
        InvertedComponentBoolean,
        EventCallback,
        EventHandler
    }
}
