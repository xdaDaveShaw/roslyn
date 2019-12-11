﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

#nullable enable

using Microsoft.CodeAnalysis.Editor.Implementation.Debugging;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.ExternalAccess.FSharp.Editor.Implementation.Debugging
{
    // TODO: Should be readonly struct.
    internal sealed class FSharpBreakpointResolutionResult
    {
        internal readonly BreakpointResolutionResult UnderlyingObject;

        private FSharpBreakpointResolutionResult(BreakpointResolutionResult result)
            => UnderlyingObject = result;

        public Document Document => UnderlyingObject.Document;
        public TextSpan TextSpan => UnderlyingObject.TextSpan;
        public string? LocationNameOpt => UnderlyingObject.LocationNameOpt;
        public bool IsLineBreakpoint => UnderlyingObject.IsLineBreakpoint;

        public static FSharpBreakpointResolutionResult CreateSpanResult(Document document, TextSpan textSpan, string? locationNameOpt = null)
            => new FSharpBreakpointResolutionResult(BreakpointResolutionResult.CreateSpanResult(document, textSpan, locationNameOpt));

        public static FSharpBreakpointResolutionResult CreateLineResult(Document document, string? locationNameOpt = null)
            => new FSharpBreakpointResolutionResult(BreakpointResolutionResult.CreateLineResult(document, locationNameOpt));
    }
}
