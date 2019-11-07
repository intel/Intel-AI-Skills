# Background Replacement

## Use Case and High-Level Description

In many video conferencing scenarios, it may not be desirable to show the background behind the user. To improve the user experience, background replacement skill can be used to segmentat out the background, thereby focusing attention on the user. This skill uses OpenVINO™ based AI inference to segment out the person(s) and replace the background with a user selected scene.

## Example

![BackgroundReplacement sample output](doc/BackgroundReplacement.PNG)

## Inputs

<ol style="list-style-type: circle">
<li>Any size image with BGRA (8 bit) color format</li>
</ol>

## Outputs

<ol style="list-style-type: circle">
<li>Background replaced image with BGRA (8 bit) color format</li>
</ol>

## API Specification

For details on Windows\* Vision Skills API, please refer to Microsoft\* <a href="https://docs.microsoft.com/en-us/windows/ai/windows-vision-skills/important-api-concepts" target="_blank">documentation</a>. Background Replacement skill implements three components</p>

<ol style="list-style-type: circle">
<li><a class="el" href="#backgroundreplacementdescriptor">BackgroundReplacementDescriptor</a></li>
<li><a class="el" href="#backgroundreplacementskill">BackgroundReplacementSkill</a></li>
<li><a class="el" href="#backgroundreplacementbinding">BackgroundReplacementBinding</a></li>
</ol>

### BackgroundReplacementDescriptor

Implements Microsoft::AI::Skills::SkillInterfacePreview::ISkillDescriptor.

#### BackgroundReplacementDescriptor

Constructor for background replacement descriptor runtime class component.

```csharp
BackgroundReplacementDescriptor()

Parameters: None

Returns: Microsoft::AI::Skills::SkillInterfacePreview::ISkillDescriptor as Intel::AI::Skills::BackgroundReplacement::BackgroundReplacementDescriptor
```
&NewLine;

#### Information

Returns structure containing information about the skill which includes name, description, author, publisher and version.

```csharp
SkillInformation Information()

Parameters: None

Returns: SkillInformation information
```
&NewLine;

#### InputFeatureDescriptors

Returns the set of input features used by the skill.

```csharp
IVectorView<ISkillFeatureDescriptor> InputFeatureDescriptors()

Parameters: None

Returns: IVectorView<Microsoft::AI::Skills::SkillInterfacePreview::ISkillFeatureDescriptor> inputFeatures
```
&NewLine;

#### OutputFeatureDescriptors

Returns the set of output features provided by the skill.

```csharp
IVectorView<ISkillFeatureDescriptor> OutputFeatureDescriptors()

Parameters: None

Returns: IVectorView<Microsoft::AI::Skills::SkillInterfacePreview::ISkillFeatureDescriptor> outputFeatures
```
&NewLine;

#### GetSupportedExecutionDevicesAsync

Returns a list of devices of hardware accelerators available on the platform which can be used to execute the skill logic.

```csharp
IAsyncOperation<IVectorView<ISkillExecutionDevice>> GetSupportedExecutionDevicesAsync()

Parameters: None

Returns: IVectorView<Microsoft::AI::Skills::SkillInterfacePreview::ISkillExecutionDevice> outputFeatures
```
&NewLine;

#### CreateSkillAsync

Instantiates the background replacement skill. The execution device is automatically selected by the skill.

```csharp
IAsyncOperation<ISkill>CreateSkillAsync()

Parameters: None

Returns: Microsoft::AI::Skills::SkillInterfacePreview::ISkill personSegmentationSkill
```
&NewLine;

#### CreateSkillAsync (executionDevice)

Instantiates the background replacement skill using the executionDevice provided by the user.

```csharp
IAsyncOperation<ISkill>CreateSkillAsync(ISkillExecutionDevice executionDevice)

Parameters: Microsoft::AI::Skills::SkillInterfacePreview::ISkillExecutionDevice executionDevice

Returns: Microsoft::AI::Skills::SkillInterfacePreview::ISkill personSegmentationSkill
```
&NewLine;

### BackgroundReplacementSkill

Implements Microsoft::AI::Skills::SkillInterfacePreview::ISkill.

#### CreateSkillBindingAsync

Instantiates the background replacement binding object.

```csharp
IAsyncOperation<ISkillBinding> CreateSkillBindingAsync()

Parameters: None

Returns: Microsoft::AI::Skills::SkillInterfacePreview::ISkillBinding as Intel::AI::Skills::BackgroundReplacement::BackgroundReplacementBinding
```
&NewLine;

#### EvaluateAsync

Executes the skill logic using input features provided by the binding object.

```csharp
IAsyncAction EvaluateAsync(ISkillBinding const binding)

Parameters: Intel::AI::Skills::BackgroundReplacement::BackgroundReplacementBinding binding

Returns: Asynchronous method with no direct return value
```
&NewLine;

### BackgroundReplacementBinding

Implements Microsoft::AI::Skills::SkillInterfacePreview::ISkillBinding. The background replacement binding is created by BackgroundReplacementSkill object.

#### SetInputImageAsync

Uses the VideoFrame object (frame) and binds it to the input. This is the image frame on which background replacement is performed.

```csharp
IAsyncAction SetInputImageAsync(VideoFrame const frame)

Parameters: Windows::Media::VideoFrame frame

Returns: Asynchronous method with no direct return value
```
&NewLine;

#### SetBackgroundImageAsync

Uses the VideoFrame object (frame) and binds it to the input. This is the user selected image which is used as the replacement background.

```csharp
IAsyncAction SetBackgroundImageAsync(VideoFrame const frame)

Parameters: Windows::Media::VideoFrame frame

Returns: Asynchronous method with no direct return value
```
&NewLine;

#### GetOutputImageAsync

Returns the output image with background replaced

```csharp
IAsyncOperation<VideoFrame> GetOutputImageAsync()

Parameters: None

Returns: VideoFrame
```
&NewLine;  
  
## Application

The application developer starts with a C# XAML form based project. As a first step, s/he needs to ingest background replacement skill by installing the nuget package from appropriate location. This makes all background replacement skill APIs available to use. The GUI design is up to the application developer. Below pseudo code show how the skill APIs can be used to perform background replacement.

```csharp
using Intel.AI.Skills.BackgroundReplacement;
using Microsoft.AI.Skills.SkillInterfacePreview;

BackgroundReplacementDescriptor skillDescriptor = null;
IReadOnlyList<ISkillExecutionDevice> availableExecutionDevices = null;
BackgroundReplacementSkill skill = null;
BackgroundReplacementBinding binding = null;
int selectedDeviceId = 0;

// Instantiate skill descriptor
skillDescriptor = new BackgroundReplacementDescriptor();

// Get list of available execution devices
availableExecutionDevices = await skillDescriptor.GetSupportedExecutionDevicesAsync();

// Initialize skill with the selected execution device
skill = await skillDescriptor.CreateSkillAsync(availableExecutionDevices[selectedDeviceId]) as BackgroundReplacementSkill;

// Instantiate skill binding object
binding = await skill.CreateSkillBindingAsync() as BackgroundReplacementBinding;

// Set user selected background replacement image
await binding.SetBackgroundImageAsync(backgroundImage);

// Set input image frame which can be from a camera feed or a file read
await binding.SetInputImageAsync(frame);

// Run background replacement skill with the binding object
await skill.EvaluateAsync(binding);

// Consume result
VideoFrame segmentedOutput = await binding.GetOutputImageAsync();

// Display background replaced image using application's implementation of a renderer
ShowImage(segmentedOutput);
```
&NewLine;
----

##### *Legal Information: Other names and brands may be claimed as the property of others.
