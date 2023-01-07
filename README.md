DISCONTINUATION OF PROJECT

This project will no longer be maintained by Intel.

Intel has ceased development and contributions including, but not limited to, maintenance, bug fixes, new releases, or updates, to this project.  

Intel no longer accepts patches to this project.

If you have an ongoing need to use this project, are interested in independently developing it, or would like to maintain patches for the open source software community, please create your own fork of this project.  

Contact: webadmin@linux.intel.com
# Intel&reg; AI Skills

## Overview

Intel&reg; AI Skills are based on the <a href="https://github.com/microsoft/WindowsVisionSkillsPreview/blob/master/doc/Microsoft.AI.Skills.SkillInterfacePreview.md">Windows\* Skills API</a> defined by Microsoft\*. The Skills API is a framework which enables algorithm developers to deliver their implementation with a standardized interface. It hides complexity from the application developer while exposing key interfaces. For a more detailed description, please refer to Microsoft\* <a href="https://docs.microsoft.com/en-us/windows/ai/windows-vision-skills/">documentation</a>. As part of the current suite, Intel&reg; has provided a set of skills as nugets along with sample application source code to demonstrate the usage. Each skill leverages Artificial Intelligence (AI) inference to run vision based algorithms. To access APIs and functionality exposed by the skill, the application developer needs to install the corresponding nuget package. The pseudo code explaining how to invoke APIs for each of the skills is covered in the documentation that follows. 

The nuget packages for each of these skills can be downloaded from <a href="https://www.nuget.org/profiles/IntelAISkills">Nuget.org</a>. For more information, please visit the Intel&reg; AI Skills <a href="https://software.intel.com/en-us/ai/on-pc/skills">webpage</a>.

## Usage

This installment of Intel&reg; AI Skills is focused on AI inference on images. These are building blocks which can be consumed by an application to showcase how AI can be used to enhance user experience. The application developer can also choose to combine two or more skills to deliver a more complex use case. The following Intel&reg; AI Skills are available. Click on the links below to get a more detailed description for each.

| Skill | Description |
| :-- | :-- |
| **[Background Blur](Applications/BackgroundBlur)** | Segments out individuals while blurring the background image to highlight the individuals in the foreground. |
| **[Background Replacement](Applications/BackgroundReplacement)** | Segments out individuals while replacing the background with a user-selected image. |
| **[Face Detection](Applications/FaceDetection)** | Detects face(s) and returns face bounding box(es) and other attributes, such as eyes, mouths, or nose tips. |
| **[Intruder Detection](Applications/IntruderDetection)** | Detects intruder by checking to see if an additional face or person is present in the video frame. |
| **[Person Detection](Applications/PersonDetection)** | Detects person(s) and returns person bounding box(es). |
| **[Super Resolution](Applications/SuperResolution)** | Converts a low-resolution image or video frame (320 x 240) to a high-resolution image (1280 x 960). |
| **[Super Resolution (WinML)](Applications/SuperResolutionWinML)** | Converts a low-resolution image or video frame (640 x 360) to a high-resolution image (1280 x 720). |

### Also see:
<ul>
<li><a class="el" href="https://www.nuget.org/profiles/VisionSkills">Skill nuget packages released by Microsoft*</a></li>
<li><a class="el" href="https://github.com/microsoft/WindowsVisionSkillsPreview">Sample applications for skills released by Microsoft*</a></li>
</ul>

----

##### \* Other names and brands may be claimed as the property of others.
