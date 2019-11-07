# Background Blur

## Use Case and High-Level Description

In many video conferencing scenarios, it may not be desirable to show the background behind the user. To improve the user experience, background blur skill can be used to blur out the background, thereby focusing attention on the user. This skill uses OpenVINO™ based AI inference to segment out the person(s) and blur everything else in the video or image.

## Example

![BackgroundBlur sample output](doc/BackgroundBlur.PNG)

## Inputs

<ol style="list-style-type: circle">
<li>Any size image with BGRA (8 bit) color format</li>
</ol>

## Outputs

<ol style="list-style-type: circle">
<li>Background blurred image with BGRA (8 bit) color format</li>
</ol>

## API Specification

For details on Windows\* Vision Skills API, please refer to Microsoft\* <a href="https://docs.microsoft.com/en-us/windows/ai/windows-vision-skills/important-api-concepts" target="_blank">documentation</a>. Background blur skill implements three components:

<ol style="list-style-type: circle">
<li><a class="el" href="#backgroundblurdescriptor">BackgroundBlurDescriptor</a></li>
<li><a class="el" href="#backgroundblurskill">BackgroundBlurSkill</a></li>
<li><a class="el" href="#backgroundblurbinding">BackgroundBlurBinding</a></li>
</ol>

### BackgroundBlurDescriptor

Implements Microsoft::AI::Skills::SkillInterfacePreview::ISkillDescriptor.

#### BackgroundBlurDescriptor

Constructor for background blur descriptor runtime class component.

```csharp
BackgroundBlurDescriptor()

Parameters: None

Returns: Microsoft::AI::Skills::SkillInterfacePreview::ISkillDescriptor as Intel::AI::Skills::BackgroundBlur::BackgroundBlurDescriptor
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

Instantiates the background blur skill. The execution device is automatically selected by the skill.

```csharp
IAsyncOperation<ISkill>CreateSkillAsync()

Parameters: None

Returns: Microsoft::AI::Skills::SkillInterfacePreview::ISkill backgroundBlurSkill
```
&NewLine;

#### CreateSkillAsync (executionDevice)

Instantiates the background blur skill using the executionDevice provided by the user.

```csharp
IAsyncOperation<ISkill>CreateSkillAsync(ISkillExecutionDevice executionDevice)

Parameters: Microsoft::AI::Skills::SkillInterfacePreview::ISkillExecutionDevice executionDevice

Returns: Microsoft::AI::Skills::SkillInterfacePreview::ISkill backgroundBlurSkill
```
&NewLine;

### BackgroundBlurSkill

Implements Microsoft::AI::Skills::SkillInterfacePreview::ISkill.

#### CreateSkillBindingAsync

Instantiates the background blur binding object.

```csharp
IAsyncOperation<ISkillBinding> CreateSkillBindingAsync()

Parameters: None

Returns: Microsoft::AI::Skills::SkillInterfacePreview::ISkillBinding as Intel::AI::Skills::BackgroundBlur::BackgroundBlurBinding
```
&NewLine;
#### EvaluateAsync

Executes the skill logic using input features provided by the binding object.

```csharp
IAsyncAction EvaluateAsync(ISkillBinding const binding)

Parameters: Intel::AI::Skills::BackgroundBlur::BackgroundBlurBinding binding

Returns: Asynchronous method with no direct return value
```
&NewLine;  

### BackgroundBlurBinding

Implements Microsoft::AI::Skills::SkillInterfacePreview::ISkillBinding. The background blur binding is created by BackgroundBlurSkill object.

#### SetInputImageAsync

Uses the VideoFrame object (frame) and binds it to the input. This is the image frame on which background blur is performed.

```csharp
IAsyncAction SetInputImageAsync(VideoFrame const frame)

Parameters: Windows::Media::VideoFrame frame

Returns: Asynchronous method with no direct return value
```
&NewLine;  

#### GetOutputImageAsync

Returns the output image with background blurred

```csharp
IAsyncOperation<VideoFrame> GetOutputImageAsync()

Parameters: None

Returns: VideoFrame
```
&NewLine;  

## Application

The application developer starts with a C# XAML form based project. As a first step, s/he needs to ingest background blur skill by installing the nuget package from appropriate location. This makes all background blur skill APIs available to use. The GUI design is up to the application developer. Below pseudo code show how the skill APIs can be used to perform background blur.

```csharp
using Intel.AI.Skills.BackgroundBlur;
using Microsoft.AI.Skills.SkillInterfacePreview;

BackgroundBlurDescriptor skillDescriptor = null;
IReadOnlyList<ISkillExecutionDevice> availableExecutionDevices = null;
BackgroundBlurSkill skill = null;
BackgroundBlurBinding binding = null;
int selectedDeviceId = 0;

// Instantiate skill descriptor
skillDescriptor = new BackgroundBlurDescriptor();

// Get list of available execution devices
availableExecutionDevices = await skillDescriptor.GetSupportedExecutionDevicesAsync();

// Initialize skill with the selected execution device
skill = await skillDescriptor.CreateSkillAsync(availableExecutionDevices[selectedDeviceId]) as BackgroundBlurSkill;

// Instantiate skill binding object
binding = await skill.CreateSkillBindingAsync() as BackgroundBlurBinding;

// Set input image frame which can be from a camera feed or a file read
await binding.SetInputImageAsync(frame);

// Run background blur skill with the binding object
await skill.EvaluateAsync(binding);

// Consume result
VideoFrame blurredOutput = await binding.GetOutputImageAsync() ;

// Display background blurred image using application's implementation of a renderer
ShowImage(blurredOutput);
```
&NewLine;

----

##### *Legal Information: Other names and brands may be claimed as the property of others.

