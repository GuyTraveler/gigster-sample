using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BRK.Common.Domain;
using ScrapServer.Enums;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using ScrapServer.ViewModels.Viewable;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ScrapServer.Utility;
using BRK.Common.Services;
using BRK.Common.Globalization;

namespace ScrapServer.ViewModels.Context
{
    public class TicketViewModel : ContextViewModel
    {
        private ObservableCollection<ViewableTicketViewModel> viewableTickets = new ObservableCollection<ViewableTicketViewModel>();
        private ViewableTicketViewModel selectedViewableTicket;
        private static int cacheSize = 10;
        private OrderedDictionary viewableTicketCache = new OrderedDictionary(cacheSize);
        private string html = string.Empty;

        private DelegateCommand deleteCommand;
        private DelegateCommand saveCommand;
        private DelegateCommand saveAllCommand;
        private DelegateCommand discardCommand;
        private DelegateCommand discardAllCommand;
        private DelegateCommand refreshCommand;
        private DelegateCommand printCommand;

        public ObservableCollection<ViewableTicketViewModel> ViewableTickets
        {
            get { return viewableTickets; }
            set
            {
                viewableTickets = value;

                this.UpdateSortableData(viewableTickets);

                this.OnPropertyChanged("ViewableTickets");
            }
        }

        public ViewableTicketViewModel SelectedViewableTicket
        {
            get { return selectedViewableTicket; }
            set
            {
                selectedViewableTicket = value;

                if (value != null)
                {
                    GetViewableTicket(value);
                }

                this.OnPropertyChanged("SelectedViewableTicket");
            }
        }

        public string Html
        {
            get { return html; }
            set
            {
                html = value;

                this.OnPropertyChanged("Html");
            }
        }

        public override bool IsDirty
        {
            get
            {
                return this.selectedViewableTicket != null && this.selectedViewableTicket.EditableTicket.IsDirty || this.viewableTickets.Where(x => x.EditableTicket.IsDirty == true).Any();
            }
        }

        public override DelegateCommand DeleteCommand
        {
            get
            {
                if (this.deleteCommand == null)
                    this.deleteCommand = new DelegateCommand(this.DeleteTicket, this.CanDelete);

                return this.deleteCommand;
            }
        }

        public override DelegateCommand SaveCommand
        {
            get
            {
                if (this.saveCommand == null)
                    this.saveCommand = new DelegateCommand(this.SaveTicket, this.CanSave);

                return this.saveCommand;
            }
        }

        public override DelegateCommand SaveAllCommand
        {
            get
            {
                if (this.saveAllCommand == null)
                    this.saveAllCommand = new DelegateCommand(this.SaveAllTicket, this.CanSaveAll);

                return this.saveAllCommand;
            }
        }

        public override DelegateCommand DiscardCommand
        {
            get
            {
                if (this.discardCommand == null)
                    this.discardCommand = new DelegateCommand(this.DiscardTicket, this.CanDiscard);

                return this.discardCommand;
            }
        }

        public override DelegateCommand DiscardAllCommand
        {
            get
            {
                if (this.discardAllCommand == null)
                    this.discardAllCommand = new DelegateCommand(this.DiscardAllTicket, this.CanDiscardAll);

                return this.discardAllCommand;
            }
        }

        public override DelegateCommand RefreshCommand
        {
            get
            {
                if (this.refreshCommand == null)
                    this.refreshCommand = new DelegateCommand(this.RefreshTicket);

                return this.refreshCommand;
            }
        }

        public override DelegateCommand PrintCommand
        {
            get
            {
                if (this.printCommand == null)
                    this.printCommand = new DelegateCommand(this.PrintTicket, this.CanPrint);

                return this.printCommand;
            }
        }

        public TicketViewModel()
            : base(ContextType.Ticket, "/BRK.Common;component/Resources/Images/Ticket.png")
        {
        }

        private async void LoadTickets()
        {
            RootViewModel.ScrapServerViewModel.IsBusy = true;
            RootViewModel.ScrapServerViewModel.BusyMessage = Resources.loadingTickets;

            Tickets getAllTicketsResult = await RootViewModel.ScrapServerViewModel.DataAccessor.GetAllTickets();

            ViewableTickets = getAllTicketsResult != null ? new ObservableCollection<ViewableTicketViewModel>(getAllTicketsResult.Select(x => new ViewableTicketViewModel(x))) : new ObservableCollection<ViewableTicketViewModel>();

            RootViewModel.ScrapServerViewModel.IsBusy = false;
        }

        public override void LoadViewModelData(bool forceRefresh = false)
        {
            if (forceRefresh || viewableTickets == null || viewableTickets.Count == 0)
            {
                LoadTickets();
            }
        }

        private async void DeleteTicket(object param)
        {
            string messageBoxText = Resources.removeTicketPrompt;
            string caption = Resources.removeTicket;
            MessageBoxButton button = MessageBoxButton.YesNo;
            MessageBoxImage icon = MessageBoxImage.Question;

            MessageBoxResult result = MessageBox.Show(messageBoxText, caption, button, icon);

            if (result == MessageBoxResult.Yes)
            {
                RootViewModel.ScrapServerViewModel.IsBusy = true;
                RootViewModel.ScrapServerViewModel.BusyMessage = Resources.deletingTicket;

                bool? removeResult = await RootViewModel.ScrapServerViewModel.DataAccessor.RemoveTickets(this.selectedViewableTicket.Ticket);

                if (removeResult == true)
                {
                    ViewableTickets.Remove(SelectedViewableTicket);
                    SelectedViewableTicket = null;

                    RootViewModel.ScrapServerViewModel.DisplayMessage(new Message(Resources.ticketRemovalSuccess, MessageStatus.Success));
                }

                else
                {
                    RootViewModel.ScrapServerViewModel.DisplayMessage(new Message(Resources.ticketRemovalFailure, MessageStatus.Error));
                }

                RootViewModel.ScrapServerViewModel.IsBusy = false;
            }
        }

        private bool CanDelete(object param)
        {
            return this.selectedViewableTicket != null;
        }

        private async void SaveTicket(object param)
        {
            RootViewModel.ScrapServerViewModel.IsBusy = true;
            RootViewModel.ScrapServerViewModel.BusyMessage = Resources.savingTicket;

            bool? saveResult = await SaveChanges(this.selectedViewableTicket);

            if (saveResult == true)
            {
                RootViewModel.ScrapServerViewModel.DisplayMessage(new Message(Resources.ticketSaveSuccess, MessageStatus.Success));
            }

            else
            {
                RootViewModel.ScrapServerViewModel.DisplayMessage(new Message(Resources.ticketSaveFailure, MessageStatus.Error));
            }

            RootViewModel.ScrapServerViewModel.IsBusy = false;
        }

        private bool CanSave(object param)
        {
            return this.selectedViewableTicket != null && this.selectedViewableTicket.EditableTicket.IsDirty;
        }

        private async void SaveAllTicket(object obj)
        {
            RootViewModel.ScrapServerViewModel.IsBusy = true;
            RootViewModel.ScrapServerViewModel.BusyMessage = Resources.savingTickets;

            bool? saveResult = await SaveAllChanges();

            if (saveResult == true)
            {
                RootViewModel.ScrapServerViewModel.DisplayMessage(new Message(Resources.ticketsSaveSuccess, MessageStatus.Success));
            }

            else
            {
                RootViewModel.ScrapServerViewModel.DisplayMessage(new Message(Resources.ticketsSaveFailure, MessageStatus.Error));
            }

            RootViewModel.ScrapServerViewModel.IsBusy = false;
        }

        private async Task<bool> SaveAllChanges()
        {
            var dirtyTickets = this.viewableTickets.Where(x => x.EditableTicket.IsDirty == true).ToList();
            bool result = true;

            foreach (var dirtyTicket in dirtyTickets)
            {
                var changesResult = await SaveChanges(dirtyTicket);

                if (changesResult != true)
                {
                    result = false;
                }
            }

            return result;
        }

        private async Task<bool?> SaveChanges(ViewableTicketViewModel viewableTicket)
        {
            bool? result = await RootViewModel.ScrapServerViewModel.DataAccessor.UpdateTickets(viewableTicket.GetEditedTicket());

            if (result == true)
            {
                viewableTicket.UpdateTicket();
            }

            return result;
        }

        private bool CanSaveAll(object obj)
        {
            return this.viewableTickets.Where(x => x.EditableTicket.IsDirty == true).Any();
        }

        private void DiscardTicket(object obj)
        {
            DiscardChanges(this.selectedViewableTicket);
        }

        private bool CanDiscard(object obj)
        {
            return this.selectedViewableTicket != null && this.selectedViewableTicket.EditableTicket.IsDirty;
        }

        private void DiscardAllTicket(object obj)
        {
            DiscardAllChanges();
        }

        private bool CanDiscardAll(object obj)
        {
            return this.viewableTickets.Where(x => x.EditableTicket.IsDirty == true).Any();
        }

        private void DiscardAllChanges()
        {
            var dirtyTickets = this.viewableTickets.Where(x => x.EditableTicket.IsDirty == true).ToList();

            foreach (var dirtyTicket in dirtyTickets)
            {
                DiscardChanges(dirtyTicket);
            }
        }

        private void DiscardChanges(ViewableTicketViewModel viewableTicket)
        {
            viewableTicket.DiscardChanges();
        }

        private void RefreshTicket(object obj)
        {
            this.LoadViewModelData(true);
        }

        private void PrintTicket(object obj)
        {
            SHDocVw.IWebBrowser2 webBrowser = WebBrowserCastingService.Cast(GetWebBrowserControl(obj));

            if (webBrowser != null)
            {
                if (RootViewModel.ScrapServerViewModel.ConfigurationData.ShowPrintDialog)
                {
                    webBrowser.ExecWB(SHDocVw.OLECMDID.OLECMDID_PRINT, SHDocVw.OLECMDEXECOPT.OLECMDEXECOPT_PROMPTUSER);
                }
                else
                {
                    webBrowser.ExecWB(SHDocVw.OLECMDID.OLECMDID_PRINT, SHDocVw.OLECMDEXECOPT.OLECMDEXECOPT_DONTPROMPTUSER);
                }
            }
        }

        private bool CanPrint(object param)
        {
            return this.selectedViewableTicket != null;
        }

        private WebBrowser GetWebBrowserControl(object obj)
        {
            return FindVisualChild<WebBrowser>(VisualTreeHelper.GetParent(obj as DependencyObject));
        }

        private childItem FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);

                if (child != null && child is childItem)
                {
                    return (childItem)child;
                }
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);

                    if (childOfChild != null)
                    {
                        return childOfChild;
                    }
                }
            }

            return null;
        }

        private async void GetViewableTicket(ViewableTicketViewModel viewableTicketViewModel)
        {
            if (viewableTicketViewModel != null)
            {
                RootViewModel.ScrapServerViewModel.IsBusy = true;
                RootViewModel.ScrapServerViewModel.BusyMessage = Resources.gettingViewableTicketContent;

                if (viewableTicketCache.Count > 0 && viewableTicketCache.Contains(viewableTicketViewModel.Ticket_ID))
                {
                    Html = viewableTicketCache[viewableTicketViewModel.Ticket_ID] as string;
                }
                else
                {
                    ViewableTicket viewableTicket = await RootViewModel.ScrapServerViewModel.DataAccessor.GetViewableTicket(viewableTicketViewModel.Ticket);

                    if (viewableTicket != null && RootViewModel.ScrapServerViewModel.ConfigurationData != null)
                    {
                        ConfigurationData configData = RootViewModel.ScrapServerViewModel.ConfigurationData;

                        Html = TicketViewerService.GetHTML(viewableTicket, configData.CompanyName, configData.CompanyLocation);

                        if (viewableTicketCache.Count == cacheSize)
                        {
                            viewableTicketCache.RemoveAt(0);
                        }

                        viewableTicketCache.Add(viewableTicketViewModel.Ticket_ID, Html);
                    }
                }

                RootViewModel.ScrapServerViewModel.IsBusy = false;
            }
        }
    }
}
