using System;

using Blazored.Modal;
using Blazored.Modal.Services;

namespace Catalog.Client.Pages
{
    public static class ModalServiceExtensions
    {
        public static IModalReference ShowMessageBox(this IModalService modalService, string message, string title,
            string? yesText = null, string? noText = null, string? cancelText = null)
        {
            ModalParameters parameters = new();
            parameters.Add(nameof(MessageBox.Message), message);
            parameters.Add(nameof(MessageBox.Title), title);
            parameters.Add(nameof(MessageBox.YesText), yesText);
            parameters.Add(nameof(MessageBox.NoText), noText);
            parameters.Add(nameof(MessageBox.CancelText), cancelText);

            return modalService.Show<MessageBox>(title, parameters);
        }
    }
}

