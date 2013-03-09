//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

using Integrate;

using System;
using System.Text;
using System.Collections.Generic;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Storage.FileProperties;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Integrate
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class multi : LayoutAwarePage
    {
        const int E_INVALIDARG = unchecked((int)0x80070057);
        StorageItemAccessList m_futureAccess = StorageApplicationPermissions.FutureAccessList;
        IReadOnlyList<StorageFile> files;
        IPropertySet m_localSettings = Windows.Storage.ApplicationData.Current.LocalSettings.Values;
        string m_fileToken;

        MainPage rootPage = MainPage.Current;

        public multi()
        {
            this.InitializeComponent();
            PickFilesButton.Click += new RoutedEventHandler(PickFilesButton_Click);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            App.Current.Suspending += new SuspendingEventHandler(SaveDataToPersistedState);

            // Reset scenario state before starting.
            ResetSessionState();

            // Attempt to load the previously saved scenario state.
            if (m_localSettings.ContainsKey("scenario1FileToken"))
            {
                RestoreDataFromPersistedState();
            }
        }

        private void SaveDataToPersistedState(object sender, SuspendingEventArgs args)
        {
            // Only save state if we have valid data.
            if (m_fileToken != null)
            {
                // Requesting a deferral prevents the application from being immediately suspended.
                SuspendingDeferral deferral = args.SuspendingOperation.GetDeferral();

                // LocalSettings does not support overwriting existing items, so first clear the collection.
                ResetPersistedState();

                /*m_localSettings.Add("scenario1Title", TitleTextbox.Text);
                m_localSettings.Add("scenario1Keywords", KeywordsTextbox.Text);
                m_localSettings.Add("scenario1DateTaken", DateTakenTextblock.Text);
                m_localSettings.Add("scenario1Make", MakeTextblock.Text);
                m_localSettings.Add("scenario1Model", ModelTextblock.Text);
                m_localSettings.Add("scenario1Orientation", OrientationTextblock.Text);
                              m_localSettings.Add("scenario1LatDeg", LatDegTextbox.Text);
                              m_localSettings.Add("scenario1LatMin", LatMinTextbox.Text);
                              m_localSettings.Add("scenario1LatSec", LatSecTextbox.Text);
                              m_localSettings.Add("scenario1LatRef", LatRefTextbox.Text);
                              m_localSettings.Add("scenario1LongDeg", LongDegTextbox.Text);
                              m_localSettings.Add("scenario1LongMin", LongMinTextbox.Text);
                              m_localSettings.Add("scenario1LongSec", LongSecTextbox.Text);
                              m_localSettings.Add("scenario1LongRef", LongRefTextbox.Text);*/
                //m_localSettings.Add("scenario1Exposure", ExposureTextblock.Text);
                // m_localSettings.Add("scenario1FNumber", FNumberTextblock.Text);
                m_localSettings.Add("scenario1FileToken", m_fileToken);

                deferral.Complete();
            }
        }

        private async void RestoreDataFromPersistedState()
        {
            try
            {
         //       rootPage.NotifyUser("Loading image file from persisted state...", NotifyType.StatusMessage);

                m_fileToken = m_localSettings["scenario1FileToken"].ToString();
                StorageFile file = await m_futureAccess.GetFileAsync(m_fileToken);
                ImageProperties m_imageProperties = await file.Properties.GetImagePropertiesAsync();

                Dictionary<string, string> propertyText = new Dictionary<string, string>();
                propertyText.Add("Title", m_localSettings["scenario1Title"].ToString());
                propertyText.Add("Keywords", m_localSettings["scenario1Keywords"].ToString());
                propertyText.Add("DateTaken", m_localSettings["scenario1DateTaken"].ToString());
                propertyText.Add("Make", m_localSettings["scenario1Make"].ToString());
                propertyText.Add("Model", m_localSettings["scenario1Model"].ToString());
                propertyText.Add("Orientation", m_localSettings["scenario1Orientation"].ToString());
                propertyText.Add("LatDeg", m_localSettings["scenario1LatDeg"].ToString());
                propertyText.Add("LatMin", m_localSettings["scenario1LatMin"].ToString());
                propertyText.Add("LatSec", m_localSettings["scenario1LatSec"].ToString());
                propertyText.Add("LatRef", m_localSettings["scenario1LatRef"].ToString());
                propertyText.Add("LongDeg", m_localSettings["scenario1LongDeg"].ToString());
                propertyText.Add("LongMin", m_localSettings["scenario1LongMin"].ToString());
                propertyText.Add("LongSec", m_localSettings["scenario1LongSec"].ToString());
                propertyText.Add("LongRef", m_localSettings["scenario1LongRef"].ToString());
                propertyText.Add("Exposure", m_localSettings["scenario1Exposure"].ToString());
                propertyText.Add("FNumber", m_localSettings["scenario1FNumber"].ToString());

                //await DisplayImageUIAsync(file, propertyText);

           //     rootPage.NotifyUser("Loaded file from persisted state: " + file.Name, NotifyType.StatusMessage);


            }
            catch (Exception err)
            {
        //        rootPage.NotifyUser("Error: " + err.Message, NotifyType.ErrorMessage);
                ResetSessionState();
                ResetPersistedState();
            }
        }

        private async void PickFilesButton_Click(object sender, RoutedEventArgs e)
        {
            ResetSessionState();
            ResetPersistedState();
            // Clear any previously returned files between iterations of this scenario
         //   rootPage.ResetScenarioOutput(OutputTextBlock);

        //    if (rootPage.EnsureUnsnapped())
            {
                FileOpenPicker openPicker = new FileOpenPicker();
                openPicker.ViewMode = PickerViewMode.List;
                openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                openPicker.FileTypeFilter.Add("*");
                files = await openPicker.PickMultipleFilesAsync();

                if (files.Count > 0)
                {
                    StringBuilder output = new StringBuilder("Picked files:\n");
                    // Application now has read/write access to the picked file(s)
                    foreach (StorageFile file in files)
                    {
                        m_fileToken = m_futureAccess.Add(file);
                        ImageProperties m_imageProperties = await file.Properties.GetImagePropertiesAsync();
                        //     IDictionary<string, object> retrievedProps = await m_imageProperties.RetrievePropertiesAsync(requests);
                        //    await DisplayImageUIAsync(file, GetImagePropertiesForDisplay(retrievedProps));
                        string keywordsText = String.Join(Environment.NewLine, m_imageProperties.Keywords);
                        string[] keywordsArray = keywordsText.Split('*');
                        output.Append(file.Path + "\n");

                        for (int i = 0; i < keywordsArray.Length; i++)
                        {
                            output.Append(keywordsArray[i] + "\n");
                        }
                    }
                    OutputTextBlock.Text = output.ToString();
                }
                else
                {
                    OutputTextBlock.Text = "Operation cancelled.";
                }
            }
        }

        private async void addtag_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            string inp = tagmulti.Text;

            //  if (files.Count > 0)
            {
                StringBuilder output = new StringBuilder("Added Tag:\n");
                foreach (StorageFile file in files)
                {
                    ImageProperties m_imageProperties = await file.Properties.GetImagePropertiesAsync();
                    if (inp.Length > 0)
                    {
                        output.Append(m_imageProperties.Keywords.Count + "\n");
                        output.Append("checking " + file.Name + inp + "\n");
                        m_imageProperties.Keywords.Add(inp + "*");
                        output.Append(m_imageProperties.Keywords.Count + "\n");
                    }
                }


                OutputTextBlock.Text = output.ToString();

            }

        }

        private async void Apply_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
         //   rootPage.NotifyUser("Saving file...", NotifyType.StatusMessage);

            // Keywords are stored as an IList of strings.


            PropertySet propertiesToSave = new PropertySet();

            // Perform some simple validation of the GPS data and package it in a format
            // better suited for writing to the file.
            bool gpsWriteFailed = false;
            double[] latitude = new double[3];
            double[] longitude = new double[3];
            string latitudeRef = String.Empty;
            string longitudeRef = String.Empty;

            try
            {/*
                latitude[0] = Math.Floor(Double.Parse(LatDegTextbox.Text));
                latitude[1] = Math.Floor(Double.Parse(LatMinTextbox.Text));
                latitude[2] = Double.Parse(LatSecTextbox.Text);

                longitude[0] =  Math.Floor(Double.Parse(LongDegTextbox.Text));
                longitude[1] = Math.Floor(Double.Parse(LongMinTextbox.Text));
                longitude[2] = Double.Parse(LongSecTextbox.Text);

                latitudeRef = LatRefTextbox.Text.ToUpper();
                longitudeRef = LongRefTextbox.Text.ToUpper();*/
            }
            catch (Exception) // Treat any exception as invalid GPS data.
            {
                gpsWriteFailed = true;
            }

            if ((latitude[0] >= 0 && latitude[0] <= 90) &&
                (latitude[1] >= 0 && latitude[1] <= 60) &&
                (latitude[2] >= 0 && latitude[2] <= 60) &&
                (latitudeRef == "N" || latitudeRef == "S") &&
                (longitude[0] >= 0 && longitude[0] <= 180) &&
                (latitude[1] >= 0 && longitude[1] <= 60) &&
                (longitude[2] >= 0 && longitude[2] <= 60) &&
                (longitudeRef == "E" || longitudeRef == "W"))
            {
                propertiesToSave.Add("System.GPS.LatitudeRef", latitudeRef);
                propertiesToSave.Add("System.GPS.LongitudeRef", longitudeRef);

                // The Latitude and Longitude properties are read-only. Instead,
                // write to System.GPS.LatitudeNumerator, LatitudeDenominator, etc.
                // These are length 3 arrays of integers. For simplicity, the
                // seconds data is rounded to the nearest 10000th.
                uint[] latitudeNumerator = 
                {
                    (uint) latitude[0],
                    (uint) latitude[1],
                    (uint) (latitude[2] * 10000)
                };

                uint[] longitudeNumerator = 
                {
                    (uint) longitude[0],
                    (uint) longitude[1],
                    (uint) (longitude[2] * 10000)
                };

                // LatitudeDenominator and LongitudeDenominator share the same values.
                uint[] denominator = 
                {
                    1,
                    1,
                    10000
                };

                propertiesToSave.Add("System.GPS.LatitudeNumerator", latitudeNumerator);
                propertiesToSave.Add("System.GPS.LatitudeDenominator", denominator);
                propertiesToSave.Add("System.GPS.LongitudeNumerator", longitudeNumerator);
                propertiesToSave.Add("System.GPS.LongitudeDenominator", denominator);
            }
            else
            {
                gpsWriteFailed = true;
            }
            StringBuilder output = new StringBuilder("trying saving:\n");
            try
            {

                foreach (StorageFile file in files)
                {
                    ImageProperties m_imageProperties = await file.Properties.GetImagePropertiesAsync();
                    // SavePropertiesAsync commits edits to the top level properties (e.g. Title) as
                    // well as any Windows properties contained within the propertwiesToSave parameter.
                    if (tagmulti.Text.Length > 0)
                    {
                        output.Append(m_imageProperties.Keywords.Count + "\n");
                        output.Append("saving tht " + file.Name + tagmulti.Text + "\n");
                        m_imageProperties.Keywords.Add(tagmulti.Text + "*");
                        output.Append(m_imageProperties.Keywords.Count + "\n");
                    }
                    await m_imageProperties.SavePropertiesAsync(propertiesToSave);
                }

            //    rootPage.NotifyUser(gpsWriteFailed ? "GPS data invalid; other properties successfully updated" :
            //"All properties successfully updated", NotifyType.StatusMessage);
            }
            catch (Exception err)
            {
                switch (err.HResult)
                {
                    case E_INVALIDARG:
                        // Some imaging formats, such as PNG and BMP, do not support Windows properties.
                        // Other formats do not support all Windows properties.
                        // For example, JPEG does not support System.FlagStatus.
              //          rootPage.NotifyUser("Error: A property value is invalid, or the file format does " +
               //             "not support one or more requested properties.", NotifyType.ErrorMessage);

                        break;

                    default:
                //        rootPage.NotifyUser("Error: " + err.Message, NotifyType.ErrorMessage);
                        break;
                }

                ResetSessionState();
                ResetPersistedState();
            }

            foreach (StorageFile file in files)
            {
                ImageProperties m_imageProperties = await file.Properties.GetImagePropertiesAsync();
                string keywordsText = String.Join(Environment.NewLine, m_imageProperties.Keywords);
                string[] keywordsArray = keywordsText.Split('*');

                for (int i = 0; i < keywordsArray.Length; i++)
                {
                    output.Append(keywordsArray[i] + "\n");
                }

            }
            OutputTextBlock.Text = output.ToString();

        }

        private void ResetPersistedState()
        {
            m_localSettings.Remove("scenario1FileToken");
            m_localSettings.Remove("scenario1Title");
            m_localSettings.Remove("scenario1Keywords");
            m_localSettings.Remove("scenario1DateTaken");
            m_localSettings.Remove("scenario1Make");
            m_localSettings.Remove("scenario1Model");
            m_localSettings.Remove("scenario1Orientation");
            m_localSettings.Remove("scenario1LatDeg");
            m_localSettings.Remove("scenario1LatMin");
            m_localSettings.Remove("scenario1LatSec");
            m_localSettings.Remove("scenario1LatRef");
            m_localSettings.Remove("scenario1LongDeg");
            m_localSettings.Remove("scenario1LongMin");
            m_localSettings.Remove("scenario1LongSec");
            m_localSettings.Remove("scenario1LongRef");
            m_localSettings.Remove("scenario1Exposure");
            m_localSettings.Remove("scenario1FNumber");
        }

        /// <summary>
        /// Clear all of the state that is stored in memory and in the UI.
        /// </summary>
        private async void ResetSessionState()
        {
            //m_imageProperties = null;
            m_fileToken = null;



            StorageFile placeholderImage = await Package.Current.InstalledLocation.GetFileAsync("Assets\\placeholder-sdk.png");
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.SetSource(await placeholderImage.OpenAsync(FileAccessMode.Read));
            //Image1.Source = bitmapImage;
            //AutomationProperties.SetName(Image1, "A placeholder image");
            /*
            TitleTextbox.Text = "";
            KeywordsTextbox.Text = "";
            DateTakenTextblock.Text = "";
            MakeTextblock.Text = "";
            ModelTextblock.Text = "";
            OrientationTextblock.Text = "";
            LatDegTextbox.Text = "";
            LatMinTextbox.Text = "";
            LatSecTextbox.Text = "";
            LatRefTextbox.Text = "";
            LongDegTextbox.Text = "";
            LongMinTextbox.Text = "";
            LongSecTextbox.Text = "";
            LongRefTextbox.Text = "";*/
            //ExposureTextblock.Text = "";
            //FNumberTextblock.Text = "";
        }

        private void back_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }
    }
}
