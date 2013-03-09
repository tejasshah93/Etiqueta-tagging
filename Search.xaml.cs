using System;
using System.Collections.Generic;
using System.Text;
using Windows.Storage;
using Windows.Storage.BulkAccess;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.Storage.Search;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.IO;

using Windows.Storage;
using Windows.Storage.BulkAccess;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Data;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Integrate
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class Search : LayoutAwarePage
    {
        MainPage rootPage = MainPage.Current;

        public Search()
        {
            this.InitializeComponent();
        }

        private void gw_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            StorageFolder musicFolder = KnownFolders.PicturesLibrary;

            List<string> fileTypeFilter = new List<string>();
            fileTypeFilter.Add("*");

            QueryOptions queryOptions = new QueryOptions(CommonFileQuery.OrderBySearchRank, fileTypeFilter);
            //use the user's input to make a query
            queryOptions.UserSearchFilter = InputTextBox.Text;
            StorageFileQueryResult queryResult = musicFolder.CreateFileQueryWithOptions(queryOptions);

            StringBuilder outputText = new StringBuilder();

            //find all files that match the query
            IReadOnlyList<StorageFile> files = await queryResult.GetFilesAsync();
            //output how many files that match the query were found
            if (files.Count == 0)
            {
                outputText.Append("No files found for '" + queryOptions.UserSearchFilter + "'");
            }
            else if (files.Count == 1)
            {
                outputText.Append(files.Count + " file found:\n\n");
            }
            else
            {
                outputText.Append(files.Count + " files found:\n\n");
            }

            //output the name of each file that matches the query
            foreach (StorageFile file in files)
            {
                outputText.Append(file.Name + "\n");
                if (file != null)
                {
                    StorageFile fileCopy = await file.CopyAsync(KnownFolders.VideosLibrary, file.Name, NameCollisionOption.ReplaceExisting);
                    OutputTextBlock.Text = "The file '" + file.Name + "' was copied and the new file was named '" + fileCopy.Name + "'.";
                }
            }
            OutputTextBlock.Text = outputText.ToString();
            //rootPage.ResetScenarioOutput(OutputTextBlock);

           // if (rootPage.EnsureUnsnapped())
            {
                FileOpenPicker openPicker = new FileOpenPicker();
                openPicker.ViewMode = PickerViewMode.Thumbnail;
                openPicker.SuggestedStartLocation = PickerLocationId.VideosLibrary;                
                openPicker.FileTypeFilter.Add(".jpg");
                openPicker.FileTypeFilter.Add(".jpeg");
                openPicker.FileTypeFilter.Add(".png");
                StorageFile file = await openPicker.PickSingleFileAsync();
                if (file != null)
                {
                    // Application now has read/write access to the picked file
                    OutputTextBlock.Text = "Picked photo: " + file.Name;
                }
                else
                {
                    
                    {
                        //  rootPage.ResetScenarioOutput(OutputTextBlock);
                        foreach (StorageFile file1 in files)
                        {
                            if (file1 != null)
                            {
                                string filename = file1.Name;
                                await file1.DeleteAsync();
                                //        rootPage.sampleFile = null;
                                //       OutputTextBlock.Text = "The file '" + filename + "' was deleted";
                            }
                        }

                        OutputTextBlock.Text = "Operation cancelled.";
                    }
                }
            }
            
        }
        

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string name = e.Parameter as string;            
            if (!string.IsNullOrWhiteSpace(name))
            {
                InputTextBox.Text = name;
                SearchButton_Click(null, null);
            }
            else
            {
                InputTextBox.Text = "";
            }
        }

        private void Button_Click_1(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }


    }
}
