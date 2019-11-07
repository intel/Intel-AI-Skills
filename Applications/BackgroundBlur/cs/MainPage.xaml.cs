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
using System.Reflection;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media.Animation;
using Intel.AI.Skills.BackgroundBlur;
using Microsoft.AI.Skills.SkillInterfacePreview;

namespace BackgroundBlurTestApp_UWP
{
    /// <summary>
    /// Parent skill class which stores instances of descriptor, skill and binding objects along with list of available execution devices.
    /// Used across child pages (SkillInfoPage and SkillAppPage) to access the same skill instance.
    /// </summary>
    public class BackgroundBlurSkillObj
    {
        public BackgroundBlurDescriptor m_skillDescriptor = null;
        public IReadOnlyList<ISkillExecutionDevice> m_availableExecutionDevices = null;
        public BackgroundBlurSkill m_skill = null;
        public BackgroundBlurBinding m_binding = null;
        public int m_selectedDeviceId = 0;

        public bool m_osVersionError = false;
        private readonly int m_osVersionMinimumBuild = 17763;

        public BackgroundBlurSkillObj()
        {
            // Check for Windows OS version before instantiating skill descriptor. Minimum supported version is 1809 build 17763 (RS5)
            if((System.Environment.OSVersion.Version.Major == 10) &&(System.Environment.OSVersion.Version.Build >= m_osVersionMinimumBuild))
            {
                // Instantiate skill descriptor. This is the gateway to skill, providing details on input/output features, available execution devices.
                m_skillDescriptor = new BackgroundBlurDescriptor();
                GetDeviceList();
            }
            else
            {
                // Flag OS version incompatibility error
                m_osVersionError = true;
            }
        }

        public async void GetDeviceList()
        {
            // Get list of available execution devices from skill descriptor
            m_availableExecutionDevices = await m_skillDescriptor.GetSupportedExecutionDevicesAsync();
        }

    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        /// <summary>
        /// MainPage consists of two views - skill info (displays details about skill) and skill app (main skill execution)
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();

            // Instantiate parent skill class
            m_psSkill = new BackgroundBlurSkillObj();

            // Set default view to SkillAppPage
            NavigateToView("BBSkillAppPage");
        }

        // Keep track of view navigation
        private NavigationViewItem lastNavItem;

        // Instance of parent skill class
        BackgroundBlurSkillObj m_psSkill;

        /// <summary>
        /// Navigation callback to switch views
        /// </summary>
        private void NavigationView_OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            var navItem = args.InvokedItemContainer as NavigationViewItem;

            if ((navItem == null) || (navItem == lastNavItem))
            {
                return;
            }

            if (navItem.Tag != null)
            {
                var selectedView = navItem.Tag.ToString();
                if (!NavigateToView(selectedView)) return;
                lastNavItem = navItem;
            }
        }

        /// <summary>
        /// Navigate to selected view
        /// </summary>
        private bool NavigateToView(string selectedView)
        {
            var view = Assembly.GetExecutingAssembly()
                .GetType($"BackgroundBlurTestApp_UWP.{selectedView}");

            if (string.IsNullOrWhiteSpace(selectedView) || view == null)
            {
                return false;
            }

            ChildFrame.Navigate(view, m_psSkill, new EntranceNavigationTransitionInfo());
            return true;
        }

        /// <summary>
        /// Handle navigation failure
        /// </summary>
        private async void ChildFrame_OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            var errMsg = new Windows.UI.Popups.MessageDialog("");
            errMsg.Title = "Cannot navigate to page";
            errMsg.Content = String.Format($"Navigation to page failed {e.Exception.Message}");
            await errMsg.ShowAsync();
        }

    }
}
