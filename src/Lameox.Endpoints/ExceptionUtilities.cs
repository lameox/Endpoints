﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;

namespace Lameox.Endpoints
{
    internal static class ExceptionUtilities
    {
        public static Exception Unreachable()
        {
            return new InvalidOperationException("This place in the code is thought to be unreachable.");
        }

        public static Exception UnexpectedValue<TValue>(TValue value)
        {
            return new InvalidOperationException($"Encountered unexpected value of type {typeof(TValue)}: {value}.");
        }

        internal static Exception DidNotAddEndpoints()
        {
            return new InvalidOperationException($"{nameof(StartupExtensions.MapSimpleEndpoints)}() was called without previously calling {nameof(StartupExtensions.AddSimpleEndpoints)}() which is required.");
        }

        internal static Exception DontCallBaseMethodsInHandlers(Type endpointType)
        {
            return new InvalidOperationException($"Bad Endpoint {endpointType.FullName}: Don't call base methods in your overrides of the request handling methods.");
        }

        internal static Exception BadOverridesInEndpoint(Type endpointType)
        {
            return new InvalidOperationException($"Bad Endpoint {endpointType.FullName}: You must override exactly one of the request handling methods of the endpoint.");
        }

        internal static Exception NoPermissionClaimType(Type endpointType)
        {
            return new InvalidOperationException($"Bad Endpoint {endpointType.FullName}: The endpoint has specified required permission claims but no permission claim type.");
        }

        internal static Exception MissingAuthenticationMiddleware(Type endpointType)
        {
            return new InvalidOperationException(
                $"The endpoint {endpointType.FullName} has authorization metadata but no authorization middleware has run. This indicates a missing middleware registration." +
                $"{Environment.NewLine}" +
                $"Ensure you have called {nameof(AuthorizationAppBuilderExtensions.UseAuthorization)}() before {nameof(StartupExtensions.UseSimpleEndpoints)}().");
        }

        internal static Exception MissingCorsMiddleware(Type endpointType)
        {
            return new InvalidOperationException($"The endpoint {endpointType.FullName} has CORS metadata but no CORS middleware has run. This indicates a missing middleware registration." +
                $"{Environment.NewLine}" +
                $"Ensure you have called {nameof(CorsMiddlewareExtensions.UseCors)}() before {nameof(StartupExtensions.UseSimpleEndpoints)}().");
        }

        internal static Exception VerbsDefinedAsAnonymousAndAuthenticated(HttpVerb invalidVerbs)
        {
            return new InvalidOperationException($"The following verbs are specified as both requiring authorization and anonymous: {invalidVerbs}.");
        }
    }
}
