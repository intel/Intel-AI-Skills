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
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.AI.Skills.SkillInterfacePreview;
using Windows.UI.Xaml.Navigation;

namespace BackgroundBlurTestApp_UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// This page is used to display details about skill
    /// </summary>
    public sealed partial class BBSkillInfoPage : Page
    {
        public BBSkillInfoPage()
        {
            this.InitializeComponent();
        }

        // Local copy of parent skill object
        BackgroundBlurSkillObj m_paramsSkillObj;

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
        private async void Info_Page_Loaded(object sender, RoutedEventArgs e)
        {          
            try
            {
                if (m_paramsSkillObj.m_skillDescriptor != null)
                {
                    // Populate skill name
                    UISkillName.Text = m_paramsSkillObj.m_skillDescriptor.Information.Name;

                    // Populate skill description
                    UISkillDescription.Text = $"{m_paramsSkillObj.m_skillDescriptor.Information.Description}" +
                    $"\n\tAuthored by: {m_paramsSkillObj.m_skillDescriptor.Information.Author}" +
                    $"\n\tPublished by: {m_paramsSkillObj.m_skillDescriptor.Information.Publisher}" +
                    $"\n\tVersion: {m_paramsSkillObj.m_skillDescriptor.Information.Version.Major}.{m_paramsSkillObj.m_skillDescriptor.Information.Version.Minor}.{m_paramsSkillObj.m_skillDescriptor.Information.Version.Build}.{m_paramsSkillObj.m_skillDescriptor.Information.Version.Revision}";

                    // Populate skill input feature description
                    var inputDesc = m_paramsSkillObj.m_skillDescriptor.InputFeatureDescriptors[0] as SkillFeatureImageDescriptor;
                    UISkillInputDescription.Text = $"\tName: {inputDesc.Name}" +
                    $"\n\tDescription: {inputDesc.Description}" +
                    $"\n\tType: {inputDesc.FeatureKind}" +
                    $"\n\tWidth: {inputDesc.Width}" +
                    $"\n\tHeight: {inputDesc.Height}" +
                    $"\n\tSupportedBitmapPixelFormat: {inputDesc.SupportedBitmapPixelFormat}" +
                    $"\n\tSupportedBitmapAlphaMode: {inputDesc.SupportedBitmapAlphaMode}";

                    // Populate skill output feature description
                    var outputDesc = m_paramsSkillObj.m_skillDescriptor.OutputFeatureDescriptors[0] as SkillFeatureImageDescriptor;
                    UISkillOutputDescription.Text = $"\tName: {outputDesc.Name}" +
                    $"\n\tDescription: {outputDesc.Description}" +
                    $"\n\tType: {outputDesc.FeatureKind}" +
                    $"\n\tWidth: {outputDesc.Width}" +
                    $"\n\tHeight: {outputDesc.Height}" +
                    $"\n\tSupportedBitmapPixelFormat: {outputDesc.SupportedBitmapPixelFormat}" +
                    $"\n\tSupportedBitmapAlphaMode: {outputDesc.SupportedBitmapAlphaMode}";

                    // Populate available execution devices
                    int numDevices = m_paramsSkillObj.m_availableExecutionDevices.Count;
                    UISkillExecutionDevices.Text = "";
                    for (int d = 0; d < numDevices; d++)
                    {
                        if (d != 0) UISkillExecutionDevices.Text += "\n";
                        UISkillExecutionDevices.Text += $"\tDevice {d + 1}: {m_paramsSkillObj.m_availableExecutionDevices[d].Name}";
                    }
                }
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }
    }
}
