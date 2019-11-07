// Copyright (c) Intel Corporation
// SPDX-License-Identifier: MIT

// This file incorporates work covered by the following copyright and  
// permission notice:  
//
// Copyright (c) Microsoft Corporation. All rights reserved.

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Intel.AI.Skills.BackgroundBlur;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Microsoft.Toolkit.Uwp.Helpers;
using System.Threading;

namespace BackgroundBlurTestApp_UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// This is the main application logic which shows how to use the skill APIs.
    /// </summary>
    public sealed partial class BBSkillAppPage : Page
    {
        // Used as a container for output image frame
        private SoftwareBitmapSource m_bitmapSource = new SoftwareBitmapSource();

        // Camera/image Input parameters
        private uint m_cameraFrameWidth, m_cameraFrameHeight;
        private bool m_isCameraFrameDimensionInitialized = false;
        private enum FrameSourceToggledType { None, ImageFile, Camera };
        private FrameSourceToggledType m_currentFrameSourceToggled = FrameSourceToggledType.None;

        // Synchronization
        private SemaphoreSlim m_lock = new SemaphoreSlim(1);

        public BBSkillAppPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        // Local copy of parent skill object
        BackgroundBlurSkillObj m_paramsSkillObj;

        // Check if page has already been loaded once
        private bool IsPageLoaded;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            m_paramsSkillObj = (BackgroundBlurSkillObj)e.Parameter;
            base.OnNavigatedTo(e);
        }

        /// <summary>
        /// Triggered after the page has loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Check for OS version error before proceeding
                if (m_paramsSkillObj.m_osVersionError)
                {
                    UISkillOutputDetails.Text = "OS Version Error! Installed Windows version not supported!";
                }
                else
                {
                    // Check if any execution devices are available
                    if (m_paramsSkillObj.m_availableExecutionDevices.Count == 0)
                    {
                        UISkillOutputDetails.Text = "DeviceError! No execution devices available, this skill cannot run on this device";
                    }
                    else
                    {
                        if (!IsPageLoaded)
                        {
                            // Display available execution devices
                            UISkillExecutionDevices.ItemsSource = m_paramsSkillObj.m_availableExecutionDevices.Select((device) => device.Name);
                            UISkillExecutionDevices.SelectedIndex = 0;
                            IsPageLoaded = true;

                            // Alow user to interact with the app
                            UICameraToggle.IsEnabled = true;
                            UIButtonFilePick.IsEnabled = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }

            // Register callback for if camera preview encounters an issue
            UICameraPreview.PreviewFailed += UICameraPreview_PreviewFailed;
        }

        /// <summary>
        /// Triggered when UICameraToggle is clicked, initializes frame grabbing from the camera stream
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void UICameraToggle_Click(object sender, RoutedEventArgs e)
        {
            UICameraToggle.IsEnabled = false;

            await m_lock.WaitAsync();

            try
            {
                UICameraPreview.Stop();
                if (UICameraPreview.CameraHelper != null)
                {
                    await UICameraPreview.CameraHelper.CleanUpAsync();
                }
                m_isCameraFrameDimensionInitialized = false;

                // Instantiate skill only if object is null or selected execution device has changed
                if ((m_paramsSkillObj.m_selectedDeviceId != UISkillExecutionDevices.SelectedIndex) || (m_paramsSkillObj.m_skill == null))
                {
                    if (m_paramsSkillObj.m_skill != null)
                    {
                        // Release previous instance
                        m_paramsSkillObj.m_skill = null;
                    }

                    // Update selected device
                    m_paramsSkillObj.m_selectedDeviceId = UISkillExecutionDevices.SelectedIndex;

                    // Initialize skill with the selected supported device
                    m_paramsSkillObj.m_skill = await m_paramsSkillObj.m_skillDescriptor.CreateSkillAsync(m_paramsSkillObj.m_availableExecutionDevices[UISkillExecutionDevices.SelectedIndex]) as BackgroundBlurSkill;

                    // Instantiate a binding object that will hold the skill's input and output resource
                    m_paramsSkillObj.m_binding = await m_paramsSkillObj.m_skill.CreateSkillBindingAsync() as BackgroundBlurBinding;
                }

                // Initialize the CameraPreview control, register frame arrived event callback
                UICameraPreview.Visibility = Visibility.Visible;
                await UICameraPreview.StartAsync();

                UICameraPreview.CameraHelper.FrameArrived += CameraHelper_FrameArrived;

                // Set a specific resolution if available
                var lrFrameFormat = UICameraPreview.CameraHelper.FrameFormatsAvailable.Find((format) => ((format.VideoFormat.Width == 640) && (format.VideoFormat.Height == 480)));
                if (lrFrameFormat != null)
                {
                    await UICameraPreview.CameraHelper.PreviewFrameSource.SetFormatAsync(lrFrameFormat);
                }

                m_currentFrameSourceToggled = FrameSourceToggledType.Camera;
            }
            catch (Exception ex)
            {
                await (new MessageDialog(ex.Message)).ShowAsync();
                m_currentFrameSourceToggled = FrameSourceToggledType.None;
            }
            finally
            {
                m_lock.Release();
            }
        }

        /// <summary>
        /// Triggered when a new frame is available from the camera stream.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void CameraHelper_FrameArrived(object sender, FrameEventArgs e)
        {
            try
            {
                // Use a lock to process frames one at a time and bypass processing if busy
                if (m_lock.Wait(0))
                {
                    uint cameraFrameWidth = UICameraPreview.CameraHelper.PreviewFrameSource.CurrentFormat.VideoFormat.Width;
                    uint cameraFrameHeight = UICameraPreview.CameraHelper.PreviewFrameSource.CurrentFormat.VideoFormat.Height;

                    // Allign overlay canvas and camera preview so that face detection rectangle looks right
                    if (!m_isCameraFrameDimensionInitialized || cameraFrameWidth != m_cameraFrameWidth || cameraFrameHeight != m_cameraFrameHeight)
                    {
                        m_cameraFrameWidth = UICameraPreview.CameraHelper.PreviewFrameSource.CurrentFormat.VideoFormat.Width;
                        m_cameraFrameHeight = UICameraPreview.CameraHelper.PreviewFrameSource.CurrentFormat.VideoFormat.Height;

                        m_isCameraFrameDimensionInitialized = true;
                    }

                    // Run the skill against the frame
                    await RunSkillAsync(e.VideoFrame);
                    m_lock.Release();
                }
                e.VideoFrame.Dispose();
            }
            catch (Exception ex)
            {
                // Show the error
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => UISkillOutputDetails.Text = ex.Message);
                m_lock.Release();
            }
        }

        /// <summary>
        /// Triggered when something wrong happens with the camera preview control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void UICameraPreview_PreviewFailed(object sender, PreviewFailedEventArgs e)
        {
            await new MessageDialog(e.Error).ShowAsync();
        }

        /// <summary>
        /// Run the skill against the frame passed as parameter
        /// </summary>
        /// <param name="frame"></param>
        /// <returns></returns>
        private async Task RunSkillAsync(VideoFrame frame)
        {
            // Update input image and run the skill against it
            await m_paramsSkillObj.m_binding.SetInputImageAsync(frame);
            
            // Evaluate skill
            await m_paramsSkillObj.m_skill.EvaluateAsync(m_paramsSkillObj.m_binding);

            VideoFrame blurredOutput = null;

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                // Retrieve result and display
                blurredOutput = await m_paramsSkillObj.m_binding.GetOutputImageAsync();
                if (blurredOutput.SoftwareBitmap == null)
                {
                    SoftwareBitmap softwareBitmapOut = await SoftwareBitmap.CreateCopyFromSurfaceAsync(blurredOutput.Direct3DSurface);
                    softwareBitmapOut = SoftwareBitmap.Convert(softwareBitmapOut, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

                    await m_bitmapSource.SetBitmapAsync(softwareBitmapOut);
                    UIOutputViewer.Source = m_bitmapSource;
                }
                else
                {
                    await m_bitmapSource.SetBitmapAsync(blurredOutput.SoftwareBitmap);
                    UIOutputViewer.Source = m_bitmapSource;
                }
            });
        }

        /// <summary>
        /// Launch file picker for user to select a picture file and return a VideoFrame
        /// </summary>
        /// <returns>VideoFrame instanciated from the selected image file</returns>
        public static IAsyncOperation<VideoFrame> LoadVideoFrameFromFilePickedAsync()
        {
            return AsyncInfo.Run(async (token) =>
            {
                // Trigger file picker to select an image file
                FileOpenPicker fileOpenPicker = new FileOpenPicker();
                fileOpenPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                fileOpenPicker.FileTypeFilter.Add(".jpg");
                fileOpenPicker.FileTypeFilter.Add(".png");
                fileOpenPicker.ViewMode = PickerViewMode.Thumbnail;
                StorageFile selectedStorageFile = await fileOpenPicker.PickSingleFileAsync();

                if (selectedStorageFile == null)
                {
                    return null;
                }

                VideoFrame resultFrame = null;
                SoftwareBitmap softwareBitmap = null;
                using (IRandomAccessStream stream = await selectedStorageFile.OpenAsync(FileAccessMode.Read))
                {
                    // Create the decoder from the stream 
                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);

                    // Get the SoftwareBitmap representation of the file in BGRA8 format
                    softwareBitmap = await decoder.GetSoftwareBitmapAsync();

                    // Convert to friendly format for UI display purpose
                    softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                }

                // Encapsulate the image in a VideoFrame instance
                resultFrame = VideoFrame.CreateWithSoftwareBitmap(softwareBitmap);

                return resultFrame;
            });
        }

        /// <summary>
        /// Triggered when UIButtonFilePick is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void UIButtonFilePick_Click(object sender, RoutedEventArgs e)
        {
            // Disable subsequent trigger of this event callback 
            UIButtonFilePick.IsEnabled = false;
            UICameraToggle.IsEnabled = false;

            // Stop Camera preview
            UICameraPreview.Stop();
            if (UICameraPreview.CameraHelper != null)
            {
                await UICameraPreview.CameraHelper.CleanUpAsync();
            }
            UICameraPreview.Visibility = Visibility.Collapsed;

            try
            {
                m_currentFrameSourceToggled = FrameSourceToggledType.ImageFile;

                var frame = await LoadVideoFrameFromFilePickedAsync();

                // Instantiate skill only if object is null or selected execution device has changed
                if ((m_paramsSkillObj.m_selectedDeviceId != UISkillExecutionDevices.SelectedIndex) || (m_paramsSkillObj.m_skill == null))
                {
                    //Release previous instance
                    if (m_paramsSkillObj.m_skill != null)
                    {
                        m_paramsSkillObj.m_skill = null;
                    }

                    // Update selected device
                    m_paramsSkillObj.m_selectedDeviceId = UISkillExecutionDevices.SelectedIndex;

                    // Initialize skill with the selected supported device
                    m_paramsSkillObj.m_skill = await m_paramsSkillObj.m_skillDescriptor.CreateSkillAsync(m_paramsSkillObj.m_availableExecutionDevices[UISkillExecutionDevices.SelectedIndex]) as BackgroundBlurSkill;

                    // Instantiate a binding object that will hold the skill's input and output resource
                    m_paramsSkillObj.m_binding = await m_paramsSkillObj.m_skill.CreateSkillBindingAsync() as BackgroundBlurBinding;
                }

                if (frame != null)
                {
                    // Update input image and run the skill against it
                    await m_paramsSkillObj.m_binding.SetInputImageAsync(frame);

                    // Evaluate skill
                    await m_paramsSkillObj.m_skill.EvaluateAsync(m_paramsSkillObj.m_binding);

                    VideoFrame blurredOutput = null;

                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                    {
                        // Retrieve result and display
                        blurredOutput = await m_paramsSkillObj.m_binding.GetOutputImageAsync();
                        if (blurredOutput.SoftwareBitmap == null)
                        {
                            SoftwareBitmap softwareBitmapOut = await SoftwareBitmap.CreateCopyFromSurfaceAsync(blurredOutput.Direct3DSurface);
                            softwareBitmapOut = SoftwareBitmap.Convert(softwareBitmapOut, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

                            await m_bitmapSource.SetBitmapAsync(softwareBitmapOut);
                            UIOutputViewer.Source = m_bitmapSource;
                        }
                        else
                        {
                            await m_bitmapSource.SetBitmapAsync(blurredOutput.SoftwareBitmap);
                            UIOutputViewer.Source = m_bitmapSource;
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                await (new MessageDialog(ex.Message)).ShowAsync();
            }

            // Enable subsequent trigger of this event callback
            UIButtonFilePick.IsEnabled = true;
            UICameraToggle.IsEnabled = true;
        }

        private void UISkillExecutionDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (m_currentFrameSourceToggled)
            {
                case FrameSourceToggledType.ImageFile:
                    UIButtonFilePick_Click(null, null);
                    break;
                case FrameSourceToggledType.Camera:
                    UICameraToggle_Click(null, null);
                    break;
                default:
                    break;
            }
        }
    }
}
