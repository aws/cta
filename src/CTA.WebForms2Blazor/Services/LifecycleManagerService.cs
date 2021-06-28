using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Codelyzer.Analysis.Model;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CTA.WebForms2Blazor.Services
{
    public class LifecycleManagerService
    {
        // Determine if method is app lifecycle hook, page lifecycle hook, or neither
        // Register events so we can retrieve an ordered list of them
        // Determine if code goes before or after middleware next() call
        // Get closest blazor page lifecycle function for an old lifecycle hook

        private int _expectedMiddlewareClasses;
        private int _numMiddlewareClasses;
        
        public void NotifyExpectedMiddleware()
        {
            _expectedMiddlewareClasses += 1;
        }

        public void RegisterInlineEventHook(MethodDeclaration lifecycleHookMethod)
        {
            throw new NotImplementedException();
        }

        public void RegisterHandlerEvent(MethodDeclaration lifecycleHookMethod, string namespaceName)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<StatementSyntax>> GetMiddlewareRegistrations(CancellationToken token)
        {
            return Enumerable.Empty<StatementSyntax>();
        }

        public Task<bool> WaitForAllMiddlewareRegistered(CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
