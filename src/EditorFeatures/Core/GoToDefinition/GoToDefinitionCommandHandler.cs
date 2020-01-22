﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.ComponentModel.Composition;
using Microsoft.CodeAnalysis.Editor.Shared.Extensions;
using Microsoft.CodeAnalysis.Notification;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.CodeAnalysis.Editor.GoToDefinition
{
    [Export(typeof(ICommandHandler))]
    [ContentType(ContentTypeNames.RoslynContentType)]
    [Name(PredefinedCommandHandlerNames.GoToDefinition)]
    internal class GoToDefinitionCommandHandler :
        ICommandHandler<GoToDefinitionCommandArgs>
    {
        [ImportingConstructor]
        public GoToDefinitionCommandHandler()
        {
        }

        public string DisplayName => EditorFeaturesResources.Go_to_Definition;

        private (Document, IGoToDefinitionService) GetDocumentAndService(ITextSnapshot snapshot)
        {
            var document = snapshot.GetOpenDocumentInCurrentContextWithChanges();
            return (document, document?.GetLanguageService<IGoToDefinitionService>());
        }

        public CommandState GetCommandState(GoToDefinitionCommandArgs args)
        {
            var (_, service) = GetDocumentAndService(args.SubjectBuffer.CurrentSnapshot);
            return service != null
                ? CommandState.Available
                : CommandState.Unspecified;
        }

        public bool ExecuteCommand(GoToDefinitionCommandArgs args, CommandExecutionContext context)
        {
            var subjectBuffer = args.SubjectBuffer;
            var (document, service) = GetDocumentAndService(subjectBuffer.CurrentSnapshot);

            // Prevent the remote workspace from handling go to definition commands
            if (service != null && document.Project.Solution.Workspace.Kind != WorkspaceKind.AnyCodeRoslynWorkspace)
            {
                var caretPos = args.TextView.GetCaretPoint(subjectBuffer);
                if (caretPos.HasValue && TryExecuteCommand(document, caretPos.Value, service, context))
                {
                    return true;
                }
            }

            return false;
        }

        // Internal for testing purposes only.
        internal bool TryExecuteCommand(ITextSnapshot snapshot, int caretPosition, CommandExecutionContext context)
            => TryExecuteCommand(snapshot.GetOpenDocumentInCurrentContextWithChanges(), caretPosition, context);

        internal bool TryExecuteCommand(Document document, int caretPosition, CommandExecutionContext context)
            => TryExecuteCommand(document, caretPosition, document.GetLanguageService<IGoToDefinitionService>(), context);

        internal bool TryExecuteCommand(Document document, int caretPosition, IGoToDefinitionService goToDefinitionService, CommandExecutionContext context)
        {
            string errorMessage = null;

            using (context.OperationContext.AddScope(allowCancellation: true, EditorFeaturesResources.Navigating_to_definition))
            {
                if (goToDefinitionService != null &&
                    goToDefinitionService.TryGoToDefinition(document, caretPosition, context.OperationContext.UserCancellationToken))
                {
                    return true;
                }

                errorMessage = EditorFeaturesResources.Cannot_navigate_to_the_symbol_under_the_caret;
            }

            if (errorMessage != null)
            {
                // We are about to show a modal UI dialog so we should take over the command execution
                // wait context. That means the command system won't attempt to show its own wait dialog 
                // and also will take it into consideration when measuring command handling duration.
                context.OperationContext.TakeOwnership();
                var workspace = document.Project.Solution.Workspace;
                var notificationService = workspace.Services.GetService<INotificationService>();
                notificationService.SendNotification(errorMessage, title: EditorFeaturesResources.Go_to_Definition, severity: NotificationSeverity.Information);
            }

            return true;
        }
    }
}
