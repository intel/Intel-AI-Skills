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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Intel.AI.Skills.FaceDetection;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI;
using Windows.UI.Xaml.Shapes;
using Microsoft.AI.Skills.SkillInterfacePreview;
using Windows.UI.Xaml.Navigation;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Microsoft.Toolkit.Uwp.Helpers;
using System.Threading;

namespace FaceDetectionTestApp_UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// This is the main application logic which shows how to use the skill APIs.
    /// </summary>
    public sealed partial class FDSkillAppPage : Page
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
        private FaceDetectorRenderer m_faceDetectorRenderer = null;

        public FDSkillAppPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        // Local copy of parent skill object
        FaceDetectionSkillObj m_paramsSkillObj;

        // Check if page has already been loaded once
        private bool IsPageLoaded;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            m_paramsSkillObj = (FaceDetectionSkillObj)e.Parameter;
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
                    if (m_faceDetectorRenderer == null)
                    {
                        m_faceDetectorRenderer = new FaceDetectorRenderer(UICanvasOverlay);
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

                m_faceDetectorRenderer.ResetCanvas();
                m_faceDetectorRenderer.IsVisible = false;

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
                    m_paramsSkillObj.m_skill = await m_paramsSkillObj.m_skillDescriptor.CreateSkillAsync(m_paramsSkillObj.m_availableExecutionDevices[UISkillExecutionDevices.SelectedIndex]) as FaceDetectionSkill;

                    // Instantiate a binding object that will hold the skill's input and output resource
                    m_paramsSkillObj.m_binding = await m_paramsSkillObj.m_skill.CreateSkillBindingAsync() as FaceDetectionBinding;
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

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                // Clear the canvas of previous results
                m_faceDetectorRenderer.ResetCanvas();

                // Retrieve result
                var numFacesVec = (m_paramsSkillObj.m_binding["NumberOfFaces"].FeatureValue as SkillFeatureTensorFloatValue).GetAsVectorView();

                int numFaces = (int)numFacesVec[0];
                if (numFaces == 0)
                {
                    // if no person found, hide the rectangle in the UI
                    m_faceDetectorRenderer.IsVisible = false;
                }
                else
                {
                    for (int i = 0; i < numFaces; i++)
                    {
                        m_faceDetectorRenderer.AddRectangle(m_paramsSkillObj.m_binding.FaceBB(i));
                        m_faceDetectorRenderer.AddEllipse(m_paramsSkillObj.m_binding.FaceLandmarkLeftEye(i));
                        m_faceDetectorRenderer.AddEllipse(m_paramsSkillObj.m_binding.FaceLandmarkRightEye(i));
                        m_faceDetectorRenderer.AddEllipse(m_paramsSkillObj.m_binding.FaceLandmarkMouthLeft(i));
                        m_faceDetectorRenderer.AddEllipse(m_paramsSkillObj.m_binding.FaceLandmarkMouthRight(i));
                        m_faceDetectorRenderer.AddEllipse(m_paramsSkillObj.m_binding.FaceLandmarkNose(i));
                        m_faceDetectorRenderer.IsVisible = true;
                    }
                }
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
    internal class FaceDetectorRenderer
    {
        private Canvas m_canvas;

        /// <summary>
        /// FaceDetectorRenderer constructor
        /// </summary>
        /// <param name="canvas"></param>
        public FaceDetectorRenderer(Canvas canvas)
        {
            m_canvas = canvas;
            IsVisible = false;
        }

        /// <summary>
        /// Set visibility of FaceDetectorRenderer UI controls
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
        /// Add face rectangle using coordinates passsed as parameter
        /// </summary>
        /// <param name="coordinates"></param>
        public void AddRectangle(IReadOnlyList<float> coordinates)
        {
            if (coordinates == null)
            {
                return;
            }
            if (coordinates.Count != 4)
            {
                throw new Exception("you can only pass a set of 4 float coordinates (left, top, right, bottom) to this method");
            }

            Rectangle m_rectangle = new Rectangle();
            m_rectangle = new Rectangle() { Stroke = new SolidColorBrush(Colors.Green), StrokeThickness = 2 };
            m_canvas.Children.Add(m_rectangle);

            m_rectangle.Width = (coordinates[2] - coordinates[0]) * m_canvas.Width;
            m_rectangle.Height = (coordinates[3] - coordinates[1]) * m_canvas.Height;
            Canvas.SetLeft(m_rectangle, coordinates[0] * m_canvas.Width);
            Canvas.SetTop(m_rectangle, coordinates[1] * m_canvas.Height);
        }

        /// <summary>
        /// Add face landmark features using coordinates passsed as parameter
        /// </summary>
        /// <param name="coordinates"></param>
        public void AddEllipse(IReadOnlyList<float> coordinates)
        {
            if (coordinates == null)
            {
                return;
            }
            if (coordinates.Count != 2)
            {
                throw new Exception("you can only pass a set of 2 float coordinates (x, y) to this method");
            }

            Ellipse m_ellipse = new Ellipse() { Stroke = new SolidColorBrush(Colors.Lime), StrokeThickness = 2 };

            m_canvas.Children.Add(m_ellipse);

            Point xy = new Point(coordinates[0] * m_canvas.Width, coordinates[1] * m_canvas.Height);

            double d = 6;
            m_ellipse.SetValue(Canvas.TopProperty, xy.Y - (d / 2));
            m_ellipse.SetValue(Canvas.LeftProperty, xy.X - (d / 2));
            m_ellipse.Width = d;
            m_ellipse.Height = d;
        }
    }
}
