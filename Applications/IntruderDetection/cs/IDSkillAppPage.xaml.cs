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
using System.Threading.Tasks;
using Windows.Media;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Intel.AI.Skills.IntruderDetection;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI;
using Windows.UI.Xaml.Shapes;
using Microsoft.AI.Skills.SkillInterfacePreview;
using Windows.UI.Xaml.Navigation;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Microsoft.Toolkit.Uwp.Helpers;
using System.Threading;

namespace IntruderDetectionApp_UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// This is the main application logic which shows how to use the skill APIs.
    /// </summary>
    public sealed partial class IDSkillAppPage : Page
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

        // Utility to draw shapes on the canvas
        private IntruderDetectorRenderer m_intruderDetectorRenderer = null;

        public IDSkillAppPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        // Local copy of parent skill object
        IntruderDetectionSkillObj m_paramsSkillObj;

        // Check if page has already been loaded once
        private bool IsPageLoaded;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            m_paramsSkillObj = (IntruderDetectionSkillObj)e.Parameter;
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
                    // Initialize helper class used to render the skill results on screen
                    if (m_intruderDetectorRenderer == null)
                    {
                        m_intruderDetectorRenderer = new IntruderDetectorRenderer(UICanvasOverlay);
                    }

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
                    m_paramsSkillObj.m_skill = await m_paramsSkillObj.m_skillDescriptor.CreateSkillAsync(m_paramsSkillObj.m_availableExecutionDevices[UISkillExecutionDevices.SelectedIndex]) as IntruderDetectionSkill;

                    // Instantiate a binding object that will hold the skill's input and output resource
                    m_paramsSkillObj.m_binding = await m_paramsSkillObj.m_skill.CreateSkillBindingAsync() as IntruderDetectionBinding;
                }

                // Initialize the CameraPreview control, register frame arrived event callback
                UICameraPreview.Visibility = Visibility.Visible;
                await UICameraPreview.StartAsync();

                UICameraPreview.CameraHelper.FrameArrived += CameraHelper_FrameArrived;

                // Set a specific resolution if available
                var lrFrameFormat = UICameraPreview.CameraHelper.FrameFormatsAvailable.Find((format) => ((format.VideoFormat.Width == 1280) && (format.VideoFormat.Height == 720)));
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

                    // Allign overlay canvas and camera preview so that output rectangle looks right
                    if (!m_isCameraFrameDimensionInitialized || cameraFrameWidth != m_cameraFrameWidth || cameraFrameHeight != m_cameraFrameHeight)
                    {
                        m_cameraFrameWidth = UICameraPreview.CameraHelper.PreviewFrameSource.CurrentFormat.VideoFormat.Width;
                        m_cameraFrameHeight = UICameraPreview.CameraHelper.PreviewFrameSource.CurrentFormat.VideoFormat.Height;

                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        {
                            UIImageViewer_SizeChanged(null, null);
                        });

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

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                // Clear the canvas of previous results
                m_intruderDetectorRenderer.ResetCanvas();
                
                // Retrieve result
                var intruderFlag = (m_paramsSkillObj.m_binding["IntruderDetected"].FeatureValue as SkillFeatureTensorBooleanValue).GetAsVectorView();
                bool isIntruderDetected = (bool)intruderFlag[0];

                // Add frame boundary (green for no intruder, red for intruder detected)
                m_intruderDetectorRenderer.AddRectangle(isIntruderDetected);
                m_intruderDetectorRenderer.IsVisible = true;
            });
        }

        private void UISkillExecutionDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m_currentFrameSourceToggled == FrameSourceToggledType.Camera)
            {
                UICameraToggle_Click(null, null);
            }
        }

        /// <summary>
        /// Triggers when the iamge control is resized, makes sure the canvas size stays in sync
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UIImageViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            float aspectRatio = (float)m_cameraFrameWidth / m_cameraFrameHeight;
            UICanvasOverlay.Width = aspectRatio >= 1.0f ? UICameraPreview.ActualWidth : UICameraPreview.ActualHeight * aspectRatio;
            UICanvasOverlay.Height = aspectRatio >= 1.0f ? UICameraPreview.ActualWidth / aspectRatio : UICameraPreview.ActualHeight;
        }

    }

    /// <summary>
    /// Convenience class for rendering a rectangle on screen
    /// </summary>
    internal class IntruderDetectorRenderer
    {
        private Canvas m_canvas;

        /// <summary>
        /// IntruderDetectorRenderer constructor
        /// </summary>
        /// <param name="canvas"></param>
        public IntruderDetectorRenderer(Canvas canvas)
        {
            m_canvas = canvas;
            IsVisible = false;
        }

        /// <summary>
        /// Set visibility of IntruderDetectorRenderer UI controls
        /// </summary>
        public bool IsVisible
        {
            get
            {
                return m_canvas.Visibility == Visibility.Visible;
            }
            set
            {
                m_canvas.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Remove previously added child elements to canvas
        /// </summary>
        public void ResetCanvas()
        {
            m_canvas.Children.Clear();
        }

        /// <summary>
        /// Add rectangle using coordinates passsed as parameter
        /// </summary>
        /// <param name="coordinates"></param>
        public void AddRectangle(bool isIntruderDetected)
        {
            Rectangle m_rectangle = new Rectangle();

            if (isIntruderDetected)
                m_rectangle = new Rectangle() { Stroke = new SolidColorBrush(Colors.OrangeRed), StrokeThickness = 4 };
            else
                m_rectangle = new Rectangle() { Stroke = new SolidColorBrush(Colors.Lime), StrokeThickness = 4 };

            m_canvas.Children.Add(m_rectangle);

            m_rectangle.Width = (Int32)(m_canvas.Width);
            m_rectangle.Height = (Int32)(m_canvas.Height);
            Canvas.SetLeft(m_rectangle, 0);
            Canvas.SetTop(m_rectangle, 0);
        }
    }
}
