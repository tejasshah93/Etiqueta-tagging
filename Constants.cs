﻿using System.Collections.Generic;
using System;
using Integrate;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.UI.ViewManagement;
using System.Threading.Tasks;

namespace Integrate
{
    public class Constants// : Integrate.Common.LayoutAwarePage
    {
        public const string FEATURE_NAME = "View/Edit Properties";

        // This list defines the scenarios covered in this sample and their titles.
        List<Scenario> scenarios = new List<Scenario>
        {
            new Scenario() { Title = "Image editor (Windows.Storage.FileProperties)", ClassType = typeof(ImagingProperties) },
            //new Scenario() { Title = "Image editor (Windows.Graphics.Imaging)", ClassType = typeof(ImagingTransforms) },
        };
    }

    public class Scenario
    {
        public string Title { get; set; }

        public Type ClassType { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }

    /// <summary>
    /// Contains helper functionality, including handling/converting EXIF orientation values.
    /// </summary>
    public static class Helpers
    {
        /// <summary>
        /// Retrieves all of the file extensions supported by the bitmap codecs on the system,
        /// and inserts them into the provided fileTypeFilter parameter.
        /// </summary>
        /// <param name="fileTypeFilter">FileOpenPicker.FileTypeFilter member</param>
        public static void FillDecoderExtensions(IList<string> fileTypeFilter)
        {
            IReadOnlyList<BitmapCodecInformation> codecInfoList =
                BitmapDecoder.GetDecoderInformationEnumerator();

            foreach (BitmapCodecInformation decoderInfo in codecInfoList)
            {
                // Each bitmap codec contains a list of file extensions it supports; add each list item
                // to fileTypeFilter.
                foreach (string extension in decoderInfo.FileExtensions)
                {
                    fileTypeFilter.Add(extension);
                }
            }
        }

        /// <summary>
        /// Returns a StorageFile containing an image in a supported format.
        /// </summary>
        public static async Task<StorageFile> GetFileFromOpenPickerAsync()
        {
            // Attempt to ensure that the view is not snapped, otherwise the picker will not display.
            if (ApplicationView.Value == ApplicationViewState.Snapped && !ApplicationView.TryUnsnap())
            {
                throw new Exception("File picker cannot display in snapped view.");
            }

            FileOpenPicker picker = new FileOpenPicker();
            FillDecoderExtensions(picker.FileTypeFilter);
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            StorageFile file = await picker.PickSingleFileAsync();

            if (file == null)
            {
                throw new Exception("User did not select a file.");
            }

            return file;
        }

        /// <summary>
        /// Returns a StorageFile that the user has selected as the encode destination.
        /// Selects a few common encoding formats.
        /// </summary>
        public static async Task<StorageFile> GetFileFromSavePickerAsync()
        {
            // Attempt to ensure that the view is not snapped, otherwise the picker will not display.
            if (ApplicationView.Value == ApplicationViewState.Snapped && !ApplicationView.TryUnsnap())
            {
                throw new Exception("File picker cannot display in snapped view.");
            }

            FileSavePicker picker = new FileSavePicker();
            picker.FileTypeChoices.Add("JPEG image", new string[] { ".jpg" });
            picker.FileTypeChoices.Add("PNG image", new string[] { ".png" });
            picker.FileTypeChoices.Add("BMP image", new string[] { ".bmp" });
            picker.DefaultFileExtension = ".png";
            picker.SuggestedFileName = "Output file";
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            StorageFile file = await picker.PickSaveFileAsync();

            if (file == null)
            {
                throw new Exception("User did not select a file.");
            }

            return file;
        }

        /// <summary>
        /// Converts a PhotoOrientation value into a human readable string.
        /// The text is adapted from the EXIF specification.
        /// Note that PhotoOrientation uses a counterclockwise convention,
        /// while the EXIF spec uses a clockwise convention.
        /// </summary>
        public static string GetOrientationString(PhotoOrientation input)
        {
            switch (input)
            {
                case PhotoOrientation.Normal:
                    return "No rotation";
                case PhotoOrientation.FlipHorizontal:
                    return "Flip horizontally";
                case PhotoOrientation.Rotate180:
                    return "Rotate 180° clockwise";
                case PhotoOrientation.FlipVertical:
                    return "Flip vertically";
                case PhotoOrientation.Transpose:
                    return "Rotate 270° clockwise, then flip horizontally";
                case PhotoOrientation.Rotate270:
                    return "Rotate 90° clockwise";
                case PhotoOrientation.Transverse:
                    return "Rotate 90° clockwise, then flip horizontally";
                case PhotoOrientation.Rotate90:
                    return "Rotate 270° clockwise";
                case PhotoOrientation.Unspecified:
                default:
                    return "Unspecified";
            }
        }

        /// <summary>
        /// Converts a Windows.Storage.FileProperties.PhotoOrientation value into a
        /// Windows.Graphics.Imaging.BitmapRotation value.
        /// For PhotoOrientation values reflecting a flip/mirroring operation, returns "None";
        /// therefore this is a potentially lossy transformation.
        /// Note that PhotoOrientation uses a counterclockwise convention,
        /// while BitmapRotation uses a clockwise convention.
        /// </summary>
        public static BitmapRotation ConvertToBitmapRotation(PhotoOrientation input)
        {
            switch (input)
            {
                case PhotoOrientation.Normal:
                    return BitmapRotation.None;
                case PhotoOrientation.Rotate270:
                    return BitmapRotation.Clockwise90Degrees;
                case PhotoOrientation.Rotate180:
                    return BitmapRotation.Clockwise180Degrees;
                case PhotoOrientation.Rotate90:
                    return BitmapRotation.Clockwise270Degrees;
                default:
                    // Ignore any flip/mirrored values.
                    return BitmapRotation.None;
            }
        }

        /// <summary>
        /// Converts a Windows.Graphics.Imaging.BitmapRotation value into a
        /// Windows.Storage.FileProperties.PhotoOrientation value.
        /// Note that PhotoOrientation uses a counterclockwise convention,
        /// while BitmapRotation uses a clockwise convention.
        /// </summary>
        public static PhotoOrientation ConvertToPhotoOrientation(BitmapRotation input)
        {
            switch (input)
            {
                case BitmapRotation.None:
                    return PhotoOrientation.Normal;
                case BitmapRotation.Clockwise90Degrees:
                    return PhotoOrientation.Rotate270;
                case BitmapRotation.Clockwise180Degrees:
                    return PhotoOrientation.Rotate180;
                case BitmapRotation.Clockwise270Degrees:
                    return PhotoOrientation.Rotate90;
                default:
                    return PhotoOrientation.Normal;
            }
        }

        /// <summary>
        /// Converts an unsigned integer, corresponding to the EXIF orientation flag, to a
        /// Windows.Storage.FileProperties.PhotoOrientation value. Note that the actual PhotoOrientation
        /// enumeration values directly map to the EXIF orientation flag; this method simply provides
        /// a typesafe means of converting between the two in C#, in addition to handling the Unspecified case.
        /// </summary>
        public static PhotoOrientation ConvertToPhotoOrientation(ushort input)
        {
            switch (input)
            {
                case 1:
                    return PhotoOrientation.Normal;
                case 2:
                    return PhotoOrientation.FlipHorizontal;
                case 3:
                    return PhotoOrientation.Rotate180;
                case 4:
                    return PhotoOrientation.FlipVertical;
                case 5:
                    return PhotoOrientation.Transpose;
                case 6:
                    return PhotoOrientation.Rotate270;
                case 7:
                    return PhotoOrientation.Transverse;
                case 8:
                    return PhotoOrientation.Rotate90;
                default:
                    return PhotoOrientation.Unspecified;
            }
        }

        /// <summary>
        /// Counterpart to ConvertToPhotoOrientation(ushort input), maps PhotoOrientation enumeration
        /// values to an unsigned 16-bit integer representing the EXIF orientation flag.
        /// </summary>
        public static ushort ConvertToExifOrientationFlag(PhotoOrientation input)
        {
            switch (input)
            {
                case PhotoOrientation.Normal:
                    return 1;
                case PhotoOrientation.FlipHorizontal:
                    return 2;
                case PhotoOrientation.Rotate180:
                    return 3;
                case PhotoOrientation.FlipVertical:
                    return 4;
                case PhotoOrientation.Transpose:
                    return 5;
                case PhotoOrientation.Rotate270:
                    return 6;
                case PhotoOrientation.Transverse:
                    return 7;
                case PhotoOrientation.Rotate90:
                    return 8;
                default:
                    return 1;
            }
        }

        /// <summary>
        /// "Adds" two PhotoOrientation values. For simplicity, does not handle any values with
        /// flip/mirroring; therefore this is a potentially lossy transformation.
        /// Note that PhotoOrientation uses a counterclockwise convention.
        /// </summary>
        public static PhotoOrientation AddPhotoOrientation(PhotoOrientation value1, PhotoOrientation value2)
        {
            switch (value2)
            {
                case PhotoOrientation.Rotate90:
                    return Add90DegreesCCW(value1);
                case PhotoOrientation.Rotate180:
                    return Add90DegreesCCW(Add90DegreesCCW(value1));
                case PhotoOrientation.Rotate270:
                    return Add90DegreesCW(value1);
                case PhotoOrientation.Normal:
                default:
                    // Ignore any values with flip/mirroring.
                    return value1;
            }
        }

        /// <summary>
        /// "Add" 90 degrees clockwise rotation to a PhotoOrientation value.
        /// For simplicity, does not handle any values with flip/mirroring; therefore this is a potentially
        /// lossy transformation.
        /// Note that PhotoOrientation uses a counterclockwise convention.
        /// </summary>
        public static PhotoOrientation Add90DegreesCW(PhotoOrientation input)
        {
            switch (input)
            {
                case PhotoOrientation.Normal:
                    return PhotoOrientation.Rotate270;
                case PhotoOrientation.Rotate90:
                    return PhotoOrientation.Normal;
                case PhotoOrientation.Rotate180:
                    return PhotoOrientation.Rotate90;
                case PhotoOrientation.Rotate270:
                    return PhotoOrientation.Rotate180;
                default:
                    // Ignore any values with flip/mirroring.
                    return PhotoOrientation.Unspecified;
            }
        }

        /// <summary>
        /// "Add" 90 degrees counter-clockwise rotation to a PhotoOrientation value.
        /// For simplicity, does not handle any values with flip/mirroring; therefore this is a potentially
        /// lossy transformation.
        /// Note that PhotoOrientation uses a counterclockwise convention.
        /// </summary>
        public static PhotoOrientation Add90DegreesCCW(PhotoOrientation input)
        {
            switch (input)
            {
                case PhotoOrientation.Normal:
                    return PhotoOrientation.Rotate90;
                case PhotoOrientation.Rotate90:
                    return PhotoOrientation.Rotate180;
                case PhotoOrientation.Rotate180:
                    return PhotoOrientation.Rotate270;
                case PhotoOrientation.Rotate270:
                    return PhotoOrientation.Normal;
                default:
                    // Ignore any values with flip/mirroring.
                    return PhotoOrientation.Unspecified;
            }
        }
    }
}
