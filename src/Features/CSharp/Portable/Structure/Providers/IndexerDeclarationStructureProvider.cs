﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Structure;

namespace Microsoft.CodeAnalysis.CSharp.Structure
{
    internal class IndexerDeclarationStructureProvider : AbstractSyntaxNodeStructureProvider<IndexerDeclarationSyntax>
    {
        protected override void CollectBlockSpans(
            IndexerDeclarationSyntax indexerDeclaration,
            ArrayBuilder<BlockSpan> spans,
            bool isMetadataAsSource,
            OptionSet options,
            CancellationToken cancellationToken)
        {
            CSharpStructureHelpers.CollectCommentBlockSpans(indexerDeclaration, spans, isMetadataAsSource);

            // fault tolerance
            if (indexerDeclaration.AccessorList == null ||
                indexerDeclaration.AccessorList.IsMissing ||
                indexerDeclaration.AccessorList.OpenBraceToken.IsMissing ||
                indexerDeclaration.AccessorList.CloseBraceToken.IsMissing)
            {
                return;
            }

            SyntaxNodeOrToken current = indexerDeclaration;
            var nextSibling = current.GetNextSibling();

            spans.AddIfNotNull(CSharpStructureHelpers.CreateBlockSpan(
                indexerDeclaration,
                indexerDeclaration.ParameterList.GetLastToken(includeZeroWidth: true),
                compressEmptyLines: !nextSibling.IsNode || nextSibling.IsKind(SyntaxKind.IndexerDeclaration) || nextSibling.IsKind(SyntaxKind.PropertyDeclaration),
                autoCollapse: true,
                type: BlockTypes.Member,
                isCollapsible: true));
        }
    }
}
