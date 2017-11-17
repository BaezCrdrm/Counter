using Counter.Logic;
using Counter.Model;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;
using Windows.UI;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml.Media;
using System.Threading.Tasks;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Counter
{
    public sealed partial class MainPage : Page
    {
        private MainPageViewModel _mainModel;
        public MainPageViewModel MainModel { get { return _mainModel; } }

        private int savedIntValue;
        private Validation val;
        private ResourceLoader resourceLoader;
        private CounterItem selectedItem;
        private bool addItem;
        private bool updateItemPane;

        public MainPage()
        {
            this.InitializeComponent();
            txbLabel1.Text = "";
            resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            addItem = true;
            updateItemPane = true;

            var colorBar = Application.Current.Resources["SystemControlForegroundAccentBrush"] as SolidColorBrush;
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationView"))
            {
                ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;

                if (ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.XamlCompositionBrushBase"))
                {
                    CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
                    titleBar.ButtonBackgroundColor = Colors.Transparent;
                    titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

                    grMain.Padding = new Thickness(0,30,0,0);
                }
                else if (titleBar != null)
                {
                    titleBar.ButtonBackgroundColor = colorBar.Color;
                    titleBar.BackgroundColor = colorBar.Color;
                }
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _mainModel = new MainPageViewModel();
            val = new Validation();

            Task.Run(() =>
            {
                GetData();
            });
        }

        private async void GetData()
        {
            _mainModel.GetData();

            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                CleanNewItemData();
                ChangeHeadString();
            
                gvItems.ItemsSource = _mainModel.CounterItemsCollection;
                prProgress.IsActive = false;
            });
        }

        private void ChangeHeadString()
        {
            if (MainModel.CounterItemsCollection.Count != 0)
                txbLabel1.Text = resourceLoader.GetString("YourItems1");
            else
                txbLabel1.Text = resourceLoader.GetString("NoItems1");
        }

        private void ChangeCount_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button _btnTemp = (Button)sender;
                int _btnTag = Int32.Parse((_btnTemp).Tag.ToString());

                _mainModel.ChangeValue(_btnTemp.Name, _btnTag);
                updateItemPane = false;
                txbMessage.Visibility = Visibility.Collapsed;
            } catch (Exception ex)
            {
#if DEBUG
                txbMessage.Visibility = Visibility.Visible;
                txbMessage.Text = "Exception found: " + ex.Message;
#endif
            }
        }

        private void gvItems_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if(updateItemPane == true)
                OpenUpdateDeletePane((GridView)sender);
            updateItemPane = true;
        }

        private void OpenUpdateDeletePane(GridView _gview)
        {
            btnDeleteItem.Visibility = Visibility.Visible;
            addItem = false;
            CounterSplitView.IsPaneOpen = true;
            SetPaneText_UpdateItem();
            selectedItem = (CounterItem)(_gview).SelectedItem;
            SetSelectedItemData();
        }

        /// <summary>
        /// This button opens the Panel.
        /// </summary>
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            CounterSplitView.IsPaneOpen = !CounterSplitView.IsPaneOpen;
            SetPaneText_NewItem();
            btnDeleteItem.Visibility = Visibility.Collapsed;
            addItem = true;
            CleanNewItemData();
        }

        private void CheckValidValue(object sender, KeyRoutedEventArgs e)
        {
            try
            {
                if(txtStartingCount.Text.Trim().Length > 0 && txtStartingCount.Text.Trim() != "-")
                    savedIntValue = Int32.Parse(txtStartingCount.Text);
            } catch(Exception)
            {
                txtStartingCount.Text = savedIntValue.ToString();
            }
        }

        private void LostFocusValidation(object sender, RoutedEventArgs e)
        {
            if (txtStartingCount.Text.Trim() == "" || 
                !val.IsIntNumeric(txtStartingCount.Text.Trim()))
                txtStartingCount.Text = savedIntValue.ToString();
        }

        private void ChangeInitialCountValue_Click(object sender, RoutedEventArgs e)
        {
            Button _btnTemp = (Button)sender;
            if (_btnTemp.Name == "btnUp_InitialCount")
                savedIntValue++;
            else if (_btnTemp.Name == "btnDown_InitialCount")
                savedIntValue--;

            txtStartingCount.Text = savedIntValue.ToString();
        }

        private void btnAccept_Click(object sender, RoutedEventArgs e)
        {
            if(txtItemTitle.Text.Trim() != "")
            {
                btnDeleteItem.Visibility = Visibility.Collapsed;
                if (addItem == true)
                {
                    CounterItem _ci = new CounterItem(MainModel.CounterItemsCollection)
                    {
                        Title = txtItemTitle.Text,
                        Description = txtItemDescription.Text,
                        Count = Int32.Parse(txtStartingCount.Text)
                    };

                    MainModel.CounterItemsCollection.Add(_ci);
                }
                else
                {
                    selectedItem.Title = txtItemTitle.Text;
                    selectedItem.Count = Int32.Parse(txtStartingCount.Text);
                    selectedItem.Description = txtItemDescription.Text;

                    MainModel.ReplaceItem(selectedItem);
                    selectedItem = null;
                } 

                CleanNewItemData();
                CounterSplitView.IsPaneOpen = false;
                ChangeHeadString();
                
                MainModel.SaveData();
            }
        }

        private void btnDeleteItem_Click(object sender, RoutedEventArgs e)
        {
            DisplayDeleteItemDialog();
        }

        private async void DisplayDeleteItemDialog()
        {
            ContentDialog deleteDialog = new ContentDialog
            {
                Title = resourceLoader.GetString("DeleteItemTitle"),
                Content = resourceLoader.GetString("DeleteItemContent"),
                CloseButtonText = resourceLoader.GetString("Close"),
                PrimaryButtonText = resourceLoader.GetString("Delete")
            };

            ContentDialogResult result = await deleteDialog.ShowAsync();
            if(result == ContentDialogResult.Primary)
            {
                MainModel.DeleteItem(selectedItem.ID);
                CounterSplitView.IsPaneOpen = false;
                selectedItem = null;
                CleanNewItemData();
            }
        }

        private void SetPaneText_NewItem()
        {
            txbItemDetailsHeader.Text = resourceLoader.GetString("NewItemHeader");
            txbItemDetailsTitle.Text = resourceLoader.GetString("SetNewItemTitle");
            txbItemDetailsCount.Text = resourceLoader.GetString("SetItemStartingCount");
            txbItemDetailsDescription.Text = resourceLoader.GetString("SetNewItemDescription");
        }

        private void SetPaneText_UpdateItem()
        {
            txbItemDetailsHeader.Text = resourceLoader.GetString("UpdateItemHeader");
            txbItemDetailsTitle.Text = resourceLoader.GetString("UpdateItemDetailsTitle");
            txbItemDetailsCount.Text = resourceLoader.GetString("CurrentItemCount");
            txbItemDetailsDescription.Text = resourceLoader.GetString("Description");
        }

        private void SetSelectedItemData()
        {
            txtItemTitle.Text = selectedItem.Title;
            savedIntValue = selectedItem.Count;
            txtStartingCount.Text = selectedItem.Count.ToString();
            txtItemDescription.Text = selectedItem.Description;
        }

        private void CleanNewItemData()
        {
            savedIntValue = 0;
            txtStartingCount.Text = savedIntValue.ToString();
            txtItemTitle.Text = "";
            txtItemDescription.Text = "";
        }

        private void Pane_Closed(SplitView sender, object args)
        {
            btnAdd.Content = "\uE109";
        }

        private void Pane_Opened(SplitView sender, object args)
        {
            btnAdd.Content = "\uE017";
        }
    }
}
