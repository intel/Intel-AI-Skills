# Super Resolution (WinML)

## Use Case and High-Level Description

Network bandwidth constraints may prohibit users from sending and consuming high resolution images or video stream over the network. Most playback applications have the capability to upscale the input image to be able to utilize the high resolution display real estate. However, this approach may not provide the best quality image viewing experience. Super resolution skill can be used to upscale the image using WinML AI inference based algorithm to improve the quality of upscaled image.

## Example

The image below shows a composite of linearly scaled version (left) and super resolution output (right). The right side of the image is generated using super resolution skill. 

![SuperResolutionWinML sample output](doc/SuperResolutionWinML.PNG)

## Inputs

<ol style="list-style-type: circle">
<li>640x360 size image with BGRA (8 bit) color format. To avoid resize, input image must be 640x360.</li>
</ol>

## Outputs

<ol style="list-style-type: circle">
<li>2X upscaled image with BGRA (8 bit) color format at 1280x720 resolution.</li>
</ol>

## API Specification

For details on Windows\* Vision Skills API, please refer to Microsoft\* <a href="https://docs.microsoft.com/en-us/windows/ai/windows-vision-skills/important-api-concepts" target="_blank">documentation</a>. Super Resolution skill implements three components:

<ol style="list-style-type: circle">
<li><a class="el" href="#SuperResolutionWinMLdescriptor">SuperResolutionWinMLDescriptor</a></li>
<li><a class="el" href="#SuperResolutionWinMLskill">SuperResolutionWinMLSkill</a></li>
<li><a class="el" href="#SuperResolutionWinMLbinding">SuperResolutionWinMLBinding</a></li>
</ol>

### SuperResolutionWinMLDescriptor

Implements Microsoft::AI::Skills::SkillInterfacePreview::ISkillDescriptor.

#### SuperResolutionWinMLDescriptor

Constructor for super resolution descriptor runtime class component.

```csharp
SuperResolutionWinMLDescriptor()

Parameters: None

Returns: Microsoft::AI::Skills::SkillInterfacePreview::ISkillDescriptor as Intel::AI::Skills::SuperResolutionWinML::SuperResolutionWinMLDescriptor
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

Instantiates the super resolution skill. The execution device is automatically selected by the skill.

```csharp
IAsyncOperation<ISkill>CreateSkillAsync()

Parameters: None

Returns: Microsoft::AI::Skills::SkillInterfacePreview::ISkill SuperResolutionWinMLSkill
```
&NewLine;

#### CreateSkillAsync (executionDevice)

Instantiates the super resolution skill using the executionDevice provided by the user.

```csharp
IAsyncOperation<ISkill>CreateSkillAsync(ISkillExecutionDevice executionDevice)

Parameters: Microsoft::AI::Skills::SkillInterfacePreview::ISkillExecutionDevice executionDevice

Returns: Microsoft::AI::Skills::SkillInterfacePreview::ISkill SuperResolutionWinMLSkill
```
&NewLine;

### SuperResolutionWinMLSkill

Implements Microsoft::AI::Skills::SkillInterfacePreview::ISkill.

#### CreateSkillBindingAsync

Instantiates the super resolution binding object.

```csharp
IAsyncOperation<ISkillBinding> CreateSkillBindingAsync()

Parameters: None

Returns: Microsoft::AI::Skills::SkillInterfacePreview::ISkillBinding as Intel::AI::Skills::SuperResolutionWinML::SuperResolutionWinMLBinding
```
&NewLine;

#### EvaluateAsync

Executes the skill logic using input features provided by the binding object.

```csharp
IAsyncAction EvaluateAsync(ISkillBinding const binding)

Parameters: Intel::AI::Skills::SuperResolutionWinML::SuperResolutionWinMLBinding binding

Returns: Asynchronous method with no direct return value
```
&NewLine;

### SuperResolutionWinMLBinding

Implements Microsoft::AI::Skills::SkillInterfacePreview::ISkillBinding. The super resolution binding is created by SuperResolutionWinMLSkill object.

#### SetInputImageAsync

Uses the VideoFrame object (frame) and binds it to the input. This is the image frame on which super resolution is performed.

```csharp
IAsyncAction SetInputImageAsync(VideoFrame const frame)

Parameters: Windows::Media::VideoFrame frame

Returns: Asynchronous method with no direct return value
```
&NewLine;

#### GetOutputImageAsync

Returns the high resolution output image

```csharp
IAsyncOperation<VideoFrame> GetOutputImageAsync()

Parameters: None

Returns: VideoFrame
```
&NewLine;  

## Application

The application developer starts with a C# XAML form based project. As a first step, s/he needs to ingest super resolution skill by installing the nuget package from appropriate location. This makes all super resolution skill APIs available to use. The GUI design is up to the application developer. Below pseudo code show how the skill APIs can be used to perform super resolution.

```csharp
using Intel.AI.Skills.SuperResolutionWinML;
using Microsoft.AI.Skills.SkillInterfacePreview;

SuperResolutionWinMLDescriptor skillDescriptor = null;
IReadOnlyList<ISkillExecutionDevice> availableExecutionDevices = null;
SuperResolutionWinMLSkill skill = null;
SuperResolutionWinMLBinding binding = null;
int selectedDeviceId = 0;

// Instantiate skill descriptor
skillDescriptor = new SuperResolutionWinMLDescriptor();

// Get list of available execution devices
availableExecutionDevices = await skillDescriptor.GetSupportedExecutionDevicesAsync();

// Initialize skill with the selected execution device
skill = await skillDescriptor.CreateSkillAsync(availableExecutionDevices[selectedDeviceId]) as SuperResolutionWinMLSkill;

// Instantiate skill binding object
binding = await skill.CreateSkillBindingAsync() as SuperResolutionWinMLBinding;

// Set input image frame which can be from a camera feed or a file read
await binding.SetInputImageAsync(frame);

// Run super resolution skill with the binding object
await skill.EvaluateAsync(binding);

// Consume result
VideoFrame superResOutput = await binding.GetOutputImageAsync();

// Display super resolution image using application's implementation of a renderer
ShowImage(superResOutput);
```

----

##### *Legal Information: Other names and brands may be claimed as the property of others.
