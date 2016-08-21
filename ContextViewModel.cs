using BRK.Common.Domain;
using BRK.Common.Globalization;
using BRK.Common.ViewModels;
using ScrapServer.Domain;
using ScrapServer.Enums;
using ScrapServer.ViewModels.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ScrapServer.ViewModels.Context
{
    public abstract class ContextViewModel : SortableViewModel
    {
        private DelegateCommand defaultCommand = new DelegateCommand(executeDefault, canExecuteDefault);

        private static void executeDefault(object obj)
        {
            RootViewModel.ScrapServerViewModel.DisplayMessage(new Message(Resources.actionExecuteFailure, MessageStatus.Notice));
        }

        private static bool canExecuteDefault(object obj)
        {
            return false;
        }

        public string DisplayName { get; protected set; }

        public string ImageSource { get; protected set; }

        public string Header { get; protected set; }

        public ContextActions Actions { get; protected set; }        

        public ContextViewModel(ContextType contextType, string ImageSource, string Header = "")
        {
            this.DisplayName = contextType != ContextType.Empty ? contextType.ToString() : "";
            this.Header = contextType != ContextType.Empty && !string.IsNullOrEmpty(Header) ? Header : Resources.action;

            this.ImageSource = ImageSource;

            Actions = new ContextActions(contextType);
        }

        public virtual void LoadViewModelData(bool forceRefresh = false)
        {
            RootViewModel.ScrapServerViewModel.DisplayMessage(new Message(Resources.loadViewModelDataFailure, MessageStatus.Warning));
        }

        public virtual bool IsDirty { get { return false; } }

        public virtual DelegateCommand AddCommand { get { return defaultCommand; } }
        public virtual DelegateCommand DeleteCommand { get { return defaultCommand; } }
        public virtual DelegateCommand SaveCommand { get { return defaultCommand; } }
        public virtual DelegateCommand SaveAllCommand { get { return defaultCommand; } }
        public virtual DelegateCommand DiscardCommand { get { return defaultCommand; } }
        public virtual DelegateCommand DiscardAllCommand { get { return defaultCommand; } }
        public virtual DelegateCommand PrintCommand { get { return defaultCommand; } }
        public virtual DelegateCommand RefreshCommand { get { return defaultCommand; } }
    }
}
